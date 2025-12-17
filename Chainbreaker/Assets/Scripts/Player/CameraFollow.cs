using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public GameObject orient, player;
    public float lookDelay = 0.125f;
    public float sensitivity = 10;
    public Vector3 offset; // how far the camera is from the player
    public int minVert, maxVert;
    public int baseFOV = 60;

    [Header("Aiming")]
    public GameObject crosshair;
    public bool aiming;
    public Vector3 aimOffset;
    public int aimFOV = 30;
    public Transform aimTarget;
    public LayerMask bossLayer;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        crosshair.SetActive(aiming);

        Vector3 wantPos;

        if (aiming)
        {
            GetComponent<Camera>().fieldOfView = aimFOV;

            wantPos = aimTarget.position + (orient.transform.rotation * aimOffset);

            transform.LookAt(aimTarget);

        }
        else
        {
            GetComponent<Camera>().fieldOfView = baseFOV;
            wantPos = orient.transform.position + (orient.transform.rotation * offset);
            transform.LookAt(orient.transform);
        }
        Vector3 pos = Vector3.Lerp(transform.position, wantPos, lookDelay);
        transform.position = pos;
    }

    public void OnRotate(InputValue val)
    {
        Vector2 inputVal = val.Get<Vector2>().normalized;

        float x;
        float y;
        
        x = Mathf.Clamp((inputVal.y * -sensitivity) + transform.rotation.eulerAngles.x, minVert, maxVert);
        y = (inputVal.x * sensitivity) + transform.rotation.eulerAngles.y;
        
        StopAllCoroutines();
        StartCoroutine(RotatePlayerY(y));
        StartCoroutine(RotatePlayerX(x));
    }

    public IEnumerator RotatePlayerY(float newY)
    {
        while (Mathf.Abs(orient.transform.eulerAngles.y - newY) > 0.1f)
        {
            Quaternion wantRot = Quaternion.Euler(transform.rotation.eulerAngles.x, newY, 0);
            orient.transform.rotation = Quaternion.Lerp(orient.transform.rotation, wantRot, 0.125f);
            aimTarget.rotation = orient.transform.rotation;
            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator RotatePlayerX(float newX)
    {
        while (Mathf.Abs(orient.transform.eulerAngles.x - newX) > 0.1f)
        {
            Quaternion wantRot = Quaternion.Euler(newX, transform.rotation.eulerAngles.y, 0);
            orient.transform.rotation = Quaternion.Lerp(orient.transform.rotation.normalized, wantRot.normalized, 0.125f);
            aimTarget.rotation = orient.transform.rotation;
            yield return new WaitForSeconds(1);
        }
    }

    public void Shoot()
    {
        Debug.Log("Shot");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, bossLayer))
        {
            Debug.Log("Hit Boss");
            hit.transform.gameObject.GetComponent<Boss>().TakeDamage();
        }
    }
}
