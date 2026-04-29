using System.Collections.Generic;
using Infraestructura.SQLite;
using Mono.Data.Sqlite;
using UnityEngine;

public class SeedPregunta
{
    private readonly ConexionSQLite conexion;

    // Tuple evita la corrupción al re-serializar instrucciones que contienen "||"
    private readonly Dictionary<int, (string texto, string recurso, string instrucciones)> retosPorActividad = new();

    public SeedPregunta(ConexionSQLite conexion)
    {
        this.conexion = conexion;
    }

    public void Ejecutar()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Data/preguntas");

        if (csvFile == null)
        {
            Debug.LogError("[SeedPregunta] No se encontró Resources/Data/preguntas.csv");
            return;
        }

        using var conn = conexion.CrearConexion();
        conn.Open();

        GuardarRetosActuales(conn);
        LimpiarPreguntasAnteriores(conn);

        var lineas = csvFile.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        bool esEncabezado = true;
        int count = 0;

        foreach (string linea in lineas)
        {
            string l = linea.Trim();
            if (string.IsNullOrEmpty(l)) continue;
            if (esEncabezado) { esEncabezado = false; continue; }

            var campos = l.Split(';');
            if (campos.Length < 8) continue;
            if (!int.TryParse(campos[0].Trim(), out int actividadId)) continue;

            // campos[1]=Nivel, campos[2]=NumeroPregunta (no usados para insertar)
            string enunciado = campos[3].Trim();
            string opciones = $"{campos[4].Trim()}|{campos[5].Trim()}|{campos[6].Trim()}";
            string respuestaCorrecta = campos[7].Trim();

            int orden = ObtenerSiguienteOrden(conn, actividadId);
            InsertarPregunta(conn, actividadId, orden, enunciado, opciones, respuestaCorrecta);
            count++;
        }

        ReinsertarRetosAlFinal(conn);
        Debug.Log($"[SeedPregunta] {count} preguntas cargadas");
    }

    private void GuardarRetosActuales(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT ActividadId, Texto, Recurso, Instrucciones FROM ContenidoActividad WHERE Tipo = 'Reto';";
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            string texto = reader.IsDBNull(1) ? "" : reader.GetString(1);
            string recurso = reader.IsDBNull(2) ? "" : reader.GetString(2);
            string instrucciones = reader.IsDBNull(3) ? "" : reader.GetString(3);
            retosPorActividad[id] = (texto, recurso, instrucciones);
        }
    }

    private void LimpiarPreguntasAnteriores(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ContenidoActividad WHERE Tipo = 'Pregunta';";
        cmd.ExecuteNonQuery();
    }

    private void ReinsertarRetosAlFinal(SqliteConnection conn)
    {
        foreach (var kvp in retosPorActividad)
        {
            int actividadId = kvp.Key;
            var (texto, recurso, instrucciones) = kvp.Value;

            var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM ContenidoActividad WHERE ActividadId = @id AND Tipo = 'Reto';";
            cmd.Parameters.AddWithValue("@id", actividadId);
            cmd.ExecuteNonQuery();

            int nuevoOrden = ObtenerSiguienteOrden(conn, actividadId);

            cmd = conn.CreateCommand();
            cmd.CommandText = @"
            INSERT INTO ContenidoActividad (ActividadId, Tipo, Orden, Texto, Recurso, Instrucciones)
            VALUES (@actividadId, 'Reto', @orden, @texto, @recurso, @instrucciones);
            ";
            cmd.Parameters.AddWithValue("@actividadId", actividadId);
            cmd.Parameters.AddWithValue("@orden", nuevoOrden);
            cmd.Parameters.AddWithValue("@texto", texto);
            cmd.Parameters.AddWithValue("@recurso", recurso);
            cmd.Parameters.AddWithValue("@instrucciones", instrucciones);
            cmd.ExecuteNonQuery();
        }
    }

    private int ObtenerSiguienteOrden(SqliteConnection conn, int actividadId)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT IFNULL(MAX(Orden), 0) + 1 FROM ContenidoActividad WHERE ActividadId = @actividadId";
        cmd.Parameters.AddWithValue("@actividadId", actividadId);
        return System.Convert.ToInt32(cmd.ExecuteScalar());
    }

    private void InsertarPregunta(SqliteConnection conn, int actividadId, int orden, string enunciado, string opciones, string respuestaCorrecta)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        INSERT OR REPLACE INTO ContenidoActividad (ActividadId, Tipo, Orden, Texto, Opciones, RespuestaCorrecta)
        VALUES (@actividadId, 'Pregunta', @orden, @texto, @opciones, @respuesta);
        ";
        cmd.Parameters.AddWithValue("@actividadId", actividadId);
        cmd.Parameters.AddWithValue("@orden", orden);
        cmd.Parameters.AddWithValue("@texto", enunciado);
        cmd.Parameters.AddWithValue("@opciones", opciones);
        cmd.Parameters.AddWithValue("@respuesta", respuestaCorrecta);
        cmd.ExecuteNonQuery();
    }
}
