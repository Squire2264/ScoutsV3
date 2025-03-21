﻿using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
	private const int maxInventoryItems = 8;
	[HideInInspector] [System.NonSerialized]
	public bool dragging;
	public GameObject ovCanvas;
	public InventorySlot[] slots;

	public GameObject itemInfoBox;
	TextMeshProUGUI itemName, description, type;
	GameObject useButton;
	public Joystick joy;
	InventorySlot selectedItem, draggingSlot;
	public InventoryDragAndDrop clonePrefab;

	#region Singleton
	public static InventoryManager instance;
	private void Awake()
	{
		if (instance != null)
		{
			throw new System.Exception("Inventory manager is not a singleton");
		}
		instance = this;
	}
	#endregion
	#region Basic Methods
	public void Add(ObjectBase item)
	{
		foreach (InventorySlot s in slots)
		{
			var i = s.item;
			if (i == item && i.currentAmount <= i.maxAmount)
			{
				s.AddItemOrReset(item);
				GameManager.instance.InventoryChanged(item);
				return;
			}
		}
		foreach (var s in slots)
		{
			if (s.item == null)
			{
				s.AddItemOrReset(item);
				GameManager.instance.InventoryChanged(item);
				return;
			}
		}
	}


	public bool Contains(ObjectBase i)
	{
		foreach (var s in slots)
		{
			if (s.item == i)
			{
				return true;
			}
		}
		return false;
	}


	public bool IsInventoryFull()
	{
		foreach (InventorySlot s in slots)
		{
			if (s.item == null || s.item.currentAmount < s.item.maxAmount)
				return false;
		}
		return true;
	} // shop checks if inventory is full before calling the method "Add"

	#endregion
	#region InGameMethods
	public GameObject inventoryPanelParent, overlay;
	bool isOpen;
	public void ToggleInventoryPanel()
	{
		isOpen = !isOpen;
		GameObject.Find("AudioManager").GetComponent<AudioManager>().Play(isOpen ? "click" : "clickDepitched");

		inventoryPanelParent.SetActive(isOpen);
		joy.canUseJoystick = !isOpen;
		overlay.SetActive(isOpen);
		PanZoom.instance.canDo = !isOpen;
		foreach (var s in slots)
		{
			s.RefreshInventoryAmount();
		}
	}
	
	#endregion
	void Start()
	{
		itemName = itemInfoBox.transform.Find("Name").GetComponent<TextMeshProUGUI>();
		description = itemInfoBox.transform.Find("Description").GetComponent<TextMeshProUGUI>();
		type = itemInfoBox.transform.Find("Type").GetComponent<TextMeshProUGUI>();
		useButton = itemInfoBox.transform.Find("Button").gameObject;
		if (slots.Length != maxInventoryItems)
			Debug.LogWarning("Inventory contains a different number of slots from the required one.");
		SetStatus(SaveSystem.instance.LoadData<Status>(SaveSystem.instance.inventoryManagerFileName, false));
	}
	#region Status
	public Status SendStatus()
	{
		var it = new ObjectBase[slots.Length];
		for (int i = 0; i < it.Length; i++)
		{
			it[i] = slots[i].item;
		}
		return new Status
		{
			items = it,
		};
	}
	void SetStatus(Status status)
	{
		if (status != null)
		{
			for (int i = 0; i < status.items.Length; i++)
			{
				slots[i].AddItemOrReset(status.items[i]);
			}
		}
	}
	public class Status
	{
		public ObjectBase[] items;
	}
	#endregion
	public void SelectItem(InventorySlot slot)
	{
		if (slot != null)
		{
			if (slot == selectedItem)
			{
				selectedItem = null;
				itemInfoBox.SetActive(false);
			}
			else
			{
				selectedItem = slot;
				itemName.text = slot.item.name;
				description.text = slot.item.description;
				type.text = slot.item.type.ToString();
				useButton.SetActive(slot.item.periodicUses.Length > slot.item.level && slot.item.periodicUses[slot.item.level].interval == PeriodicActionInterval.Once);
				useButton.GetComponentInChildren<TextMeshProUGUI>().text = "Usa";
				itemInfoBox.SetActive(true);
			}
		}
		else
		{
			selectedItem = null;
			itemInfoBox.SetActive(false);
		}
	}

	public void UseItem()
	{
		selectedItem.item.currentAmount--;
		selectedItem.item.DoAction();
		selectedItem.RefreshInventoryAmount();
		SelectItem(null);
	}

	private void Update()
	{
		if (isOpen && Input.touchCount >= 1)
		{
			Touch t = Input.GetTouch(0);
			draggingSlot = CheckIfNearASlot(t);
			if (t.phase == TouchPhase.Moved && !dragging && draggingSlot != null && draggingSlot.item != null)
			{
				draggingSlot.amountText.gameObject.SetActive(false);
				draggingSlot.GetComponent<Image>().enabled = false;
				var clone = Instantiate(clonePrefab, t.position, Quaternion.identity, ovCanvas.transform);
				clone.GetComponent<Image>().sprite = draggingSlot.item.icon;
				clone.parent = draggingSlot;
				dragging = true;
			}
		}
	}






	public static InventorySlot CheckIfNearASlot(Touch t)
	{
		var slots = FindObjectsOfType<InventorySlot>();
		foreach (var s in slots)
		{
			if (Vector2.Distance(t.position, s.transform.position) <= 60)
			{
				return s;
			}
		}
		return null;
	}
}
