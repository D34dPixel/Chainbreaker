using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    public float lookDelay = 0.125f;
    public float sensitivity = 10;
    public Vector3 offset; // how far the camera is from the player

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void FixedUpdate()
    {
        
        Vector3 wantPos = player.transform.position + (player.transform.rotation * offset);
        Vector3 pos = Vector3.Lerp(transform.position, wantPos, lookDelay);
        transform.position = pos;

        transform.LookAt(player.transform);

        
    }

    public void OnRotate(InputValue val)
    {
        Vector2 inputVal = val.Get<Vector2>();
        //Debug.Log(inputVal);
        player.transform.rotation = Quaternion.Euler(0f, (inputVal.x * sensitivity) + transform.rotation.eulerAngles.y, 0f);
    }
}
