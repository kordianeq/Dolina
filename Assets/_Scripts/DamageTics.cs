using UnityEngine;

public class DamageTics : MonoBehaviour
{
    HandPowers Hnd;
    bool tickReset;
    bool countdown;
    // Start is called before the first frame update
    void Start()
    {
        tickReset = true;
        Hnd = FindAnyObjectByType<HandPowers>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    private void OnTriggerStay(Collider other)
    {
        if(countdown == false)
        {
            countdown = true;
            Invoke(nameof(DestroyObject), Hnd.timeOnGround);
        }
        Debug.Log(other.gameObject.name);
        if(other.gameObject.CompareTag("Enemy")&& tickReset == true)
        {
            tickReset=false;
            other.gameObject.GetComponent<IDamagable>().Damaged(Hnd.stationaryDamage);

            Invoke(nameof(ResetTick),Hnd.tickSpeed);
        }
    }

    private void ResetTick()
    {
        tickReset = true;
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
