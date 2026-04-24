using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuNivelTTJController : MonoBehaviour
{
    public void CargarActividad(int actividadId)
    {
        ActivityManager.ActividadActualId = actividadId;
        ActivityManager.NivelActualId = 3;
        ActivityManager.NivelNombre = "Trabajemos Todos Juntos";
        ActivityManager.EscenaMenuNivel = "mp_ttj";

        SceneManager.LoadScene($"n3_a{actividadId}");
    }

    public void VolverAlMenu()
    {
        SceneManager.LoadScene("mp_estudiante");
    }
}
