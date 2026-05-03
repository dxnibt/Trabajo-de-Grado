public static class ActivityManager
{
    public static int ActividadActualId { get; set; }
    public static int NivelActualId { get; set; }
    public static string NivelNombre { get; set; }
    public static string EscenaMenuNivel { get; set; }

    // Sesión activa del estudiante o grupo
    public static int EstudianteId { get; set; }
    public static string EstudianteNombre { get; set; }
    public static bool EsGrupo { get; set; }

    // Tipo de sesión confirmado para la actividad actual (se resetea al completar)
    public static bool TipoConfirmado { get; set; }

    // Modo docente: abre PDFs en lugar de cargar escenas de actividad
    public static bool ModoDocente { get; set; }

    public static void AbrirPDF(int actividadIdGlobal)
    {
        string carpeta = actividadIdGlobal <= 10 ? "Nivel1" : "Nivel2";
        string pdfPath = System.IO.Path.Combine(
            UnityEngine.Application.streamingAssetsPath,
            "PDFs", carpeta,
            $"actividad_{actividadIdGlobal}.pdf"
        );

        if (System.IO.File.Exists(pdfPath))
            System.Diagnostics.Process.Start(pdfPath);
        else
            UnityEngine.Debug.LogWarning($"[PDF] No encontrado: {pdfPath}");
    }
}
