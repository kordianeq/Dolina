using TMPro;
using UnityEngine;

public class ButelkiStats : MonoBehaviour
{
    public int target;
    public int current;
    public TextMeshProUGUI butelkiText;

    public bool keepSpawning;

    private void Start()
    {
        current = 0;
        
        keepSpawning = true;
        butelkiText.text = "Butelki: " + current.ToString() +" / " +target.ToString();
    }
    public void UpdateStats()
    {
        if (current + 1 < target)
        {
            current++;
            butelkiText.text = "Butelki: " + current.ToString() + " / " + target.ToString();
            keepSpawning = true;
        }
        else
        {
            keepSpawning = false;
            butelkiText.text = "YOU WON";
        }
    }
}
