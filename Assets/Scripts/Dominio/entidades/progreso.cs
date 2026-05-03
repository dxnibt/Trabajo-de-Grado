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
    if (ActividadActual == null)
    {
        UnityEngine.Debug.LogWarning("[Progreso] ✗ No hay actividad iniciada");
        return false;
    }

    var actual = ObtenerActual();
    UnityEngine.Debug.Log($"[Progreso] Intento de avanzar desde índice {IndiceContenido} ({actual.GetType().Name})");
    UnityEngine.Debug.Log($"[Progreso] Total contenidos: {ActividadActual.TotalContenidos}");

    if (actual is Pregunta && !PuedeAvanzar)
    {
        UnityEngine.Debug.Log("[Progreso] ✗ Es pregunta y no ha respondido correctamente");
        return false;
    }

    if (IndiceContenido < ActividadActual.TotalContenidos - 1)
    {
        IndiceContenido++;
        PuedeAvanzar = false;
        UnityEngine.Debug.Log($"[Progreso] ✓ Avanzó a índice {IndiceContenido}");
        return true;
    }

    UnityEngine.Debug.LogWarning($"[Progreso] ✗ Ya está en el último contenido (índice {IndiceContenido})");
    return false;
}

    public bool Retroceder()
    {
        if (ActividadActual == null) return false;

        if (IndiceContenido > 0)
        {
            IndiceContenido--;
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
        if (ActividadActual == null) return 0;
        return ActividadActual.TotalContenidos;
    }

    public void RestaurarIndice(int indice)
    {
        if (ActividadActual == null) return;
        IndiceContenido = System.Math.Min(indice, ActividadActual.TotalContenidos - 1);
        PuedeAvanzar = false;
    }

    public void FinalizarActividadActual()
    {
        ActividadActual = null;
        IndiceContenido = 0;
        PuedeAvanzar = false;
    }
}
}

