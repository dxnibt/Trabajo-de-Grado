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
}
