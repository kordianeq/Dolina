using UnityEngine;

public class Horse : MonoBehaviour, IInteracted
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void NewInteraction()
    {
        Debug.Log("Horse interaction triggered.");
        //gameObject.GetComponent<BoxCollider>().enabled = false;
        GameManager.Instance.playerRef.transform.position = this.transform.position + new Vector3(0, 3, 0);
  

    }
}
