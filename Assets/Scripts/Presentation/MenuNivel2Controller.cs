using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNivel2Controller : MonoBehaviour
{
    public void CargarActividad(int actividadId)
    {
        ActivityManager.ActividadActualId = actividadId;
        ActivityManager.NivelActualId = 2;
        ActivityManager.NivelNombre = "Nivel 2";
        ActivityManager.EscenaMenuNivel = "mp_nivel2";

        SceneManager.LoadScene($"n2_a{actividadId}");
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("mp_estudiante");
    }
}
