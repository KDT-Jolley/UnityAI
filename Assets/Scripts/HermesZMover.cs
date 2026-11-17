using UnityEngine;

// Moves the GameObject back and forth along the Z axis between -5 and +5.
public class HermesZMover : MonoBehaviour
{
    public float minZ = -5f;
    public float maxZ = 5f;
    public float speed = 2f; // units per second

    void Update()
    {
        float t = Mathf.PingPong(Time.time * speed, maxZ - minZ);
        Vector3 p = transform.position;
        p.z = minZ + t;
        transform.position = p;
    }
}
