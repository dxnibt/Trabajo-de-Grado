namespace Dominio.entidades{

public class Historia : ContenidoActividad
{
    public string Recurso { get; set; }

    public Historia(int orden, string recurso)
    {
        Orden = orden;
        Recurso = recurso;
    }
}
}