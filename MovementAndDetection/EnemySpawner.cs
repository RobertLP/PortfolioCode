using UnityEngine;
using System.Collections;

public class EnemySpawner : MonoBehaviour 
{
	#region Pools
	[Header("Enemy Pools")]
	public GameObject smallNormalEnemyPool;
	private ObjPoolerScript smallNormalEnemyPoolscrpt;

	public GameObject smallPongEnemyPool;
	private ObjPoolerScript smallPongEnemyPoolscrpt;

	public GameObject smallZigZagEnemyPool;
	private ObjPoolerScript smallZigZagEnemyPoolscrpt;

	public GameObject smallPulseEnemyPool;
	private ObjPoolerScript smallPulseEnemyPoolscrpt;
	#endregion

	#region SpawnActivators
	[Header("Spawn-activators")]
	public bool normalSmallSpawn;
	public bool pongSmallSpawn;
	public bool zigZagSmallSpawn;
	public bool pulseSmallSpawn;
	#endregion

	#region Spawn Data
	private int smallNormalEnemiesSpawned = 0;
	private int smallPulseEnemiesSpawned = 0;
	private int smallPongEnemiesSpawned = 0;
	private int smallZigZagEnemiesSpawned = 0;
	#endregion

	#region Objects to spawn
	[Header("ObjectSpawns")]
	public int zigzagToSpawn;
	public int normalToSpawn;
	public int pongToSpawn;
	public int pulseToSpawn;
	[HideInInspector]
	public int spawnType = -2; // 0, random. 1, linear
	private int prevSpawnType = -1;
	[HideInInspector]
	public int burstMin = 4;
	[HideInInspector]
	public int burstMax = 5;
	[HideInInspector]
	public float burstPauze;
	#endregion

	#region Timer
	private float timerZigZagPong;
	private float timerSmallNormal;
	private float timerPulse;
	[Header("Timers")]
	public float timeNextSmallNormal;
	public float timeNextZigZagPong;
	public float timeNextPulse;
	#endregion
	public bool spawnUnlimitedEverything;

	private GameObject player;

	void InstantiatePools()
	{
		//instantiate normal pool
		smallNormalEnemyPool = (GameObject)Instantiate(smallNormalEnemyPool);
		smallNormalEnemyPool.transform.SetParent(this.transform);
		smallNormalEnemyPool.name = "SmallNormalEnemyPool";
		smallNormalEnemyPoolscrpt = smallNormalEnemyPool.GetComponent<ObjPoolerScript>();
		//instantiate pong pool
		smallPongEnemyPool = (GameObject)Instantiate(smallPongEnemyPool);
		smallPongEnemyPool.transform.SetParent(this.transform);
		smallPongEnemyPool.name = "SmallPongEnemyPool";
		smallPongEnemyPoolscrpt = smallPongEnemyPool.GetComponent<ObjPoolerScript>();
		//instantiate zigzag pool
		smallZigZagEnemyPool = (GameObject)Instantiate(smallZigZagEnemyPool);
		smallZigZagEnemyPool.transform.SetParent(this.transform);
		smallZigZagEnemyPool.name = "SmallZigZagEnemyPool";
		smallZigZagEnemyPoolscrpt = smallZigZagEnemyPool.GetComponent<ObjPoolerScript>();
		//instantiate pulse pool
		smallPulseEnemyPool = (GameObject)Instantiate(smallPulseEnemyPool);
		smallPulseEnemyPool.transform.SetParent(this.transform);
		smallPulseEnemyPool.name = "SmallPulseEnemyPool";
		smallPulseEnemyPoolscrpt = smallPulseEnemyPool.GetComponent<ObjPoolerScript>();
	}

	void CounterMilliSeconds()
	{
		timerSmallNormal += 0.01f;
		timerZigZagPong += 0.01f;
		timerPulse += 0.01f;
	}

	void Awake()
	{
		InstantiatePools();
		InvokeRepeating("CounterMilliSeconds",0.01f,0.01f);
	}

	void Spawn()
	{
		player = GameObject.FindGameObjectWithTag("PlayerTurret");
		if(timerSmallNormal > timeNextSmallNormal && normalSmallSpawn)
			SmallEnemyNormal();
		if(timerZigZagPong > timeNextZigZagPong && pongSmallSpawn)
			SmallEnemyPong();
		if(timerZigZagPong > timeNextZigZagPong && zigZagSmallSpawn)
			SmallEnemyZigZag();
		if(timerPulse > timeNextPulse && pulseSmallSpawn)
			SmallEnemyPulse();
	}

