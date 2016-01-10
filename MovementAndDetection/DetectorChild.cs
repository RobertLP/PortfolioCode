using UnityEngine;
using System.Collections;

public class DetectorChild : MonoBehaviour 
{
	private Transform parentTrans;
	private Detector detScript;
	public bool debug_ColCheckOff; // TEMP DEBUG

	void Start()
	{
		detScript = this.GetComponentInParent<Detector>();
		parentTrans = detScript.gameObject.transform;
	}

	void OnTriggerStay(Collider other) 
	{

		if(other.transform != parentTrans && other.tag == parentTrans.tag && !debug_ColCheckOff) // double check if not checking parent
		{
			if(other.transform.position.z > parentTrans.transform.position.z) //+ (parentTrans.transform.localScale.z) )
				detScript.leftObstructed = true;


			if(other.transform.position.z < parentTrans.transform.position.z) //- (parentTrans.transform.localScale.z) )
				detScript.rightObstructed = true;

			
			if(other.transform.position.x > parentTrans.transform.position.x) // + (parentTrans.transform.localScale.x) )
				detScript.frontObstructed = true;
			else
				detScript.frontObstructed = false;
			
			if(other.transform.position.x < parentTrans.transform.position.x + (parentTrans.transform.localScale.x) && other.transform.position.x > parentTrans.transform.position.x - (parentTrans.transform.localScale.x)
			   && other.transform.position.z < parentTrans.transform.position.z + (parentTrans.transform.localScale.z) && other.transform.position.z > parentTrans.transform.position.z - (parentTrans.transform.localScale.z))
				detScript.collision = true;
			else
				detScript.collision = false;
		}
	}
	void OnTriggerExit(Collider other)
	{
		detScript.leftObstructed = false;
		detScript.rightObstructed = false;
		detScript.frontObstructed = false;
		detScript.collision = false;
	}

	void OnEnable()
	{
		StartCoroutine(ResetVal());
	}
	void OnDisable()
	{
		if(detScript != null)
		{
			detScript.leftObstructed = false;
			detScript.rightObstructed = false;
			detScript.frontObstructed = false;
			detScript.collision = false;
		}
		StopAllCoroutines();
	}

	private IEnumerator ResetVal()
	{
		while(true)
		{
			yield return new WaitForSeconds(0.2f);
			detScript.leftObstructed = false;
			detScript.rightObstructed = false;
			detScript.frontObstructed = false;
			detScript.collision = false;
			yield return new WaitForSeconds(0.2f);
		}
	}
}
