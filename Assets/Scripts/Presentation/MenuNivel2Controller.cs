using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNivel2Controller : MonoBehaviour
{
    public void CargarActividad(int actividadId)
    {
        int globalId = actividadId + 10; // IDs globales 11-20 en la BD
        ActivityManager.ActividadActualId = globalId;
        ActivityManager.NivelActualId = 2;
        ActivityManager.NivelNombre = "Nivel 2";
        ActivityManager.EscenaMenuNivel = "mp_nivel2";

        if (ActivityManager.ModoDocente)
        {
            ActivityManager.AbrirPDF(globalId);
            return;
        }

        SceneManager.LoadScene($"n2_a{actividadId}");
    }

    public void VolverAlMenu()
    {
        string destino = ActivityManager.ModoDocente ? "mp_docente" : "mp_estudiante";
        ActivityManager.ModoDocente = false;
        SceneManager.LoadScene(destino);
    }
}
