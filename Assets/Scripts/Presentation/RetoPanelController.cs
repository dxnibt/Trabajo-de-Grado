using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Dominio.entidades;

public class RetoPanelController : MonoBehaviour
{
    [Header("ELEMENTOS DEL PANEL")]
    public Image imagen1;
    public TMP_Text texto1;
    public Image imagen2;
    public TMP_Text texto2;

    [Header("BOTONES")]
    public Button botonSiguiente;
    public Button botonRetroceder;
    public Button botonFinalizar;

    private List<InstruccionPar> instruccionesPares;
    private int indiceActual = 0;
    
    // Modo individual para instrucciones impares
    private bool modoIndividual = false;

    void Start()
    {
        if (botonSiguiente != null)
            botonSiguiente.onClick.AddListener(MostrarSiguientePar);

        if (botonRetroceder != null)
            botonRetroceder.onClick.AddListener(MostrarAnteriorPar);
    }

    public void InicializarConReto(Reto reto)
    {
        instruccionesPares = reto.InstruccionesPares;
        indiceActual = 0;
        modoIndividual = true;

        if (instruccionesPares == null || instruccionesPares.Count == 0)
        {
            Debug.LogError($"[RetoPanelController] El reto no tiene instrucciones configuradas. InstruccionesPares count: {instruccionesPares?.Count ?? 0}");
            Debug.LogError($"[RetoPanelController] Reto.Instrucciones RAW: '{reto.Instrucciones}'");
            Debug.LogError($"[RetoPanelController] Reto.Instrucciones length: {reto.Instrucciones?.Length ?? 0}");
            OcultarInstrucciones();
            if (botonFinalizar != null)
                botonFinalizar.gameObject.SetActive(true);
            return;
        }

        Debug.Log($"[RetoPanelController] ✓ Reto iniciado con {instruccionesPares.Count} instrucciones");
        MostrarPar(0);
        ActualizarBotones();
    }

    private void MostrarPar(int indice)
    {
        if (indice < 0 || indice >= instruccionesPares.Count)
            return;

        indiceActual = indice;
        var par = instruccionesPares[indice];

        // Ocultar ambos slots inicialmente
        if (imagen1 != null) imagen1.gameObject.SetActive(false);
        if (imagen2 != null) imagen2.gameObject.SetActive(false);
        if (texto1 != null) texto1.gameObject.SetActive(false);
        if (texto2 != null) texto2.gameObject.SetActive(false);

        if (modoIndividual)
        {
            // Modo individual: mostrar solo la primera columna
            MostrarInstruccionIndividual(par.Imagen1, par.Texto1);
        }
        else
        {
            // Modo par original
            MostrarInstruccion(par.Imagen1, par.Texto1, imagen1, texto1);
            MostrarInstruccion(par.Imagen2, par.Texto2, imagen2, texto2);
        }

        ActualizarBotones();
    }

    private void MostrarInstruccionIndividual(string rutaImagen, string texto)
    {
        bool tieneImagen = !string.IsNullOrEmpty(rutaImagen);
        bool tieneTexto = !string.IsNullOrEmpty(texto);

        // Mostrar imagen en el slot 1
        if (imagen1 != null && tieneImagen)
        {
            Sprite sprite = Resources.Load<Sprite>(rutaImagen);
            if (sprite != null)
            {
                imagen1.sprite = sprite;
                imagen1.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Imagen no encontrada: {rutaImagen}");
            }
        }

        // Mostrar texto en el slot 1
        if (texto1 != null && tieneTexto)
        {
            texto1.text = texto;
            texto1.gameObject.SetActive(true);
        }
    }

    private void MostrarInstruccion(string rutaImagen, string texto, Image imagenTarget, TMP_Text textoTarget)
    {
        bool tieneImagen = !string.IsNullOrEmpty(rutaImagen);
        bool tieneTexto = !string.IsNullOrEmpty(texto);

        if (imagenTarget != null && tieneImagen)
        {
            Sprite sprite = Resources.Load<Sprite>(rutaImagen);
            if (sprite != null)
            {
                imagenTarget.sprite = sprite;
                imagenTarget.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"Imagen no encontrada: {rutaImagen}");
            }
        }

        if (textoTarget != null && tieneTexto)
        {
            textoTarget.text = texto;
            textoTarget.gameObject.SetActive(true);
        }
    }

    private void MostrarSiguientePar()
    {
        if (indiceActual < instruccionesPares.Count - 1)
            MostrarPar(indiceActual + 1);
    }

    private void MostrarAnteriorPar()
    {
        if (indiceActual > 0)
            MostrarPar(indiceActual - 1);
    }

    private void ActualizarBotones()
    {
        if (instruccionesPares == null || instruccionesPares.Count == 0)
            return;
            
        bool esElPrimero = indiceActual == 0;
        bool esElUltimo = indiceActual == instruccionesPares.Count - 1;

        if (botonRetroceder != null)
            botonRetroceder.interactable = !esElPrimero;

        if (botonSiguiente != null)
            botonSiguiente.interactable = !esElUltimo;

        if (botonFinalizar != null)
            botonFinalizar.gameObject.SetActive(esElUltimo);
    }

    public void OcultarPanel()
    {
        gameObject.SetActive(false);
    }

    public void OcultarInstrucciones()
    {
        if (imagen1 != null) imagen1.gameObject.SetActive(false);
        if (imagen2 != null) imagen2.gameObject.SetActive(false);
        if (texto1 != null) texto1.gameObject.SetActive(false);
        if (texto2 != null) texto2.gameObject.SetActive(false);
        if (botonSiguiente != null) botonSiguiente.gameObject.SetActive(false);
        if (botonRetroceder != null) botonRetroceder.gameObject.SetActive(false);
    }

    public bool TieneInstrucciones()
    {
        return instruccionesPares != null && instruccionesPares.Count > 0;
    }
}