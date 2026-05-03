using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EntradaEstudianteUI : MonoBehaviour
{
    public TMP_Text textoNombre;
    public TMP_Text textoTipo;
    public TMP_Text textoProgreso;
    public Slider sliderProgreso;

    public void Configurar(string nombre, bool esGrupo, int completadas, int total, float porcentaje)
    {
        if (textoNombre != null) textoNombre.text = nombre;
        if (textoTipo != null) textoTipo.text = esGrupo ? "Grupo" : "Individual";
        if (textoProgreso != null) textoProgreso.text = $"{completadas}/{total}";
        if (sliderProgreso != null)
        {
            sliderProgreso.interactable = false;
            sliderProgreso.minValue = 0f;
            sliderProgreso.maxValue = 1f;
            sliderProgreso.value = porcentaje;
        }
    }
}
