using System;

namespace Dominio.entidades
{
 public class Progreso
{
    public int EstudianteId { get; set; }

    public Actividad ActividadActual { get; private set; }

    public int IndiceContenido { get; private set; }

    public bool PuedeAvanzar { get; private set; }

    public Progreso()
    {
        IndiceContenido = 0;
        PuedeAvanzar = false;
    }
    public void IniciarActividad(Actividad actividad)
    {
        ActividadActual = actividad;
        IndiceContenido = 0;
        PuedeAvanzar = false;
    }

    public ContenidoActividad ObtenerActual()
    {
        if (ActividadActual == null)
            throw new InvalidOperationException("No hay actividad iniciada.");

        return ActividadActual.ObtenerContenido(IndiceContenido);
    }

    public void MarcarRespuestaCorrecta()
    {
        PuedeAvanzar = true;
    }

    public bool Avanzar()
{
    if (ActividadActual == null) return false;

    var actual = ObtenerActual();

    // ✅ Si es pregunta, validar respuesta
    if (actual is Pregunta && !PuedeAvanzar)
        return false;

    // ✅ Historia y Reto avanzan sin restricción
    if (IndiceContenido < ActividadActual.TotalContenidos - 1)
    {
        IndiceContenido++;
        PuedeAvanzar = false;
        return true;
    }

    return false;
}

    public bool EstaFinalizado()
    {
        return ActividadActual != null &&
               IndiceContenido >= ActividadActual.TotalContenidos - 1;
    }

    public int ObtenerTotalActividades()
    {
    if (ActividadActual == null)
        return 0;

    return ActividadActual.TotalContenidos;
    }

    public void FinalizarActividadActual()
    {
    ActividadActual = null;
    IndiceContenido = 0;
    PuedeAvanzar = false;
    }   
}   
}

