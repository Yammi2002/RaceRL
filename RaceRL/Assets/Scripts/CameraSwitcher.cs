using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Camera[] cameras;   // array di camere (assegna in Inspector)

    private int currentIndex = 0;

    void Start()
    {
        // Disattiva tutte e attiva solo la prima
        for (int i = 0; i < cameras.Length; i++)
            cameras[i].enabled = (i == 0);
    }

    void Update()
    {
        // Premi barra spaziatrice per passare alla prossima camera
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Disattiva la corrente
            cameras[currentIndex].enabled = false;

            // Avanza indice (ciclo)
            currentIndex = (currentIndex + 1) % cameras.Length;

            // Attiva la nuova
            cameras[currentIndex].enabled = true;
        }
    }
}
