using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MenuNivelTTJController : MonoBehaviour
{
    // Mapeo de actividades TTJ a sus escenas reales (nivel + id)
    private readonly Dictionary<int, (int nivel, int actividadId)> actividadesTTJ = new Dictionary<int, (int, int)>
    {
        { 1, (2, 1) },    // TTJ actividad 1 → Nivel 2, actividad 1
        { 2, (2, 7) },    // TTJ actividad 2 → Nivel 2, actividad 7
        { 3, (2, 2) },    // TTJ actividad 3 → Nivel 2, actividad 2
        { 4, (1, 9) },    // TTJ actividad 4 → Nivel 1, actividad 9
        { 5, (2, 5) }     // TTJ actividad 5 → Nivel 2, actividad 5
    };

    public void CargarActividad(int ttjActividadId)
    {
        if (!actividadesTTJ.ContainsKey(ttjActividadId))
        {
            Debug.LogError($"Actividad TTJ {ttjActividadId} no existe en el mapeo");
            return;
        }

        var (nivel, actividadId) = actividadesTTJ[ttjActividadId];

        ActivityManager.ActividadActualId = nivel == 1 ? actividadId : actividadId + 10;
        ActivityManager.NivelActualId = nivel;
        ActivityManager.NivelNombre = "Trabajemos Todos Juntos";
        ActivityManager.EscenaMenuNivel = "mp_ttj";

        string escenaName = nivel == 1 ? $"n1_a{actividadId}" : $"n2_a{actividadId}";
        SceneManager.LoadScene(escenaName);
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("mp_estudiante");
    }
}
