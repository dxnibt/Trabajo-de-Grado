using Mono.Data.Sqlite;

namespace Infraestructura.SQLite
{
    public class InicializadorBD
    {
        private readonly ConexionSQLite conexion;

        public InicializadorBD(ConexionSQLite conexion)
        {
            this.conexion = conexion;
        }

        public void CrearTablas()
        {
            using var conn = conexion.CrearConexion();
            conn.Open();

            // Asegurar UTF-8 en SQLite
            var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA encoding = 'UTF-8';";
            cmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();

            cmd.CommandText = @"

            CREATE TABLE IF NOT EXISTS Actividad (
                Id INTEGER PRIMARY KEY,
                Titulo TEXT NOT NULL,
                NivelId INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ContenidoActividad (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ActividadId INTEGER NOT NULL,
                Tipo TEXT NOT NULL,
                Orden INTEGER NOT NULL,
                Texto TEXT,
                Recurso TEXT,
                Opciones TEXT,
                RespuestaCorrecta TEXT,
                Instrucciones TEXT,
                FOREIGN KEY (ActividadId) REFERENCES Actividad(Id)
            );

            CREATE TABLE IF NOT EXISTS Progreso (
                EstudianteId INTEGER NOT NULL,
                ActividadId INTEGER NOT NULL,
                Completada INTEGER NOT NULL,
                IndiceContenidoActual INTEGER NOT NULL,
                PRIMARY KEY (EstudianteId, ActividadId)
            );

            CREATE TABLE IF NOT EXISTS Respuestas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EstudianteId INTEGER NOT NULL,
                PreguntaId INTEGER NOT NULL,
                RespuestaSeleccionada TEXT NOT NULL,
                EsCorrecta INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Estudiante (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nombre TEXT NOT NULL UNIQUE,
                EsGrupo INTEGER NOT NULL DEFAULT 0
            );

            CREATE TABLE IF NOT EXISTS ActividadCompletada (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EstudianteId INTEGER NOT NULL,
                ActividadId INTEGER NOT NULL,
                Fecha TEXT,
                UNIQUE(EstudianteId, ActividadId)
            );

            ";

            cmd.ExecuteNonQuery();

            // Migración: columna Instrucciones en ContenidoActividad
            try
            {
                cmd.CommandText = "ALTER TABLE ContenidoActividad ADD COLUMN Instrucciones TEXT;";
                cmd.ExecuteNonQuery();
            }
            catch { }

            // Migración: Progreso pasa de PK simple (EstudianteId) a PK compuesta (EstudianteId, ActividadId)
            try
            {
                cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='Progreso' LIMIT 1;";
                var tableSql = cmd.ExecuteScalar() as string;

                if (tableSql != null && !tableSql.Contains("PRIMARY KEY (EstudianteId"))
                {
                    cmd.CommandText = "ALTER TABLE Progreso RENAME TO Progreso_v1;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"
                        CREATE TABLE Progreso (
                            EstudianteId INTEGER NOT NULL,
                            ActividadId INTEGER NOT NULL,
                            Completada INTEGER NOT NULL,
                            IndiceContenidoActual INTEGER NOT NULL,
                            PRIMARY KEY (EstudianteId, ActividadId)
                        );";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"
                        INSERT OR IGNORE INTO Progreso
                        SELECT EstudianteId, ActividadId, Completada, IndiceContenidoActual
                        FROM Progreso_v1 WHERE ActividadId IS NOT NULL AND ActividadId != 0;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "DROP TABLE Progreso_v1;";
                    cmd.ExecuteNonQuery();
                }
            }
            catch { }
        }
    }
}