using TMPro;
using UnityEngine;

public class LookController : MonoBehaviour
{
    
    public float maxRange = 20;
    public float timeToDecrease;
    public float timeToIncrease;
    public float swietosc = 0;
    public int goodIncrement, badIncrement;

    float decrease, increase, zmiana;
    public TextMeshProUGUI swietoscText;

    // Start is called before the first frame update
    void Start()
    {
        decrease = timeToDecrease;
        increase = timeToIncrease;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxRange))
        {
            
            //Debug.Log(hit.collider.gameObject.layer);
            if (hit.collider.gameObject.layer == 9)
            {
                
                
                if (decrease <= 0)
                {
                    decrease = timeToDecrease;
                    zmiana =  badIncrement;
                    
                    
                }
                else if( decrease > 0)
                {
                    decrease = (decrease) - (2f * Time.deltaTime);
                    zmiana = 0;
                }

            }
            else if (hit.collider.gameObject.layer == 8)
            {
                if (increase <= 0)
                {
                    increase = timeToIncrease;
                    zmiana =  goodIncrement;
                }
                else if (increase > 0)
                {
                    increase = increase - (2f * Time.deltaTime);
                    zmiana = 0;
                }
                
            }
            else
            {
                zmiana = 0;
            }

            if (zmiana != 0)
            {
                if ((swietosc + zmiana) >= -100 && (swietosc + zmiana) <= 100)
                {
                    swietosc = swietosc + zmiana;
                }
                else
                {

                }
            }
        }

       
        swietoscText.text = "ŒWIÊTOŒÆ : " + swietosc.ToString();
    }
}
