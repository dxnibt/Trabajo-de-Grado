using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class menuPrincipalController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] public Button boton_estudiante;
    [SerializeField] public Button boton_docente;
    [SerializeField] public Texture2D cursorManita;

    //private Cursor.LockState estadoCursorOriginal; 

    private void Start()
    {
        boton_estudiante.onClick.AddListener(() => CambiarEscena("mp_estudiante"));
        boton_docente.onClick.AddListener(() => CambiarEscena("mp_docente"));

        EventTrigger triggerEstudiante = boton_estudiante.gameObject.AddComponent<EventTrigger>();
        EventTrigger triggerDocente = boton_docente.gameObject.AddComponent<EventTrigger>();

        AgregarEventTrigger(triggerEstudiante, EventTriggerType.PointerEnter, CambiarCursorManita);
        AgregarEventTrigger(triggerEstudiante, EventTriggerType.PointerExit, RestaurarCursor);

        AgregarEventTrigger(triggerDocente, EventTriggerType.PointerEnter, CambiarCursorManita);
        AgregarEventTrigger(triggerDocente, EventTriggerType.PointerExit, RestaurarCursor);       

    }

    private void CambiarEscena(string nombreEscena)
    {
        SceneManager.LoadScene(nombreEscena);
    }

    private void CambiarCursorManita(BaseEventData data)
    {
        if (cursorManita != null)
        {
            Cursor.SetCursor(cursorManita, Vector2.zero, CursorMode.Auto);
        }
        else 
        {
            Debug.LogWarning("No se asigno textura para el cursor manita");
        }
    }

    private void RestaurarCursor(BaseEventData data)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void AgregarEventTrigger(EventTrigger trigger, EventTriggerType tipo, UnityEngine.Events.UnityAction<BaseEventData> callback)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = tipo; 
        entry.callback.AddListener(callback);
        trigger.triggers.Add(entry);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CambiarCursorManita(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RestaurarCursor(eventData);
    }

    
}