namespace Dominio.entidades{

public class Historia : ContenidoActividad
{
    public string AudioPath { get; set; }

    public Historia(int orden, string audioPath)
    {
        Orden = orden;
        AudioPath = audioPath;
    }
}
}