using Dominio.entidades;

namespace Dominio.gateway
{

public interface ProgresoGateway
{
    void Guardar(Progreso progreso);
    Progreso ObtenerPorEstudiante(int estudianteId);

}
}