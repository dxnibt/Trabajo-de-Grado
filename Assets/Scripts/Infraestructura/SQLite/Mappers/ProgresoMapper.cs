using System.Collections.Generic;
using Dominio.entidades;
using Infraestructura.SQLite.Models;

namespace Infraestructura.SQLite.Mappers{

public static class ProgresoMapper
{
    public static Progreso ToDomain(ProgresoData data)
    {
        var nivel = new Nivel(1, "Nivel 1");
        var estudiante = new Estudiante(data.EstudianteId, "Nombre", nivel);

        var progreso = estudiante.Progreso;
        progreso.EstudianteId = data.EstudianteId;

        if (!data.Completada && data.ActividadId != 0)
        {
            var actividad = new Actividad(
                data.ActividadId,
                "Actividad",
                nivel,
                new List<ContenidoActividad>()
            );

            progreso.IniciarActividad(actividad);

            for (int i = 0; i < data.IndiceContenidoActual; i++)
                progreso.Avanzar();
        }

        return progreso;
    }

    public static ProgresoData ToData(Progreso progreso, int estudianteId)
    {
        return new ProgresoData
        {
            EstudianteId = estudianteId,
            ActividadId = progreso.ActividadActual?.Id ?? 0,
            Completada = progreso.ActividadActual == null,
            IndiceContenidoActual = progreso.IndiceContenido
        };
    }
}
}