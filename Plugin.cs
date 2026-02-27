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
    [BepInPlugin("BrokenStone.Consoleless", "Consoleless", "1.0.3")]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly System.Random random = new System.Random();
        public void Awake()
        {
            // random id shit, so mods cant patch it out via harmony id -xenon
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            string id = new string(Enumerable.Repeat(chars, random.Next(8, 513)) // made random-ish instead of 16 char
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
            "https://iidk.online/", // stops most telemetry stuff and server data
            "https://raw.githubusercontent.com/iiDk-the-actual/Console", // server data assets
            "https://hamburbur.org/data", // new data url for hamburbur intelligence agency
            "https://hamburbur.org/telemetry", // big stinky, no explanation needed
            "https://data.hamburbur.org", //legacy, but still keeping it
            "https://files.hamburbur.org", //legacy i think, but still keeping it
            "https://faggot.click", //malicous url used by emerald
            "https://sentinelhook.lol" //malicous url used by emerald, sends stuff to a webhook

            // this is why we cant have nice things
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

