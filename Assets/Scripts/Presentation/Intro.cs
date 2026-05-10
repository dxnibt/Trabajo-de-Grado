using UnityEngine;
using UnityEngine.SceneManagement;

public class Intro : MonoBehaviour
{
    void Start()
    {
        Invoke(nameof(CargarMenu), 3f);
    }

    void CargarMenu()
    {
        SceneManager.LoadScene("mp");
    }
}