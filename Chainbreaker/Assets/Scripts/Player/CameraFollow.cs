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
    public int baseFOV = 60;

    [Header("Aiming")]
    public bool aiming;
    public Vector3 aimOffset;
    public int aimFOV = 30;
    public Transform aimTarget;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void FixedUpdate()
    {
        Vector3 wantPos;

        if (aiming)
        {
            GetComponent<Camera>().fieldOfView = aimFOV;

            wantPos = aimTarget.position + (player.transform.rotation * aimOffset);

            transform.LookAt(aimTarget);

        }
        else
        {
            GetComponent<Camera>().fieldOfView = baseFOV;
            wantPos = player.transform.position + (player.transform.rotation * offset);
            transform.LookAt(player.transform);
        }
        Vector3 pos = Vector3.Lerp(transform.position, wantPos, lookDelay);
        transform.position = pos;
    }

    public void OnRotate(InputValue val)
    {
        Vector2 inputVal = val.Get<Vector2>();

        float x;
        float y;

        if (aiming)
        {
            x = (inputVal.y * sensitivity) + transform.rotation.eulerAngles.x;
            y = (inputVal.x * sensitivity) + transform.rotation.eulerAngles.y;
        }

        else
        {
            x = Mathf.Clamp((inputVal.y * sensitivity) + transform.rotation.eulerAngles.x, minVert, maxVert);
            y = (inputVal.x * sensitivity) + transform.rotation.eulerAngles.y;

        }
        StopAllCoroutines();
        StartCoroutine(RotatePlayerY(y));
        StartCoroutine(RotatePlayerX(x));
    }

    public IEnumerator RotatePlayerY(float newY)
    {
        while (player.transform.rotation.y != newY)
        {
            Quaternion wantRot = Quaternion.Euler(transform.rotation.eulerAngles.x, newY, 0);
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, wantRot, 0.125f);
            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator RotatePlayerX(float newX)
    {
        while (player.transform.rotation.y != newX)
        {
            Quaternion wantRot = Quaternion.Euler(newX, transform.rotation.eulerAngles.y, 0);
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, wantRot, 0.125f);
            yield return new WaitForSeconds(1);
        }
    }
}
