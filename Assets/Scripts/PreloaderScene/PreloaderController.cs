using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class PreloaderController : MonoBehaviour {

	/* Animacja preloadera */
	private Animation preloaderAnim;

	/* Inicjalizacja */
	IEnumerator Start () 
	{

		/* Ustawienie optymalnych FPS */
		Application.targetFrameRate = 60;

		/* Animacja początkowa */
		preloaderAnim = this.GetComponent<Animation> ();

		yield return new WaitUntil (() => SplashScreen.isFinished);

		preloaderAnim.Play ("PreloaderFadeIn");

		yield return new WaitUntil (() => !preloaderAnim.isPlaying);

		preloaderAnim.Play ("PreloaderFadeOut");

		yield return new WaitUntil (() => !preloaderAnim.isPlaying);

		/* Załadowanie kolejnej sceny */
		SceneManager.LoadScene ("InitialScene");

	}

}
