using System.Collections.Generic;
using Infraestructura.SQLite;
using Mono.Data.Sqlite;
using UnityEngine;

public class SeedActividad
{
    private readonly ConexionSQLite conexion;
    private Dictionary<int, List<string>> instruccionesPorActividad;

    public SeedActividad(ConexionSQLite conexion)
    {
        this.conexion = conexion;
    }

    public void Ejecutar()
    {
        instruccionesPorActividad = CargarTodasLasInstrucciones();

        using var conn = conexion.CrearConexion();
        conn.Open();

        // ── Actividades con Historia + Preguntas + Reto propios ──────────────
        // Estas deben llamarse ANTES del bucle para que no sean sobreescritas.
        // Para agregar una nueva actividad completa:
        //   1. Crea SeedActividad{N}(conn) igual que SeedActividad1 con sus datos
        //   2. Agrega el id a 'conSeedCompleto'
        //   3. Agrega el audio en Assets/Resources/Audios/historia{N}.mp3
        //   4. Agrega las imágenes del reto siguiendo la ruta del CSV

        SeedActividad1(conn);
        var conSeedCompleto = new System.Collections.Generic.HashSet<int> { 1 };

        // ── Resto: Historia automática + Reto desde CSV ──────────────────────
        // Audio esperado: Assets/Resources/Audios/historia{actividadId}.mp3
        foreach (int id in instruccionesPorActividad.Keys)
        {
            if (conSeedCompleto.Contains(id)) continue;
            SeedearReto(conn, id);
        }
    }

    // ── Actividades con contenido completo ───────────────────────────────────

    private void SeedActividad1(SqliteConnection conn)
    {
        LimpiarActividad(conn, 1);

        var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Actividad (Id, Titulo, NivelId) VALUES (1, 'El Sonido', 1);";
        cmd.ExecuteNonQuery();

        cmd = conn.CreateCommand();
        cmd.CommandText = @"
        INSERT INTO ContenidoActividad
        (ActividadId, Tipo, Orden, Texto, Recurso, Opciones, RespuestaCorrecta, Instrucciones)
        VALUES
        (1, 'Historia', 0, '', 'Audios/historia1', NULL, NULL, NULL),
        (1, 'Pregunta', 1,
        'Cual fue la causa principal de la tristeza en Pueblo Sonoro al inicio de la historia?',
        NULL,
        'Se prohibió el gran festival de música reciclada|Los pájaros dejaron de silbar sus melodías|Los instrumentos musicales desaparecieron por completo|Doña vibración decidió que no habrían más fiestas',
        'Los instrumentos musicales desaparecieron por completo', NULL),
        (1, 'Pregunta', 2,
        'Segun la profesora Tono-fonia, que es lo que ocurre cuando algo choca o se mueve rapidamente?',
        NULL,
        'Se produce una vibración|El objeto cambia de color|El sonido desaparece instantáneamente|Se crea un vacío en el aire',
        'Se produce una vibración', NULL),
        (1, 'Pregunta', 3,
        'Como viaja el sonido por el aire para llegar a nuestros oidos?',
        NULL,
        'A través de hilos musicales ocultos|Únicamente a través de tubos de cartón|Como una línea recta de luz|Como una ola invisible',
        'Como una ola invisible', NULL);
        ";
        cmd.ExecuteNonQuery();

        InsertarReto(conn, actividadId: 1, orden: 4, texto: "Construye una maraca", recurso: "Imagenes/reto1");
    }

    // ── Seed del Reto desde el CSV para actividades sin contenido completo ───

