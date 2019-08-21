using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuNavigatorController : MonoBehaviour {

	/* Inicjalizacja, włączenie kompasu */
	void Start () 
	{
		Input.compass.enabled = true;
	}
	
	/* Rotacja przycisku zgodnie ze wskazaniami kompasu */
	void Update () 
	{
		float tmpZ = this.transform.eulerAngles.z;
		tmpZ = Mathf.LerpAngle (tmpZ, -Input.compass.magneticHeading, Time.deltaTime * 5f);
		this.transform.eulerAngles = new Vector3 (0, 0, tmpZ);
	}
}
