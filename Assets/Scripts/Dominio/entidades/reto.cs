namespace Dominio.entidades{

public class Reto : ContenidoActividad
{
    public string Instrucciones { get; set; }
    public string ImagenPath { get; set; }

    public Reto(int orden, string instrucciones, string imagenPath)
    {
        Orden = orden;
        Instrucciones = instrucciones;
        ImagenPath = imagenPath;
    }
}
}