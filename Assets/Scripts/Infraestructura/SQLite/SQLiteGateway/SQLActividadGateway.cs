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
                Debug.Log($"[SQLActividadGateway] Cargado: {tipo} con Orden={orden}");

                string texto = reader2.IsDBNull(2) ? "" : reader2.GetString(2);
                string recurso = reader2.IsDBNull(3) ? "" : reader2.GetString(3);
                string opcionesTexto = reader2.IsDBNull(4) ? "" : reader2.GetString(4);
                string respuestaCorrecta = reader2.IsDBNull(5) ? "" : reader2.GetString(5);
                string instrucciones = reader2.IsDBNull(6) ? "" : reader2.GetString(6);

                if (tipo == "Reto")
                {
                    Debug.Log($"[SQLActividadGateway] RETO LEIDO");
                    Debug.Log($"[DEBUG] Instrucciones RAW >>>{instrucciones}<<<");
                    Debug.Log($"[DEBUG] Es NULL: {reader2.IsDBNull(6)}");
                    Debug.Log($"[DEBUG] Longitud: {instrucciones?.Length}");
                }

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
                                opciones.Add(opcion.Trim());
                            }
                        }

                        string respuestaTrim = respuestaCorrecta.Trim();
                        contenidos.Add(new Pregunta(orden, orden, texto, opciones, respuestaTrim));
                        break;

                    case "Reto":
                        var reto = new Reto(orden, texto, recurso, instrucciones);
                        Debug.Log($"[SQLActividadGateway] RETO CARGADO:");
                        Debug.Log($"  - Orden: {orden}");
                        Debug.Log($"  - Instrucciones RAW: '{instrucciones}'");
                        Debug.Log($"  - Longitud: {instrucciones.Length}");
                        CargarInstruccionesPares(reto, instrucciones);
                        Debug.Log($"[SQLActividadGateway] DESPUÉS CargarInstruccionesPares: {reto.InstruccionesPares.Count} instrucciones");
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

            Debug.Log($"[SQLActividadGateway] Total contenidos cargados para actividad {actividadId}: {actividad.TotalContenidos}");
            for (int i = 0; i < actividad.Contenidos.Count; i++)
            {
                Debug.Log($"  [{i}] {actividad.Contenidos[i].GetType().Name} - Orden={actividad.Contenidos[i].Orden}");
            }

            return actividad;
        }

        private void CargarInstruccionesPares(Reto reto, string instruccionesTexto)
        {
            Debug.Log($"[CargarInstruccionesPares] INICIO");
            Debug.Log($"[CargarInstruccionesPares] Texto length: {instruccionesTexto?.Length ?? 0}");
            Debug.Log($"[CargarInstruccionesPares] Es null: {instruccionesTexto == null}");
            Debug.Log($"[CargarInstruccionesPares] Texto RAW: '{instruccionesTexto}'");

            if (string.IsNullOrEmpty(instruccionesTexto))
            {
                Debug.LogWarning("[CargarInstruccionesPares] Texto vacío, retornando");
                return;
            }

            // Debug: mostrar caracteres especiales
            Debug.Log($"[CargarInstruccionesPares] Bytes: {System.BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(instruccionesTexto))}");

            string[] pasos = instruccionesTexto.Split(new string[] { "//" }, System.StringSplitOptions.RemoveEmptyEntries);
            Debug.Log($"[CargarInstruccionesPares] Split por //, encontrados {pasos.Length} pasos");

            if (pasos.Length == 0)
            {
                Debug.LogWarning("[CargarInstruccionesPares] No se encontraron pasos después de split por //");
                return;
            }

            foreach (string paso in pasos)
            {
                Debug.Log($"[CargarInstruccionesPares] Procesando paso: '{paso}'");
                string[] partes = paso.Split(new string[] { "||" }, System.StringSplitOptions.None);
                Debug.Log($"[CargarInstruccionesPares] Paso tiene {partes.Length} partes");

                if (partes.Length >= 2)
                {
                    string img = partes[0].Trim();
                    string txt = partes[1].Trim();
                    Debug.Log($"[CargarInstruccionesPares] ✓ Agregando: img='{img}', txt='{txt.Substring(0, Math.Min(50, txt.Length))}'...");
                    reto.AgregarParInstruccion(img, txt, "", "");
                }
                else
                {
                    Debug.LogWarning($"[CargarInstruccionesPares] ✗ Paso ignorado - esperaba 2+ partes, encontró {partes.Length}");
                }
            }

            Debug.Log($"[CargarInstruccionesPares] FIN - Total: {reto.InstruccionesPares.Count}");
        }       
    }
}