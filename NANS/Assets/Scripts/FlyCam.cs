using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyCam : MonoBehaviour
{

	public Transform playerCamera = null;
	public float mouseSensitivity = 30f;
	[Range(0.0f, 0.5f)] 
	public float mouseSmoothTime = 0.03f;

	public bool inFly = true;
	public int speed = 10;
	
	Vector2 currentMouseDelta = Vector2.zero;
	Vector2 currentMouseDeltaVelocity = Vector2.zero;
	private float cameraPitch = 0.0f;
	RaycastHit HitInfo;
	World world;
	float timer = 0;
	
	// Start is called before the first frame update
	void Awake()
	{
		transform.position = new Vector3(0, 50, 0);
		transform.rotation = Quaternion.Euler(0, 0, 0);
		world = GameObject.Find("World Holder").GetComponent<World>();
		SetInFilight(inFly);
	}

	// Update is called once per frame
	void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
			SetInFilight(!inFly);

		}
        if (inFly)
        {
			transform.position += transform.rotation * GetBaseInput() * Time.deltaTime * speed;
			UpdateMouseLook();

			if (Input.GetMouseButton(0)) {
				// Remove ground
				timer += Time.deltaTime;
				if(timer > 0.04f) { 
					if (Physics.Raycast(playerCamera.position, playerCamera.forward, out HitInfo, 32f))
					{
						world.SendMessage("RemoveGround", HitInfo.point);
					}
					timer = 0;
				}
			}
			else if (Input.GetMouseButton(1))
            {
				// Add ground
				timer += Time.deltaTime;
				if (timer > 0.04f)
				{
					if (Physics.Raycast(playerCamera.position, playerCamera.forward, out HitInfo, 32f))
					{
						world.SendMessage("AddGround", HitInfo.point);
					}
				}
			}
		}
		
	}

	private Vector3 GetBaseInput()
	{ //returns the basic values, if it's 0 than it's not active.
		Vector3 p_Velocity = new Vector3();
		if (Input.GetKey(KeyCode.W))
		{
			p_Velocity += new Vector3(0, 0, 1);
		}
		if (Input.GetKey(KeyCode.S))
		{
			p_Velocity += new Vector3(0, 0, -1);
		}
		if (Input.GetKey(KeyCode.A))
		{
			p_Velocity += new Vector3(-1, 0, 0);
		}
		if (Input.GetKey(KeyCode.D))
		{
			p_Velocity += new Vector3(1, 0, 0);
		}
		if (Input.GetKey(KeyCode.Space))
		{
			p_Velocity += new Vector3(0, 1, 0);
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			p_Velocity += new Vector3(0, -1, 0);
		}
		return p_Velocity;
	}

	void UpdateMouseLook()
	{
		Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

		currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);

		cameraPitch -= currentMouseDelta.y * mouseSensitivity;
		cameraPitch = Mathf.Clamp(cameraPitch, -90.0f, 90.0f);

		playerCamera.localEulerAngles = Vector3.right * cameraPitch;
		transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
	}

	public void SetInFilight(bool t)
    {
        if (t)
        {
			inFly = true;
			Cursor.lockState = CursorLockMode.Locked;
		}
        else
        {
			inFly = false;
			Cursor.lockState = CursorLockMode.None;
		}

    }

}
