using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Mono.Data.Sqlite;
using Infraestructura.SQLite;

public class SeedActividad
{
    private readonly ConexionSQLite conexion;

    // Rutas de audio en Resources/Audios/ para cada actividad
    private static readonly Dictionary<int, string> AudioPorActividad = new()
    {
        { 1,  "Audios/sonido" },
        { 2,  "Audios/circo" },
        { 3,  "Audios/catapulta" },
        { 4,  "Audios/atrapapelota" },
    };

    public SeedActividad(ConexionSQLite conexion)
    {
        this.conexion = conexion;
    }

    public void Ejecutar()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Data/actividades");

        if (csvFile == null)
        {
            Debug.LogError("[SeedActividad] No se encontró Resources/Data/actividades.csv");
            return;
        }

        using var conn = conexion.CrearConexion();
        conn.Open();

        LimpiarTablas(conn);

        var lineas = csvFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        bool esEncabezado = true;
        var datos = new Dictionary<int, List<(string ruta, string instruccion)>>();

        foreach (string linea in lineas)
        {
            string l = linea.Trim();
            if (string.IsNullOrEmpty(l)) continue;

            if (esEncabezado) { esEncabezado = false; continue; }

            var campos = l.Split(';');
            if (campos.Length < 3) continue;

            if (!int.TryParse(campos[0], out int actividadId)) continue;

            string ruta = campos[1].Trim();
            string instruccion = campos[2].Trim();

            if (!datos.ContainsKey(actividadId))
                datos[actividadId] = new List<(string, string)>();

            datos[actividadId].Add((ruta, instruccion));
        }

        foreach (var kvp in datos)
        {
            int actividadId = kvp.Key;
            var lista = kvp.Value;

            InsertarActividad(conn, actividadId);
            InsertarHistoria(conn, actividadId);

            var sb = new StringBuilder();
            for (int i = 0; i < lista.Count; i++)
            {
                if (i > 0) sb.Append("//");
                sb.Append(lista[i].ruta).Append("||").Append(lista[i].instruccion);
            }

            InsertarReto(conn, actividadId, sb.ToString());
        }

        Debug.Log($"[SeedActividad] {datos.Count} actividades sembradas");
    }

    private void LimpiarTablas(SqliteConnection conn)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM ContenidoActividad;";
        cmd.ExecuteNonQuery();
        cmd.CommandText = "DELETE FROM Actividad;";
        cmd.ExecuteNonQuery();
    }

    private void InsertarActividad(SqliteConnection conn, int actividadId)
    {
        int nivelId = actividadId <= 10 ? 1 : 2;
        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Actividad (Id, Titulo, NivelId) VALUES (@id, @titulo, @nivelId);";
        cmd.Parameters.AddWithValue("@id", actividadId);
        cmd.Parameters.AddWithValue("@titulo", $"Actividad {actividadId}");
        cmd.Parameters.AddWithValue("@nivelId", nivelId);
        cmd.ExecuteNonQuery();
    }

    private void InsertarHistoria(SqliteConnection conn, int actividadId)
    {
        AudioPorActividad.TryGetValue(actividadId, out string recurso);
        recurso ??= "";

        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO ContenidoActividad (ActividadId, Tipo, Orden, Recurso)
        VALUES (@actividadId, 'Historia', 0, @recurso);
        ";
        cmd.Parameters.AddWithValue("@actividadId", actividadId);
        cmd.Parameters.AddWithValue("@recurso", recurso);
        cmd.ExecuteNonQuery();
    }

    private void InsertarReto(SqliteConnection conn, int actividadId, string instrucciones)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO ContenidoActividad (ActividadId, Tipo, Orden, Texto, Instrucciones)
        VALUES (@actividadId, 'Reto', 1, 'Realiza el reto', @instrucciones);
        ";
        cmd.Parameters.AddWithValue("@actividadId", actividadId);
        cmd.Parameters.AddWithValue("@instrucciones", instrucciones);
        cmd.ExecuteNonQuery();
    }
}
