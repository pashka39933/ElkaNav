using UnityEngine;
using UnityEngine.UI;

public class IconController : MonoBehaviour {

    /* Kontroler/Obiekt kamery */
    CameraController camController;

	/* Metoda inicjacji, ustawienie komponentu kamery, ustawienie napisu na ikonie */
	public void Init (CameraController cam)
    {
        camController = cam;
		Text Tag = this.GetComponentInChildren<Text> ();
		if (Tag != null) 
		{
			if (this.name.Contains ("UndefinedDoors"))
				Tag.text = "";
			else
				Tag.text = this.name;
		}
	}
	
	/* Utrzymywanie rotacji ikony względem kamery */
	void Update ()
    {
        float eulerZ = this.transform.eulerAngles.z;
        eulerZ = Mathf.LerpAngle(eulerZ, (camController != null && camController.FollowNavigator) ? camController.transform.eulerAngles.z : Vector3.zero.z, Time.deltaTime * 5);
        this.transform.eulerAngles = new Vector3(0, 0, eulerZ);
    }
}
