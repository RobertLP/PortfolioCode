using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class smallEnemy : MonoBehaviour 
{
	public GameObject modelMesh;

	#region Movement
	[Header("ScreenEdgeDetection")]
	public bool offScreenDetection = true;
	public float disableAngleThreshold = 10;
	#endregion
	
	#region Movement
	[Header("Movement")]
	public float enginePowerX = 1;
	public float enginePowerZ = 1;
	public float accelerationX = 5f;
	public float accelerationZ = 5f;
	public float friction = 0;
	public float maxVelocityX;
	public float maxVelocityZ;

	[HideInInspector] public float velocityX;
	[HideInInspector] public float velocityZ;

	public float bMov_edgeOffset;

	public float pulseTime;
	public float pulseBrake;
	public float pulseMinimumSpeedPercentofMaxVel;
	public float switchDirTime;

	private Detector sensor;
	private MovementTypes movType;


	public MovementType movementType;
	public enum MovementType
	{
		A, B, C, D
	}
	public ZigZagDir zigZagDir;
	public enum ZigZagDir
	{
		Left, Right
	}

	private Vector3 spawnDirection;
	#endregion

	#region Orders
	[Header("Orders")]

	public bool offScreenLeft;
	public bool offScreenRight;

	public bool orderedLeft;
	public bool orderedRight;

	public string receivedOrder;
	public GameObject senderOfOrder;
	#endregion

	#region Status
	[Header("Status")]
	public float health = 10;
	private float startHealth;
	[HideInInspector]public bool kamikaze;
	#endregion

	#region Effects
	[Header("Effects")]
	public float bankDegrees = 45f;
	public float bankToZeroSpeed = 0.1f;
	private float bankRot = 0;
	#endregion

	void Awake()
	{
		if(modelMesh == null)
			modelMesh = this.gameObject;
		startHealth = health;
		sensor = this.GetComponent<Detector>();
		movType = this.gameObject.AddComponent<MovementTypes>();
	}

	void Update ()
	{

		if(movementType == MovementType.A)
		{
			CheckPos();
			MovementTypeA();
		}
		else if(movementType == MovementType.B)
		{
			CheckPosAlt();
			MovementTypeB();
		}
		else if(movementType == MovementType.C)
		{
			CheckPosAlt();
			MovementTypeC();
		}
		else if(movementType == MovementType.D)
		{
			CheckPos();
			MovementTypeD();
		}
		if(health <= 0)
			DestroyEnemy();

		BankRotation();

		EdgeDetection();
	}

	void EdgeDetection()
	{
		float angledifferenceWithCam = Quaternion.Angle(this.transform.rotation, Camera.main.transform.parent.transform.rotation);
		if(angledifferenceWithCam > disableAngleThreshold)
			offScreenDetection = false;
		else
			offScreenDetection = true;
	}

	void BankRotation()
	{
		if(checkSensor() == 0)
		{
			bankRot = Mathf.Lerp(bankRot,0,bankToZeroSpeed);
		}
		else
		{
			bankRot = Mathf.Clamp(velocityZ*1000,-bankDegrees,bankDegrees);
		}
		modelMesh.transform.localEulerAngles = new Vector3(bankRot,0,0); // FIX ME!
	}

	void DestroyEnemy()
	{
		if(kamikaze)
		{
			//Debug.Log("Kamikaze!");
			this.GetComponent<PointSystem>().Kamikaze();
			this.gameObject.SetActive(false);
			kamikaze = false;
		}
		else
		{
			//Debug.Log("Normal death");
			this.GetComponent<PointSystem>().Killed();
			this.gameObject.SetActive(false);
		}
	}

	void CheckPos()
	{
		if(Camera.main.WorldToViewportPoint(this.transform.position - this.transform.localScale).x > 1)
			gameObject.SetActive(false);
		else if(Camera.main.WorldToViewportPoint(this.transform.position + (this.transform.localScale/2)).y > 1)
			offScreenLeft = true;
		else if(Camera.main.WorldToViewportPoint(this.transform.position - (this.transform.localScale/2)).y < 0)
			offScreenRight = true;
		else
		{
		    offScreenLeft = false;
			offScreenRight = false;
		}
	}

	void CheckPosAlt()
	{
		if(Camera.main.WorldToViewportPoint(this.transform.position - this.transform.localScale).x > 1)
			gameObject.SetActive(false);
		else if(Camera.main.WorldToViewportPoint(this.transform.position + (this.transform.localScale/2) + new Vector3(0,0,bMov_edgeOffset) ).y  > 1)
		{
			offScreenLeft = true;
			offScreenRight = false;
		}
		else if(Camera.main.WorldToViewportPoint(this.transform.position - (this.transform.localScale/2) - new Vector3(0,0,bMov_edgeOffset) ).y  < 0)
		{
			offScreenRight = true;
			offScreenLeft = false;
		}
		else
		{
			offScreenLeft = false;
			offScreenRight = false;
		}
	}
	
	void MovementTypeA()
	{
		velocityX = MovementNormal().x;
		velocityZ = MovementNormal().z;
		transform.Translate(new Vector3(velocityX,0,velocityZ));
	}
	void MovementTypeB()
	{
		velocityX = MovementPong().x;
		velocityZ = MovementPong().z;
		transform.Translate(new Vector3(velocityX,0,velocityZ));
	}
	void MovementTypeC()
	{
		velocityX = MovementZigZag().x;
		velocityZ = MovementZigZag().z;
		transform.Translate(new Vector3(velocityX,0,velocityZ));
	}
	void MovementTypeD()
	{
		velocityX = MovementPulse().x;
		velocityZ = MovementPulse().z;
		transform.Translate(new Vector3(velocityX,0,velocityZ));
	}


	int checkSensor()
	{
		comChannel();

		if(orderedLeft)
			return -2;
		else if(orderedRight)
			return -1;
		else if(sensor.frontObstructed && (!sensor.rightObstructed || !sensor.leftObstructed)) // obstructed in front.
			return 1;
		else if(offScreenLeft && !sensor.frontObstructed && offScreenDetection) // offscreen left
			return 2;
		else if(offScreenRight && !sensor.frontObstructed && offScreenDetection) // offscreen right
			return 3;
		else if(offScreenLeft && sensor.frontObstructed && offScreenDetection) // offscreen left and obstructed front
			return 4;
		else if(offScreenRight && sensor.frontObstructed && offScreenDetection) // offscreen right and obstructed front
			return 5;
		else if(sensor.frontObstructed && sensor.leftObstructed && !sensor.rightObstructed) // obstructed front and left.
			return 6;
		else if(sensor.frontObstructed && sensor.rightObstructed && !sensor.leftObstructed) // obstructed front and right.
			return 7;
		else if(sensor.frontObstructed && sensor.rightObstructed && sensor.leftObstructed ) // obstructed front, left and right.
			return 8;
		else if(sensor.rightObstructed && !sensor.leftObstructed && !sensor.frontObstructed) // obstructed right.
			return 9;
		else if(sensor.leftObstructed && !sensor.rightObstructed && !sensor.frontObstructed) // obstructed left.
			return 10;
		else if(sensor.rightObstructed && sensor.leftObstructed && !sensor.frontObstructed) // obstructed right and left.
			return 11;
		return 0; // return 0 if nothing is obstructed
	}

	Vector3 MovementNormal()
	{
		switch(checkSensor())
		{
			case -2:
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveNormal() + movType.BankLeft();

			case -1:
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveNormal() + movType.BankRight();

			case 0: // Nothing obstructed
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveNormal() + movType.noBank();

			case 1: // obstructed in front.
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveSlow() + movType.noBank();

			case 2: // offscreen Left
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveNormal() + movType.BankRight();

			case 3: // offscreen Right
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveNormal() + movType.BankLeft();

			case 4: // offscreen Left and obstructed front
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveSlow() + movType.BankRight();
			
			case 5: // offscreen Right and obstructed front
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveSlow() + movType.BankLeft();
			
			case 6: // obstructed front and left.
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveSlow() + movType.BankRight();

			case 7: // obstructed front and right.
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveSlow() + movType.BankLeft();

			case 8: // obstructed front, left and right.
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveSlow() + movType.noBank();

			case 9: // obstructed right.
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveNormal() + movType.BankLeft();

			case 10: // obstructed left.
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveNormal() + movType.BankRight();

			case 11: // obstructed right and left.
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveNormal() + movType.noBank();

			default:  // Nothing obstructed
				movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
				return movType.MoveNormal() + movType.noBank();
		}
	}

	Vector3 MovementPong()
	{
		switch(checkSensor())
		{
		case 0: // Nothing obstructed
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MovePong((int)zigZagDir);
			
		case 1: // obstructed in front.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveSlow() + movType.MovePong((int)zigZagDir);
			
		case 2: // offscreen Left
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			zigZagDir = ZigZagDir.Right;
			return movType.MoveNormal() + movType.MovePong((int)zigZagDir);
			
		case 3: // offscreen Right
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			zigZagDir = ZigZagDir.Left;
			return movType.MoveNormal() + movType.MovePong((int)zigZagDir);
			
		case 4: // offscreen Left and obstructed front
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			zigZagDir = ZigZagDir.Right;
			return movType.MoveSlow() + movType.MovePong((int)zigZagDir);
			
		case 5: // offscreen Right and obstructed front
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			zigZagDir = ZigZagDir.Left;
			return movType.MoveSlow() + movType.MovePong((int)zigZagDir);
			
		case 6: // obstructed front and left.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			zigZagDir = ZigZagDir.Right;
			return movType.MoveSlow() + movType.MovePong((int)zigZagDir);
			
		case 7: // obstructed front and right.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			zigZagDir = ZigZagDir.Left;
			return movType.MoveSlow() + movType.MovePong((int)zigZagDir);
			
		case 8: // obstructed front, left and right.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveSlow() + movType.noBank();
			
		case 9: // obstructed right.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			zigZagDir = ZigZagDir.Left;
			return movType.MoveNormal() + movType.MovePong((int)zigZagDir);
			
		case 10: // obstructed left.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			zigZagDir = ZigZagDir.Right;
			return movType.MoveNormal() + movType.MovePong((int)zigZagDir);
			
		case 11: // obstructed right and left.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.noBank();
			
		default:  // Nothing obstructed
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MovePong((int)zigZagDir);
		}
	}

	Vector3 MovementZigZag()
	{
		switch(checkSensor())
		{
		case 0: // Nothing obstructed
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MoveZigZag(switchDirTime, false);
			
		case 1: // obstructed in front.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MoveZigZag(switchDirTime, false);
			
		case 2: // offscreen Left
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.BankRight();
			
		case 3: // offscreen Right
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.BankLeft();
			
		case 4: // offscreen Left and obstructed front
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.BankRight();
			
		case 5: // offscreen Right and obstructed front
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.BankLeft();
			
		case 6: // obstructed front and left.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MoveZigZag(switchDirTime, false);
			
		case 7: // obstructed front and right.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MoveZigZag(switchDirTime, false);
			
		case 8: // obstructed front, left and right.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MoveZigZag(switchDirTime, false);
			
		case 9: // obstructed right.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MoveZigZag(switchDirTime, false);
			
		case 10: // obstructed left.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MoveZigZag(switchDirTime, false);
			
		case 11: // obstructed right and left.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MoveZigZag(switchDirTime, false);
			
		default:  // Nothing obstructed
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MoveNormal() + movType.MoveZigZag(switchDirTime, false);
		}
	}
	
	Vector3 MovementPulse()
	{
		switch(checkSensor())
		{
			case 0: // Nothing obstructed
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 1: // obstructed in front.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 2: // offscreen Left
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 3: // offscreen Right
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 4: // offscreen Left and obstructed front
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 5: // offscreen Right and obstructed front
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 6: // obstructed front and left.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 7: // obstructed front and right.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 8: // obstructed front, left and right.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 9: // obstructed right.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 10: // obstructed left.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			case 11: // obstructed right and left.
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
				
			default:  // Nothing obstructed
			movType.InsertInfo(enginePowerX,enginePowerZ,accelerationX,accelerationZ,friction,velocityX,velocityZ,maxVelocityX,maxVelocityZ, (int)movementType, spawnDirection);
			return movType.MovePuls(pulseTime,pulseBrake, pulseMinimumSpeedPercentofMaxVel);
		}
	}

	void OnDisable() 
	{
		velocityX = 0;
		velocityZ = 0;
	}
	void OnEnable()
	{
		spawnDirection = Camera.main.transform.parent.eulerAngles;
		this.transform.eulerAngles = spawnDirection;
		health = startHealth;
		velocityX = maxVelocityX;
		if(movementType == MovementType.B)
		{
			StatsBMovement();
			if(Random.Range(0,2) == 0)
				zigZagDir = ZigZagDir.Left;
			else
				zigZagDir = ZigZagDir.Right;
		}
		else if(movementType == MovementType.C)
		{
			StatsBMovement();
			if(Random.Range(0,2) == 0)
				zigZagDir = ZigZagDir.Left;
			else
				zigZagDir = ZigZagDir.Right;
		}
		else if(movementType == MovementType.D)
		{
			StatsDMovement();
		}
		else
			StatsAMovement();
	}

	public void comChannelCommands(string order, GameObject sender)
	{
		receivedOrder = order;
		senderOfOrder = sender;
	}

	void comChannel()
	{
		if(receivedOrder.Contains("move") && receivedOrder.Contains("away")) // what to do if ordered out of the way.
		{
			Vector3 senderPosition = senderOfOrder.transform.position - this.transform.position;
			if(senderPosition.z < -this.transform.localScale.z || senderPosition.z > this.transform.localScale.z) // order is finished and forgotten.
			{
				orderedRight = false;
				orderedLeft = false;
				receivedOrder = "";
				senderOfOrder = null;
			}
			else if(senderPosition.z >= 0)
			{
				orderedRight = true;
				orderedLeft = false;
			}
			else if(senderPosition.z < 0)
			{
				orderedLeft = true;
				orderedRight = false;
			}
		}
	}

	// called at enable. here for easy try-outs with public vars
	void StatsAMovement()
	{
//		enginePowerX = 1;
//		enginePowerZ = 1;
//		accelerationX = 0.03f;
//		accelerationZ = 0.03f;
//		friction = 0;
//		maxVelocityX = 0.02f;
//		maxVelocityZ = 0.04f;
	}
	void StatsBMovement()
	{
//		enginePowerX = 1;
//		enginePowerZ = 1;
//		accelerationX = 0.09f;
//		accelerationZ = 0.09f;
//		friction = 0;
//		maxVelocityX = 0.02f;
//		maxVelocityZ = 0.04f;
//		//bMov_edgeOffset = 1;
	}
	void StatsDMovement()
	{
//		enginePowerX = 10;
//		enginePowerZ = 1;
//		accelerationX = 0.03f;
//		accelerationZ = 0.03f;
//		friction = 0;
//		maxVelocityX = 0.1f;
//		maxVelocityZ = 0.04f;
	}

}
