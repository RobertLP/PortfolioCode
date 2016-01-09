using UnityEngine;
using System.Collections;

public class ShieldMine : MonoBehaviour 
{
	[Header("Status")]
	#region Status
	public float health;
	private float startHealth;
	private bool startDestruction;
	private bool startFlicker;
	[HideInInspector]public bool finishFlicker;
	#endregion

	[Header("Connections")]
	#region Connections
	public GameObject brother;
	public bool shieldCaster;
	#endregion

	[Header("Properties")]
	#region Properties
	public float unfoldHeight;
	public float unfoldSpeed;
	public float waitSecForDeploy;
	#endregion
	
	private bool unfolded;
	private bool shieldCreated;

	[HideInInspector]public Transform topEmit;
	[HideInInspector]public Transform bottomEmit;
	private Vector3 topEmitStart;
	private Vector3 bottomEmitStart;

	[Header("EmitterEffect")]
	#region EmitterEffect
	public Material emitMaterial;
	public float emitWidth;
	private LineRenderer emitEffect;
	#endregion

	[Header("Shield")]
	#region Shield
	public Material shieldMaterial;
	private GameObject shieldWall;
	#endregion

	private Vector3 viewportPos;

	private ShieldMineSystem mineSys;
	
	void Awake() // assign children and start locations. Save health to starthealth for reset
	{
		mineSys = GameObject.Find("LogicHook").GetComponent<ShieldMineSystem>();
		startHealth = health;
		topEmit = this.transform.GetChild(0);
		topEmitStart = topEmit.transform.localPosition;
		bottomEmit = this.transform.GetChild(1);
		bottomEmitStart = bottomEmit.transform.localPosition;
	}

	void OnEnable() // assign new (closest) brother, set the emitter, and start unfolding
	{
		viewportPos = Camera.main.WorldToViewportPoint(this.transform.position);
		mineSys.GoTroughActivatedMines(this.gameObject);
		SetEmitter();
		StartCoroutine(UnfoldEmitter());
	}
	
	void OnDisable() // reset emitter Obj locations, stop all coroutines, and reset values
	{
		topEmit.transform.localPosition = topEmitStart;
		bottomEmit.transform.localPosition = bottomEmitStart;
		StopAllCoroutines();
		ResetToDefault();
	}

	private void SetEmitter() //@OnEnable add linerenderer (emitter) to the object if its not there
	{
		if(emitEffect == null)
		{
			emitEffect = this.gameObject.AddComponent<LineRenderer>();
			emitEffect.material = emitMaterial; 
			emitEffect.SetWidth(emitWidth,emitWidth);
			emitEffect.useWorldSpace = true;
		}
	}

	private void ResetToDefault() // @OnDisable reset values to original values
	{
		health = startHealth;
		topEmit.localPosition = topEmitStart;
		bottomEmit.localPosition = bottomEmitStart;
		unfolded = false;
		shieldCreated = false;
		startDestruction = false;
		startFlicker = false;
		finishFlicker = false;
		brother = null;
		if(shieldWall != null)
			Destroy(shieldWall);
	}
	

	void Update()
	{
		emitEffect.SetPosition(0, topEmit.transform.position); // Update emitter locations because of camera movement.
		emitEffect.SetPosition(1, bottomEmit.transform.position);
		if(health <= 0)
			FindDeathType();
		//this.transform.position = Camera.main.ViewportToWorldPoint(viewportPos);
		//this.transform.rotation = Camera.main.transform.parent.rotation;
	}

	private void EmitterEnabled(bool status, LineRenderer brother)
	{
		emitEffect.enabled = status;
		brother.enabled = status;
	}

	private IEnumerator UnfoldEmitter()
	{
		yield return new WaitForSeconds(waitSecForDeploy);
		emitEffect.enabled = true;
		while(!unfolded)
		{
			// Calculate current distance between emitters
			float emitDistance = topEmit.position.y - this.transform.position.y;
			// If the emitter has reached or has exceeded its wanted distance, stop the while loop.
			if(emitDistance >= unfoldHeight)
				unfolded = true;
			// Move emitters
			topEmit.Translate(Vector3.up*unfoldSpeed*Time.deltaTime);
			bottomEmit.Translate(Vector3.down*unfoldSpeed*Time.deltaTime);
			
			yield return null;
		}
		// if this object is a shield caster. Start shield instantiation
		if(shieldCaster && brother != null)
			StartCoroutine(GenerateShield());
	}

