using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using GameAnalyticsSDK;

public class DestinationChooserController : MonoBehaviour {

	/* Content listy z pokojami */
	public GameObject ListContent;

	/* Prefab elementu listy */
	public GameObject ListElementPrefab;

	/* Pole tekstowe do wyszukiwania celu wędrówki */
	public InputField SearchInput;

	/* Animacja fade */
	public Animation FadeAnim;

	/* Zmienna pomocnicza */
	bool LaunchedFromNavigationModule;

	/* Dodanie elementów listy na obiektu budynku */
	void Start ()
	{

		/* Sprawdzenie skąd została włączona scena */
		if (PlayerPrefs.GetInt ("DestinationChooserLaunchedFromNavigationModule", 0) == 1) 
		{
			PlayerPrefs.SetInt ("DestinationChooserLaunchedFromNavigationModule", 0);
			LaunchedFromNavigationModule = true;
		}

		/* Wyszukanie obiektu BuildingRoomsListController */
		BuildingRoomsListController BuildingRoomsDictionaryController = GameObject.FindObjectOfType<BuildingRoomsListController> ();
		if (BuildingRoomsDictionaryController == null) 
		{
			Debug.Log ("Building object not found! Launching initial scene...");
			PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
			SceneManager.LoadScene ("InitialScene");
			return;
		}

		foreach (KeyValuePair<string, string> room in BuildingRoomsDictionaryController.BuildingRoomsList) 
		{
			/* Dodanie pokoju do listy */
			GameObject ListElement = Instantiate (ListElementPrefab);
			ListElement.transform.SetParent (ListContent.transform);
			ListElement.transform.GetChild (0).GetComponent<Text> ().text = room.Key;
			ListElement.transform.GetChild (1).GetComponent<Text> ().text = room.Value;
			ListElement.GetComponent<Button> ().onClick.AddListener (() => { RoomChosen (room.Key); });
			ListElement.name = room.Key + "_" + room.Value;
			ListElement.transform.localScale = Vector3.one;
			ListContent.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, ListContent.GetComponent<RectTransform> ().offsetMax.y + 200);
		}
		ListContent.transform.position = Vector2.zero;

	}

	/* Metoda wyłączania/włączania odpowiednich obiektów listy podczas wyszukiwania */
	public void InputTypingCallback()
	{
		string SearchedString = SearchInput.text;
		for (int i = 0; i < ListContent.transform.childCount; i++) 
		{
			if (ListContent.transform.GetChild (i).name.IndexOf (SearchedString, System.StringComparison.OrdinalIgnoreCase) >= 0 && !ListContent.transform.GetChild (i).gameObject.activeSelf) 
			{
				ListContent.transform.GetChild (i).gameObject.SetActive (true);
				ListContent.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, ListContent.GetComponent<RectTransform> ().offsetMax.y + 200);
			}
			else if(ListContent.transform.GetChild (i).name.IndexOf (SearchedString, System.StringComparison.OrdinalIgnoreCase) < 0 && ListContent.transform.GetChild (i).gameObject.activeSelf)
			{
				ListContent.transform.GetChild (i).gameObject.SetActive (false);
				ListContent.GetComponent<RectTransform> ().offsetMax = new Vector2 (0, ListContent.GetComponent<RectTransform> ().offsetMax.y - 200);
			}
		}
		ListContent.transform.position = Vector2.zero;
	}

	/* Metoda callbacku przycisku wyboru miejsca docelowego */
	private void RoomChosen(string RoomName)
	{
		PlayerPrefs.SetString ("USER_DESTINATION", RoomName);

		/* Wysłanie eventu do Analytics */ 
		GameAnalytics.NewDesignEvent ("CHOOSEN_DESTINATION:" + RoomName);

		FadeAnim.Play ("FadeIn");
		StartCoroutine (WaitForFade());
	}

	/* Coroutine wyczekania animacji fade */
	IEnumerator WaitForFade()
	{
		yield return new WaitWhile (() => FadeAnim.isPlaying);
		if (LaunchedFromNavigationModule) 
		{
			PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
			SceneManager.LoadScene ("NavigationModuleScene");
		}
		else 
		{
			PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
			SceneManager.LoadScene ("ARModuleScene");
		}
	}

	/* Metoda obsługi BackButton */
	public void BackButtonCallback()
	{
		StartCoroutine (SwitchToPreviousScene ());
	}

	/* Coroutine przełączenia na poprzednią scenę */
	IEnumerator SwitchToPreviousScene()
	{
		FadeAnim.Play ("FadeIn");
		yield return new WaitWhile (() => FadeAnim.isPlaying);
		string previousScene = PlayerPrefs.GetString ("previousLevel", "InitialScene");
		Debug.Log (previousScene);
		if(previousScene.Contains("ARModuleScene"))
			previousScene = "MenuScene";
		PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
		SceneManager.LoadScene (previousScene);
	}
}
