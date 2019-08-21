using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using GameAnalyticsSDK;

public class CameraController : MonoBehaviour {

    /* Obiekty pięter */
    GameObject[] Floors;

    /* Obiekt fade */
    public CanvasGroup fadeImg;

    /* Tekst piętra */
    public Text FloorInfoText;

    /* Obecnie aktywne piętro */
    public int ActiveFloor = 0;

    /* Kontroler ściezki budynku */
    BuildingPathController BuildingPathController;

    /* Nawigator */
    public BuildingNavigator Navigator;

    /* Tryb kamery (podążaj za wskaźnikiem lub freestyle) */
    public bool FollowNavigator = false;

    /* Komponent kamery */
    Camera cam;

    /* Inicjalizacja kamery */
    void Start ()
    {

        /* Komponent kamery */
        cam = this.GetComponent<Camera>();

        /* Wyszukanie komponentu budynku */
        BuildingPathController = GameObject.FindObjectOfType<BuildingPathController>();

        /* Restart aplikacji w przypadku nie znalezienia komponentu budynku (ponowne załadowanie) */
        if(BuildingPathController == null)
        {
            Debug.Log("Building object not found! Launching initial scene...");
			PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
			SceneManager.LoadScene("InitialScene");
            return;
        }

        /* Zebranie obiektów pięter */
        Floors = new GameObject[BuildingPathController.transform.childCount];
        for(int i = 0; i < Floors.Length; i++)
        {
            Floors[i] = BuildingPathController.transform.GetChild(i).gameObject;
        }

        /* Aktywowanie wszystkich pięter */
        foreach (GameObject floor in Floors)
            floor.SetActive(true);

        /* Wyszukanie i zainicjowanie skryptów ikon */
        IconController[] icons = GameObject.FindObjectsOfType<IconController>();
        foreach (IconController ico in icons)
            ico.Init(this);

        /* Sprawdzenie czy tryb eksploracji mapy nie jest włączony */
        if (PlayerPrefs.GetString("USER_POSITION", "INITIAL_VALUE") == "MAP_EXPLORE_MODE")
        {
            /* Dezaktywowanie wszystkich pięter */
            foreach (GameObject floor in Floors)
                floor.SetActive(false);
            /* Aktywowanie piętra domyślnego */
            ActiveFloor = 2;
            Floors[ActiveFloor].SetActive(true);
			FloorInfoText.text = (ActiveFloor-2).ToString();
            /* Wyłączenie przycisków */
            GameObject.Find("ToggleCameraMode").transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            GameObject.Find("ToggleCameraMode").GetComponent<Button>().interactable = false;
            GameObject.Find("ScanDoor").transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            GameObject.Find("ScanDoor").GetComponent<Button>().interactable = false;
            GameObject.Find("DestinationChooser").transform.GetChild(0).GetComponent<Image>().color = Color.gray;
            GameObject.Find("DestinationChooser").GetComponent<Button>().interactable = false;
            return;
        }

        /* Wyszukanie pozycji użytkownika */
        GameObject userPos = GameObject.Find(PlayerPrefs.GetString("USER_POSITION", "INITIAL_VALUE"));

		/* Jeśli pozycji nie znaleziono, próba wyszukania możliwie bliskiego pokoju */
		if (userPos == null) 
		{
			string roomString = PlayerPrefs.GetString ("USER_POSITION", "INITIAL_VALUE");
			if (!roomString.Contains ("P") && !roomString.Contains ("CS") && !roomString.Contains ("DS")) 
			{
				string roomNumber = System.Text.RegularExpressions.Regex.Replace (roomString, @"[^0-9]+", "");
				userPos = GameObject.Find(roomNumber);
				for (char tmp = 'A'; tmp < 'L'; tmp++) 
				{
					if (userPos != null)
						break;
					userPos = GameObject.Find(roomNumber + tmp);
				}
			}
		}

        if(userPos != null)
        {

            /* Wyszukanie celu użytkownika i narysowanie ścieżki */
            GameObject userDest = GameObject.Find(PlayerPrefs.GetString("USER_DESTINATION", "CS2"));
            if (userDest != null)
            {
                /* Narysowanie ścieżki */
                List<KeyValuePair<int, List<Transform>>> BuildingPath = BuildingPathController.DrawPath(userPos, userDest);
                /* Inicjacja wskaźnika pozycji */
                Navigator.Init(BuildingPath);
            }
            /* Ustawienie kamery */
            this.transform.position = new Vector3(Navigator.transform.position.x, Navigator.transform.position.y, this.transform.position.z);

        }
        else
        {
			/* Dezaktywowanie wszystkich pięter */
			foreach (GameObject floor in Floors)
				floor.SetActive(false);
            Debug.Log("Position ID not found!");

			/* Wysłanie eventu do Analytics */
			GameAnalytics.NewErrorEvent (GAErrorSeverity.Warning, "Position ID not found: " + PlayerPrefs.GetString("USER_POSITION"));

			PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
			SceneManager.LoadScene("ARModuleScene");
			return;
        }

        /* Dezaktywowanie wszystkich pięter */
        foreach (GameObject floor in Floors)
            floor.SetActive(false);

        /* Aktywowanie piętra użytkownika */
        GameObject ActiveFloorTmp = userPos.transform.parent.parent.parent.gameObject;
        ActiveFloorTmp.SetActive(true);
        ActiveFloor = int.Parse(ActiveFloorTmp.tag.Split(' ')[1]);
        FloorInfoText.text = ActiveFloor.ToString();

		/* Wysłanie eventu do Analytics */
		GameAnalytics.NewDesignEvent ("PATH_DRAWING:" + PlayerPrefs.GetString ("USER_POSITION") + "-" + PlayerPrefs.GetString("USER_DESTINATION"));

		/* Przełączenie na tryb follow */
		ToggleCameraMode ();

    }

