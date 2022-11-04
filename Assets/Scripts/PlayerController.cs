using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using System.Linq;

// define the screen boundaries in Unity
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

	// allow the ship to fire
	public GameObject shot;
	public Transform shotSpawn;
	public float fireRate;
	private float nextFire;
	public int fireCount = 0;
	public int upCount = 0;
    public int downCount = 0;
    public int leftCount = 0;
    public int rightCount= 0;

    private GameController gameController;
	private KeyCode? lastKeyPressed;

	private static readonly KeyCode[] keyCodes = System.Enum.GetValues(typeof(KeyCode))
												 .Cast<KeyCode>()
												 .Where(k => ((int)k < (int)KeyCode.Mouse0))
												 .ToArray();

	private static KeyCode? GetCurrentKeyDown()
	{
		if (!Input.anyKey)
		{	
			return null;
		}

		for (int i = 0; i < keyCodes.Length; i++)
		{
			if (Input.GetKey(keyCodes[i]))
			{
				return keyCodes[i];
			}
		}
		return null;
	}



	void Start()
    {
		gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
	

    }

	public void ResetCount()
    {
		Debug.Log("Reset pavlovian count...");
        leftCount = 0;
        rightCount = 0;
        upCount = 0;
        downCount = 0;
        fireCount = 0;
    }


	void Update()
	{
		KeyCode? keyDown = GetCurrentKeyDown(); 

		if (keyDown == null)
        {
			lastKeyPressed = null;
        }


		if ((Input.GetButton("Fire1") || Input.GetKey("space")) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation); 
            GetComponent<AudioSource>().Play(); // fire sound
			fireCount++;
        }

        if (keyDown == lastKeyPressed)
            return;

        switch (keyDown)
        {
            case KeyCode.DownArrow:	
    			downCount++;
                break;

            case KeyCode.UpArrow:
                upCount++;
                break;
            case KeyCode.LeftArrow:
                leftCount++;
                break;
            case KeyCode.RightArrow:
                rightCount++;
                break;

        }
        lastKeyPressed = keyDown;

    }


    void FixedUpdate()
	{
		// move the ship
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical   = Input.GetAxis("Vertical");
      
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

		// adapt the speed
		GetComponent<Rigidbody>().velocity = movement * speed;

		// constraint the ship inside the screen
		GetComponent<Rigidbody>().position = new Vector3
		(
			Mathf.Clamp(GetComponent<Rigidbody>().position.x, boundary.xMin, boundary.xMax),	// constraint the vertical movement with the math function Clamp
			0.0f,																				// the ship does not move in depth
			Mathf.Clamp(GetComponent<Rigidbody>().position.z, boundary.zMin, boundary.zMax)		// constraint the horizontal movement with the math function Clamp
		);

		// tilt the ship to its side when moving
		GetComponent<Rigidbody>().rotation = Quaternion.Euler(0.0f, 0.0f, GetComponent<Rigidbody>().velocity.x * -tilt);
	}
}