	void SmallEnemyNormal()
	{
		GameObject smallEnemyNormal = smallNormalEnemyPoolscrpt.GetPooledObj();
		if(smallEnemyNormal != null)
		{
			Vector3 spawnLoc = spawnLocation(smallEnemyNormal.transform.localScale.x,smallEnemyNormal.transform.localScale.z,player.transform.position.y);
			bool spawnClear = SpawnAreaClear(smallEnemyNormal.transform.localScale.x, smallEnemyNormal.transform.localScale.z, spawnLoc, "Enemy", 2, 2);
			if(spawnClear)
			{
				smallEnemyNormal.transform.position = spawnLoc;
				smallEnemyNormal.name = "smallEnemyNormal" + smallNormalEnemiesSpawned;
				smallEnemyNormal.SetActive(true);
				smallNormalEnemiesSpawned++;
				timerSmallNormal = 0;
			}
		}
	}

	void SmallEnemyPong()
	{
		GameObject smallEnemyPong = smallPongEnemyPoolscrpt.GetPooledObj();
		if(smallEnemyPong != null)
		{
			Vector3 spawnLoc = spawnLocation(smallEnemyPong.transform.localScale.x,smallEnemyPong.transform.localScale.z,player.transform.position.y-0.16f); // -0.13 zigzang and pong layer
			bool spawnClear = SpawnAreaClear(smallEnemyPong.transform.localScale.x, smallEnemyPong.transform.localScale.z, spawnLoc, "Enemy", 2, 2);
			if(spawnClear)
			{
				smallEnemyPong.transform.position = spawnLoc;
				smallEnemyPong.name = "smallEnemyPong" + smallPongEnemiesSpawned;
				smallEnemyPong.SetActive(true);
				smallPongEnemiesSpawned++;
				timerZigZagPong = 0;
			}
		}
	}

	void SmallEnemyZigZag()
	{
		GameObject smallEnemyZigZag = smallZigZagEnemyPoolscrpt.GetPooledObj();
		if(smallEnemyZigZag != null)
		{
			Vector3 spawnLoc = spawnLocation(smallEnemyZigZag.transform.localScale.x,smallEnemyZigZag.transform.localScale.z,player.transform.position.y-0.16f); // -0.13 zigzang and pong layer
			bool spawnClear = SpawnAreaClear(smallEnemyZigZag.transform.localScale.x, smallEnemyZigZag.transform.localScale.z, spawnLoc, "Enemy", 2, 2);
			if(spawnClear)
			{
				smallEnemyZigZag.transform.position = spawnLoc;
				smallEnemyZigZag.name = "smallEnemyZigZag" + smallZigZagEnemiesSpawned;
				smallEnemyZigZag.SetActive(true);
				smallZigZagEnemiesSpawned++;
				timerZigZagPong = 0;
			}
		}
	}

	void SmallEnemyPulse()
	{
		GameObject smallEnemyPulse = smallPulseEnemyPoolscrpt.GetPooledObj();
		if(smallEnemyPulse != null)
		{
			Vector3 spawnLoc = spawnLocation(smallEnemyPulse.transform.localScale.x*15,smallEnemyPulse.transform.localScale.z,player.transform.position.y);
			bool spawnClear = SpawnAreaClear(smallEnemyPulse.transform.localScale.x, smallEnemyPulse.transform.localScale.z, spawnLoc, "Enemy", 2, 2);
			if(spawnClear)
			{
				smallEnemyPulse.transform.position = spawnLoc;
				smallEnemyPulse.name = "smallEnemyNormal" + smallPulseEnemiesSpawned;
				smallEnemyPulse.SetActive(true);
				smallPulseEnemiesSpawned++;
				timerPulse = 0;
			}
		}
	}

	Vector3 spawnLocation(float distRampEdge, float objWidth, float spwnHeight)
	{
		float randomPos = Random.Range(0.0f,1.0f);
		Vector3 calcRandomViewport = new Vector3(0,randomPos,Camera.main.transform.position.y-player.transform.position.y);
		Vector3 viewportToWorld =  Camera.main.ViewportToWorldPoint(calcRandomViewport);
		return new Vector3(viewportToWorld.x,spwnHeight,viewportToWorld.z) - new Vector3(distRampEdge,0,0);
	}

	bool SpawnAreaClear(float xScale, float zScale, Vector3 spawnLoc, string checkTag, int raysPerUnitX, int raysPerUnitZ)
	{
		RaycastHit hit;
		Ray ray;
		float rayStepsX = xScale/raysPerUnitX;
		float rayStepsZ = zScale/raysPerUnitZ;
		for(int i = 0; i < raysPerUnitX +1; i++ )
		{
			for(int o = 0; o < raysPerUnitZ +1 ; o++ )
			{
				ray = new Ray(new Vector3(spawnLoc.x - (xScale/2) + (i*rayStepsX),spawnLoc.y-5, spawnLoc.z - (zScale/2) + (o*rayStepsZ)), Vector3.up);
				
				if (Physics.Raycast(ray, out hit) && hit.transform.tag.Contains(checkTag))
				{
					Debug.DrawLine(new Vector3(spawnLoc.x - (xScale/2) + (i*rayStepsX),spawnLoc.y-5,spawnLoc.z - (zScale/2) + (o*rayStepsZ)), hit.point, Color.red);
					return false;
				}
			}
		}
		return true;
	}

