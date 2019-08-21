using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Vuforia;
using System.Collections.Generic;
using System;
using System.Collections;

public class DynamicDataSetLoader : MonoBehaviour
{
    /* Nazwy datasets z Vuforia developer portal */
    public string[] dataSetNames;
	private string dataSetName;

	/* Zmienna pomocnicza do porządkowania obiektów */
	GameObject ParentObject;

	/* Prefab elementu wirtualnego w rzeczywistości rozszerzonej */
	public GameObject AugmentationObject;

	/* Flaga określająca stan załadowania DataSets */
	public bool AllDataSetsLoaded = false;

    /* Inicjalizacja, załadowanie DataSets */
    void Start()
    {

		/* Wyłączenie wszystkich pięter budynku */
		BuildingPathController Building = GameObject.FindObjectOfType<BuildingPathController> ();
		/* Restart aplikacji w przypadku nie znalezienia komponentu budynku (ponowne załadowanie) */
		if(Building == null)
		{
			Debug.Log("Building object not found! Launching initial scene...");
			PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
			SceneManager.LoadScene("InitialScene");
			return;
		}

		for (int i = 0; i < Building.transform.childCount; i++) 
		{
			Building.transform.GetChild (i).gameObject.SetActive (false);
		}

		/* Załadowanie DataSets */
		StartCoroutine (LoadDataSetsCoroutine ());
    }

	/* Coroutine do ładowania kolejnych DataSets */
	IEnumerator LoadDataSetsCoroutine()
	{
		/* Znalezienie obiektu VuforiaBehaviour i zarejestrowanie behaviour ImageTargetów */
		VuforiaBehaviour vb = GameObject.FindObjectOfType<VuforiaBehaviour>();
		foreach (string ds in dataSetNames) 
		{
			dataSetName = ds;
			ParentObject = new GameObject ();
			ParentObject.name = dataSetName;
			ParentObject.transform.parent = this.transform;
			vb.RegisterVuforiaStartedCallback(LoadDataSet);
			yield return new WaitWhile (() => DataSetLoading);
		}
		AllDataSetsLoaded = true;
	}

	/* Metoda załadowania DataSet */
	private bool DataSetLoading = false;
    void LoadDataSet ()
	{

		/* Ustawienie flagi ładowania */
		DataSetLoading = true;

		/* ObjectTracker */
		ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker> ();
		DataSet dataSet = objectTracker.CreateDataSet ();

		/* Jeśli udało się załadować DataSet */
		if (dataSet.Load (dataSetName)) {

			objectTracker.Stop ();

			/* Aktywacja */
			if (!objectTracker.ActivateDataSet (dataSet)) {
				/* Max 1000 ImageTargetów */
				Debug.Log ("Failed to Activate DataSet: " + dataSetName);
			}

			if (!objectTracker.Start ()) {
				Debug.Log ("Tracker Failed to Start.");
			}
				
			/* Definiowanie Trackables */
			IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager ().GetTrackableBehaviours ();
			foreach (TrackableBehaviour tb in tbs) {
				if (tb.name == "New Game Object") {

					/* Nazwa obiektu */
					tb.gameObject.name = "ImageTarget_" + tb.TrackableName;

					/* Komponenty */
					tb.gameObject.AddComponent<DefaultTrackableEventHandler> ();
					tb.gameObject.AddComponent<TurnOffBehaviour> ();

					/* Obiekt wirtualny AR */
					GameObject aug = Instantiate (AugmentationObject);
					aug.transform.SetParent (tb.transform);
					tb.transform.parent = ParentObject.transform;
				
				}
			}
		} else {
			Debug.LogError ("Failed to load dataset: " + dataSetName);
		}

		/* Ustawienie flagi ładowania */
		DataSetLoading = false;
	}
}