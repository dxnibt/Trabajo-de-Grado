using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Dominio.entidades;
using System.Collections.Generic;

public class PreguntaPanelController : MonoBehaviour
{
    [Header("ELEMENTOS DEL PANEL")]
    public TMP_Text textoEnunciado;
    public Transform contenedorOpciones;
    public Button botonSiguiente;
    public Button botonRetroceder;
    public Button botonFinalizar;

    [Header("PREFAB")]
    public Button prefabBotonOpcion;

    private List<Pregunta> preguntas;
    private int indiceActual = 0;
    private string respuestaSeleccionada;

    void Start()
    {
        if (botonSiguiente != null)
            botonSiguiente.onClick.AddListener(MostrarSiguiente);

        if (botonRetroceder != null)
            botonRetroceder.onClick.AddListener(MostrarAnterior);
    }

    public void InicializarConPreguntas(List<Pregunta> preguntasActividad)
    {
        preguntas = preguntasActividad;
        indiceActual = 0;

        if (preguntas == null || preguntas.Count == 0)
        {
            Debug.LogWarning("No hay preguntas para mostrar");
            OcultarPanel();
            return;
        }

        Debug.Log($"[PreguntaPanelController] Inicializando con {preguntas.Count} preguntas");
        MostrarPregunta(0);
        ActualizarBotones();
    }

    private void MostrarPregunta(int indice)
    {
        if (indice < 0 || indice >= preguntas.Count)
            return;

        indiceActual = indice;
        var pregunta = preguntas[indice];
        respuestaSeleccionada = null;

        MostrarEnunciado(pregunta.Enunciado);
        LimpiarOpciones();
        CrearBotonesOpciones(pregunta.Opciones);

        ActualizarBotones();
    }

    private void MostrarEnunciado(string enunciado)
    {
        if (textoEnunciado != null)
            textoEnunciado.text = enunciado;
    }

    private void CrearBotonesOpciones(List<string> opciones)
    {
        if (contenedorOpciones == null || opciones == null)
            return;

        for (int i = 0; i < opciones.Count; i++)
        {
            var boton = Instantiate(prefabBotonOpcion, contenedorOpciones);
            var textoBoton = boton.GetComponentInChildren<TMP_Text>();
            if (textoBoton != null)
                textoBoton.text = opciones[i];

            string opcion = opciones[i];
            boton.onClick.AddListener(() => SeleccionarOpcion(opcion));
        }
    }

    private void LimpiarOpciones()
    {
        if (contenedorOpciones == null)
            return;

        foreach (Transform hijo in contenedorOpciones)
            Destroy(hijo.gameObject);
    }

    private void SeleccionarOpcion(string opcion)
    {
        respuestaSeleccionada = opcion;
        Debug.Log($"[PreguntaPanelController] Respuesta seleccionada: {opcion}");
    }

    private void MostrarSiguiente()
    {
        if (indiceActual < preguntas.Count - 1)
        {
            ValidarRespuestaActual();
            MostrarPregunta(indiceActual + 1);
        }
    }

    private void MostrarAnterior()
    {
        if (indiceActual > 0)
            MostrarPregunta(indiceActual - 1);
    }

    private void ActualizarBotones()
    {
        bool esElPrimero = indiceActual == 0;
        bool esElUltimo = indiceActual == preguntas.Count - 1;

        if (botonRetroceder != null)
            botonRetroceder.interactable = !esElPrimero;

        if (botonSiguiente != null)
            botonSiguiente.interactable = !esElUltimo;

        if (botonFinalizar != null)
            botonFinalizar.gameObject.SetActive(esElUltimo);
    }

    private void ValidarRespuestaActual()
    {
        if (respuestaSeleccionada == null)
        {
            Debug.LogWarning("No se seleccionó respuesta");
            return;
        }

        var pregunta = preguntas[indiceActual];
        bool esCorrecta = pregunta.ValidarRespuesta(respuestaSeleccionada);

        Debug.Log($"[PreguntaPanelController] Pregunta {indiceActual + 1}: {(esCorrecta ? "✓ Correcta" : "✗ Incorrecta")}");
    }

    public void OcultarPanel()
    {
        gameObject.SetActive(false);
    }
}
