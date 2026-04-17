using System.Collections.Generic;
using Dominio.entidades;


namespace Dominio.gateway
{

public interface RespuestaGateway
{
    void Guardar(Respuesta respuesta);
    List<Respuesta> ObtenerPorActividad(int actividadId);
}
}