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
                Titulo TEXT,
                NivelId INTEGER
            );

            CREATE TABLE IF NOT EXISTS ContenidoActividad (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ActividadId INTEGER,
                Tipo TEXT,
                Orden INTEGER,
                Texto TEXT,
                Recurso TEXT,
                RespuestaCorrecta TEXT
            );

            CREATE TABLE IF NOT EXISTS Progreso (
                EstudianteId INTEGER PRIMARY KEY,
                ActividadId INTEGER,
                Completada INTEGER,
                IndiceContenidoActual INTEGER
            );

            CREATE TABLE IF NOT EXISTS Respuestas (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                EstudianteId INTEGER,
                PreguntaId INTEGER,
                RespuestaSeleccionada TEXT,
                EsCorrecta INTEGER
            );

            ";

            cmd.ExecuteNonQuery();
        }
    }
}