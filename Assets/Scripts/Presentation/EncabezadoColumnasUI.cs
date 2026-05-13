using UnityEngine;
using TMPro;

public class EncabezadoColumnasUI : MonoBehaviour
{
    public TMP_Text encabezadoNombre;
    public TMP_Text encabezadoTipo;
    public TMP_Text encabezadoIntentos;

    public void Configurar()
    {
        if (encabezadoNombre != null) encabezadoNombre.text = "Nombre";
        if (encabezadoTipo != null) encabezadoTipo.text = "Modo de trabajo";
        if (encabezadoIntentos != null) encabezadoIntentos.text = "Respuestas";
    }
}
