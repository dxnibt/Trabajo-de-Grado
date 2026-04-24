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
            SELECT Tipo, Orden, Texto, Recurso, Opciones, RespuestaCorrecta, Instrucciones
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
                string instrucciones = reader2.IsDBNull(6) ? "" : reader2.GetString(6);

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
                                // 🔥 Trim para eliminar espacios en blanco
                                opciones.Add(opcion.Trim());
                            }
                        }

                        // 🔥 También trim a la respuesta correcta
                        string respuestaTrim = respuestaCorrecta.Trim();

                        contenidos.Add(new Pregunta(
                            orden,
                            orden,
                            texto,
                            opciones,
                            respuestaTrim
                        ));

                        break;

                    case "Reto":
                        var reto = new Reto(orden, texto, recurso, instrucciones);
                        CargarInstruccionesPares(reto, instrucciones);
                        contenidos.Add(reto);
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

        private void CargarInstruccionesPares(Reto reto, string instruccionesTexto)
        {
            if (string.IsNullOrEmpty(instruccionesTexto) || !instruccionesTexto.Contains("||"))
            {
                UnityEngine.Debug.Log("No hay instrucciones en pares para este reto");
                return;
            }

            string[] pares = instruccionesTexto.Split(new string[] { "//" }, System.StringSplitOptions.RemoveEmptyEntries);
            UnityEngine.Debug.Log($"Se encontraron {pares.Length} pares de instrucciones");

            foreach (string par in pares)
            {
                string[] elementos = par.Split(new string[] { "||" }, System.StringSplitOptions.RemoveEmptyEntries);

                UnityEngine.Debug.Log($"Procesando par con {elementos.Length} elementos");

                if (elementos.Length >= 2)
                {
                    string imagen1 = elementos[0].Trim();
                    string texto1 = elementos[1].Trim();
                    string imagen2 = elementos.Length >= 3 ? elementos[2].Trim() : "";
                    string texto2 = elementos.Length >= 4 ? elementos[3].Trim() : "";

                    UnityEngine.Debug.Log($"Agregando instrucciones: {imagen1} - {texto1} | {imagen2} - {texto2}");

                    reto.AgregarInstrucciones(imagen1, texto1, imagen2, texto2);
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"Elemento con menos de 2 partes: {elementos.Length}");
                }
            }

            UnityEngine.Debug.Log($"Total de pares cargados: {reto.InstruccionesPares.Count}");
        }
    }
}