    private void SeedearReto(SqliteConnection conn, int actividadId)
    {
        int nivelId = ObtenerNivelId(actividadId);

        var cmd = conn.CreateCommand();
        cmd.CommandText = $"INSERT OR REPLACE INTO Actividad (Id, Titulo, NivelId) VALUES ({actividadId}, '', {nivelId});";
        cmd.ExecuteNonQuery();

        // Reemplazar Historia y Reto (no toca Preguntas si se añaden después)
        cmd = conn.CreateCommand();
        cmd.CommandText = $"DELETE FROM ContenidoActividad WHERE ActividadId = {actividadId} AND Tipo IN ('Historia', 'Reto');";
        cmd.ExecuteNonQuery();

        // Historia: agrega Assets/Resources/Audios/historia{actividadId}.mp3 para que suene
        cmd = conn.CreateCommand();
        cmd.CommandText = $@"INSERT INTO ContenidoActividad
            (ActividadId, Tipo, Orden, Texto, Recurso, Opciones, RespuestaCorrecta, Instrucciones)
            VALUES ({actividadId}, 'Historia', 0, '', 'Audios/historia{actividadId}', NULL, NULL, NULL);";
        cmd.ExecuteNonQuery();

        // Reto al final (orden 1 si no hay preguntas; si luego se agregan, usar SeedActividad{N})
        InsertarReto(conn, actividadId, orden: 1, texto: "", recurso: "");
    }

    // ── Helpers reutilizables ─────────────────────────────────────────────────

    private void LimpiarActividad(SqliteConnection conn, int actividadId)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = $"DELETE FROM ContenidoActividad WHERE ActividadId = {actividadId};";
        cmd.ExecuteNonQuery();

        cmd = conn.CreateCommand();
        cmd.CommandText = $"DELETE FROM Actividad WHERE Id = {actividadId};";
        cmd.ExecuteNonQuery();
    }

    private void InsertarReto(SqliteConnection conn, int actividadId, int orden, string texto, string recurso)
    {
        string instrucciones = ObtenerInstrucciones(actividadId);
        var cmd = conn.CreateCommand();
        cmd.CommandText = $@"
        INSERT INTO ContenidoActividad
        (ActividadId, Tipo, Orden, Texto, Recurso, Opciones, RespuestaCorrecta, Instrucciones)
        VALUES ({actividadId}, 'Reto', {orden}, '{Escapar(texto)}', '{Escapar(recurso)}', NULL, NULL, '{instrucciones}');
        ";
        cmd.ExecuteNonQuery();
    }

    private int ObtenerNivelId(int actividadId)
    {
        if (!instruccionesPorActividad.TryGetValue(actividadId, out var pasos) || pasos.Count == 0)
            return 1;

        // Derivar nivel desde la ruta de imagen: "Imagenes/nivel2/..." → 2
        string primeraRuta = pasos[0].Split(new[] { "||" }, System.StringSplitOptions.None)[0];
        for (int n = 9; n >= 2; n--)
            if (primeraRuta.Contains($"nivel{n}")) return n;
        return 1;
    }

    // ── Carga y construcción de instrucciones desde el CSV ────────────────────

    private Dictionary<int, List<string>> CargarTodasLasInstrucciones()
    {
        var resultado = new Dictionary<int, List<string>>();
        TextAsset csvFile = Resources.Load<TextAsset>("Data/actividades");

        if (csvFile == null)
        {
            Debug.LogWarning("[SeedActividad] No se encontró Resources/Data/actividades.csv — el Reto usará instrucciones vacías.");
            return resultado;
        }

        foreach (string linea in csvFile.text.Split('\n'))
        {
            string l = linea.Trim();
            if (string.IsNullOrEmpty(l)) continue;

            int sep1 = l.IndexOf(';');
            int sep2 = sep1 >= 0 ? l.IndexOf(';', sep1 + 1) : -1;
            if (sep1 < 0 || sep2 < 0) continue;
            if (!int.TryParse(l.Substring(0, sep1), out int id)) continue;

            string ruta  = l.Substring(sep1 + 1, sep2 - sep1 - 1).Trim();
            string texto = l.Substring(sep2 + 1).Trim();

            if (!resultado.ContainsKey(id))
                resultado[id] = new List<string>();
            resultado[id].Add($"{ruta}||{texto}");
        }

        return resultado;
    }

    private string ObtenerInstrucciones(int actividadId)
    {
        if (!instruccionesPorActividad.TryGetValue(actividadId, out var pasos) || pasos.Count == 0)
            return string.Empty;

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < pasos.Count; i++)
        {
            sb.Append(pasos[i]);
            if (i < pasos.Count - 1)
                sb.Append(i % 2 == 0 ? "||" : "//");
        }
        return Escapar(sb.ToString());
    }

    private static string Escapar(string valor) => valor.Replace("'", "''");
}
