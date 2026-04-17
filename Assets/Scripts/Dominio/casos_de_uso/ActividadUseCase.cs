using Dominio.entidades;
using System;

namespace Dominio.casos_de_uso
{

public class GestionarActividadUseCase
{
    public void IniciarActividad(Estudiante estudiante, Actividad nuevaActividad)
    {
        if (estudiante == null)
            throw new ArgumentNullException(nameof(estudiante));

        if (nuevaActividad == null)
            throw new ArgumentNullException(nameof(nuevaActividad));

        var progreso = estudiante.Progreso;

        if (progreso.ActividadActual != null)
        {
            throw new InvalidOperationException("Ya existe una actividad en curso.");
        }

        progreso.IniciarActividad(nuevaActividad);
    }

    public void FinalizarActividad(Estudiante estudiante)
    {
        var progreso = estudiante.Progreso;

        if (progreso.ActividadActual == null)
        {
            throw new InvalidOperationException("No hay actividad en curso.");
        }

        progreso.FinalizarActividadActual();
    }

    public void RetrocederActividad(Estudiante estudiante, Actividad actividadAnterior)
    {
        var progreso = estudiante.Progreso;

        if (actividadAnterior == null)
        {
            throw new ArgumentNullException(nameof(actividadAnterior));
        }

        progreso.IniciarActividad(actividadAnterior);
    }
}
}