using System;
using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using Infraestructura.SQLite;

public class SeedActividad
{
    private readonly ConexionSQLite conexion;

    public SeedActividad(ConexionSQLite conexion)
    {
        this.conexion = conexion;
    }

    public void Ejecutar()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Data/actividades");

        if (csvFile == null)
        {
            Debug.LogError("[SeedActividad] ✗ No se encontró Resources/Data/actividades.csv");
            return;
        }

        Debug.Log($"[SeedActividad] CSV cargado. Tamaño: {csvFile.text.Length} caracteres");

        using var conn = conexion.CrearConexion();
        conn.Open();

        LimpiarActividades(conn);
        LimpiarContenidos(conn);

        var lineas = csvFile.text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        Debug.Log($"[SeedActividad] Total de líneas: {lineas.Length}");

        bool esEncabezado = true;

        // 🔥 agrupamos por actividad
        Dictionary<int, List<(string ruta, string instruccion)>> datos = new();
        int lineasProcesadas = 0;

        foreach (string linea in lineas)
        {
            string l = linea.Trim();
            if (string.IsNullOrEmpty(l)) continue;

            if (esEncabezado)
            {
                esEncabezado = false;
                Debug.Log($"[SeedActividad] Encabezado: {l}");
                continue;
            }

            var campos = l.Split(';');

            if (campos.Length < 3)
            {
                Debug.LogWarning($"[SeedActividad] Línea ignorada (menos de 3 campos): {l.Substring(0, Math.Min(50, l.Length))}");
                continue;
            }

            int actividadId = int.Parse(campos[0]);
            string ruta = campos[1].Trim();
            string instruccion = campos[2].Trim();

            if (!datos.ContainsKey(actividadId))
                datos[actividadId] = new List<(string, string)>();

            datos[actividadId].Add((ruta, instruccion));
            lineasProcesadas++;
        }

        Debug.Log($"[SeedActividad] Líneas procesadas: {lineasProcesadas}");
        Debug.Log($"[SeedActividad] Total de actividades a procesar: {datos.Count}");

        // 🔥 insertar actividades y retos
        foreach (var kvp in datos)
        {
            int actividadId = kvp.Key;
            var lista = kvp.Value;

            InsertarActividad(conn, actividadId);

            string instruccionesFormateadas = "";

            for (int i = 0; i < lista.Count; i++)
            {
                var item = lista[i];

                // formato que tu parser espera
                instruccionesFormateadas += $"{item.ruta}||{item.instruccion}";

                if (i < lista.Count - 1)
                    instruccionesFormateadas += "//";
            }

            Debug.Log($"[SeedActividad] Instrucciones formateadas para actividad {actividadId}: {instruccionesFormateadas.Length} caracteres");
            Debug.Log($"[SeedActividad] Preview: {instruccionesFormateadas.Substring(0, Math.Min(100, instruccionesFormateadas.Length))}...");

            InsertarReto(conn, actividadId, instruccionesFormateadas);

            Debug.Log($"[SeedActividad] ✓ Actividad {actividadId} con {lista.Count} instrucciones");
        }
    }

    private void LimpiarActividades(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Actividad;";
        cmd.ExecuteNonQuery();
    }

    private void LimpiarContenidos(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ContenidoActividad;";
        cmd.ExecuteNonQuery();
    }

    private void InsertarActividad(SqliteConnection conn, int actividadId)
    {
        int nivelId = actividadId <= 10 ? 1 : 2;

        var cmd = conn.CreateCommand();

        cmd.CommandText = @"
        INSERT INTO Actividad (Id, Titulo, NivelId)
        VALUES (@id, @titulo, @nivelId);
        ";

        cmd.Parameters.AddWithValue("@id", actividadId);
        cmd.Parameters.AddWithValue("@titulo", $"Actividad {actividadId}");
        cmd.Parameters.AddWithValue("@nivelId", nivelId);

        cmd.ExecuteNonQuery();
    }

    private void InsertarReto(SqliteConnection conn, int actividadId, string instrucciones)
    {
        var cmd = conn.CreateCommand();

        cmd.CommandText = @"
        INSERT INTO ContenidoActividad
        (ActividadId, Tipo, Orden, Texto, Recurso, Opciones, RespuestaCorrecta, Instrucciones)
        VALUES (@actividadId, 'Reto', 1, 'Realiza el reto', NULL, NULL, NULL, @instrucciones);
        ";

        cmd.Parameters.AddWithValue("@actividadId", actividadId);
        cmd.Parameters.AddWithValue("@instrucciones", instrucciones);

        cmd.ExecuteNonQuery();
    }
}