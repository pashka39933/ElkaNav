using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    /* Animacja fade */
    public Animation fadeAnim;

    /* Aniamcja InfoPanel */
    public Animation infoPanelAnim;

    /* zmienna przechowująca którą scene trzeba włączyc */
    private string SceneToLaunch = "";

    /* Sprawdzenie obiektu budynku */
    void Start()
    {

        /* Wyszukanie obiektu budynku */
        BuildingPathController BuildingObject = GameObject.FindObjectOfType<BuildingPathController>();
        if (BuildingObject == null)
        {
            Debug.Log("Building object not found! Launching initial scene...");
			PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
			SceneManager.LoadScene("InitialScene");
            return;
        }

        /* Wyłączenie wszystkich pięter budynku */
        for (int i = 0; i < BuildingObject.transform.childCount; i++)
        {
            BuildingObject.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

	/* Callback przycisku MAP */
    public void LaunchMap()
    {
		PlayerPrefs.SetString("USER_POSITION", "MAP_EXPLORE_MODE");
		SceneToLaunch = "NavigationModuleScene";
        fadeAnim.Play("FadeIn");
        StartCoroutine(WaitForFade());
    }

    /* Callback przycisku NAVIGATION */
    public void LaunchNav()
    {
		SceneToLaunch = "DestinationChooser";
        fadeAnim.Play("FadeIn");
        StartCoroutine(WaitForFade());
    }

    /* Coroutine wyczekania animacji fade */
    IEnumerator WaitForFade()
    {
        yield return new WaitWhile(() => fadeAnim.isPlaying);
		PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);

		if (SceneToLaunch == "DestinationChooser")
			SceneManager.LoadScene ("DestinationChooser");
		else
			SceneManager.LoadScene(SceneToLaunch);
    }

    /* Callback przycisku pokazania/schowania InfoPanel */
    bool firstClick = false;
    public void ToggleInfoPanel()
    {
        if (!firstClick)
        {
            infoPanelAnim.Play();
            firstClick = true;
        }
        else
        {
            infoPanelAnim[infoPanelAnim.clip.name].speed = -infoPanelAnim[infoPanelAnim.clip.name].speed;
            if (infoPanelAnim[infoPanelAnim.clip.name].speed < 0)
                infoPanelAnim[infoPanelAnim.clip.name].time = infoPanelAnim[infoPanelAnim.clip.name].length;
            else
                infoPanelAnim[infoPanelAnim.clip.name].time = 0;
            infoPanelAnim.Play();
        }
    }
}
