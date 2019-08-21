/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using UnityEngine;
using UnityEngine.UI;

namespace Vuforia
{
    /// <summary>
    /// A custom handler that implements the ITrackableEventHandler interface.
    /// </summary>
    public class DefaultTrackableEventHandler : MonoBehaviour,
                                                ITrackableEventHandler
    {
        #region PRIVATE_MEMBER_VARIABLES
 
        private TrackableBehaviour mTrackableBehaviour;

		private Dropdown[] dropdowns = null;
		private Button GoButton = null;
    
        #endregion // PRIVATE_MEMBER_VARIABLES



        #region UNTIY_MONOBEHAVIOUR_METHODS
    
        void Start()
        {
            
			mTrackableBehaviour = GetComponent<TrackableBehaviour>();
            if (mTrackableBehaviour)
            {
                mTrackableBehaviour.RegisterTrackableEventHandler(this);
            }

        }

        #endregion // UNTIY_MONOBEHAVIOUR_METHODS



        #region PUBLIC_METHODS

        /// <summary>
        /// Implementation of the ITrackableEventHandler function called when the
        /// tracking state changes.
        /// </summary>
        public void OnTrackableStateChanged(
                                        TrackableBehaviour.Status previousStatus,
                                        TrackableBehaviour.Status newStatus)
        {

			if (dropdowns == null) 
			{
				GameObject dropdownsParent = GameObject.Find ("Dropdowns");
				dropdowns = new Dropdown[dropdownsParent.transform.childCount];
				for (int i = 0; i < dropdowns.Length; i++) 
				{
					dropdowns [i] = dropdownsParent.transform.GetChild (i).GetComponent<Dropdown> ();
				}
			}

			if (GoButton == null)
				GoButton = GameObject.Find ("AcceptButton").GetComponent<Button> ();
			
            if (newStatus == TrackableBehaviour.Status.DETECTED ||
                newStatus == TrackableBehaviour.Status.TRACKED ||
                newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
            {
                OnTrackingFound();
            }
            else
            {
                OnTrackingLost();
            }
        }

		public void UnregisterThis()
		{
			mTrackableBehaviour = GetComponent<TrackableBehaviour>();
			if (mTrackableBehaviour)
			{
				mTrackableBehaviour.UnregisterTrackableEventHandler(this);
			}
		}

        #endregion // PUBLIC_METHODS



        #region PRIVATE_METHODS


        private void OnTrackingFound()
        {
			
			for (int i = 0; i < this.transform.childCount; i++) 
			{
				this.transform.GetChild (i).gameObject.SetActive (true);
			}

			if (mTrackableBehaviour.TrackableName.Contains("STRONG")) 
			{
				PlayerPrefs.SetString("USER_POSITION", mTrackableBehaviour.TrackableName);
				GameObject.FindObjectOfType<LoadNavigationModule> ().LoadNavigation (true);
				return;
			}

			for (int i = 0; i < dropdowns.Length; i++) 
			{
				if (i >= mTrackableBehaviour.TrackableName.Length)
					break;
				dropdowns[i].value = dropdowns[i].options.FindIndex(x => x.text == mTrackableBehaviour.TrackableName [i].ToString());
			}

			Animation cameraAnim = GameObject.Find ("Camera").GetComponent<Animation>();
			if (cameraAnim.clip.name == "DetectionLostCamera") 
			{
				cameraAnim.clip = cameraAnim.GetClip ("DetectionFoundCamera");
				cameraAnim.Play ();
			}
			Animation detectionCanvasAnim = GameObject.Find ("DetectionCanvas").GetComponent<Animation> ();
			if (detectionCanvasAnim.clip.name == "DetectionLostCanvas") 
			{
				detectionCanvasAnim.clip = detectionCanvasAnim.GetClip ("DetectionFoundCanvas");
				detectionCanvasAnim.Play ();
			}

        }


        private void OnTrackingLost()
        {

			Animation cameraAnim = GameObject.Find ("Camera").GetComponent<Animation>();
			if (cameraAnim.clip.name == "DetectionFoundCamera") 
			{
				cameraAnim.clip = cameraAnim.GetClip ("DetectionLostCamera");
				cameraAnim.Play ();
			}
			Animation detectionCanvasAnim = GameObject.Find ("DetectionCanvas").GetComponent<Animation> ();
			if (detectionCanvasAnim.clip.name == "DetectionFoundCanvas") 
			{
				detectionCanvasAnim.clip = detectionCanvasAnim.GetClip ("DetectionLostCanvas");
				detectionCanvasAnim.Play ();
			}

			for (int i = 0; i < this.transform.childCount; i++)
			{
				this.transform.GetChild (i).gameObject.SetActive (false);
			}

			for (int i = 0; i < dropdowns.Length; i++) 
			{
				dropdowns [i].value = 0;
			}

        }

        #endregion // PRIVATE_METHODS
    }
}