	void Update () 
	{
		Spawn();
		// if all tospawns are spawned. stop the coroutines
		// insert function here
		ModeCheck();
	}

	void ModeCheck()
	{
		//StartCoroutine(RandomSpawn());
		//StartCoroutine(BurstSpawn(5,5,0));
		if(spawnType != prevSpawnType && !EverythingSpawned())
		{
			StopAllCoroutines();
			if(spawnType == 0)
				StartCoroutine(RandomSpawn());
			else if(spawnType == 1)
				StartCoroutine(BurstSpawn(burstMin, burstMax, burstPauze));//5,5,0));
			prevSpawnType = spawnType;
		}
	}

	// spawn paterns.
	IEnumerator RandomSpawn()
	{
		while(!EverythingSpawned() || spawnUnlimitedEverything) // temp spawnUnlimitedEverything
		{
			int rand = Random.Range(0,4);

			// check if selected rand is not already spawned maximally
			if(checkIfSpawnable(rand) || spawnUnlimitedEverything) // temp spawnUnlimitedEverything
			{
				switch(rand)
				{
					case 0:
						// spawn normal
						normalSmallSpawn = true;
						normalToSpawn--;
						yield return new WaitForSeconds(0.005f);
						normalSmallSpawn = false;
						yield return new WaitForSeconds(timeNextSmallNormal);
						break;
					case 1:
						// spawn zigzag
						zigZagSmallSpawn = true;
						zigzagToSpawn--;
						yield return new WaitForSeconds(0.005f);
						zigZagSmallSpawn = false;
						yield return new WaitForSeconds(timeNextZigZagPong);
						break;
					case 2:
						// spawn pong
						pongSmallSpawn = true;
						pongToSpawn--;
						yield return new WaitForSeconds(0.005f);
						pongSmallSpawn = false;
						yield return new WaitForSeconds(timeNextZigZagPong);
						break;
					case 3:
						// spawn pulse
						pulseSmallSpawn = true;
						pulseToSpawn--;
						yield return new WaitForSeconds(0.005f);
						pulseSmallSpawn = false;
						yield return new WaitForSeconds(timeNextPulse);
						break;
					default:
						// spawn normal
						normalSmallSpawn = true;
						normalToSpawn--;
						yield return new WaitForSeconds(0.005f);
						normalSmallSpawn = false;
						yield return new WaitForSeconds(timeNextSmallNormal);
						break;
				}
			}
			yield return null;
		}
	}

	IEnumerator BurstSpawn(int burstMin, int burstMax, float pauzeAfterBurst)
	{
		while(!EverythingSpawned())
		{
			// pick ship to burst
			int rand = Random.Range(0,4);
			// how much do I need to burst
			int burstRand = Random.Range(burstMin,burstMax);
			// run until burst is done
			while(burstRand > 0)
			{
				// check if selected rand is not already spawned maximally
				if(!checkIfSpawnable(rand))
					break;

				switch(rand)
				{
				case 0:
					// spawn normal
					normalSmallSpawn = true;
					burstRand--;
					normalToSpawn--;
					yield return new WaitForSeconds(0.005f);
					normalSmallSpawn = false;
					yield return new WaitForSeconds(timeNextSmallNormal);
					break;
				case 1:
					// spawn zigzag
					zigZagSmallSpawn = true;
					burstRand--;
					zigzagToSpawn--;
					yield return new WaitForSeconds(0.005f);
					zigZagSmallSpawn = false;
					yield return new WaitForSeconds(timeNextZigZagPong);
					break;
				case 2:
					// spawn pong
					pongSmallSpawn = true;
					burstRand--;
					pongToSpawn--;
					yield return new WaitForSeconds(0.005f);
					pongSmallSpawn = false;
					yield return new WaitForSeconds(timeNextZigZagPong);
					break;
				case 3:
					// spawn pulse
					pulseSmallSpawn = true;
					burstRand--;
					pulseToSpawn--;
					yield return new WaitForSeconds(0.005f);
					pulseSmallSpawn = false;
					yield return new WaitForSeconds(timeNextPulse);
					break;
				default:
					// spawn normal
					normalSmallSpawn = true;
					burstRand--;
					normalToSpawn--;
					yield return new WaitForSeconds(0.005f);
					normalSmallSpawn = false;
					yield return new WaitForSeconds(timeNextSmallNormal);
					break;
				}

				yield return null;
			}


			yield return new WaitForSeconds(pauzeAfterBurst);
		}
	}

	bool checkIfSpawnable(int shipType)
	{
		if(shipType == 0 && normalToSpawn > 0)
			return true;
		else if(shipType == 1 && zigzagToSpawn > 0)
			return true;
		else if(shipType == 2 && pongToSpawn > 0)
			return true;
		else if(shipType == 3 && pulseToSpawn > 0)
			return true;
		return false;
	}

	bool EverythingSpawned()
	{
		if(normalToSpawn > 0 || zigzagToSpawn > 0 || pongToSpawn > 0 || pulseToSpawn > 0)
			return false;
		return true;
	}



}

