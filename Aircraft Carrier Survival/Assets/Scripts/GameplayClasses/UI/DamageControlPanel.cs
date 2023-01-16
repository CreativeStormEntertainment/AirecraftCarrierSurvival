using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageControlPanel : MonoBehaviour
{
	[SerializeField]
	private int maxButtonsCount = 9;
	//	TODO	Marek	-	add object pooling
	[SerializeField]
	private RoomDamageButton damageButtonPrefab = null;
	[SerializeField]
	private string generatorsDestructionAlternateID = null;
	[SerializeField]
	private List<SectionRoom> generatorsRooms = null;

	private Dictionary<SectionRoom, RoomDamageButton> roomsButtons;


	public void Setup()
	{
		roomsButtons = new Dictionary<SectionRoom, RoomDamageButton>();

		List<List<SectionRoom>> sectionsRooms = SectionRoomManager.Instance.RoomsListBySections;
		List<SectionRoom> rooms;

		for (int i = 0; i < sectionsRooms.Count; i++)
		{
			rooms = sectionsRooms[i];

			for (int j = 0; j < rooms.Count; j++)
			{
				var room = rooms[j];
				rooms[j].SectionWorkingChanged += (state) => OnRoomConditionChange(room, state);
			}
		}

		////Debug.LogFormat(this, "<color=lime>Damage control panel ready ({0})!</color>", count);
	}

	private void OnRoomConditionChange(SectionRoom room, bool state)
	{
		if (roomsButtons.TryGetValue(room, out RoomDamageButton btn))
		{
			if (state)
			{
				////Debug.LogFormat(this, "<color=white>Removing button for {0}</color>", room.RoomName);
				Destroy(btn.gameObject);
				roomsButtons.Remove(room);

				UpdateButtonsState();
			}
			////else
			////{
			////	Debug.LogFormat(room, "<color=red>Room {0} is already damaged, was state change intended?</color>", room.RoomName);
			////}
		}
		else
		{
			if (state)
			{
				////Debug.LogFormat(room, "<color=yellow>Room {0} changed state to {1}, and wasn't registered as damaged before.</color>", room.RoomName, state);
				return;
			}

			btn = Instantiate(damageButtonPrefab);
			btn.gameObject.SetActive(roomsButtons.Count < maxButtonsCount);
			btn.transform.SetParent(transform, false);
			btn.room = room;
			btn.icon.sprite = room.Icon;

			////Debug.LogFormat(this, "<color=orange>Added damage button for {0}</color>", room.RoomName);
			roomsButtons.Add(room, btn);

			btn.ownTooltip.TitleID = room.DamageTooltipTitleID;
			btn.ownTooltip.DescriptionID = room.DamageTooltipDescID;

			CheckGeneratorRooms(room, state);
			btn.ownTooltip.FireParamsChanged();
		}
	}


	private void CheckGeneratorRooms(SectionRoom room, bool state)
	{
		if (!generatorsRooms.Contains(room))
		{
			return;
		}

		bool useAlternateText = true;

		for (int i = 0; i < generatorsRooms.Count; i++)
		{
			useAlternateText &= !generatorsRooms[i].IsWorking;
		}

		for (int i = 0; i < generatorsRooms.Count; i++)
		{
			SectionRoom r = generatorsRooms[i];
			if (!roomsButtons.ContainsKey(r))
			{
				continue;
			}
			if (useAlternateText)
			{
				roomsButtons[r].ownTooltip.DescriptionID = generatorsDestructionAlternateID;
			}
			else
			{
				//roomsButtons[r].ownTooltip.DescriptionID = room.DescriptionID;
			}

			roomsButtons[r].ownTooltip.FireParamsChanged();
		}
	}

	private void UpdateButtonsState()
	{
		int counter = 0;

		foreach (var pair in roomsButtons)
		{
			pair.Value.gameObject.SetActive(true);
			counter++;

			if (counter == maxButtonsCount)
			{
				break;
			}
		}
	}
}