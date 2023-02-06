// Project:         LivelyCities for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Cliffworms
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Hazelnut

using UnityEngine;
using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Weather;

namespace LivelyCities
{
    public class LivelyCitiesMod : MonoBehaviour
    {
        public const byte npcFlagHideDay        = 1;    // 0b_0000_0001
        public const byte npcFlagHideNight      = 2;    // 0b_0000_0010
        public const byte npcFlagHideWeather    = 4;    // 0b_0000_0100

        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<LivelyCitiesMod>();
        }

        void Awake()
        {
            InitMod();
            mod.IsReady = true;
        }

        public static void InitMod()
        {
            Debug.Log("Begin mod init: LivelyCities");

            PlayerGPS.OnEnterLocationRect += UpdateExteriorNPCs_OnEnterLocationRect;
            WeatherManager.OnWeatherChange += UpdateExteriorNPCs_OnWeatherChange;
            WorldTime.OnDawn += UpdateExteriorNPCs;
            WorldTime.OnDusk += UpdateExteriorNPCs;

            Debug.Log("Finished mod init: LivelyCities");
        }

        static void UpdateExteriorNPCs_OnEnterLocationRect(DFLocation location)
        {
            UpdateExteriorNPCs();
        }

        static void UpdateExteriorNPCs_OnWeatherChange(WeatherType weather)
        {
            UpdateExteriorNPCs();
        }

        static void UpdateExteriorNPCs()
        {
            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;

            bool day = DaggerfallUnity.Instance.WorldTime.Now.IsDay;
            bool raining = GameManager.Instance.WeatherManager.IsRaining;

            if (playerGPS.IsPlayerInTown(false, true))
            {
                StaticNPC staticNpc = null;
                Billboard[] dfBillboards = playerEnterExit.ExteriorParent.GetComponentsInChildren<Billboard>(true);
                foreach (Billboard billboard in dfBillboards)
                {
                    staticNpc = billboard.GetComponent<StaticNPC>();
                    if (staticNpc != null && staticNpc.Data.factionID != 0)
                    {
                        // Show/Hide depending on NPC flag data.

                        bool hidingFromRain = (staticNpc.Data.flags & npcFlagHideWeather) != 0 && raining;

                        if ((staticNpc.Data.flags & npcFlagHideDay) != 0)
                        {
                            billboard.gameObject.SetActive(!day && !hidingFromRain);
                        }
                        else if ((staticNpc.Data.flags & npcFlagHideNight) != 0)
                        {
                            billboard.gameObject.SetActive(day && !hidingFromRain);
                        }
                        else
                        {
                            billboard.gameObject.SetActive(!hidingFromRain);
                        }

                        //Debug.LogFormat("Updated NPC sprite {0} {1} flags: {2}  enabled: {3}", billboard.Summary.Archive, billboard.Summary.Record, staticNpc.Data.flags, billboard.gameObject.activeInHierarchy);
                    }
                }
            }
        }
    }
}