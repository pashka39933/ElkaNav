using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BuildingNavigator : MonoBehaviour {

    /* Tekst piętra, zmienna pomocnicza */
    public Text FloorInfoText;

    private List<KeyValuePair<int, List<Vector2>>> BuildingPath;

    /* Zmienna przechowująca numer etapu na którym znajduje się wskaźnik */
    private int CurrentPart = 0;

    /* Zmienna przechowująca numer węzła na którym znajduje się wskaźnik */
    private int CurrentNode = 0;

    /* Zmienna okreslająca piętro na którym obecnie znajduje się nawigator */
    public int CurrentFloor = 0;

    /* Kontroler kamery */
    public CameraController camController;

    /* Zmienna pomocnicza określająca czy NAvigator był inicjowany */
    public bool isInitialized = false;

    /* Metoda zainicjowania nawigatora */
    public void Init(List<KeyValuePair<int, List<Transform>>> BuildingPath)
    {

        /* Wyczyszczenie innych nawigatorów jeśli znajdują się w scenie */
        BuildingNavigator[] navs = GameObject.FindObjectsOfType<BuildingNavigator>();
        for(int i = 0; i < navs.Length; i++)
        {
            if (navs[i] != this)
                Destroy(navs[i].gameObject);
        }

        /* Przyporządkowanie zmiennych */
        this.BuildingPath = ConvertAndOptimizePath(BuildingPath, 1.25f);

        this.transform.position = BuildingPath[CurrentPart].Value[CurrentNode].position;
        this.CurrentFloor = BuildingPath[CurrentPart].Key;

		/* Jeśli wykryty tag nie jest tagiem STRONG, przesuwamy wskaźnik w przód (przed drzwi) */
		if (!PlayerPrefs.GetString ("USER_POSITION").Contains ("STRONG"))
			MoveToNextNode ();

        this.GetComponent<SpriteRenderer>().enabled = true;

        /* Włączenie magnetometru */
        Input.compass.enabled = true;

        /* Ustawienie flagi */
        isInitialized = true;
    }

    /* Metoda przesunięcia wskaźnika do następnego węzła (animationSpeed == 0, brak animacji) */
    public void MoveToNextNode()
    {
        if (MoveNavigatorCoroutineVariable == null)
        {
            CurrentNode++;
            if (CurrentNode > BuildingPath[CurrentPart].Value.Count - 1)
            {
                CurrentPart++;
                if (CurrentPart > BuildingPath.Count - 1)
                {
                    CurrentPart--;
                    CurrentNode = BuildingPath[CurrentPart].Value.Count - 1;
                    Debug.Log("Can't move to next node, path completed.");
                    return;
                }
                CurrentNode = 0;
            }

            int FloorDiff = BuildingPath[CurrentPart].Key - this.CurrentFloor;
            this.CurrentFloor = BuildingPath[CurrentPart].Key;

            MoveNavigatorCoroutineVariable = StartCoroutine(MoveNavigatorCoroutine(BuildingPath[CurrentPart].Value[CurrentNode], FloorDiff));
            //Debug.Log("PART: " + CurrentPart + ", NODE: " + CurrentNode);
        }
    }

    /* Metoda przesunięcia wskaźnika do następnego węzła (animationSpeed == 0, brak animacji) */
    public void MoveToPreviousNode()
    {
        if (MoveNavigatorCoroutineVariable == null)
        {
            CurrentNode--;
            if (CurrentNode < 0)
            {
                CurrentPart--;
                if (CurrentPart < 0)
                {
                    CurrentPart++;
                    CurrentNode = 0;
                    Debug.Log("Can't move to next node, path completed.");
                    return;
                }
                CurrentNode = BuildingPath[CurrentPart].Value.Count - 1;
            }

            int FloorDiff = BuildingPath[CurrentPart].Key - this.CurrentFloor;
            this.CurrentFloor = BuildingPath[CurrentPart].Key;

            MoveNavigatorCoroutineVariable = StartCoroutine(MoveNavigatorCoroutine(BuildingPath[CurrentPart].Value[CurrentNode], FloorDiff));
            //Debug.Log("PART: " + CurrentPart + ", NODE: " + CurrentNode);
        }
    }

    /* Coroutine dla ruchu wskaźnika */
    Coroutine MoveNavigatorCoroutineVariable;
    IEnumerator MoveNavigatorCoroutine(Vector2 destPosition, int FloorDiff)
    {

        SpriteRenderer NavigatorSpriteRenderer = this.GetComponent<SpriteRenderer>();
        Color initial = NavigatorSpriteRenderer.color, transparent = new Color(0, 0, 0, 0);

        if (FloorDiff != 0)
        {
            while (NavigatorSpriteRenderer.color != transparent)
            {
                NavigatorSpriteRenderer.color = Color.Lerp(NavigatorSpriteRenderer.color, transparent, Time.deltaTime * 25);
                yield return new WaitForEndOfFrame();
            }
            NavigatorSpriteRenderer.color = transparent;

            if (camController.FollowNavigator)
            {
                camController.ChangeFloor(FloorDiff);
            }
        }

        /* Chowamy albo pokazujemy wskaźnik */
        this.GetComponent<SpriteRenderer>().enabled = (this.CurrentFloor == camController.ActiveFloor);

        if (camController.FollowNavigator || FloorDiff == 0)
        {
            while (Vector2.Distance(this.transform.position, destPosition) > 0.01f)
            {
                this.transform.position = Vector2.Lerp(this.transform.position, destPosition, Time.deltaTime * 20);
                yield return new WaitForEndOfFrame();
            }
        }
        this.transform.position = destPosition;

        if(FloorDiff != 0)
        {
            while (NavigatorSpriteRenderer.color != initial)
            {
                NavigatorSpriteRenderer.color = Color.Lerp(NavigatorSpriteRenderer.color, initial, Time.deltaTime * 25);
                yield return new WaitForEndOfFrame();
            }
            NavigatorSpriteRenderer.color = initial;
        }

        /* Czyszczenie zmiennej coroutine */
        MoveNavigatorCoroutineVariable = null;
    }

    /* Metoda optymalizacji wyznaczonej ścieżki, maxDist - maksymalna odległość między dwoma wierzchołkami */
    public List<KeyValuePair<int, List<Vector2>>> ConvertAndOptimizePath(List<KeyValuePair<int, List<Transform>>> list, float maxDist)
    {
        /* Ścieżka zwracana budynku */
        List<KeyValuePair<int, List<Vector2>>> retBuildingList = new List<KeyValuePair<int, List<Vector2>>>();

        /* Iteracja po etapach ścieżki */
        for (int i = 0; i < list.Count; i++)
        {
            /* Ścieżka zwracana etapu */
            List<Vector2> retFloorList = new List<Vector2>();
            /* Uproszczenie etapu ścieżki (usuwanie węzłów nie będących na zakrętach ścieżki) */
            List<Transform> iterList = list[i].Value;
            for(int itt = 1; itt < iterList.Count - 2; itt++)
            {
                if (iterList[itt].position.x == iterList[itt + 1].position.x && iterList[itt].position.x == iterList[itt - 1].position.x)
                    iterList.RemoveAt(itt);
                else if (iterList[itt].position.y == iterList[itt + 1].position.y && iterList[itt].position.y == iterList[itt - 1].position.y)
                    iterList.RemoveAt(itt);
            }
            /* Iteracja po węzłach danego etapu (uproszczonego) ścieżki */
            for (int j = 0; j < iterList.Count - 1; j++)
            {
                /* Dodanie obecnego węzła */
                retFloorList.Add(iterList[j].position);
                /* Obliczenie odległości do kolejnego węzła */
                float RealDist = Vector2.Distance(iterList[j].position, iterList[j+1].position);
                /* Obliczenie potrzebnej ilości dodatkowych węzłów */
                int AdditionalNodes = Mathf.FloorToInt(RealDist / maxDist);
                if (AdditionalNodes > 0)
                {
                    /* Obliczenie optymalnej odległości między węzłami */
                    float OptimizedDist = RealDist / (AdditionalNodes + 1);
                    /* Iteracyjne tworzenie nowych węzłów */
                    for (int k = 0; k < AdditionalNodes; k++)
                    {
                        Vector2 newNode = list[i].Value[j].position;
                        newNode = iterList[j].position + ((iterList[j+1].position - iterList[j].position).normalized * (OptimizedDist * (k + 1)));
                        retFloorList.Add(newNode);
                    }
                }
            }
            retFloorList.Add(iterList[iterList.Count - 1].position);
            retBuildingList.Add(new KeyValuePair<int, List<Vector2>>(list[i].Key, retFloorList));
        }

        return retBuildingList;

    }

    /* Rotacja wskaźnika na podstawie odczytu magnetometru */
    void Update()
    {
        float eulerZ = this.transform.eulerAngles.z;
        eulerZ = Mathf.LerpAngle(eulerZ, -Input.compass.magneticHeading + 180, Time.deltaTime * 5);
        this.transform.eulerAngles = new Vector3(0, 0, eulerZ);
    }

}
