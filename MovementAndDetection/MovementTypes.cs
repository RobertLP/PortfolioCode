using UnityEngine;
using System.Collections;

public class MovementTypes : MonoBehaviour
{
	private float enginePowerX;
	private float enginePowerZ;
	private float accelerationX;
	private float accelerationZ;
	private float friction;
	private float velocityX;
	private float velocityZ;
	private float maxVelocityX;
	private float maxVelocityZ;
	private int movementType;
	private Vector3 spawnDirection;

	public float timer;
	public bool zigZagDir;

	void Awake()
	{
		zigZagDir = (Random.Range(0,2) == 0) ? true : false;
	}

	public void InsertInfo(float enginePowX, float enginePowZ, float accelX, float accelZ, float frict, float velX, float velZ, float maxVelX, float maxVelZ, int movType, Vector3 spawnDir)
	{
		enginePowerX = enginePowX;
		enginePowerZ = enginePowZ;
		accelerationX = accelX;
		accelerationZ = accelZ;
		friction = frict;
		velocityX = velX;
		velocityZ = velZ;
		maxVelocityX = maxVelX;
		maxVelocityZ = maxVelZ;
		movementType = movType;
		spawnDirection = spawnDir;
	}
	public void InsertInfo(float enginePowX, float enginePowZ, float accelX, float accelZ, float frict, float velX, float velZ, float maxVelX, float maxVelZ, Vector3 spawnDir)
	{
		enginePowerX = enginePowX;
		enginePowerZ = enginePowZ;
		accelerationX = accelX;
		accelerationZ = accelZ;
		friction = frict;
		velocityX = velX;
		velocityZ = velZ;
		maxVelocityX = maxVelX;
		maxVelocityZ = maxVelZ;
		spawnDirection = spawnDir;
	}


	public Vector3 MoveNormal()
	{
		velocityX += accelerationX * enginePowerX * Time.deltaTime;
		velocityX -= velocityX * friction * Time.deltaTime;
		velocityX = Mathf.Clamp(velocityX,-maxVelocityX,maxVelocityX);
		return new Vector3 (velocityX,0,0);
	}

	public Vector3 BankLeft()
	{
		velocityZ += accelerationZ * enginePowerZ * Time.deltaTime;
		velocityZ -= velocityZ * friction * Time.deltaTime;
		velocityZ = Mathf.Clamp(velocityZ,-maxVelocityZ,maxVelocityZ);
		return new Vector3 (0,0,velocityZ);
	}

	public Vector3 BankRight()
	{
		velocityZ += accelerationZ * -enginePowerZ * Time.deltaTime;
		velocityZ -= velocityZ * friction * Time.deltaTime;
		velocityZ = Mathf.Clamp(velocityZ,-maxVelocityZ,maxVelocityZ);
		return new Vector3 (0,0, velocityZ);
	}

	public Vector3 MoveSlow()
	{
		velocityX += accelerationX * (-enginePowerX/3) * Time.deltaTime; // was -enginePowerX or enginePowerX/3
		velocityX -= velocityX * friction * Time.deltaTime;
		velocityX = Mathf.Clamp(velocityX,-maxVelocityX,maxVelocityX);
		return new Vector3 (velocityX,0,0);
	}

	public Vector3 noBank()
	{
		velocityZ += accelerationZ * 0 * Time.deltaTime;
		velocityZ -= velocityZ * 1 * Time.deltaTime; // 1 replaced friction
		velocityZ = Mathf.Clamp(velocityZ,-maxVelocityZ,maxVelocityZ);
		return new Vector3 (0,0, velocityZ);
	}

	public Vector3 MovePong(int eN)
	{
		if(eN == 0) // Left
			return BankLeft();
		else // Right
			return BankRight();
	}

	public Vector3 MoveZigZag(float switchDirTime, bool forcedDirChange)
	{
		timer += Time.deltaTime;
		if(timer > switchDirTime || forcedDirChange)
		{
			zigZagDir = !zigZagDir;
			timer = 0;
		}
		if(zigZagDir) // Left
			return BankLeft();
		else // Right
			return BankRight();
	}

	public Vector3 MovePuls(float pulseTime, float pulseBreak, float pulseMinimumSpeed)
	{
		timer += Time.deltaTime;
		if(timer < pulseTime)
		{
			velocityX += accelerationX * enginePowerX * Time.deltaTime;
			velocityX -= velocityX * friction * Time.deltaTime;
			velocityX = Mathf.Clamp(velocityX,-maxVelocityX,maxVelocityX);
		}
		else
		{
			velocityX += accelerationX * 0 * Time.deltaTime;
			velocityX -= velocityX * pulseBreak * Time.deltaTime;
			velocityX = Mathf.Clamp(velocityX,-maxVelocityX,maxVelocityX);
		}
		if(velocityX < (maxVelocityX/100)*pulseMinimumSpeed)
			timer = 0;
		return new Vector3 (velocityX,0,0);
	}

	public Vector3 MoveNormalHeavy()
	{
		return new Vector3(0,0,0); // TEMP
	}

	public Vector3 MoveNormalBoss()
	{
		return new Vector3(0,0,0); // TEMP
	}
	
}
