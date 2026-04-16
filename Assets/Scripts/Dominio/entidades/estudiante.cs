namespace Dominio.entidades{

public class Estudiante
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public Nivel Nivel { get; set; }
    public Progreso Progreso { get; set; }

    public Estudiante(int id, string nombre, Nivel nivel)
    {
        Id = id;
        Nombre = nombre;
        Nivel = nivel;
        Progreso = new Progreso(); // ✔ ahora funciona
    }
}
}