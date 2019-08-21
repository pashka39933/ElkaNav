using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BuildingPathController : MonoBehaviour {

	/* Zmienna do przechowywania ścieżki (BuildingPath[ETAP_ŚCIEŻKI].Key == numer piętra, BuildingPath[ETAP_ŚCIEŻKI].Value = lista węzłów) */
	List<KeyValuePair<int, List<Transform>>> BuildingPath;

    /* Załadowanie budynku w scenie początkowej, uzupełnienie listy pomieszczeń i przejście do menu aplikacji */
    void Start()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);
		PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);

		/* Uzupełnienie listy pomieszczeń */
		this.GetComponent<BuildingRoomsListController> ().FillBuildingRoomsList ();

		/* Załadowanie menu */
		SceneManager.LoadScene("MenuScene");
    }

    /* Kontrolery rysowania ścieżek na każdym piętrze */
    public FloorPathController[] FloorPathControllers = new FloorPathController[8];

	/* Metoda rysowania ścieżki w obrębie budynku */
    public List<KeyValuePair<int, List<Transform>>> DrawPath(GameObject Start, GameObject End)
    {

        /* Określenie piętra startowego i końcowego */
        int StartFloor, EndFloor;

        Transform itt = Start.transform;
        while(itt.parent != this.transform)
        {
            itt = itt.parent;
        }
        StartFloor = int.Parse(itt.tag.Split(' ')[1]) + 2;

        itt = End.transform;
        while (itt.parent != this.transform)
        {
            itt = itt.parent;
        }
        EndFloor = int.Parse(itt.tag.Split(' ')[1]) + 2;

        string StartPart = FloorPathControllers[StartFloor].GetBuildingPart(Start);
        string EndPart = FloorPathControllers[EndFloor].GetBuildingPart(End);

        //Debug.Log("Start floor: " + StartFloor + ", part: " + StartPart + ", End floor: " + EndFloor + ", part: " + EndPart);

        //
        //
        // ###############################
        // ###### RYSOWANIE ŚCIEŻKI ######
        // ###############################
        //
        //

        /* Zmienna do przechowywania ścieżki (BuildingPath[ETAP_ŚCIEŻKI].Key == numer piętra, BuildingPath[ETAP_ŚCIEŻKI].Value = lista węzłów) */
        BuildingPath = new List<KeyValuePair<int, List<Transform>>>();
        /* Przypadek nr. 1: start i koniec w różnych skrzydłach */
        if(StartPart != EndPart)
        {

            /* Zmienne pomocnicze */
            int FloorItt = StartFloor;
            GameObject CurrentGameobjectItt = Start;

            /* Rysujemy ścieżkę od startu do piętra -1 */
            while (FloorItt != 1)
            {
                /* Ścieżka do najbliższych schodów na danym piętrze */
                KeyValuePair<GameObject, List<Transform>> tmp = FloorPathControllers[FloorItt].DrawPathToNearestStairs(CurrentGameobjectItt, FloorItt > 1 ? "down" : "up", false, "Stairs");
                GameObject tmpStairs = tmp.Key;
                List<Transform> path = tmp.Value;

                /* Zapamiętanie części ścieżki */
                BuildingPath.Add(new KeyValuePair<int, List<Transform>>(FloorItt - 2, path));

                /* Obecne schody */
                CurrentGameobjectItt = FloorPathControllers[FloorItt + (int)Mathf.Sign(1 - FloorItt)].GetStairsByName(tmpStairs.name.Replace(FloorItt > 1 ? "down" : "up", FloorItt > 1 ? "up" : "down"));
                /* Iterator piętra */
                FloorItt += (int)Mathf.Sign(1 - FloorItt);
            }

            FloorItt = EndFloor;
            GameObject StartGameobjectItt = CurrentGameobjectItt;
            CurrentGameobjectItt = End;
            List<KeyValuePair<int, List<Transform>>> BuildingPathPart3 = new List<KeyValuePair<int, List<Transform>>>();

            /* Rysujemy ścieżkę od końca do piętra -1 */
            while (FloorItt != 1)
            {
                /* Ścieżka do najbliższych schodów na danym piętrze */
                KeyValuePair<GameObject, List<Transform>> tmp = FloorPathControllers[FloorItt].DrawPathToNearestStairs(CurrentGameobjectItt, FloorItt > 1 ? "down" : "up", true, "Stairs");
                GameObject tmpStairs = tmp.Key;
                List<Transform> path = tmp.Value;

                /* Zapamiętanie części ścieżki */
                BuildingPathPart3.Add(new KeyValuePair<int, List<Transform>>(FloorItt - 2, path));

                /* Obecne schody */
                CurrentGameobjectItt = FloorPathControllers[FloorItt + (int)Mathf.Sign(1 - FloorItt)].GetStairsByName(tmpStairs.name.Replace(FloorItt > 1 ? "down" : "up", FloorItt > 1 ? "up" : "down"));
                /* Iterator piętra */
                FloorItt += (int)Mathf.Sign(1 - FloorItt);
            }
            BuildingPathPart3.Reverse();

            /* Rysujemy ścieżkę na piętrze -1 */
            List<Transform> BuildingPathPart2 = FloorPathControllers[FloorItt].DrawPath(StartGameobjectItt, CurrentGameobjectItt, false);

            BuildingPath.Add(new KeyValuePair<int, List<Transform>>(FloorItt - 2, BuildingPathPart2));
            BuildingPath.AddRange(BuildingPathPart3);

        }
        /* Przypadek nr. 2: start i koniec w tym samym skrzydle */
        else
        {

            /* Zmienne pomocnicze */
            int FloorItt = StartFloor;
            GameObject CurrentGameobjectItt = Start;

            /* Rysujemy ścieżkę od startu do piętra docelowego */
            while (FloorItt != EndFloor)
            {
                /* Ścieżka do najbliższych schodów z danego skrzydła na danym piętrze */
                KeyValuePair<GameObject, List<Transform>> tmp = FloorPathControllers[FloorItt].DrawPathToNearestStairs(CurrentGameobjectItt, FloorItt > EndFloor ? "down" : "up", false, StartPart);
                GameObject tmpStairs = tmp.Key;
                List<Transform> path = tmp.Value;

                /* Zapamiętanie części ścieżki */
                BuildingPath.Add(new KeyValuePair<int, List<Transform>>(FloorItt - 2, path));

                /* Obecne schody */
                CurrentGameobjectItt = FloorPathControllers[FloorItt + (int)Mathf.Sign(EndFloor - FloorItt)].GetStairsByName(tmpStairs.name.Replace(FloorItt > EndFloor ? "down" : "up", FloorItt > EndFloor ? "up" : "down"));
                /* Iterator piętra */
                FloorItt += (int)Mathf.Sign(EndFloor - FloorItt);
            }

            /* Rysujemy ścieżkę na piętrze docelowym */
            List<Transform> tmpPath = FloorPathControllers[FloorItt].DrawPath(CurrentGameobjectItt, End, false);
            BuildingPath.Add(new KeyValuePair<int, List<Transform>>(FloorItt - 2, tmpPath));

        }

        return BuildingPath;
    }

    /* Callback przycisków schodów, zmiana piętra kamerą */
    public void ChangeFloor(int diff)
    {
        CameraController cameraController = GameObject.FindObjectOfType<CameraController>();
        if (cameraController != null)
            cameraController.ChangeFloorButton(diff);
    }

	/* Metoda czyszczenia ścieżki */
	public void ClearPath()
	{
		/* Aktywowanie wszystkich pięter */
		for (int i = 0; i < this.transform.childCount; i++)
			this.transform.GetChild (i).gameObject.SetActive (true);

		/* Czyszczenie ścieżek */
		LineRenderer[] Nodes = GameObject.FindObjectsOfType<LineRenderer> ();
		foreach (LineRenderer node in Nodes) 
		{
			node.GetComponent<SpriteRenderer> ().enabled = false;
			node.enabled = false;
			if (node.name == "START_NODE" || node.name == "END_NODE")
				Destroy (node.gameObject);
			if (node.name == "START_NODE_PARENT" || node.name == "END_NODE_PARENT")
				node.name = "Node";
		}
		/* Czyszczenie navigation arrows */
		GameObject[] NavigationArrows = GameObject.FindGameObjectsWithTag ("NAVIGATION_ARROW_INSTANCE");
		foreach (GameObject navArrow in NavigationArrows) 
		{
			Destroy (navArrow);
		}

		/* Dezaktywowanie wszystkich pięter */
		for (int i = 0; i < this.transform.childCount; i++)
			this.transform.GetChild (i).gameObject.SetActive (false);

		BuildingPath = null;
	}
}
