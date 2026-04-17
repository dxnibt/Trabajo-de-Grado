namespace Infraestructura.SQLite.Models
{

public class ProgresoData
{
    public int EstudianteId { get; set; }
    public int ActividadId { get; set; }
    public bool Completada { get; set; }
    public int IndiceContenidoActual { get; set; }
}
}