using UnityEngine;

public class DoorMechanic : MonoBehaviour, IInteracted
{
    bool isOpen = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NewInteraction()
    {
        if (!isOpen)
        {
            isOpen = true;
            transform.Rotate(0, 90, 0);
        }
        else
        {
            isOpen = false;
            transform.Rotate(0,-90,0);
        }

    }
}
