using UnityEngine;
using System.Collections;
using Vuforia;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GameAnalyticsSDK;

public class LoadNavigationModule : MonoBehaviour {

	/* Animacja obiektu fade */
	public Animation FadeAnim, CameraAnim, DetectionCanvasAnim;

	/* Loader DataSets */
	public DynamicDataSetLoader DataSetsLoader;

	/* Metoda inicjacji */
	void Start()
	{

		StartCoroutine (FadeWaitForVuforiaCamera ());
	}

	/* Coroutine wyczekania na inicjalizacje kamery vuforii */
	IEnumerator FadeWaitForVuforiaCamera()
	{
		yield return new WaitUntil (() => DataSetsLoader.AllDataSetsLoaded);
		FadeAnim.Play ("FadeOut");
	}

	/* Metoda schowania canvasa sugestii i załadowania modułu nawigacji */
	public void LoadNavigation(bool strongTag)
	{
		/* Jeśli nie został wykryty tag STRONG, zapisujemy numer z dropdowns */
		if (!strongTag) 
		{
			/* Pobranie pozycji z dropdownów */
			GameObject DropdownsHolder = GameObject.Find ("Dropdowns");
			string UserPosition = "";
			for (int i = 0; i < DropdownsHolder.transform.childCount; i++) {
				string DropdownValue = DropdownsHolder.transform.GetChild (i).GetComponentInChildren<Text> ().text;
				UserPosition = UserPosition + DropdownValue;
			}
			if (UserPosition.Length < 1)
				return;
			else
				PlayerPrefs.SetString ("USER_POSITION", UserPosition);
		}

		/* Wyłączenie obiektów augmentation związanych z ImageTargets */
		DefaultTrackableEventHandler[] handlers = GameObject.FindObjectsOfType<DefaultTrackableEventHandler> ();
		foreach (DefaultTrackableEventHandler handler in handlers) 
		{
			for (int i = 0; i < handler.transform.childCount; i++)
			{
				handler.transform.GetChild (i).gameObject.SetActive (false);
			}
		}
		/* Schowanie DetectionCanvasa i rozszerzenie kamery */
		if (CameraAnim.clip.name == "DetectionFoundCamera") 
		{
			CameraAnim.clip = CameraAnim.GetClip ("DetectionLostCamera");
			CameraAnim.Play ();
		}
		if (DetectionCanvasAnim.clip.name == "DetectionFoundCanvas") 
		{
			DetectionCanvasAnim.clip = DetectionCanvasAnim.GetClip ("DetectionLostCanvas");
			DetectionCanvasAnim.Play ();
		}

		/* Wysłanie eventu do Analytics */ 
		GameAnalytics.NewDesignEvent ("USER_POSITION:" + PlayerPrefs.GetString ("USER_POSITION"));

		/* Uruchomienie coroutine przełączenia */
		StartCoroutine (SwitchToNavigation ());
	}

	/* Coroutine wyczekania końca animacji fade */
	IEnumerator SwitchToNavigation()
	{
		yield return new WaitWhile (() => CameraAnim.isPlaying);
		yield return new WaitWhile (() => DetectionCanvasAnim.isPlaying);
		GameObject BG = GameObject.Find ("BackgroundPlane");
		Vector3 destScale = BG.transform.localScale * 3;
		while (Vector3.Distance(BG.transform.localScale, destScale) > 1f) 
		{
			BG.transform.localScale = Vector3.Lerp (BG.transform.localScale, destScale, Time.deltaTime * 10f);
			yield return new WaitForEndOfFrame ();
		}
		FadeAnim.Play ("FadeIn");
		yield return new WaitWhile (() => FadeAnim.isPlaying);

		/* Wyłączenie Vuforii */
		VuforiaBehaviour qcarBehaviour = (VuforiaBehaviour)UnityEngine.Object.FindObjectOfType(typeof(VuforiaBehaviour));
		qcarBehaviour.enabled = false;

		PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
		SceneManager.LoadScene ("NavigationModuleScene");
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

		/* Wyłączenie Vuforii */
		VuforiaBehaviour qcarBehaviour = (VuforiaBehaviour)UnityEngine.Object.FindObjectOfType(typeof(VuforiaBehaviour));
		qcarBehaviour.enabled = false;

		string previousScene = PlayerPrefs.GetString ("previousLevel", "InitialScene");
		PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
		SceneManager.LoadScene (previousScene);
	}
}
