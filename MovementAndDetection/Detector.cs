using UnityEngine;
using System.Collections;

public class Detector : MonoBehaviour 
{

	private SphereCollider detSphere;
	private CapsuleCollider detCap;
	private Rigidbody rB;
	private GameObject sensor;

	public bool debug_ColCheckOff;

	public float sensorDistance = 2;
	public float capsuleWidth = 4;
	
	public bool frontObstructed = false;
	public bool leftObstructed = false;
	public bool rightObstructed = false;
	public bool collision = false;
	
	private smallEnemy enemyScrpt;

	void Awake()
	{
		enemyScrpt = this.GetComponent<smallEnemy>();

		sensor = new GameObject("sensor");
		sensor.transform.SetParent(this.transform);
		sensor.transform.position = this.transform.position;
		DetectorChild childScript = sensor.gameObject.AddComponent<DetectorChild>();
		childScript.debug_ColCheckOff = debug_ColCheckOff; // TEMP DEBUG ADITION
		sensor.layer = 2;

		rB = sensor.AddComponent<Rigidbody>();
		rB.useGravity = false;
		rB.constraints = RigidbodyConstraints.FreezeAll;

		if(GetComponentInParent<smallEnemy>().movementType == smallEnemy.MovementType.A)
		{
			detSphere = sensor.AddComponent<SphereCollider>();
			detSphere.radius = sensorDistance;
			detSphere.isTrigger = true;
			Physics.IgnoreCollision(detSphere, this.GetComponent<Collider>());
		}
		else if(GetComponentInParent<smallEnemy>().movementType == smallEnemy.MovementType.B || GetComponentInParent<smallEnemy>().movementType == smallEnemy.MovementType.C)
		{
			detCap = sensor.AddComponent<CapsuleCollider>();
			detCap.radius = sensorDistance;
			detCap.height = capsuleWidth*2;
			detCap.direction = 2;
			Physics.IgnoreCollision(detCap, this.GetComponent<Collider>());
		}

	}

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.CompareTag("Board"))
		{
			enemyScrpt.health -= 10; // TEMP
		}
		if(other.gameObject.CompareTag("PlayerProjectile"))
		{
			enemyScrpt.health -= other.GetComponent<Projectile_BeamMovement>().damage;
		}
		if(other.gameObject.CompareTag ("PlayerTurret")) 
		{
			GameObject.FindGameObjectWithTag("PlayerTurret").GetComponent<PlayerTurret>().shaking = true;
			enemyScrpt.kamikaze = true;
			enemyScrpt.health = 0;
		}
	}
	void OnTriggerStay(Collider other)
	{
		if(other.gameObject.CompareTag("SuperBeam"))
		{
			enemyScrpt.health -= other.transform.parent.GetComponent<SuperBeam>().superBeamDamage;
		}
	}
}
