namespace Infraestructura.SQLite.Models{

public class RespuestaData
{
    public int EstudianteId { get; set; }
    public int PreguntaId { get; set; }
    public string Valor { get; set; } = string.Empty;
    public bool EsCorrecta { get; set; }
}
}