using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuNivelTTJController : MonoBehaviour
{
    private readonly Dictionary<int, (int nivel, int actividadId)> actividadesTTJ = new Dictionary<int, (int, int)>
    {
        { 1, (2, 1) },
        { 2, (2, 7) },
        { 3, (2, 2) },
        { 4, (1, 9) },
        { 5, (2, 5) }
    };

    public void CargarActividad(int ttjActividadId)
    {
        if (!actividadesTTJ.ContainsKey(ttjActividadId))
        {
            Debug.LogError($"Actividad TTJ {ttjActividadId} no existe en el mapeo");
            return;
        }

        var (nivel, actividadId) = actividadesTTJ[ttjActividadId];
        int globalId = nivel == 1 ? actividadId : actividadId + 10;

        ActivityManager.ActividadActualId = globalId;
        ActivityManager.NivelActualId = nivel;
        ActivityManager.NivelNombre = "Trabajemos Todos Juntos";
        ActivityManager.EscenaMenuNivel = "mp_ttj";

        if (ActivityManager.ModoDocente)
        {
            ActivityManager.AbrirPDF(globalId);
            return;
        }

        string escenaName = nivel == 1 ? $"n1_a{actividadId}" : $"n2_a{actividadId}";
        SceneManager.LoadScene(escenaName);
    }

    public void VolverAlMenu()
    {
        string destino = ActivityManager.ModoDocente ? "mp_docente" : "mp_estudiante";
        ActivityManager.ModoDocente = false;
        SceneManager.LoadScene(destino);
    }
}
