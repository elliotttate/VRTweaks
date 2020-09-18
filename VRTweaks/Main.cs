﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using QModManager.API.ModLoading;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.XR;

namespace VRTweaks
{
    [QModCore]
    public static class Loader
    {
        public static VRTweaks Instance { get; internal set; }

        [QModPatch]
        public static void Initialize()
        {
            File.AppendAllText("VRTweaksLog.txt", "Initializing" + Environment.NewLine);

            Harmony harmony = new Harmony("VRTweaks");
            harmony.PatchAll();

            SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(OnSceneLoaded);

            File.AppendAllText("VRTweaksLog.txt", "Done Initializing" + Environment.NewLine);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            File.AppendAllText("VRTweaksLog.txt", "Scene Loaded " + scene.name + Environment.NewLine);

            if (scene.name == "StartScreen")
            {
                new VRTweaks();
            }
        }
    }

    public class VRTweaks : MonoBehaviour
    {
        private static VRTweaks s_instance;

        public VRTweaks()
        {
            if (Loader.Instance == null)
            {
                Loader.Instance = FindObjectOfType(typeof(VRTweaks)) as VRTweaks;

                if (Loader.Instance == null)
                {
                    GameObject vrTweaks = new GameObject("VRTweaks");
                    Loader.Instance = vrTweaks.AddComponent<VRTweaks>();
                }
            }
            else
            {
                Loader.Instance.Awake();
            }

            DontDestroyOnLoad(gameObject);
        }

        private void Awake()
        {
            File.AppendAllText("VRTweaksLog.txt", "Mono Behaviour Started" + Environment.NewLine);
            SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(OnSceneLoaded);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Recenter();
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                RemoveComponents();
            }
        }

        public static void Recenter()
        {
            if (XRSettings.loadedDeviceName == "Oculus")
            {
                File.AppendAllText("VRTweaksLog.txt", "Recentering Oculus" + Environment.NewLine);
                InputTracking.Recenter();
            }
            if (XRSettings.loadedDeviceName == "OpenVR")
            {
                File.AppendAllText("VRTweaksLog.txt", "Recentering OpenVR" + Environment.NewLine);
                Valve.VR.OpenVR.System.ResetSeatedZeroPose();
                Valve.VR.OpenVR.Compositor.SetTrackingSpace(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated);
            }
        }

        public static void RemoveComponents()
        {
            foreach (WBOIT w in FindObjectsOfType(typeof(WBOIT)) as WBOIT[])
            {
                w.enabled = false;
            }

            foreach (PlayerMask m in FindObjectsOfType(typeof(PlayerMask)) as PlayerMask[])
            {
                m.enabled = false;
                m.gameObject.SetActive(false);

                foreach(MeshFilter f in m.GetAllComponentsInChildren<MeshFilter>())
                {
                    f.mesh = null;
                }
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Recenter();
            RemoveComponents();
        }
    }
}
