using System.Collections.Generic;
using Infraestructura.SQLite;
using Mono.Data.Sqlite;
using UnityEngine;

public class SeedPregunta
{
    private readonly ConexionSQLite conexion;
    private Dictionary<int, string> retosPorActividad = new Dictionary<int, string>();

    public SeedPregunta(ConexionSQLite conexion)
    {
        this.conexion = conexion;
    }

    public void Ejecutar()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Data/preguntas");

        if (csvFile == null)
        {
            Debug.LogError("[SeedPregunta] ✗ No se encontró Resources/Data/preguntas.csv");
            return;
        }

        using var conn = conexion.CrearConexion();
        conn.Open();

        GuardarRetosActuales(conn);
        LimpiarPreguntasAnteriores(conn);

        var lineas = csvFile.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);

        bool esEncabezado = true;
        int contadorPreguntas = 0;

        foreach (string linea in lineas)
        {
            string l = linea.Trim();
            if (string.IsNullOrEmpty(l)) continue;

            if (esEncabezado)
            {
                esEncabezado = false;
                continue;
            }

            var campos = l.Split(';');
            if (campos.Length < 8)
            {
                Debug.LogWarning($"[SeedPregunta] ⚠ Línea inválida: {linea}");
                continue;
            }

            if (!int.TryParse(campos[0].Trim(), out int actividadId))
            {
                Debug.LogWarning($"[SeedPregunta] ⚠ ActividadId inválido: {campos[0]}");
                continue;
            }

            string enunciado = campos[3].Trim();
            string opcion1 = campos[4].Trim();
            string opcion2 = campos[5].Trim();
            string opcion3 = campos[6].Trim();
            string respuestaCorrecta = campos[7].Trim();

            string opciones = $"{opcion1}|{opcion2}|{opcion3}";

            int ordenGlobal = ObtenerSiguienteOrden(conn, actividadId);

            InsertarPregunta(conn, actividadId, ordenGlobal, enunciado, opciones, respuestaCorrecta);
            contadorPreguntas++;
        }

        ReinsertarRetosAlFinal(conn);

        Debug.Log($"[SeedPregunta] ✓ {contadorPreguntas} preguntas cargadas");
    }

    private void LimpiarPreguntasAnteriores(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ContenidoActividad WHERE Tipo = 'Pregunta';";
        cmd.ExecuteNonQuery();
        Debug.Log("[SeedPregunta] Preguntas anteriores eliminadas");
    }

    private void GuardarRetosActuales(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT ActividadId, Texto, Recurso, Instrucciones
            FROM ContenidoActividad
            WHERE Tipo = 'Reto';
        ";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int actividadId = reader.GetInt32(0);
            string texto = reader.IsDBNull(1) ? "" : reader.GetString(1);
            string recurso = reader.IsDBNull(2) ? "" : reader.GetString(2);
            string instrucciones = reader.IsDBNull(3) ? "" : reader.GetString(3);

            retosPorActividad[actividadId] = $"{texto}||{recurso}||{instrucciones}";
        }

        Debug.Log($"[SeedPregunta] Guardados {retosPorActividad.Count} retos");
    }

    private void ReinsertarRetosAlFinal(SqliteConnection conn)
    {
        foreach (var kvp in retosPorActividad)
        {
            int actividadId = kvp.Key;
            string datos = kvp.Value;
            string[] partes = datos.Split(new string[] { "||" }, System.StringSplitOptions.None);

            string texto = partes.Length > 0 ? partes[0] : "";
            string recurso = partes.Length > 1 ? partes[1] : "";
            string instrucciones = partes.Length > 2 ? partes[2] : "";

            var cmd = conn.CreateCommand();
            cmd.CommandText = $"DELETE FROM ContenidoActividad WHERE ActividadId = {actividadId} AND Tipo = 'Reto';";
            cmd.ExecuteNonQuery();

            int nuevoOrden = ObtenerSiguienteOrden(conn, actividadId);

            cmd = conn.CreateCommand();
            cmd.CommandText = $@"
            INSERT INTO ContenidoActividad
            (ActividadId, Tipo, Orden, Texto, Recurso, Opciones, RespuestaCorrecta, Instrucciones)
            VALUES ({actividadId}, 'Reto', {nuevoOrden}, '{Escapar(texto)}', '{Escapar(recurso)}', NULL, NULL, '{Escapar(instrucciones)}');
            ";
            cmd.ExecuteNonQuery();

            Debug.Log($"[SeedPregunta] Reto de actividad {actividadId} reinsertado con orden {nuevoOrden}");
        }
    }

    private static string Escapar(string valor) => valor.Replace("'", "''");

    // 🔥 NUEVO: obtiene el siguiente orden disponible por actividad
    private int ObtenerSiguienteOrden(SqliteConnection conn, int actividadId)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT IFNULL(MAX(Orden), 0) + 1
            FROM ContenidoActividad
            WHERE ActividadId = @actividadId
        ";

        cmd.Parameters.AddWithValue("@actividadId", actividadId);

        var resultado = cmd.ExecuteScalar();
        return System.Convert.ToInt32(resultado);
    }

    private void InsertarPregunta(SqliteConnection conn, int actividadId, int orden, string enunciado, string opciones, string respuestaCorrecta)
    {
        try
        {
            var cmd = conn.CreateCommand();

            cmd.CommandText = @"
            INSERT OR REPLACE INTO ContenidoActividad
            (ActividadId, Tipo, Orden, Texto, Recurso, Opciones, RespuestaCorrecta, Instrucciones)
            VALUES (@actividadId, 'Pregunta', @orden, @texto, NULL, @opciones, @respuesta, NULL);
            ";

            cmd.Parameters.AddWithValue("@actividadId", actividadId);
            cmd.Parameters.AddWithValue("@orden", orden);
            cmd.Parameters.AddWithValue("@texto", enunciado);
            cmd.Parameters.AddWithValue("@opciones", opciones);
            cmd.Parameters.AddWithValue("@respuesta", respuestaCorrecta);

            cmd.ExecuteNonQuery();

            Debug.Log($"✓ Actividad {actividadId}, Pregunta orden {orden} insertada");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SeedPregunta] ✗ Error al insertar: {ex.Message}");
        }
    }
}