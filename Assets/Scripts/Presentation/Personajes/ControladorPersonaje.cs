using UnityEngine;
using System.Collections;

public class ControladorPersonaje : MonoBehaviour
{
    private Animator animator;

    [Header("Configuración de Escena")]
    // Ahora verás una casilla para arrastrar el archivo (el triángulo)
    public AnimationClip animacionParaEstaEscena;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (animacionParaEstaEscena != null)
        {
            // El código toma el nombre directamente del archivo que arrastraste
            StartCoroutine(ForzarAnimacion(animacionParaEstaEscena.name));
        }
    }

    IEnumerator ForzarAnimacion(string nombre)
    {
        yield return null; // Espera un frame para que el Animator esté listo
        animator.Play(nombre, 0, 0f);
    }
}