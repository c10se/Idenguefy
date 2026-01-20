using Idenguefy.Entities;
using System.Collections.Generic;
using UnityEngine;

namespace Idenguefy.Controllers
{
    /// <summary>
    /// Handles fetching and preprocessing of preventive health tips
    /// 
    /// Responsibilities:
    /// <list type="number">
    ///     <item><description> Takes in a list of strings for helping to prevent Dengue </description></item>
    ///     <item><description> Convert the strings to Health Tip objects </description></item>
    ///     <item><description> Store these into a list of Health Tip object called Tips </description></item>
    /// Provides a public Tips list accessible by PreventiveHealthTipView
    /// </list>
    /// </summary>
    /// <remarks>
    /// Author: Sharina, Xavier
    /// Version: 1.1
    /// Notes: N/A
    /// </remarks>
    public class TipController
    {
        /// <summary>
        /// Gets the list of preventive health tips for indoor environments.
        /// </summary>
        public List<PreventiveHealthTip> IndoorTips { get; private set; }

        /// <summary>
        /// Gets the list of preventive health tips for outdoor environments.
        /// </summary>
        public List<PreventiveHealthTip> OutdoorTips { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TipController
        /// and populates the indoor and outdoor tip lists with hardcoded data.
        /// </summary>
        public TipController()
        {
            //Simple hardcoded data – no PlayerPrefs or JSON
            IndoorTips = new List<PreventiveHealthTip>
            {
                new PreventiveHealthTip("Remove stagnant water around your home every 2-3 days"),
                new PreventiveHealthTip("Ensure flower pot plates are dry or filled with sand"),
                new PreventiveHealthTip("Keep toilets and sinks clean and dry"),
                new PreventiveHealthTip("Install window and door screens to keep mosquitoes out")
            };

            OutdoorTips = new List<PreventiveHealthTip>
            {
                new PreventiveHealthTip("Apply mosquito repellent before going outside"),
                new PreventiveHealthTip("Wear long-sleeved shirts and pants when outdoors"),
                new PreventiveHealthTip("Avoid outdoor activities at dawn and dusk"),
                new PreventiveHealthTip("Clear stagnant water in outdoor containers and drains")
            };
        }
    }

}