    /* Metoda zmiany piętra */
    public void ChangeFloor(int diff)
    {
        if(ChangeFloorCoroutineVariable == null)
            ChangeFloorCoroutineVariable = StartCoroutine(ChangeFloorCoroutine(diff));
    }

    /* Obsługa przycisku zmiany piętra */
    public void ChangeFloorButton(int diff)
    {
        if(!FollowNavigator)
            ChangeFloor(diff);
    }

    /* Coroutine zmiany piętra */
    private Coroutine ChangeFloorCoroutineVariable;
    IEnumerator ChangeFloorCoroutine(int diff)
    {
        /* Określamy obecne i docelowe piętra */
        int CurrentFloor = 0, DestinationFloor = 0;
        for (int i = 0; i < Floors.Length; i++)
        {
            if (Floors[i].activeSelf)
            {
                CurrentFloor = i;
                DestinationFloor = i + diff;
                break;
            }
        }

        /* Sprawdzamy czy docelowe piętro istnieje */
        if (DestinationFloor < 0 || DestinationFloor > 7)
        {
            Debug.Log("Can't change floor!");
            yield return null;
        }
        else
        {
            /* Ukrywamy ekran */
            fadeImg.alpha = 0;
            while (fadeImg.alpha != 1)
            {
                fadeImg.alpha += 0.1f;
                yield return new WaitForEndOfFrame();
            }

            /* Zmieniamy piętro */
            Floors[CurrentFloor].SetActive(false);
            Floors[DestinationFloor].SetActive(true);
            ActiveFloor = int.Parse(Floors[DestinationFloor].tag.Split(' ')[1]);
            FloorInfoText.text = ActiveFloor.ToString();

            /* Jeśli były wciśnięte schody, nakierowujemy kamerę na odpowiednie schody na nowym piętrze */
            if (EventSystem.current.currentSelectedGameObject != null)
            {
                string[] split = EventSystem.current.currentSelectedGameObject.name.Split(' ');
                if (split[0] == "Stairs")
                {
                    string ClickedStairsDir = split[split.Length - 1];
                    Vector2 ClickedStairsPosition = EventSystem.current.currentSelectedGameObject.transform.position, NewStairsPosition = new Vector2(float.MaxValue, float.MaxValue);
                    Transform Staircases = GameObject.Find("STAIRCASES").transform;

                    for (int i = 0; i < Staircases.childCount; i++)
                    {
                        Vector2 tmpPos = Staircases.GetChild(i).position;
                        if (!Staircases.GetChild(i).name.Contains(ClickedStairsDir) && (Vector2.Distance(tmpPos, ClickedStairsPosition) < Vector2.Distance(NewStairsPosition, ClickedStairsPosition)))
                        {
                            NewStairsPosition = tmpPos;
                        }
                    }
                    this.transform.position = new Vector3(NewStairsPosition.x, NewStairsPosition.y, this.transform.position.z);
                }
            }

            /* Chowamy albo pokazujemy wskaźnik pozycji */
            if(Navigator.isInitialized)
                Navigator.GetComponent<SpriteRenderer>().enabled = (DestinationFloor == Navigator.CurrentFloor + 2);

            /* Pokazujemy ekran */
            while (fadeImg.alpha != 0)
            {
                fadeImg.alpha -= 0.1f;
                yield return new WaitForEndOfFrame();
            }
        }

        /* Czyszczenie zmiennej coroutine */
        ChangeFloorCoroutineVariable = null;

    }

    /* Coroutine płynnego przejścia w tryb follow navigator */
    Coroutine SmoothLocalizeCoroutineVariable;
    IEnumerator SmoothLocalize(Vector3 dest)
    {

        /* Włączenie trybu follow */
        FollowNavigator = true;
        this.transform.parent = Navigator.transform;
        this.GetComponent<TouchCamera>().enabled = false;
        GameObject.Find("ToggleCameraMode").transform.GetChild(0).GetComponent<Image>().color = Color.green;
        GameObject.Find("FloorUp").transform.GetChild(0).GetComponent<Image>().color = Color.gray;
        GameObject.Find("FloorDown").transform.GetChild(0).GetComponent<Image>().color = Color.gray;

        while (ChangeFloorCoroutineVariable != null)
            yield return new WaitForEndOfFrame();

        while (Vector2.Distance(this.transform.position, dest) > 0.025f)
        {
            this.transform.position = Vector3.Lerp(this.transform.position, dest, Time.deltaTime * 10);
            yield return new WaitForEndOfFrame();
        }
        this.transform.position = dest;

        while (Mathf.Abs(cam.orthographicSize - 3f) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 3f, Time.deltaTime * 10);
            yield return new WaitForEndOfFrame();
        }
        cam.orthographicSize = 3f;

