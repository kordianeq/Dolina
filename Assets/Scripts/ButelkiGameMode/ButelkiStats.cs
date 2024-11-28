using TMPro;
using UnityEngine;

public class ButelkiStats : MonoBehaviour
{
    public int target;
    public int current;
    public TextMeshProUGUI butelkiText;

    [HideInInspector] public bool spawnButelki;

    private void Start()
    {
        current = 0;
        
        spawnButelki = true;
        butelkiText.text = "Butelki: " + target.ToString() + " / " + current.ToString();
    }
    public void UpdateStats()
    {
        if (current + 1 < target)
        {
            current++;
            butelkiText.text = "Butelki: " + target.ToString() + " / " + current.ToString();
        }
        else
        {
            spawnButelki = false;
            butelkiText.text = "YOU WON";

            
        }
    }
}
