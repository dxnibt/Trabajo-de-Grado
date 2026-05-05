using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class menuPrincipalController : MonoBehaviour
{
    public void cambiarEscena(string nombreEscena){
        if (ActivityManager.ModoDocente) return;
        SceneManager.LoadScene(nombreEscena);
    }
}