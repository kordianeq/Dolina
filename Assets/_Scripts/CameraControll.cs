using UnityEngine;

public class CameraControll : MonoBehaviour
{
    public float sensX, sensY;
    public Transform orientation;

    float xRotation;
    float yRotation;

    bool lockMode;
    // Start is called before the first frame update
    void Start()
    {
        LockCamera(false);
    }

    // Update is called once per frame
    void Update()
    {
       
        if (lockMode == false)
        {


            float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;
           
            yRotation += mouseX;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
        }
    }

    public void LockCamera(bool state /*true == camera locked */)
    {
        if (state)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        lockMode = state;
    }
}
