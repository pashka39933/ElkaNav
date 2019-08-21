using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingRoomsListController : MonoBehaviour {

	/* Lista przechowująca pomieszczenia budynku <numer, opis> */
	public List<KeyValuePair<string, string>> BuildingRoomsList = new List<KeyValuePair<string, string>>();

	/* Plik tekstowy z nazwami pokojów */
	public TextAsset RoomsDicitionary;

	/* Metoda uzupełnienia listy pomieszczeń */
	public void FillBuildingRoomsList()
	{
		/* Wyłączenie wszystkich pięter budynku */
		for (int i = 0; i < this.transform.childCount; i++) 
		{
			this.transform.GetChild (i).gameObject.SetActive (false);
		}

		/* Pobieranie i dodawanie do listy nazw pokojów obiektu budynku */
		List<string> RoomsDicitionaryStrings = new List<string> ();
		RoomsDicitionaryStrings.AddRange(RoomsDicitionary.text.Split(System.Environment.NewLine.ToCharArray(), System.StringSplitOptions.None));
		for (int i = 0; i < this.transform.childCount; i++) 
		{
			for (int j = 0; j < this.transform.GetChild(i).GetChild (0).GetChild(0).childCount; j++) 
			{
				string RoomNumber = this.transform.GetChild (i).GetChild (0).GetChild(0).GetChild (j).name;

				/* Jeśli numer pokoju jest poprawny */
				if (!RoomNumber.Contains ("_")) 
				{
					/* Sprawdzenie czy nazwa pokoju jest w słowniku */
					string RoomInfo = "";
					for (int k = 0; k < RoomsDicitionaryStrings.Count; k++) 
					{
						if (RoomsDicitionaryStrings [k].Contains (RoomNumber)) 
						{
							string[] DicitionarySplitted = RoomsDicitionaryStrings [k].Split ("\t<=>\t".ToCharArray (), System.StringSplitOptions.None);
							if (RoomNumber == DicitionarySplitted [0]) 
							{
								RoomInfo = DicitionarySplitted [DicitionarySplitted.Length-1];
								RoomsDicitionaryStrings.RemoveAt (k);
								break;
							}
						}
					}

					/* Dodanie pokoju do listy */
					if(BuildingRoomsList.FindIndex(x => x.Key == RoomNumber) == -1)
						BuildingRoomsList.Add (new KeyValuePair<string, string>(RoomNumber, RoomInfo));
				}
			}
		}
		/* Posortowanie listy */
		BuildingRoomsList.Sort((room1,room2) => room1.Key.CompareTo(room2.Key));
	}
}
