//#define USE_TILT
#define USE_TOUCHPAD

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Boundary
{
    public float xMin, xMax, zMin, zMax;
}


public class PlayerController : MonoBehaviour
{

    public float speed;
    public float tilt;
    public Boundary boundary;

    public GameObject shot;
    public Transform shotSpawn;
    public float fireRate;

    private float nextFire;
    private AudioSource audioSource;

#if (USE_TILT)
    private Quaternion calibrationQuaternion;
#elif (USE_TOUCHPAD)
    private Quaternion calibrationQuaternion;
    public SimpleTouchPad touchPad;
    public SimpleTouchAreaButton areaButton;
#endif

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
#if (USE_TILT || USE_TOUCHPAD)
        CalibrateAccelerometer();
#endif
    }


    private void Update()
    {
#if (USE_TOUCHPAD)
        if (areaButton.CanFire() && Time.time > nextFire)
        //if (Input.GetButton("Fire1") && Time.time > nextFire)
#else
        if (Input.GetButton("Fire1") && Time.time > nextFire)
#endif
        {
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);
            audioSource.Play();
        }
    }


    private void FixedUpdate()
    {
#if (USE_TILT)
        Vector3 accelerationRaw = Input.acceleration;
        Vector3 acceleration = FixAcceleration(accelerationRaw);
        Vector3 movement = new Vector3(acceleration.x, 0.0f, acceleration.y);
#elif (USE_TOUCHPAD)
        Vector2 direction = touchPad.GetDirection();
        Vector3 movement = new Vector3(direction.x, 0.0f, direction.y);
#else
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
#endif
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.velocity = movement * speed;

        rb.position = new Vector3
        (
            Mathf.Clamp(rb.position.x, boundary.xMin, boundary.xMax),
            0.0f,
            Mathf.Clamp(rb.position.z, boundary.zMin, boundary.zMax)
        );

        rb.rotation = Quaternion.Euler(0.0f, 0.0f, rb.velocity.x * -tilt);
    }

#if (USE_TILT || USE_TOUCHPAD)
    //Used to calibrate the Iput.acceleration input
    void CalibrateAccelerometer()
    {
        Vector3 accelerationSnapshot = Input.acceleration;
        Quaternion rotateQuaternion = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, -1.0f), accelerationSnapshot);
        calibrationQuaternion = Quaternion.Inverse(rotateQuaternion);
    }

    //Get the 'calibrated' value from the Input
    Vector3 FixAcceleration(Vector3 acceleration)
    {
        Vector3 fixedAcceleration = calibrationQuaternion * acceleration;
        return fixedAcceleration;
    }
#endif
}
