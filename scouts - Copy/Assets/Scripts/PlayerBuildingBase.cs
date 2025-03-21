﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Threading;

public abstract class PlayerBuildingBase : InGameObject
{
	[HideInInspector]
	[System.NonSerialized]
	public GameObject healthBar;
	[HideInInspector]
	[System.NonSerialized]
	public Vector3 healthBarRelativeOffset = new Vector3(0, -0.2f, 0);
	[HideInInspector]
	[System.NonSerialized]
	public int health;
	protected bool isSafe, isDestroyed;
	public PlayerBuilding building;

	int timeLeftBeforeHealthLoss;
	protected override void Start()
	{
		base.Start();
		if (customDataFileName != null && customDataFileName != "")
		{
			SetPBStatus(SaveSystem.instance.LoadData<PBStatus>(customDataFileName, false));
		}
		objectName = building.name;
		objectSubName = "Livello " + (building.level + 1);
		healthBar = Instantiate(GameManager.instance.healthBarPrefab, loadingBar.transform.position + healthBarRelativeOffset, Quaternion.identity, wpCanvas.transform);
		health = building.healthInfos[building.level].maxHealth;
		healthBar.GetComponent<Slider>().maxValue = building.healthInfos[building.level].maxHealth;
		healthBar.GetComponent<Slider>().value = health;
		timeLeftBeforeHealthLoss = building.healthInfos[building.level].healthLossInterval;
		InvokeRepeating(nameof(LoseHealthWhenRaining), 1f, 1f);
		MoveUI();
	}
	protected override void SaveData()
	{
		SaveSystem.instance.SaveData(SendPBStatus(), customDataFileName, false);
	}


	protected override int? GetLevel()
	{
		return building.level + 1;
	}

	public override void Select()
	{
		StartCoroutine(ToggleHealthBar(true));
		base.Select();
	}
	public override void Deselect()
	{
		if (!GameManager.instance.isRaining)
		{
			StartCoroutine(ToggleHealthBar(false));
		}
		base.Deselect();
	}

	protected virtual void LoseHealthWhenRaining()
	{
		if (GameManager.instance.isRaining)
		{
			StartCoroutine(ToggleHealthBar(true));
			if (!isSafe && !isDestroyed)
			{
				timeLeftBeforeHealthLoss--;
				if (timeLeftBeforeHealthLoss == 0)
				{
					timeLeftBeforeHealthLoss = building.healthInfos[building.level].healthLossInterval;
					health--;
				}

				if (health <= 0)
				{
					health = 0;
					isDestroyed = true;
					RefreshButtonsState();
					if (!hasBeenClicked)
						StartCoroutine(ToggleHealthBar(false));
				}
				healthBar.GetComponent<Slider>().value = health;
				healthBar.transform.Find("HealthValue").GetComponent<TextMeshProUGUI>().text = health.ToString();
			}
		}
		else if (!hasBeenClicked)
		{
			StartCoroutine(ToggleHealthBar(false));
		}
		else
		{
			isSafe = false;
		}
	}

	protected override void RefreshButtonsState()
	{
		base.RefreshButtonsState();
		if (ActionButtons.instance.selected == this)
		{
			buttonsText.text = isDestroyed ? objectName + " (Distrutto)" : objectName;
			nameText.GetComponent<TextMeshProUGUI>().text = isDestroyed ? objectName + " (Distrutto)" : objectName;
		}
	}
	protected override bool GetConditionValue(ConditionType t)
	{
		switch (t)
		{
			case ConditionType.ConditionIsSafe: return isSafe;
			case ConditionType.ConditionIsDestroyed: return isDestroyed;
			default: return base.GetConditionValue(t);
		}
	}

	protected void MettiAlSicuro()
	{
		isSafe = true;
		RefreshButtonsState();
	}

	protected void Ripara()
	{
		isDestroyed = false;
		health = building.healthInfos[building.level].maxHealth;
		healthBar.GetComponent<Slider>().value = health;
		healthBar.transform.Find("HealthValue").GetComponent<TextMeshProUGUI>().text = health.ToString();
		RefreshButtonsState();
	}


	public override void MoveUI()
	{
		base.MoveUI();
		healthBar.transform.position = loadingBar.transform.position + healthBarRelativeOffset;
	}
	public override IEnumerator ToggleHealthBar(bool active)
	{
		yield return new WaitForEndOfFrame();
		healthBar.gameObject.SetActive(active);
	}

	public class PBStatus : Status
	{
		public int health;
		public bool isSafe;
		public bool isDestroyed;
		public ObjectBase.Status building;
	}
	public virtual PBStatus SendPBStatus()
	{
		var b = new ActionButton.Status[buttons.Length];
		for (int i = 0; i < b.Length; i++)
		{
			b[i] = buttons[i].SendStatus();
		}
		return new PBStatus
		{
			position = transform.position,
			active = gameObject.activeSelf,
			actionButtonInfos = b,
			health = health,
			isSafe = isSafe,
			isDestroyed = isDestroyed,
			building = building.SendStatus(),
		};
	}
	public virtual void SetPBStatus(PBStatus status)
	{
		if (status != null)
		{
			health = status.health;
			isDestroyed = status.isDestroyed;
			isSafe = status.isSafe;
			building.SetStatus(status.building);
		}
	}





}


