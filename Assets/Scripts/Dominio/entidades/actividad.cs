using System.Collections.Generic;
using System.Linq;


namespace Dominio.entidades
{
public class Actividad
{
    public int Id { get; private set; }
    public string Titulo { get; private set; }
    public Nivel Nivel { get; private set; }
    public List<ContenidoActividad> Contenidos { get; private set; }

    public Actividad(int id, string titulo, Nivel nivel, List<ContenidoActividad> contenidos)
    {
        Id = id;
        Titulo = titulo;
        Nivel = nivel;
        Contenidos = contenidos.OrderBy(c => c.Orden).ToList();
    }

    public ContenidoActividad ObtenerContenido(int indice)
    {
        return Contenidos[indice];
    }

    public int TotalContenidos => Contenidos.Count;
}
}