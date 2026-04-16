using System.Collections.Generic;
namespace Dominio.entidades{

public class Docente
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }

    public Docente(int id, string nombre)
    {
        Id = id;
        Nombre = nombre;
    }

    public int ConsultarProgresoEstudiante(Estudiante estudiante)
    {
        return estudiante.Progreso.ObtenerTotalActividades();
    }
}
}