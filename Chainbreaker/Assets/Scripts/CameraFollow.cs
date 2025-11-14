using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject player;
    public float lookDelay = 0.125f;
    public Vector3 offset; // how far the camera is from the player

    private void FixedUpdate()
    {
        Vector3 wantPos = player.transform.position + offset;
        Vector3 pos = Vector3.Lerp(transform.position, wantPos, lookDelay);
        transform.position = pos;

        transform.LookAt(player.transform);
    }
}
