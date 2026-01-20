using Idenguefy.Controllers;
using UnityEngine;


namespace Idenguefy.Developer
{
    public class Developer : MonoBehaviour
    {
        /// <summary>
        /// Dev method that deletes all saved pointers
        /// </summary>
        public void TestDeleteAllPointers() //Add a dev class along with trigger alert
        {
            //ok
            PlayerPrefs.DeleteKey("Idenguefy_Pointers");
        }

        /// <summary>
        /// Dev method that triggers manual evaluation of alerts
        /// </summary>
        public void AlertTest()
        {
            AlertManager.Instance.EvaluateAlerts();
            Debug.LogWarning("Alert Test Triggered");
        }
    }
}
