using UnityEngine;
using System.Collections;

public class SightAndCommunication : MonoBehaviour {

	private Vector3 shipScale;

	#region Properties
	[Header("Properties")]
	public float UpdateTime = 1;
	#endregion

	#region Communications
	[Header("Communications")]
	[TextArea(3,10)]
	public string command;
	#endregion

	#region DistanceDencityWidth
	[Header("Distance, dencities and width")]
	public int raysPerUnitZ = 2;
	public float sightDistance = 10;
	public int extraWidthSteps = 0;
	#endregion

	#region allegiances
	[Header("Allegiances (Layers)")]
	public LayerMask friendlies;
	public LayerMask enemies;
	#endregion

	#region AdvancedSearch
	[Header("Advanced search")]
	public string friendlyTagContains = "Enemy";
	public string enemyTagContains;
	#endregion

	#region checkfors
	[Header("Check for")]
	public bool checkForFriendlies;
	public bool checkForEnemies;
	#endregion

	void Awake() 
	{
		shipScale = this.transform.localScale;
	}

	void OnEnable()
	{
		StartCoroutine(TimedUpdate());
	}
	void OnDisable()
	{
		StopCoroutine(TimedUpdate());
	}

	void ObjInSight()
	{
		float rayStepsZ = shipScale.z/raysPerUnitZ;
		RaycastHit hitEnemy;
		RaycastHit hitFriend;
		Ray rayEnemyCheck;
		Ray rayFriendlyCheck;

		for(int i=0-extraWidthSteps; i<raysPerUnitZ+1+extraWidthSteps; i++)
		{
			if(checkForFriendlies)
			{
				rayFriendlyCheck = new Ray(new Vector3(this.transform.position.x + (shipScale.x/2), this.transform.position.y, this.transform.position.z - (shipScale.z/2) + (i * rayStepsZ) ), Vector3.right);
				if (Physics.Raycast(rayFriendlyCheck, out hitFriend,sightDistance,friendlies) && hitFriend.transform.tag.Contains(friendlyTagContains))
				{
					Debug.DrawLine(new Vector3(this.transform.position.x + (shipScale.x/2), this.transform.position.y, this.transform.position.z - (shipScale.z/2) + (i * rayStepsZ) ), hitFriend.point, Color.green);
					if(hitFriend.transform.tag.Contains("Small"))
					{
						hitFriend.transform.GetComponent<smallEnemy>().comChannelCommands(command,this.gameObject);
					}
				}
			}
			if(checkForEnemies)
			{
				rayEnemyCheck = new Ray(new Vector3(this.transform.position.x + (shipScale.x/2), this.transform.position.y, this.transform.position.z - (shipScale.z/2) + (i * rayStepsZ) ), Vector3.right);
				if (Physics.Raycast(rayEnemyCheck, out hitEnemy,sightDistance,enemies) && hitEnemy.transform.tag.Contains(enemyTagContains))
				{
					Debug.DrawLine(new Vector3(this.transform.position.x + (shipScale.x/2), this.transform.position.y, this.transform.position.z - (shipScale.z/2) + (i * rayStepsZ) ), hitEnemy.point, Color.red);
					// do more here
				}
			}
		}
	}

	IEnumerator TimedUpdate()
	{
		while (true) 
		{
			if(checkForFriendlies || checkForEnemies)
			{
				ObjInSight();
			}
			yield return new WaitForSeconds(UpdateTime);
		}
	}

}
