using UnityEngine;
using System.Collections;

public class cowboyController : MonoBehaviour {
	
	[Header("Movement")]
	public float velocityh;
	public float velocityv;
	public float acceleration = 1.5f;
	public float friction = 10f;

	public float inAirSpeed = 3f;
	public float airSlowdown = 0.1f;
	[Range(0.1f, 1f)][SerializeField] public float maxSpeed = 0.1f;
	[Range(1f, 100f)][SerializeField] public float gravityStrength = 2f;
	private Vector3 movement;
	private Vector3 jumpMovement;
	private RaycastHit floorHit;
	
	[Range(1f, 10f)][SerializeField]public float jumpStrength = 12f;
	public bool isGrounded;
	public GameObject standingOnObject;
	public float terrainSlope;
	
	private Animator anim;                      
	private Rigidbody playerRigidbody;
	private int floorMask;                      
	private float camRayLength = 100f;
	private Camera mainCamera;

	private GameObject OxGun;
	[Header("Debug")]
	public bool visibleRange = false;

	LineRenderer lineRenderer;
	public Material lineMat;

	[Header("Camera")]
	public GameObject CameraBase;

	[Header("CharacterModel")]
	public GameObject characterModel;
	private Animator aniMat;

	public bool killed;

	
	void Awake ()
	{
		aniMat = characterModel.GetComponent<Animator>();
		CameraBase = GameObject.Find("CameraBase");
		OxGun = GameObject.Find("Gun");
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = lineMat;
		lineRenderer.SetWidth(0.2F, 0.2F);
		//lineMat.renderQueue = 9999999;
		// NEW ABOVE
		floorMask = LayerMask.GetMask ("FloorMask");
		anim = GetComponent <Animator> ();
		playerRigidbody = GetComponent <Rigidbody> ();
		playerRigidbody.mass = 100f;
		mainCamera = GameObject.Find("Camera").GetComponent<Camera>();
		GameObject[] FoundTerrain = GameObject.FindGameObjectsWithTag("Terrain");

		// turn of bounciness?
		foreach(GameObject terrainPiece in FoundTerrain)
		{
			terrainPiece.layer = 8; // 8 is floormask
			terrainPiece.GetComponent<Collider>().material.bounciness = 0f;
		}
		this.GetComponent<Collider>().material.bounciness = 0f;
	}
		
	void Update()
	{
		if(visibleRange) 
			RangeLine(); 
		else 
			lineRenderer.enabled = false;

	}

	void AnimationsFunc(float h, float v)
	{
		aniMat.SetInteger("WalkHorizontal", Mathf.RoundToInt(h));
		aniMat.SetInteger("WalkVertical", Mathf.RoundToInt(v));
		aniMat.SetFloat("WalkHorizontalF", h);
		aniMat.SetFloat("WalkVerticalF",v);
	}
		
	void RangeLine()
	{
		//print(floorHit.point);
		float lineRange = Vector3.Distance(this.transform.position, floorHit.point);
		if(lineRange <= OxGun.GetComponent<BlowGun>().gunRange )
		{
			lineRenderer.enabled = true;
			lineRenderer.SetPosition(0,new Vector3(this.transform.position.x,this.transform.position.y-(this.transform.localScale.y),this.transform.position.z));
			lineRenderer.SetPosition(1,floorHit.point);
			//centerPt + Vector3.ClampMagnitude(offset, radius);

		}
		else
		{
			lineRenderer.enabled = false;
		}

	}
	
	void FixedUpdate ()
	{
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");
		//print ("Horizontal: " + h + " Vertical: "+ v);
		AnimationsFunc(h,v);
		Move(h, v);
		Turning();
		CheckAngleTerrain(h, v);
		//Animating (h, v);
	}
	
	void Move (float h, float v)
	{
		if(this.transform.parent != null && !isGrounded)
			this.transform.parent = null;

		velocityh += acceleration * h * Time.deltaTime;
		velocityh -= velocityh * friction * Time.deltaTime;
		velocityv += acceleration * v * Time.deltaTime;
		velocityv -= velocityv * friction * Time.deltaTime;

		Vector3 newMovement = this.transform.TransformDirection(velocityh,0,velocityv);
		movement.Set(newMovement.x, 0, newMovement.z);

		if(!isGrounded)
			playerRigidbody.MovePosition (transform.position + movement);
		else if(isGrounded && standingOnObject.name != "elevatorFrame")
		{
			playerRigidbody.MovePosition (transform.position + (movement * terrainSlope));
			if(CameraBase.GetComponent<CameraFollow>().onElevator)
				CameraBase.GetComponent<CameraFollow>().onElevator = false;
		}
		else if(isGrounded && standingOnObject.name == "elevatorFrame") 
			ElevatorStandMovement(movement);
	}

	void ElevatorStandMovement(Vector3 movement)
	{
		CameraBase.GetComponent<CameraFollow>().onElevator = true;
		playerRigidbody.MovePosition (transform.position + movement);
		if(this.transform.parent != standingOnObject.transform) 
			this.transform.SetParent(standingOnObject.transform);
	}

	void HandleAirborneMovement()
	{
		Vector3 extraGravityForce = (Physics.gravity * gravityStrength) - Physics.gravity;
		playerRigidbody.AddForce(extraGravityForce);
	}
	
	void Turning ()
	{
		this.transform.Rotate(new Vector3(0,Input.GetAxis("Mouse X"),0));
	}
	
	void CheckAngleTerrain(float h, float v)
	{
		if(( h>0 || v>0)){
			RaycastHit hit;
			if(Physics.SphereCast(new Vector3(this.transform.position.x, this.transform.position.y+(this.transform.localScale.y/2), this.transform.position.z), this.transform.localScale.x, Vector3.down,out hit, transform.localScale.y))
			{
				standingOnObject = hit.transform.gameObject;
			}
			terrainSlope = hit.normal.y;
		}
	}
	

	void Animating (float h, float v)
	{
		// Create a boolean that is true if either of the input axes is non-zero.
		bool walking = h != 0f || v != 0f;
		
		// Tell the animator whether or not the player is walking.
		anim.SetBool ("IsWalking", walking);
	}
	
	void OnCollisionStay (Collision collisionInfo)
	{
		if(collisionInfo.gameObject == standingOnObject) isGrounded = true;
	}
	
	void OnCollisionExit (Collision collisionInfo)
	{
		if(collisionInfo.gameObject == standingOnObject) isGrounded = false;
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.CompareTag("killer"))
		{
			if(!killed)
			{
				GameObject.Find("QuestCam").GetComponent<QuestHandler>().StartSceneExit();
				killed = true;
			}
		}
	}
}

