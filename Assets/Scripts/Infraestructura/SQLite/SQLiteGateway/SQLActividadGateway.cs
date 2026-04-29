using System;
using System.Collections.Generic;
using Dominio.entidades;
using Mono.Data.Sqlite;
using UnityEngine;

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
            cmd.CommandText = "SELECT Id, Titulo FROM Actividad WHERE Id = $id;";
            cmd.Parameters.AddWithValue("$id", actividadId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;

            int id = reader.GetInt32(0);
            string titulo = reader.GetString(1);
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
                        var opciones = new List<string>();
                        if (opcionesTexto != "")
                        {
                            foreach (string o in opcionesTexto.Split('|'))
                                opciones.Add(o.Trim());
                        }
                        contenidos.Add(new Pregunta(orden, orden, texto, opciones, respuestaCorrecta.Trim()));
                        break;

                    case "Reto":
                        var reto = new Reto(orden, texto, recurso, instrucciones);
                        CargarInstruccionesPares(reto, instrucciones);
                        contenidos.Add(reto);
                        break;

                    default:
                        Debug.LogWarning($"[SQLiteActividadGateway] Tipo desconocido: {tipo}");
                        break;
                }
            }

            Debug.Log($"[SQLiteActividadGateway] Actividad {actividadId}: {contenidos.Count} contenidos cargados");
            return new Actividad(id, titulo, nivel, contenidos);
        }

        private void CargarInstruccionesPares(Reto reto, string instruccionesTexto)
        {
            if (string.IsNullOrEmpty(instruccionesTexto)) return;

            string[] pasos = instruccionesTexto.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

            // Agrupa los pasos de a dos: paso i y paso i+1 forman un InstruccionPar
            for (int i = 0; i < pasos.Length; i += 2)
            {
                ParsearPaso(pasos[i], out string img1, out string txt1);

                string img2 = "", txt2 = "";
                if (i + 1 < pasos.Length)
                    ParsearPaso(pasos[i + 1], out img2, out txt2);

                reto.AgregarParInstruccion(img1, txt1, img2, txt2);
            }
        }

        private void ParsearPaso(string paso, out string imagen, out string texto)
        {
            string[] partes = paso.Split(new string[] { "||" }, StringSplitOptions.None);
            if (partes.Length >= 2)
            {
                imagen = partes[0].Trim();
                texto  = partes[1].Trim();
            }
            else
            {
                Debug.LogWarning($"[SQLiteActividadGateway] Paso mal formado: '{paso}'");
                imagen = "";
                texto  = paso.Trim();
            }
        }
    }
}
