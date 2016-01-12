using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlowGun : MonoBehaviour 
{
	public Texture oxygenBarText;
	[Header("Audio")]
	public float soundFlux = 0;
	public int randomAudioFluxChance = 1;
	public float soundVolume = 1F;
	public float addRemoveSoundSpeed = 0.1F;
	[Space(10)]
	public float pushMaxSoundPitchAndPowerMultiplier = 1F;
	public float suckMaxSoundPitchAndPowerMultiplier = 1F;
	[Space(10)]
	public AudioClip soundEffect;
	public AudioClip soundGunPush;
	public AudioClip soundGunSuck;
	private int randomAudioNumberGen;
	private AudioSource gunEffect;
	private AudioSource pushAudioSource;
	private AudioSource suckAudioSource;
	private GameObject pushSoundObject;
	private GameObject suckSoundObject;

	[Header("Oxygen")]
	public float maxOgygen = 100F;
	public float oxygenCost = 0.2F;
	public float oxygenSuck = 0.2F;
	[HideInInspector]public float oxygen;

	[Header("Gun Modes")]
	public bool linearPull = false;
	public float linearPullMultiplier = 8F; // temp var
	public enum GunMode{
		suck,blow
	}
	public GunMode gunMode;

	[Header("Gun Properties")]
	public float pullAttractForce = 1F;
	public float sidewardPullForce = 0.5F;
	[Space(10)]
	public float frontalBlowForce = 1F;
	public float sidewardBlowForce = 0.5F;
	[Space(10)]
	public float sidewardThreshold = 0.5F;
	public float gunRange = 10F;

	[HideInInspector]public bool GunInUse = false;

	[Header("GameObjects")]
	public GameObject interActiveObject;
	public List <GameObject> InRangeObjects;
	public List <GameObject> LoadedTargets;
	public TargetFind[] foundObjects;
	[System.Serializable]
	public class TargetFind{
		public string TagName;
	}

	public Camera cam;

	void Awake()
	{
		cam = GameObject.FindGameObjectWithTag("MainCam").GetComponent<Camera>();
		CreateAudio();
		SetUpLists();
		oxygen = maxOgygen;
	}

	void SetUpLists()
	{
		InRangeObjects = new List<GameObject>();
		LoadedTargets = new List<GameObject>();
		foreach(TargetFind tf in foundObjects)
		{      
			GameObject[] TempLoadedTargets = GameObject.FindGameObjectsWithTag(tf.TagName);
			foreach(GameObject loadedTarget in TempLoadedTargets)
			{
				LoadedTargets.Add(loadedTarget);
			}
		}
	}

	void CreateAudio()
	{
		pushSoundObject = new GameObject();
		suckSoundObject = new GameObject();
		pushSoundObject.transform.position = this.transform.position;
		suckSoundObject.transform.position = this.transform.position;
		pushSoundObject.name = "PushSoundObject";
		suckSoundObject.name = "SuckSoundObject";
		pushSoundObject.transform.parent = this.transform;
		suckSoundObject.transform.parent = this.transform;
		gunEffect = gameObject.AddComponent<AudioSource>();
		pushSoundObject.AddComponent<AudioSource>();
		suckSoundObject.AddComponent<AudioSource>();
		pushAudioSource = pushSoundObject.GetComponent<AudioSource>();
		suckAudioSource = suckSoundObject.GetComponent<AudioSource>();
		pushAudioSource.spatialBlend = 1;
		suckAudioSource.spatialBlend = 1;
		gunEffect.spatialBlend = 1;
		gunEffect.clip = soundEffect;
		pushAudioSource.clip = soundGunPush;
		suckAudioSource.clip = soundGunSuck;
		pushAudioSource.loop = true;
		suckAudioSource.loop = true;
		pushAudioSource.pitch = 0F;
		suckAudioSource.pitch = 0F;
		gunEffect.volume = 0.8F;
		pushAudioSource.volume = 0F;
		suckAudioSource.volume = 0F;
	}

	void Update()
	{
		// TEMP
		if(Input.GetKeyDown(KeyCode.E) && gunMode == GunMode.suck)
		{
			gunMode = GunMode.blow;
			if(!gunEffect.isPlaying)gunEffect.Play();
		}
		else if (Input.GetKeyDown(KeyCode.E) && gunMode == GunMode.blow) 
		{
			gunMode = GunMode.suck;
			if(!gunEffect.isPlaying)gunEffect.Play();
		}
		// TEMP
		if(Input.GetMouseButton(0))
		{
			ActivateGun();
			GunInUse = true;
		}
		else 
			GunInUse = false;
		SilenceSound();
		if(oxygen < 0) 
			oxygen = 0;
		if(!GunInUse) 
			ResetSpecialTargetValues();
	}

	void ActivateGun()
	{
		randomAudioNumberGen = Mathf.RoundToInt(Random.Range(0,randomAudioFluxChance));
		GetTargets();
		CalcTargets();
		OxygenLoss();
		GenerateSound();
	}

	void GetTargets()
	{
		foreach(GameObject loadedTarget in LoadedTargets)
		{
			float targetDistance = Vector3.Distance(loadedTarget.transform.position, this.transform.position);
			if(targetDistance <= gunRange && !InRangeObjects.Contains(loadedTarget))
			{
				InRangeObjects.Add(loadedTarget);
			}
			else if(targetDistance > gunRange && InRangeObjects.Contains(loadedTarget))
			{
				InRangeObjects.Remove(loadedTarget);
			}
		}
	}

	void CalcTargets()
	{
		foreach(GameObject inRangeObject in InRangeObjects) // x is offset
		{
			if(this.transform.InverseTransformPoint(inRangeObject.transform.position).z > 0)
			{
				float dist = Vector3.Distance(inRangeObject.transform.position, this.transform.position);
				Vector3 relativeTargetPosition = this.transform.InverseTransformPoint(inRangeObject.transform.position);
				relativeTargetPosition.y = 0;
				relativeTargetPosition.z = 0;

				if(inRangeObject.tag == "Propellor" || inRangeObject.tag == "OxygenPlant" || inRangeObject.tag == "GeneratorTumbleGreed")
				{
					SpecialTargets(inRangeObject, relativeTargetPosition.x, dist, relativeTargetPosition.z);
				}
				else if(gunMode == GunMode.suck) 
					ManipTargetPull(dist, relativeTargetPosition.x, inRangeObject);
				else if(gunMode == GunMode.blow && oxygen > 0) 
					ManipTargetPush(dist, relativeTargetPosition.x, inRangeObject);
			}
		}
	}

	void SpecialTargets(GameObject target, float offset, float distance, float frontCheck)
	{
		if(target.tag == "OxygenPlant")
		{
			if(gunMode == GunMode.suck)
				RetrieveOxygen(target,offset,distance);
			else if(gunMode == GunMode.blow)
				InsertOxygen(target,offset,distance);
		}
		else if(target.tag == "Propellor")
		{
			interActiveObject = GameObject.FindGameObjectWithTag("Elevator");
			float MaxMoveUpSpeed = interActiveObject.GetComponent<Elevator>().maxMoveUpSpeed;
			interActiveObject.GetComponent<Elevator>().frontCheck = frontCheck;

			if(gunMode == GunMode.blow && oxygen > 0)
			{
				float gunValue = ((gunRange-distance)*1.4f )*0.01f;
				interActiveObject.GetComponent<Elevator>().elevatorSpeed += gunValue;
				interActiveObject.GetComponent<Elevator>().currentSpeedInput = gunValue;
			}
		}
		else if(target.tag == "GeneratorTumbleGreed")
		{
			interActiveObject = target;
			if(!interActiveObject.GetComponent<TumbleGreedInGenerator>().fleeBack)
			{
				if(gunMode == GunMode.suck) 
					ManipTargetPull(distance, offset, target);
				else if(gunMode == GunMode.blow && oxygen > 0) 
					ManipTargetPush(distance, offset, target);
			}
			interActiveObject.GetComponent<TumbleGreedInGenerator>().beingSuckedOut = true;

		}
	}

	void ResetSpecialTargetValues()
	{
		if(interActiveObject != null && interActiveObject.tag == "Elevator" && interActiveObject.GetComponent<Elevator>().currentSpeedInput != 0)
		{
			interActiveObject.GetComponent<Elevator>().currentSpeedInput = 0;

		}
		else if(interActiveObject != null && interActiveObject.tag == "GeneratorTumbleGreed")
			interActiveObject.GetComponent<TumbleGreedInGenerator>().beingSuckedOut = false;
	}

	void ManipTargetPush(float distance, float offset, GameObject target)
	{
		if(offset > -sidewardThreshold && offset < sidewardThreshold){
			Rigidbody rb = null;
			if(rb == null) rb = target.GetComponent<Rigidbody>();

			float frontalForce = (gunRange-distance) * frontalBlowForce;
			float sidewardForce = (offset*sidewardBlowForce) * (gunRange-distance);//sidewardBlowForce;

			Vector3 PlayerDirectionFrontal = this.transform.TransformDirection(Vector3.forward * (frontalForce * pushAudioSource.pitch));
			Vector3 PlayerDirectionSideward = this.transform.TransformDirection(Vector3.right * (sidewardForce * pushAudioSource.pitch));
			Vector3 PlayerDirectionCalc = PlayerDirectionFrontal + PlayerDirectionSideward;

			rb.AddForce(PlayerDirectionCalc);
		}
	}

	void ManipTargetPull(float distance, float offset, GameObject target)
	{
		if(linearPull)
			target.transform.position = Vector3.Lerp(target.transform.position, this.transform.position, (pullAttractForce/1000)*((gunRange-distance)*linearPullMultiplier) );
		else if(offset > -sidewardThreshold && offset < sidewardThreshold)
		{
			Rigidbody rb = null;
			if(rb == null) rb = target.GetComponent<Rigidbody>();

			float pullForce = (gunRange-distance) * pullAttractForce;
			float pullSideForce = (offset*sidewardPullForce) * (gunRange-distance);

			Vector3 PlayerDirectionBackward = this.transform.TransformDirection(Vector3.back * (pullForce * suckAudioSource.pitch));
			Vector3 PlayerDirectionInward = this.transform.TransformDirection(Vector3.left * (pullSideForce * suckAudioSource.pitch));
			Vector3 PlayerDirectionPullCalc = PlayerDirectionBackward + PlayerDirectionInward;

			rb.AddForce(PlayerDirectionPullCalc);
		}
	}

	void OxygenLoss()
	{
		if(gunMode == GunMode.blow && oxygen > 0) oxygen -= oxygenCost;
	}

	void RetrieveOxygen(GameObject selecTarget, float offset, float distance)
	{
		if(oxygen < maxOgygen)
		{
			float finalkOxSuck = oxygenSuck;
			selecTarget.GetComponent<Plant>().oxygenHolding -= finalkOxSuck;
			oxygen += finalkOxSuck;
		}
	}

	void InsertOxygen(GameObject selecTarget, float offset, float distance)
	{
		if(oxygen > 0 && selecTarget.GetComponent<Plant>().oxygenHolding < selecTarget.GetComponent<Plant>().oxygenStorage)
		{
			float finalkOxSuck = oxygenSuck;
			selecTarget.GetComponent<Plant>().oxygenHolding += finalkOxSuck;
			oxygen -= finalkOxSuck;
		}
	}

	void GenerateSound()
	{
		if(oxygen > 0 && gunMode == GunMode.blow)
		{
			if(!pushAudioSource.isPlaying) pushAudioSource.Play();
			if(pushAudioSource.pitch < pushMaxSoundPitchAndPowerMultiplier) pushAudioSource.pitch += addRemoveSoundSpeed;
			if(pushAudioSource.volume < soundVolume) pushAudioSource.volume += addRemoveSoundSpeed;
			if(pushAudioSource.volume >= soundVolume && pushAudioSource.pitch >= pushMaxSoundPitchAndPowerMultiplier && randomAudioNumberGen == 0)
				pushAudioSource.pitch = Mathf.Lerp(pushAudioSource.pitch, pushMaxSoundPitchAndPowerMultiplier + Random.Range(0F,soundFlux), 0.1F);
		}
		if(gunMode == GunMode.suck)
		{
			if(!suckAudioSource.isPlaying) suckAudioSource.Play();
			if(suckAudioSource.pitch < suckMaxSoundPitchAndPowerMultiplier) suckAudioSource.pitch += addRemoveSoundSpeed;
			if(suckAudioSource.volume < soundVolume) suckAudioSource.volume += addRemoveSoundSpeed;
			if(suckAudioSource.volume >= soundVolume && suckAudioSource.pitch >= suckMaxSoundPitchAndPowerMultiplier && randomAudioNumberGen == 0)
				suckAudioSource.pitch = Mathf.Lerp(suckAudioSource.pitch, suckMaxSoundPitchAndPowerMultiplier + Random.Range(0F,soundFlux), 0.1F);
		}
	}

	void SilenceSound()
	{
		if(gunMode == GunMode.suck || oxygen <= 0)
		{
			if(pushAudioSource.pitch > 0) pushAudioSource.pitch -= addRemoveSoundSpeed;
			if(pushAudioSource.volume > 0) pushAudioSource.volume -= addRemoveSoundSpeed;
			if(pushAudioSource.pitch <= 0 && pushAudioSource.volume <=0 )
			{
				pushAudioSource.pitch = 0;
				pushAudioSource.Stop();
			}
		}
		else if(gunMode == GunMode.blow)
		{
			if(suckAudioSource.pitch > 0) suckAudioSource.pitch -= addRemoveSoundSpeed;
			if(suckAudioSource.volume > 0) suckAudioSource.volume -= addRemoveSoundSpeed;
			if(suckAudioSource.pitch <= 0 && suckAudioSource.volume <=0 )
			{
				suckAudioSource.pitch = 0;
				suckAudioSource.Stop();
			}
		}
		if(!GunInUse)
		{
			if(pushAudioSource.pitch > 0) pushAudioSource.pitch -= addRemoveSoundSpeed*(pushAudioSource.pitch);
			if(pushAudioSource.volume > 0) pushAudioSource.volume -= addRemoveSoundSpeed;
			if(suckAudioSource.pitch > 0) suckAudioSource.pitch -= addRemoveSoundSpeed*(suckAudioSource.pitch);
			if(suckAudioSource.volume > 0) suckAudioSource.volume -= addRemoveSoundSpeed;

			if(pushAudioSource.pitch <= 0 && pushAudioSource.volume <=0 )
			{
				pushAudioSource.pitch = 0;
				pushAudioSource.Stop();
			}
			if(suckAudioSource.pitch <= 0 && suckAudioSource.volume <=0 )
			{
				suckAudioSource.pitch = 0;
				suckAudioSource.Stop();
			}
		}
	}

	public void OnGUI()
	{
		Vector3 OxygenBarSize = cam.ViewportToScreenPoint(new Vector3(0.01f,0.03f,0));
		Rect oxygenBar = new Rect((Screen.width)-OxygenBarSize.x, 10 ,(-oxygen*2), OxygenBarSize.y);
		UnityEngine.GUI.DrawTexture(oxygenBar, oxygenBarText,ScaleMode.StretchToFill);
	}

}