using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShieldMineSystem : MonoBehaviour 
{
	public GameObject ShieldMine;
	public List<GameObject> ShieldMines = new List<GameObject>();

	public void GoTroughActivatedMines(GameObject self)
	{
		GameObject closest = FindClosestMine(self.gameObject);
		if(closest != null) // if someone was found, assign it as the brother
		{
			ShieldMine mineScrpt = self.GetComponent<ShieldMine>();
			mineScrpt.brother = closest;
			mineScrpt.shieldCaster = true;
		}
	}

	private GameObject FindClosestMine(GameObject self)
	{
		float closestDistance = Mathf.Infinity;
		int selectedMine = -1;
		for(int i=0; i < ShieldMines.Count; i++) // go trough all active mines excluding self
		{
			if(ShieldMines[i].activeSelf && ShieldMines[i] != self)
			{
				float calcDistance = Vector3.Distance(self.transform.position, ShieldMines[i].transform.position);
				if(calcDistance < closestDistance)
				{
					closestDistance = calcDistance;
					selectedMine = i;
				}
			}
		}
		if(selectedMine == -1) // if selectedMine is still -1, then self is only active
			return null;
		else
			return ShieldMines[selectedMine].gameObject;
	}

	public int FindActiveMines()
	{
		int foundMines = 0;
		foreach(GameObject mine in ShieldMines)
		{
			if(mine.gameObject.activeSelf)
				foundMines++;
		}
		return foundMines;
	}

	public GameObject[] FindActiveBrothers(GameObject me)
	{
		List<GameObject> ActiveBrothers = new List<GameObject>();
		foreach(GameObject mine in ShieldMines)
		{
			if(mine.GetComponent<ShieldMine>().brother == me)
				ActiveBrothers.Add(mine);
		}
		return ActiveBrothers.ToArray();
	}

	public bool	LinkedFinishedFlickering(GameObject me) // checks if all the brothers of me are finished flickering
	{
		GameObject[] ActiveBrothers = FindActiveBrothers(me);

		foreach(GameObject mine in ActiveBrothers)
		{
			if(!mine.GetComponent<ShieldMine>().finishFlicker)
				return false;
		}
		return true;
	}

	public bool CastingBrothers(GameObject[] ShieldMines)
	{
		foreach(GameObject mine in ShieldMines)
		{
			if(mine.GetComponent<ShieldMine>().shieldCaster == true)
				return true;
		}
		return false;
	}

	public GameObject GetMine()
	{
		for(int i = 0; i < ShieldMines.Count; i++)
		{
			if(!ShieldMines[i].activeInHierarchy)
				return ShieldMines[i];
		}
		// if none are available
		GameObject obj = (GameObject)Instantiate(ShieldMine);
		obj.transform.SetParent(this.transform);
			ShieldMines.Add(obj);
		return obj;
	}

}