	private IEnumerator GenerateShield()
	{
		// Creation, parenting and positioning.
		shieldWall = new GameObject("ShieldWall", typeof(MeshFilter), typeof(MeshRenderer));
		shieldWall.transform.SetParent(this.transform);
		shieldWall.transform.localPosition = new Vector3(0,0,0);

		// Mesh collider creation
		MeshCollider mc = shieldWall.AddComponent<MeshCollider>();
		mc.sharedMesh = ShieldCreation(shieldWall);

		// Tag and material
		shieldWall.tag = "Obstruction";
		shieldWall.GetComponent<Renderer>().material = shieldMaterial;
		yield return null;
	}

	private Mesh ShieldCreation(GameObject sh)
	{
		ShieldMine brotherScrpt = brother.GetComponent<ShieldMine>();
		MeshFilter mf = sh.GetComponent<MeshFilter>();
		Mesh mesh = new Mesh();
		//vertices
		Vector3[] verticies = new Vector3[4]
		{
			//bottomEmit.position
			new Vector3(0,bottomEmit.position.y - this.transform.position.y,0),
			//brotherScrpt.bottomEmit.position,
			new Vector3(brother.transform.position.x - this.transform.position.x,brotherScrpt.bottomEmit.position.y - this.transform.position.y, brother.transform.position.z - this.transform.position.z),
			//topEmit.position,
			new Vector3(0,topEmit.position.y - this.transform.position.y,0),
			//brotherScrpt.topEmit.position
			new Vector3(brother.transform.position.x - this.transform.position.x,brotherScrpt.topEmit.position.y - this.transform.position.y, brother.transform.position.z - this.transform.position.z)
		};
		// triangles
		int[] tri = new int[6];
		tri[0] = 0;
		tri[1] = 2;
		tri[2] = 1;
		tri[3] = 2;
		tri[4] = 3;
		tri[5] = 1;
		//UVs
		Vector2[] uv = new Vector2[4];
		uv[0] = new Vector2(0,0);
		uv[1] = new Vector2(1,0);
		uv[2] = new Vector2(0,1);
		uv[3] = new Vector2(1,1);
		//assign arrays
		mesh.vertices = verticies;
		mesh.triangles = tri;
		mesh.uv = uv;
		//Recalculate normals
		mesh.RecalculateNormals();
		//Recalculate bounds before normals
		mesh.RecalculateBounds();
		//Optimize
		mesh.Optimize();
		// asign mesh to mesh filter
		mf.mesh = mesh;
		// return the mesh for collider
		return mf.mesh;
	}

	private void OnTriggerEnter(Collider other) 
	{
		if(other.gameObject.CompareTag("PlayerProjectile"))
		{
			ReceiveDamage(other.GetComponent<Projectile_BeamMovement>().damage);
		}
	}

	private void ReceiveDamage(float Amount)
	{
		health -= Amount;
		if(health <= 0)
		{
			FindDeathType();
		}
	}

	private void FindDeathType()
	{
		if(mineSys.FindActiveMines() == 1) // if the mine is the only one active, just disable self.
			this.gameObject.SetActive(false);
		else if(!mineSys.CastingBrothers((mineSys.FindActiveBrothers(this.gameObject)))			&& this.transform.childCount == 2) // EDIT EDIT EDIT!
			this.gameObject.SetActive(false);
		else
			StartCoroutine(Destruction());
	}

	private IEnumerator Destruction()
	{
		// if I am destroyed, go trough everyone that is connected to me including my self and tell to start destroying your shields (flicker coroutine).
		foreach(GameObject mine in mineSys.ShieldMines)
		{
			if(mine.GetComponent<ShieldMine>().brother == this.gameObject && mine.transform.childCount > 2 || mine == this.gameObject) // Find others that have me as a brother, and my self
				mine.GetComponent<ShieldMine>().StartFlicker();
		}
		yield return null;
	}

	public void StartFlicker()
	{
		if(!startFlicker && this.gameObject.activeSelf)
		{
			StartCoroutine(StartFlickerRoutine());
			startFlicker = true;
		}
	}
	
	private IEnumerator StartFlickerRoutine()
	{
		if(this.transform.childCount > 2)
		{
			int flickers = 0;
			while(flickers < 3)
			{
				yield return new WaitForSeconds(0.05f);
				this.transform.GetChild(2).gameObject.SetActive(false);
				yield return new WaitForSeconds(0.05f);
				this.transform.GetChild(2).gameObject.SetActive(true);
				flickers++;
				yield return null;
			}
			finishFlicker = true; // signals that the flickering is finished
			Destroy(this.transform.GetChild(2).gameObject);
			shieldCaster = false; // NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW NEW
		}
		if(this.health <= 0)
			StartCoroutine(DisableMine());
	}

	private IEnumerator DisableMine()
	{
		if(this.brother == null)
		{
			while(true)
			{
				if(mineSys.LinkedFinishedFlickering(this.gameObject))
					this.gameObject.SetActive(false);
				yield return null;
			}
		}
		else
			this.gameObject.SetActive(false);
	}

}
