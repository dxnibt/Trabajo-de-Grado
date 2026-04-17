using System.Collections.Generic;
using Dominio.entidades;
using Mono.Data.Sqlite;

namespace Infraestructura.SQLite.SQLiteGateway
{
    public class SQLiteActividadGateway
    {
        private readonly ConexionSQLite conexion;

        public SQLiteActividadGateway(ConexionSQLite conexion)
        {
            this.conexion = conexion;
        }

        public Actividad ObtenerPorId(int actividadId, Nivel nivel)
        {
            using var conn = conexion.CrearConexion();
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
            SELECT Id, Titulo
            FROM Actividad
            WHERE Id = $id;
            ";

            cmd.Parameters.AddWithValue("$id", actividadId);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            var actividad = new Actividad(
                reader.GetInt32(0),
                reader.GetString(1),
                nivel,
                new List<ContenidoActividad>()
            );

            reader.Close();

            var cmd2 = conn.CreateCommand();
            cmd2.CommandText = @"
            SELECT Tipo, Orden, Texto, Recurso, Opciones, RespuestaCorrecta
            FROM ContenidoActividad
            WHERE ActividadId = $id
            ORDER BY Orden;
            ";

            cmd2.Parameters.AddWithValue("$id", actividadId);

            using var reader2 = cmd2.ExecuteReader();

            var contenidos = new List<ContenidoActividad>();

            while (reader2.Read())
            {
                string tipo = reader2.GetString(0);
                int orden = reader2.GetInt32(1);

                string texto = reader2.IsDBNull(2) ? "" : reader2.GetString(2);
                string recurso = reader2.IsDBNull(3) ? "" : reader2.GetString(3);
                string opcionesTexto = reader2.IsDBNull(4) ? "" : reader2.GetString(4);
                string respuestaCorrecta = reader2.IsDBNull(5) ? "" : reader2.GetString(5);

                switch (tipo)
                {
                    case "Historia":
                        contenidos.Add(new Historia(orden, recurso));
                        break;

                    case "Pregunta":

                        List<string> opciones = new List<string>();

                        if (opcionesTexto != "")
                        {
                            string[] arreglo = opcionesTexto.Split('|');

                            foreach (string opcion in arreglo)
                            {
                                opciones.Add(opcion);
                            }
                        }

                        contenidos.Add(new Pregunta(
                            orden,
                            orden,
                            texto,
                            opciones,
                            respuestaCorrecta
                        ));

                        break;

                    case "Reto":
                        contenidos.Add(new Reto(orden, texto, recurso));
                        break;
                }
            }

            actividad = new Actividad(
                actividad.Id,
                actividad.Titulo,
                nivel,
                contenidos
            );

            return actividad;
        }
    }
}