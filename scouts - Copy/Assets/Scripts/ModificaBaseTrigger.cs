﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using Pathfinding;

public class ModificaBaseTrigger : MonoBehaviour
{
	public BuildingSlot buildingSlot;
	public GameObject[] objectsToToggle;
	MoveBuildings[] buildings;
	Camera cam;
	public Transform angolo;
	Transform player;
	public bool isModifying;
	public Joystick joy;
	public bool execTransition = false;
	public string objectBought;
	Vector3 exPos;
	public GameObject confiniAngoli;
	#region Singleton
	public static ModificaBaseTrigger instance;
	private void Awake()
	{
		if (instance != null)
			throw new System.Exception("ModificaBaseTrigger non è un singleton");
		instance = this;
	}
	#endregion


	#region Status
	public Status SendStatus()
	{
		return new Status
		{
			execTransition = execTransition,
			objectBought = objectBought,
		};
	}
	void SetStatus(Status status)
	{
		if (status != null)
		{
			objectBought = status.objectBought;
			execTransition = status.execTransition;
		}
	}
	public class Status
	{
		public string objectBought;
		public bool execTransition;
	}

	#endregion

	void Start()
	{
		cam = Camera.main;
		StartCoroutine(GetSq());
		player = Player.instance.transform;
		CheckExecTransition();
		SetStatus(SaveSystem.instance.LoadData<Status>(SaveSystem.instance.modificaBaseTriggerFileName, false));
	}

	void CheckExecTransition()
    {
        if (execTransition)
        {
			objectBought = objectBought.ToLower();

			//cerca l'oggetto
			GameObject obj;
			
			SpriteRenderer [] spriteBuildings = SquadrigliaManager.instance.GetPlayerSq().buildings;



			foreach (SpriteRenderer s in spriteBuildings)
            {
				string name = s.gameObject.name.ToLower();
				if(name==objectBought){
					obj = s.gameObject; 
					obj.SetActive(true);
				}
			}

			//attiva objectBought
			execTransition = !execTransition;
        }
    }

	public void SetBuildingSlotInfo(PlayerBuilding building)
	{
		buildingSlot.GetComponent<Image>().sprite = building.icon;
		buildingSlot.building = building;
		foreach (var b in buildings)
		{
			var bg = b.GetComponent<PlayerBuildingBase>();
			if (bg.building == buildingSlot.building)
			{
				buildingSlot.buildingParent = bg;
			}
			b.GetComponent<MoveBuildings>().isBeingBuilt = false;
		}
		buildingSlot.buildingParent.GetComponent<MoveBuildings>().isBeingBuilt = true;
		ToggleModificaBase(true);
	}
	public PlayerBuildingBase FindBuildingInSceneFromItem(PlayerBuilding building)
	{
		foreach (var b in buildings)
		{
			var bg = b.GetComponent<PlayerBuildingBase>();
			if (bg.building == building)
			{
				return bg;
			}
		}
		return null;
	}

	IEnumerator GetSq()
	{
		var frame = new WaitForEndOfFrame();
		yield return frame;
		yield return frame;
		yield return frame;

		SpriteRenderer[] bArray = SquadrigliaManager.instance.GetPlayerSq().buildings;
		buildings = new MoveBuildings[bArray.Length];
		for (int b = 0; b < buildings.Length; b++)
		{
			buildings[b] = bArray[b].GetComponent<MoveBuildings>();
		}
	}

	public void ToggleModificaBase(bool isBuilding)
	{
        if (isModifying)
        {
			int oneJustMoved = 0;
			//controlli per collisione
			foreach(MoveBuildings m in buildings)
            {
                if (m.gameObject.activeSelf == true&&m.isTouching)
                {
                    if (oneJustMoved == 1)
                    {
						Vector3 startPos = modificaAngolo.instance.posizioneIniziale;
						//all'interno del metodo viene fatto un controllo per il tocco
						m.OnEndDragging(startPos);
					}
					
					oneJustMoved++;
				}					
            }
			Player.instance.transform.position = exPos;
			modificaAngolo.instance.spawnPoints.SetActive(true);
			confiniAngoli.SetActive(false);

			//aggiornare path
			//Invoke("UpdatePath",1f);
			
		}
		else
        {
			exPos = Player.instance.transform.position;
			Player.instance.transform.position = new Vector3(0,0,0);

            if (!modificaAngolo.instance.firstIteraction)
            {
				modificaAngolo.instance.spawnPoints.SetActive(false);
            }
			confiniAngoli.SetActive(true);
        }

		GameObject.Find("AudioManager").GetComponent<AudioManager>().Play("click");
		joy.canUseJoystick = isModifying;
		isModifying = !isModifying;
		buildingSlot.gameObject.SetActive(isBuilding);

		if (!isBuilding && buildingSlot.buildingParent != null)
		{
			buildingSlot.buildingParent.GetComponent<MoveBuildings>().isBeingBuilt = false;
			buildingSlot.buildingParent = null;
		}

		foreach (GameObject g in objectsToToggle)
		{
			g.SetActive(!isModifying);
		}
		foreach (var b in buildings)
		{
			if (buildingSlot.buildingParent != null && b.GetComponent<PlayerBuildingBase>() == buildingSlot.buildingParent)
			{
				b.componentEnabled = isModifying;
			}
			else if (b.gameObject.activeSelf)
			{
				b.GetComponent<InGameObject>().MoveUI();
				b.componentEnabled = isModifying;
			}
		}
		modificaAngolo.instance.enabled = isModifying;
		cam.GetComponent<FollowPlayer>().SetTarget(isModifying ? angolo : player, isModifying ? 4f : 0);
		cam.GetComponent<PanZoom>().canDo = !isModifying;
		cam.GetComponent<FollowPlayer>().EnableFollow();
		if (ActionButtons.instance.selected != null)
		{
			ActionButtons.instance.selected.Deselect();
		}
		GetComponent<Animator>().Play(isModifying ? "SalvaEdEsci" : "Modifica");

				
		BaseAI[] aiControllers = FindObjectsOfType(typeof(BaseAI)) as BaseAI[];
		
		foreach(BaseAI a in aiControllers)
        {
			a.ReEnableMovementAnim();
        }

		//controlli per costruzione
	}

	void UpdatePath()
    {
		var graphToScan = AstarPath.active.data.gridGraph;
		AstarPath.active.Scan(graphToScan);
	}

	public void ToggleButtonComponent(bool toggle)
    {
		gameObject.GetComponent<Button>().enabled = toggle;
    }
}
