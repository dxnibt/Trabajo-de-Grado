using UnityEngine;
using UnityEngine.SceneManagement;

public class menuPrincipalController : MonoBehaviour
{
    // Variable para rastrear qué panel está visible actualmente
    private GameObject panelActual;

    public void cambiarEscena(string nombreEscena)
    {
        if (ActivityManager.ModoDocente) 
        {
            if (!nombreEscena.StartsWith("mp_")) return;
            SceneManager.LoadScene("mp_docente");
            return;
        }
        SceneManager.LoadScene(nombreEscena);
    }

    // Nueva función para navegar entre paneles sin superposición
    public void mostrarPanel(GameObject panelDestino)
    {
        if (panelDestino == null) return;

        // 1. Si hay un panel abierto anteriormente, lo ocultamos
        if (panelActual != null)
        {
            panelActual.SetActive(false);
        }

        // 2. Activamos el nuevo panel
        panelDestino.SetActive(true);

        // 3. Actualizamos la referencia para la siguiente vez
        panelActual = panelDestino;
    }

    // Función para cerrar el panel actual y volver al "vacío" o menú base
    public void cerrarPanelActual()
    {
        if (panelActual != null)
        {
            panelActual.SetActive(false);
            panelActual = null;
        }
    }
}