        while (Vector3.Distance(this.transform.eulerAngles, Navigator.transform.eulerAngles) > 0.5f)
        {
            float eulerZ = this.transform.eulerAngles.z;
            eulerZ = Mathf.LerpAngle(eulerZ, Navigator.transform.eulerAngles.z, Time.deltaTime * 10);
            this.transform.eulerAngles = new Vector3(0, 0, eulerZ);
            yield return new WaitForEndOfFrame();
        }
        this.transform.eulerAngles = Navigator.transform.eulerAngles;

		GameObject NavButtons = GameObject.Find ("NavigationButtons");
		NavButtons.GetComponent<Animation> ().Play ("FadeIn");
		NavButtons.GetComponent<CanvasGroup> ().interactable = true;
		NavButtons.GetComponent<CanvasGroup> ().blocksRaycasts = true;

        /* Czyszczenie zmiennej coroutine */
        SmoothLocalizeCoroutineVariable = null;

    }

    /* Metoda przejścia w tryb follow navigator */
    private void Localize()
    {

        /* Określamy obecne piętro */
        int CurrentFloor = 0;
        for (int i = 0; i < Floors.Length; i++)
        {
            if (Floors[i].activeSelf)
            {
                CurrentFloor = i;
            }
        }

        /* Określamy piętro wskaźnika */
        int DestFloor = Navigator.CurrentFloor + 2;

        if(DestFloor != CurrentFloor)
            ChangeFloor(DestFloor - CurrentFloor);

        if (SmoothLocalizeCoroutineVariable == null)
            SmoothLocalizeCoroutineVariable = StartCoroutine(SmoothLocalize(new Vector3(Navigator.transform.position.x, Navigator.transform.position.y, this.transform.position.z)));

    }

    /* Coroutine płynnego przejścia w tryb freestyle */
    Coroutine SmoothUnlocalizeCoroutineVariable;
    IEnumerator SmoothUnlocalize()
    {

        this.transform.parent = null;

		GameObject NavButtons = GameObject.Find ("NavigationButtons");
		NavButtons.GetComponent<Animation> ().Play ("FadeOut");
		NavButtons.GetComponent<CanvasGroup> ().interactable = false;
		NavButtons.GetComponent<CanvasGroup> ().blocksRaycasts = false;

        while (Vector3.Distance(this.transform.eulerAngles, Vector3.zero) > 0.5f)
        {
            float eulerZ = this.transform.eulerAngles.z;
            eulerZ = Mathf.LerpAngle(eulerZ, Vector3.zero.z, Time.deltaTime * 10);
            this.transform.eulerAngles = new Vector3(0, 0, eulerZ);
            yield return new WaitForEndOfFrame();
        }
        this.transform.eulerAngles = Vector3.zero;

        while (Mathf.Abs(cam.orthographicSize - 4f) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, 4f, Time.deltaTime * 10);
            yield return new WaitForEndOfFrame();
        }
        cam.orthographicSize = 4f;

        /* Wyłączenie trybu follow */
        FollowNavigator = false;
        this.GetComponent<TouchCamera>().enabled = true;
        GameObject.Find("ToggleCameraMode").transform.GetChild(0).GetComponent<Image>().color = Color.white;
        GameObject.Find("FloorUp").transform.GetChild(0).GetComponent<Image>().color = Color.white;
        GameObject.Find("FloorDown").transform.GetChild(0).GetComponent<Image>().color = Color.white;

        /* Czyszczenie zmiennej coroutine */
        SmoothUnlocalizeCoroutineVariable = null;

    }

    /* Metoda przejścia w tryb follow freestyle */
    private void Unlocalize()
    {

        if (SmoothUnlocalizeCoroutineVariable == null)
            SmoothUnlocalizeCoroutineVariable = StartCoroutine(SmoothUnlocalize());

    }

    /* Obsługa przycisku wyboru trybu nawigacji */
    public void ToggleCameraMode()
    {
        if (SmoothLocalizeCoroutineVariable == null && SmoothUnlocalizeCoroutineVariable == null)
        {
            if (FollowNavigator)
            {
                Unlocalize();
            }
            else
            {
                Localize();
            }
        }
    }

	/* Obsługa przycisków nawigacji */
	public void MoveToNode(bool next)
	{
		if (next)
			Navigator.MoveToNextNode();
		else
			Navigator.MoveToPreviousNode();
	}
}
