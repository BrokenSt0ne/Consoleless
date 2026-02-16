using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Consoleless
{
    [BepInPlugin("BrokenStone.Consoleless", "Consoleless", "1.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly System.Random random = new System.Random();
        public void Awake()
        {
            // random id shit, so mods cant patch it out via harmony id
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string id = new string(Enumerable.Repeat(chars, 16)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var harmony = new Harmony(id);
            harmony.PatchAll();
            Debug.Log("[Consoleless] Initialized");
        }
    }
    
    public class Constants
    {
        public static List<string> BlockedUrls = new List<string>()
        {
            "https://iidk.online/",
            "https://raw.githubusercontent.com/iiDk-the-actual/Console",
            "https://hamburbur.org/data",
            "https://hamburbur.org/telemetry",
            "https://data.hamburbur.org",
            "https://files.hamburbur.org"
        };
    }

    [HarmonyPatch(typeof(UnityWebRequest), nameof(UnityWebRequest.SendWebRequest))]
    public class UnityWebRequestPatch
    {
        [HarmonyPrefix]
        static bool Prefix(UnityWebRequest __instance)
        {
            if (Constants.BlockedUrls.Any(blocked => __instance.url.StartsWith(blocked)))
            {
                Debug.Log($"[Consoleless] Blocked {__instance.url}");
                __instance.url = null;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(HttpClient), nameof(HttpClient.GetByteArrayAsync), new[] { typeof(string) })]
    public class HttpClientPatch
    {
        [HarmonyPrefix]
        static bool Prefix(string requestUri, ref Task<byte[]> __result)
        {
            if (Constants.BlockedUrls.Any(blocked => requestUri.StartsWith(blocked)))
            {
                Debug.Log($"[Consoleless] Blocked {requestUri}");
                __result = Task.FromResult(new byte[0]);
                return false;
            }
            return true;
        }
    }
}

