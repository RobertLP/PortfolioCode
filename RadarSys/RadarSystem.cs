using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _RadarSystem : MonoBehaviour 
{
	[Header("Radar")]
	public float radarDrawRange = 200f;
	public Texture radarBG;
	public Transform centerObject;
	public float mapScale = 0.3f;
	public float RadarSize=150f;
	public float maxDist = 200;
	public float radarOffsetTop = 5;
	public float radarOffsetSide = 20;
	
	private Vector2 mapCenterR;
	private Vector2 mapCenterL;

	[Header("Target reticle")]
	public Texture retTexture;
	public float retSize = 90f;

	[Header("Lists")]
	public List <GameObject> RadarObjectList;
	public RComp[] RadaObjects;
	[System.Serializable]
	public class RComp{
		public Texture OnRadar;
		public string TagName;
	}
	
	void Awake()
	{
		FillRadarList();
	}
	
	void FillRadarList()
	{
		RadarObjectList = new List<GameObject>();
		foreach(RComp tf in RadaObjects)
		{      
			GameObject[] LoadedTargets = GameObject.FindGameObjectsWithTag(tf.TagName);
			foreach(GameObject loadedTarget in LoadedTargets)
			{
				RadarObjectList.Add(loadedTarget);
			}
		}
	}

	private void OnGUI()
	{
		//EnergySystemVisuals();
		SetUpTargRet();
		if(centerObject){
			Rect r=new Rect(Screen.width-radarOffsetSide - RadarSize, radarOffsetTop, RadarSize, RadarSize); // location of the radar rect.
			UnityEngine.GUI.DrawTexture(r, radarBG,ScaleMode.StretchToFill); // fit the texture on the rectangle
			mapCenterR = new Vector2(Screen.width-radarOffsetSide-RadarSize/2,radarOffsetTop+RadarSize/2);
			
			Rect l=new Rect(radarOffsetSide, radarOffsetTop, RadarSize, RadarSize); // location of the radar rect.
			UnityEngine.GUI.DrawTexture(l, radarBG,ScaleMode.StretchToFill); // fit the texture on the rectangle
			mapCenterL = new Vector2(radarOffsetSide+RadarSize/2,radarOffsetTop+RadarSize/2);

			foreach(RComp tf in RadaObjects)
			{      
				GameObject[] LoadedTargets = GameObject.FindGameObjectsWithTag(tf.TagName);
				foreach(GameObject loadedTarget in LoadedTargets)
				{
					drawBlip(loadedTarget, tf.OnRadar);
				}
			}
		}
	}

	private void SetUpTargRet()
	{
		Rect ret = new Rect((Screen.width/2)-(retSize/2), (Screen.height/2)-(retSize/2) ,retSize, retSize);
		UnityEngine.GUI.DrawTexture(ret, retTexture,ScaleMode.StretchToFill);
	}
	// radar draw function
	private void drawBlip(GameObject go, Texture aTexture)
	{
		float dist = Vector3.Distance(go.transform.position, centerObject.position);
		Vector3 relativeTargetPosition = centerObject.transform.InverseTransformPoint(go.transform.position);
		relativeTargetPosition.z = 0;
		relativeTargetPosition = Vector3.ClampMagnitude(relativeTargetPosition/(dist*0.015f), radarDrawRange);
		float bX = relativeTargetPosition.x;
		float bY = -relativeTargetPosition.y;
		if (centerObject.transform.InverseTransformPoint(go.transform.position).z > 0)
		{
			UnityEngine.GUI.DrawTexture(new Rect(mapCenterL.x + bX, mapCenterL.y + bY, 2, 2), aTexture);
		}
		else
		{
			UnityEngine.GUI.DrawTexture(new Rect(mapCenterR.x + bX, mapCenterR.y + bY, 2, 2), aTexture);
		}
	}

}
