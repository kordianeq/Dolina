using UnityEngine;

public class ConstantScale : MonoBehaviour
{
    [Header("Ustawienia Skali")]
    [Tooltip("Dopasuj tę wartość w Inspektorze, aby uzyskać pożądany bazowy rozmiar.")]
    [SerializeField] private float sizeMultiplier = 0.1f; 

    private Camera mainCamera;

    private void Start()
    {
        // Pobieramy referencję do głównej kamery na starcie
        mainCamera = Camera.main;
    }

    // Używamy LateUpdate zamiast Update. 
    // Dzięki temu mamy pewność, że kamera wykonała już swój ruch w danej klatce,
    // zanim zaczniemy obliczać względem niej odległość. Zapobiega to "drżeniu" obiektu.
    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // 1. Obliczamy odległość między tym obiektem a kamerą
        float distance = Vector3.Distance(transform.position, mainCamera.transform.position);

        // 2. Obliczamy docelową skalę (odległość razy nasz mnożnik)
        float finalScale = distance * sizeMultiplier;

        // 3. Przypisujemy nową skalę do obiektu we wszystkich trzech osiach (X, Y, Z)
        transform.localScale = Vector3.one * finalScale;
    }
}