using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    public GameObject orient, player;
    public float lookDelay = 0.125f;
    public float sensitivity = 5;
    public Vector3 offset; // how far the camera is from the orient object
    public int minVert, maxVert; //minimum and maximum vertical distance able to be looked
    public int baseFOV = 60;

    [Header("Aiming")]
    public GameObject crosshair;
    public bool aiming;
    public float aimSense = 2;
    public Vector3 aimOffset; // how far the camera is from the aim target
    public int aimFOV = 30;
    public Transform aimTarget; 
    public LayerMask bossLayer; //used for shooting to test that what has been shot is the boss

    private void Start()
    {
        //locks cursor
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        //set the crosshair active if aiming
        crosshair.SetActive(aiming);

        Vector3 wantPos; //the desired position

        //get want positioned position based on if aiming
        if (aiming)
        {
            //set fov
            GetComponent<Camera>().fieldOfView = aimFOV;

            //add offset to rotation to make it look just ahead of the player
            wantPos = aimTarget.position + (orient.transform.rotation.normalized * aimOffset);

            transform.LookAt(aimTarget);

        }
        else
        {
            //set fov
            GetComponent<Camera>().fieldOfView = baseFOV;

            //add offset to rotation and make it look just above the player
            wantPos = orient.transform.position + (orient.transform.rotation.normalized * offset);
            transform.LookAt(orient.transform);
        }

        //THIS IS BROKEN I THINK
        //smoothly lerp the current pos to the wanted position
        Vector3 pos = Vector3.Lerp(transform.position, wantPos, lookDelay);
        transform.position = pos;
    }

    //called when the mouse is moved in accordance with the input system
    //this is an inefficient way of doing this and should be done in LateUpdate instead of contained in the OnRotate
    public void OnRotate(InputValue val)
    {
        Vector2 inputVal = val.Get<Vector2>();

        float x;
        float y;

        //set sense multiplier
        float sense = sensitivity;
        if (aiming)
            sense = aimSense;


        //* by timescale stops the camera from rotating when the game is frozen
        //create the y input
        y = (inputVal.x * sense) + transform.rotation.eulerAngles.y * Time.timeScale;


        //normalise and clamp x according to this stackoverflow answer
        //https://stackoverflow.com/questions/76875952/how-do-i-clamp-horizontal-axis-rotation

        x = (inputVal.y * -sense) + transform.rotation.eulerAngles.x * Time.timeScale;

        x = (x + 180) % 360;
        if (x < 0)
            x += 360;
        x -= 180;

        x = Mathf.Clamp(x, minVert, maxVert);

        //stop any movement to x or y and start new movement to them
        StopAllCoroutines();
        StartCoroutine(RotatePlayerY(y));
        StartCoroutine(RotatePlayerX(x));
    }

    public IEnumerator RotatePlayerY(float newY)
    {
        //check that the difference of old and new y is greater than 0.1
        while (Mathf.Abs(orient.transform.eulerAngles.y - newY) > 0.1f)
        {
            //get the wanted rotation
            Quaternion wantRot = Quaternion.Euler(transform.rotation.eulerAngles.x, newY, 0);

            //smoothly lerp both orientation and aim target to the wanted location
            orient.transform.rotation = Quaternion.Lerp(orient.transform.rotation, wantRot, 0.125f);
            aimTarget.rotation = orient.transform.rotation;

            yield return new WaitForSeconds(1);
        }
    }

    public IEnumerator RotatePlayerX(float newX)
    {
        //check difference of old and new x and perform the same as above
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

        //Check if hit is on the boss layer and call the hit object to take damage if so
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, bossLayer))
        {
            Debug.Log("Hit Boss");
            hit.transform.gameObject.GetComponent<Boss>().TakeDamage();
        }
    }
}
