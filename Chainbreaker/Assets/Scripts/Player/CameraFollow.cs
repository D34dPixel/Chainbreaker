using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    public float lookDelay = 0.125f;
    public float sensitivity = 10;
    public Vector3 offset; // how far the camera is from the player
    public int minVert, maxVert;
    public bool aiming;

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
        //float x = Mathf.Clamp((inputVal.y * sensitivity) + transform.rotation.eulerAngles.x, minVert, maxVert);
        //float x = (inputVal.y * sensitivity) + transform.rotation.eulerAngles.x;

        StopAllCoroutines();
        StartCoroutine(RotatePlayer(0f, (inputVal.x * sensitivity) + transform.rotation.eulerAngles.y));
    }

    public IEnumerator RotatePlayer(float newX, float newY)
    {
        for (int i = 0; player.transform.rotation.y != newY; i++)
        {
            Quaternion wantRot = Quaternion.Euler(newX, newY, 0);
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, wantRot, 0.125f);
            yield return new WaitForSeconds(1);
        }
    }
}
