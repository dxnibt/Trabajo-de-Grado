using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNivel1Controller : MonoBehaviour
{
    public void CargarActividad(int actividadId)
    {
        ActivityManager.ActividadActualId = actividadId;
        ActivityManager.NivelActualId = 1;
        ActivityManager.NivelNombre = "Nivel 1";
        ActivityManager.EscenaMenuNivel = "mp_nivel1";

        if (ActivityManager.ModoDocente)
        {
            ActivityManager.AbrirPDF(actividadId);
            return;
        }

        SceneManager.LoadScene($"n1_a{actividadId}");
    }

    public void VolverAlMenu()
    {
        string destino = ActivityManager.ModoDocente ? "mp_docente" : "mp_estudiante";
        ActivityManager.ModoDocente = false;
        SceneManager.LoadScene(destino);
    }
}
