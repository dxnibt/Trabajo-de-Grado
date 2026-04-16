namespace Dominio.entidades{

public class Respuesta
{
    public int EstudianteId { get; set; }
    public Pregunta Pregunta { get; private set; }
    public string Valor { get; private set; }

    public Respuesta(int estudianteId, Pregunta pregunta, string valor)
    {
        EstudianteId = estudianteId;
        Pregunta = pregunta;
        Valor = valor;
    }

    public bool EsCorrecta()
    {
        return Pregunta.ValidarRespuesta(Valor);
    }
}
}