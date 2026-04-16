namespace Dominio.entidades{
public class Nivel
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }

    public Nivel(int id, string nombre)
    {
        Id = id;
        Nombre = nombre;
    }
}
}