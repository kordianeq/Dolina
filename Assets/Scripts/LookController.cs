using UnityEngine;

public class LookController : MonoBehaviour
{

    public float maxInteractionDistance;
    RaycastHit hit;
    RaycastHit lasthit;
    [Header("swietosc stuff")]
    public float maxRange = 5;
    public float timeToDecrease;
    public float timeToIncrease;
    public float swietosc = 0;
    public int goodIncrement, badIncrement;

    float decrease, increase, zmiana;


    PlayerStats playerStats;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();
        decrease = timeToDecrease;
        increase = timeToIncrease;
        
    }

    // Update is called once per frame
    void Update()
    {
        LookCheck();
    }

    void LookCheck()
    {
        

        if (Physics.Raycast(transform.position, transform.forward, out hit, maxRange))
        {
            SwietoscIncrements(hit);

            if (hit.collider.gameObject.TryGetComponent<Interact>(out Interact interacion))
            {
                if (Vector3.Distance(hit.collider.transform.position, transform.position) <= maxInteractionDistance)
                {
                    interacion.RayCastLookAt();
                }

            }
        }

        DistableUiElement();

        playerStats.swietosc = swietosc;
       
       

        
        

    }
    void SwietoscIncrements(RaycastHit hit)
    {
        //Debug.Log(hit.collider.gameObject.layer);
        if (hit.collider.gameObject.layer == 9)
        {
            if (decrease <= 0)
            {
                decrease = timeToDecrease;
                zmiana = badIncrement;


            }
            else if (decrease > 0)
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
                zmiana = goodIncrement;
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

    void DistableUiElement()
    {
        if (hit.collider != null)
        {

            if (lasthit.collider != null)
            {
                if (hit.collider.gameObject != lasthit.collider.gameObject)
                {

                    if (lasthit.collider.TryGetComponent<Interact>(out Interact interacion))
                    {
                        interacion.DistableUi();
                    }
                }
                else
                {

                }
            }
            else
            {

            }
        }
        else
        if (hit.collider == null)
        {

            if (lasthit.collider != null)
            {

                
                if (lasthit.collider.TryGetComponent<Interact>(out Interact interacion))
                {
                    interacion.DistableUi();
                }

            }
        }
        lasthit = hit;
    }
}
