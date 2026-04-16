using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class menuPrincipalController : MonoBehaviour
{
    public void cambiarEscena(string nombreEscena){
        
        SceneManager.LoadScene(nombreEscena);

    }
}