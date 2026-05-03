using UnityEngine;
using TMPro;

public class EntradaEstudianteUI : MonoBehaviour
{
    public TMP_Text textoNombre;
    public TMP_Text textoTipo;
    public TMP_Text textoIntentos;

    public void Configurar(string nombre, bool esGrupo, int totalIntentos, int incorrectos)
    {
        if (textoNombre != null) textoNombre.text = nombre;
        if (textoTipo != null) textoTipo.text = esGrupo ? "Grupo" : "Individual";
        if (textoIntentos != null)
            textoIntentos.text = $"{incorrectos} incorrectos / {totalIntentos} intentos";
    }
}
