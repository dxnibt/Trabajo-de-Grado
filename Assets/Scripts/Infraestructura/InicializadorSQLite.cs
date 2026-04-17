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

            var cmd = conn.CreateCommand();

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
                FOREIGN KEY (ActividadId) REFERENCES Actividad(Id)
            );

            CREATE TABLE IF NOT EXISTS Progreso (
                EstudianteId INTEGER PRIMARY KEY,
                ActividadId INTEGER,
                Completada INTEGER NOT NULL,
                IndiceContenidoActual INTEGER NOT NULL,
                FOREIGN KEY (ActividadId) REFERENCES Actividad(Id)
            );

            CREATE TABLE IF NOT EXISTS Respuestas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EstudianteId INTEGER NOT NULL,
                PreguntaId INTEGER NOT NULL,
                RespuestaSeleccionada TEXT NOT NULL,
                EsCorrecta INTEGER NOT NULL
            );

            ";

            cmd.ExecuteNonQuery();
        }
    }
}