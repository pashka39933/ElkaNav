using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class NavBarController : MonoBehaviour {

    /* Menu */
    public GameObject Menu;

	/* Animacja obiektu fade */
	public Animation FadeAnim;

    /* Zmienna pomocnicza określająca stan menu */
    bool MenuOn = false;

    /* Inicjalizacja */
    void Start()
    {

        Menu.GetComponent<RectTransform>().offsetMin = new Vector2(180f, 0);
    }

    /* Metoda chowania/pokazywania menu */
    Coroutine ToggleMenuCoroutineVariable;
	public void ToggleMenu()
    {
        if(ToggleMenuCoroutineVariable != null)
            StopCoroutine(ToggleMenuCoroutineVariable);

        ToggleMenuCoroutineVariable = StartCoroutine(ToggleMenuCoroutine());
    }

    /* Coroutine płynnego pokazania/chowania menu */
    IEnumerator ToggleMenuCoroutine()
    {

        RectTransform MenuRect = Menu.GetComponent<RectTransform>();
        float MenuDestLeft = 0f;

        if(MenuOn)
        {
            MenuDestLeft = 180f;
        }
        else
        {
            MenuDestLeft = 0f;
        }

        while(MenuRect.offsetMin.x != MenuDestLeft)
        {
            if(MenuOn)
            {
                MenuRect.offsetMin = new Vector2(MenuRect.offsetMin.x + 30f, 0);
            }
            else
            {
                MenuRect.offsetMin = new Vector2(MenuRect.offsetMin.x - 30f, 0);
            }
            yield return new WaitForEndOfFrame();
        }

        MenuOn = !MenuOn;
    }

    /* Metoda przełączenia na moduł AR */
    public void LaunchARModule()
    {
		FadeAnim.Play ("FadeIn");
		StartCoroutine (WaitForFadeARModule ());
    }

	/* Coroutine wyczekania zaciemnienia */
	IEnumerator WaitForFadeARModule()
	{
		yield return new WaitWhile (() => FadeAnim.isPlaying);
		GameObject.FindObjectOfType<BuildingPathController> ().ClearPath ();
		PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
		SceneManager.LoadScene("ARModuleScene");
	}

	/* Metoda przełączenia na scene wyboru miejsca docelowego */
	public void LaunchDestinationChooser()
	{
		FadeAnim.Play ("FadeIn");
		StartCoroutine (WaitForFadeDestinationChooser ());
	}

	/* Coroutine wyczekania zaciemnienia */
	IEnumerator WaitForFadeDestinationChooser()
	{
		yield return new WaitWhile (() => FadeAnim.isPlaying);
		GameObject.FindObjectOfType<BuildingPathController> ().ClearPath ();
		PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
		PlayerPrefs.SetInt ("DestinationChooserLaunchedFromNavigationModule", 1);
		SceneManager.LoadScene ("DestinationChooser");
	}

    /* Metoda przełączenia na scene menu */
    public void LaunchMenu()
    {
        FadeAnim.Play("FadeIn");
        StartCoroutine(WaitForFadeMenu());
    }

    /* Coroutine wyczekania zaciemnienia */
    IEnumerator WaitForFadeMenu()
    {
        yield return new WaitWhile(() => FadeAnim.isPlaying);
		GameObject.FindObjectOfType<BuildingPathController> ().ClearPath ();
		PlayerPrefs.SetString ("previousLevel", Application.loadedLevelName);
		SceneManager.LoadScene("MenuScene");
    }
}
