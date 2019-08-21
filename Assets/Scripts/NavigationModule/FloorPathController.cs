using UnityEngine;
using System.Collections.Generic;

public class FloorPathController : MonoBehaviour
{

    /* Prefab pojedyńczego węzła */
    public GameObject NodePrefab;

    /* Stairs */
    public Transform Stairs;

    /* Node arrows */
    public GameObject PathArrowPrefab;

    /* Metoda rysowania ścieżki w obrębie piętra, zwraca true jeśli ścieżka została znaleziona */
    public List<Transform> DrawPath(GameObject Start, GameObject End, bool ReverseDirection)
    {

        Vector2 StartPos = Start.transform.position;
        Vector2 EndPos = End.transform.position;

        /* Szukamy rzutu pozycji startu ścieżki */
        FloorPointProjection startPointProjection = new FloorPointProjection();
        for(int i = 0; i < this.transform.childCount; i++)
        {
            FloorPointProjection startPointProjectionTmp = GetPointProjection(this.transform.GetChild(i), StartPos, Start.tag, startPointProjection);

            if (Vector2.Distance(StartPos, startPointProjectionTmp.FoundPoint) < Vector2.Distance(StartPos, startPointProjection.FoundPoint))
                startPointProjection = startPointProjectionTmp;
        }

        /* Jeśli nie został znaleziony rzut startu kończymy działanie funkcji */
        if (!startPointProjection.ProjectionFound)
        {
            Debug.Log("Can't localize start position on current floor.");
            return null;
        }

        /* Dodajemy węzły startowe */
        Transform startNodeParent = Instantiate(NodePrefab).transform;
        startNodeParent.position = startPointProjection.FoundPoint;
        startNodeParent.parent = startPointProjection.ParentNode;
        startPointProjection.ChildNode.parent = startNodeParent;
        startNodeParent.name = "START_NODE_PARENT";
        Transform startNode = Instantiate(NodePrefab).transform;
        startNode.position = StartPos;
        startNode.parent = startNodeParent;
        startNode.name = "START_NODE";

        /* Szukamy rzutu pozycji końca ścieżki */
        FloorPointProjection endPointProjection = new FloorPointProjection();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            FloorPointProjection endPointProjectionTmp = GetPointProjection(this.transform.GetChild(i), EndPos, End.tag, endPointProjection);

            if (Vector2.Distance(EndPos, endPointProjectionTmp.FoundPoint) < Vector2.Distance(EndPos, endPointProjection.FoundPoint))
                endPointProjection = endPointProjectionTmp;
        }

        /* Jeśli nie został znaleziony rzut końca kończymy działanie funkcji */
        if (!endPointProjection.ProjectionFound)
        {
            Debug.Log("Can't localize end position on current floor.");
            return null;
        }

        /* Dodajemy węzły końcowe */
        Transform endNodeParent = Instantiate(NodePrefab).transform;
        endNodeParent.position = endPointProjection.FoundPoint;
        endNodeParent.parent = endPointProjection.ParentNode;
        endPointProjection.ChildNode.parent = endNodeParent;
        endNodeParent.name = "END_NODE_PARENT";
        Transform endNode = Instantiate(NodePrefab).transform;
        endNode.position = EndPos;
        endNode.parent = endNodeParent;
        endNode.name = "END_NODE";

        /* Określenie ścieżki */
        List<Transform> path = FindPath(startNode, endNode);

        /* Nie znaleziono ścieżki */
        if (path == null)
        {
            Debug.Log("Path from " + Start.name + " to " + End.name + " not found.");
            return null;
        }

        /* Odwrócenie listy dla dróg odwrotnych */
        if (ReverseDirection)
            path.Reverse();

        /* Rysowanie ścieżki */
        for (int i = 0; i < path.Count - 1; i++)
        {
            /* Ustawienie strzałki nawigacji */
            if (Vector2.Distance(path[i].position, path[i + 1].position) > 0.3f)
            {
                GameObject arrow = Instantiate(PathArrowPrefab);
				arrow.tag = "NAVIGATION_ARROW_INSTANCE";
                arrow.transform.parent = path[i];
                arrow.transform.position = path[i].position + (path[i + 1].position - path[i].position).normalized * (Vector2.Distance(path[i].position, path[i + 1].position) / 2);

                if (Mathf.Abs(path[i].position.y - path[i + 1].position.y) < 1f/1000f)
                    arrow.transform.eulerAngles = new Vector3(0, 0, (path[i].position.x > path[i + 1].position.x) ? 90 : -90);
                if (Mathf.Abs(path[i].position.x - path[i + 1].position.x) < 1f / 1000f)
                    arrow.transform.eulerAngles = new Vector3(0, 0, (path[i].position.y > path[i + 1].position.y) ? 180 : 0);
            }

            /* Okreslanie pozycji dla line renderera */
            Vector3[] positions = { path[i].position + Vector3.back, path[i + 1].position + Vector3.back };
            path[i].GetComponent<SpriteRenderer>().enabled = true;
            path[i].GetComponent<LineRenderer>().enabled = true;
            path[i].GetComponent<LineRenderer>().SetVertexCount(positions.Length);
            path[i].GetComponent<LineRenderer>().SetPositions(positions);
			path [i].GetComponent<LineRenderer> ().startColor = new Color (136f / 255f, 204f / 255f, 85f / 255f, 255f / 255f);
			path [i].GetComponent<LineRenderer> ().endColor = new Color (136f / 255f, 204f / 255f, 85f / 255f, 255f / 255f);
        }

