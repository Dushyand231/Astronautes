using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class NasaLiveFeed : MonoBehaviour
{
    [Header("NASA Open API Configuration")]
    public string nasaApiKey = "DEMO_KEY"; // Unnoda NASA API Key inga paste pannu

    [Header("UI Display (Optional)")]
    public TMP_Text liveTelemetryText; // EventLogText or separate TelemetryText link pannalam

    [Header("Live Status Flags (Used by OutpostManager)")]
    public bool realSolarFlareActive = false;
    public string latestFlareClass = "None";
    public string latestRoverSol = "4102";

    void Start()
    {
        StartCoroutine(FetchLiveNasaData());
    }

    IEnumerator FetchLiveNasaData()
    {
        if (liveTelemetryText != null)
            liveTelemetryText.text = "Connecting to NASA Deep Space Network (DONKI & Mars Rover API)...";

        // 1. Fetch Real Space Weather (NASA DONKI Solar Flares - last 7 days)
        string startDate = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-dd");
        string donkiUrl = $"https://api.nasa.gov/DONKI/FLR?startDate={startDate}&api_key={nasaApiKey}";

        using (UnityWebRequest donkiReq = UnityWebRequest.Get(donkiUrl))
        {
            yield return donkiReq.SendWebRequest();

            if (donkiReq.result == UnityWebRequest.Result.Success)
            {
                string json = donkiReq.downloadHandler.text;
                if (json.Contains("classType"))
                {
                    realSolarFlareActive = true;
                    int idx = json.IndexOf("classType") + 12;
                    latestFlareClass = json.Substring(idx, Mathf.Min(4, json.Length - idx)).Replace("\"", "").Replace(",", "");
                }
            }
        }

        // 2. Fetch Real Curiosity Rover Manifest (Gale Crater - NASA Open APIs)
        string roverUrl = $"https://api.nasa.gov/mars-photos/api/v1/manifests/curiosity?api_key={nasaApiKey}";

        using (UnityWebRequest roverReq = UnityWebRequest.Get(roverUrl))
        {
            yield return roverReq.SendWebRequest();

            if (roverReq.result == UnityWebRequest.Result.Success)
            {
                string json = roverReq.downloadHandler.text;
                if (json.Contains("max_sol"))
                {
                    int idx = json.IndexOf("max_sol") + 9;
                    latestRoverSol = json.Substring(idx, Mathf.Min(5, json.Length - idx)).Replace(",", "").Replace("\"", "").Trim();
                }
            }
        }

        // 3. Update Live UI Banner (with clean offline fallback if rate-limited/offline)
        if (liveTelemetryText != null)
        {
            string flareMsg = realSolarFlareActive
                ? $"LIVE NASA DONKI ALERT: Solar Flare Class {latestFlareClass} detected! Raise Shielding > 45%."
                : "LIVE NASA DONKI: Solar activity nominal. Curiosity Rover active at Gale Crater (Sol " + latestRoverSol + ").";

            liveTelemetryText.text = flareMsg;
        }
    }
}