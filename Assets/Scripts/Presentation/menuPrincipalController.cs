using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class menuPrincipalController : MonoBehaviour
{
    public void cambiarEscena(string nombreEscena){
        if (ActivityManager.ModoDocente) {
            // Bloquear escenas de actividad; para menús (mp_*) redirigir al docente
            if (!nombreEscena.StartsWith("mp_")) return;
            SceneManager.LoadScene("mp_docente");
            return;
        }
        SceneManager.LoadScene(nombreEscena);
    }
}