        path[path.Count-1].GetComponent<SpriteRenderer>().enabled = true;

        return path;

    }

    /* Metoda rysowania ścieżki do najbliższych schodów, parametr dir określa górę albo dół (up, down) */
    public KeyValuePair<GameObject, List<Transform>> DrawPathToNearestStairs(GameObject Start, string dir, bool ReverseDirection, string BuildingPart)
    {
        GameObject FoundStairs = null;
        float minDistance = float.MaxValue;
        for(int i = 0; i < Stairs.childCount; i++)
        {
            if (Stairs.GetChild(i).name.Contains(dir) && Vector2.Distance(Stairs.GetChild(i).position, Start.transform.position) < minDistance && Stairs.GetChild(i).name.Contains(BuildingPart))
            {
                FoundStairs = Stairs.GetChild(i).gameObject;
                minDistance = Vector2.Distance(FoundStairs.transform.position, Start.transform.position);

            }
        }

        if(FoundStairs == null)
        {
            Debug.Log("There are no " + dir + "stairs on floor " + this.transform.parent.parent.name);
            return new KeyValuePair<GameObject, List<Transform>>(null, null);
        }

        List<Transform> path = DrawPath(Start, FoundStairs, ReverseDirection);
        return new KeyValuePair<GameObject, List<Transform>>(FoundStairs, path);
    }

    /* Metoda zwracająca schody o podanej nazwie z tego piętra */
    public GameObject GetStairsByName(string name)
    {
        for(int i = 0; i < Stairs.childCount; i++)
        {
            if (Stairs.GetChild(i).name == name)
                return Stairs.GetChild(i).gameObject;
        }
        return null;
    }

    /* Metoda zwracająca skrzydło w których znajduje się dany obiekt */
    public string GetBuildingPart(GameObject point)
    {

        /* Szukamy rzutu podanego obiektu na szkielet piętra */
        FloorPointProjection PointProjection = new FloorPointProjection();
        for (int i = 0; i < this.transform.childCount; i++)
        {
            FloorPointProjection PointProjectionTmp = GetPointProjection(this.transform.GetChild(i), point.transform.position, point.tag, PointProjection);

            if (Vector2.Distance(point.transform.position, PointProjectionTmp.FoundPoint) < Vector2.Distance(point.transform.position, PointProjection.FoundPoint))
                PointProjection = PointProjectionTmp;

        }

        /* Jeśli nie zostały znalezione oba rzuty kończymy działanie funkcji */
        if (!PointProjection.ProjectionFound)
        {
            Debug.Log("Can't localize this position on current floor.");
            return null;
        }

        Transform itt = PointProjection.ParentNode;
        while (itt.parent != this.transform)
            itt = itt.parent;

        return itt.name;

    }

    /* Metoda zwracająca listę wierzchołków zawartych w ścieżce (od startu do końca) */
    private List<Transform> FindPath(Transform StartNode, Transform EndNode)
    {

        /* Sprawdzenie czy docelowe węzły są w tym samym drzewie i określenie korzenia */
        Transform StartRootNode, EndRootNode, RootNode;
        Transform itt = StartNode;

        /* Korzeń drzewa startu */
        while (itt.parent != this.transform)
            itt = itt.parent;
        StartRootNode = itt;

        /* Korzeń drzewa końca */
        itt = EndNode;
        while (itt.parent != this.transform)
            itt = itt.parent;
        EndRootNode = itt;

        /* Jeśli start i koniec nie są w tym samym drzewie kończymy działanie */
        if (StartRootNode != EndRootNode)
            return null;
        RootNode = StartRootNode;

        /* Utworzenie i inicjalizacja iteratora na ostatni wierchołek ścieżki */
        itt = EndNode;

        /* Zapisanie gałęzi końcowej ścieżki (od EndNode do RootNode) */
        List<Transform> EndBranch = new List<Transform>();
        while (itt != RootNode)
        {
            EndBranch.Add(itt);
            /* Jesli napotkany zostął węzeł startowy, zwracamy kompletną ścieżkę */
            if (itt == StartNode)
            {
                return EndBranch;
            }
            itt = itt.parent;
        }
        EndBranch.Add(itt);

        /* Utworzenie i inicjalizacja gałęzni początkowej ścieżki (od StartNode do miejsca spotkania z gałęzią końcową) */
        List<Transform> StartBranch = new List<Transform>();
        itt = StartNode;
        while (!EndBranch.Contains(itt))
        {
            StartBranch.Add(itt);
            itt = itt.parent;
        }
        StartBranch.Add(itt);

        /* Konstrukcja ścieżki na podstawie wyszukanych gałęzi */
        List<Transform> path = StartBranch;
        int EndBranchIndex = EndBranch.FindIndex(x => (x.gameObject == StartBranch[StartBranch.Count - 1].gameObject)) - 1;

        /* Ścieżka nie znaleziona! */
        if (EndBranchIndex < 0)
        {
            return null;
        }

        for (int i = EndBranchIndex; i > -1; i--)
        {
            path.Add(EndBranch[i]);
        }

        return path;
    }

    /* Struktura pomocnicza określająca rzut dowolnego punktu na szkielet piętra */
    private class FloorPointProjection
    {
        public FloorPointProjection()
        {
            FoundPoint = new Vector2(float.MaxValue, float.MaxValue);
            ProjectionFound = false;
        }
        public Transform ParentNode, ChildNode;
        public Vector2 FoundPoint;
        public bool ProjectionFound;
    }

    /* Metoda rekurencyjna zwracająca rzut na szkielet piętra dowolnego punktu o jednym z 4 kierunków */
    private FloorPointProjection GetPointProjection(Transform Root, Vector2 OriginalPoint, string Direction, FloorPointProjection CurrentFound)
    {
        /* Iteruj przez wszystkie dzieci danego węzła */
        for (int i = 0; i < Root.childCount; i++)
        {
            /* Warunek znalezienia optymalnego rzutu, lewo, prawo */
            if (    /* Warunek odpowiedniego kierunku */
                    (
                    (Direction == "DIR_LEFT" && Root.position.x < OriginalPoint.x) || (Direction == "DIR_RIGHT" && Root.position.x > OriginalPoint.x)
                    )
                    &&
                    /* Warunek zawierania się rzutu na krawędzi szkieletu  */
                    (
                    (Root.position.y <= OriginalPoint.y && OriginalPoint.y <= Root.GetChild(i).position.y) || (Root.GetChild(i).position.y <= OriginalPoint.y && OriginalPoint.y <= Root.position.y)
                    )
                    &&
                    /* Warunek najbliższej krawędzi szkieletu */
                    (
                    Mathf.Abs(OriginalPoint.x - Root.position.x) < Mathf.Abs(OriginalPoint.x - CurrentFound.FoundPoint.x)
                    )
               )
            {
                /* Parametry nowego, lepszego rzutu */
                CurrentFound.ParentNode = Root;
                CurrentFound.ChildNode = Root.GetChild(i);
                CurrentFound.FoundPoint = new Vector2(Root.position.x, OriginalPoint.y);
                CurrentFound.ProjectionFound = true;
            }

            /* Warunek znalezienia optymalnego rzutu, góra, dół */
            if (
                   /* Warunek odpowiedniego kierunku */
                   (
                   (Direction == "DIR_DOWN" && Root.position.y < OriginalPoint.y) || (Direction == "DIR_UP" && Root.position.y > OriginalPoint.y)
                   )
                   &&
                   /* Warunek zawierania się rzutu na krawędzi szkieletu  */
                   (
                   (Root.position.x <= OriginalPoint.x && OriginalPoint.x <= Root.GetChild(i).position.x) || (Root.GetChild(i).position.x <= OriginalPoint.x && OriginalPoint.x <= Root.position.x)
                   )
                   &&
                   /* Warunek najbliższej krawędzi szkieletu */
                   (
                   Mathf.Abs(OriginalPoint.y - Root.position.y) < Mathf.Abs(OriginalPoint.y - CurrentFound.FoundPoint.y)
                   )
               )
            {
                /* Parametry nowego, lepszego rzutu */
                CurrentFound.ParentNode = Root;
                CurrentFound.ChildNode = Root.GetChild(i);
                CurrentFound.FoundPoint = new Vector2(OriginalPoint.x, Root.position.y);
                CurrentFound.ProjectionFound = true;
            }

            /* Dalsze przeszukiwanie poddrzew */
                CurrentFound = GetPointProjection(Root.GetChild(i), OriginalPoint, Direction, CurrentFound);
        }

        /* Węzeł bez poddrzew, zwracamy obecny rzut */
        return CurrentFound;

    }

}
