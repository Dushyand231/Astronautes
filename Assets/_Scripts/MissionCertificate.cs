using UnityEngine;
using TMPro;
using System;

public class MissionCertificate : MonoBehaviour
{
    [Header("Certificate UI References")]
    public GameObject certificatePanel;
    public TMP_InputField cadetNameInput;
    public TMP_Text certificateBodyText;
    public TMP_Text solText; // Link your existing SolText here

    private bool certificateTriggered = false;

    void Start()
    {
        if (certificatePanel != null)
            certificatePanel.SetActive(false);

        if (cadetNameInput != null)
            cadetNameInput.onValueChanged.AddListener(_ => RefreshCertificateText());
    }

    void Update()
    {
        // Sol 7/7 reach aanadhum auto-trigger aagum
        if (!certificateTriggered && solText != null && solText.text.Contains("7/7"))
        {
            certificateTriggered = true;
            ShowCertificate();
        }
    }

    public void ShowCertificate()
    {
        if (certificatePanel == null) return;
        certificatePanel.SetActive(true);
        RefreshCertificateText();
    }

    public void RefreshCertificateText()
    {
        if (certificateBodyText == null) return;

        string cadetName = (cadetNameInput != null && !string.IsNullOrWhiteSpace(cadetNameInput.text))
            ? cadetNameInput.text.Trim().ToUpper()
            : "JUNIOR ASTRONAUT";

        certificateBodyText.text =
            $"<b><size=22><color=#38BDF8>NASA SPACE APPS MISSION COMMAND</color></size></b>\n" +
            $"<size=14><color=#FBBF24>OFFICIAL JUNIOR ASTRONAUT CERTIFICATION</color></size>\n\n" +
            $"This certifies that\n" +
            $"<b><size=24><color=#FFFFFF>{cadetName}</color></size></b>\n\n" +
            $"has successfully commanded the Mars Outpost for 7 Sols, managed crew life support & solar flare radiation shielding, and completed NASA planetary telemetry analysis.\n\n" +
            $"<size=13><color=#94A3B8>Issued: {DateTime.Now:dd MMM yyyy}  |  Mission Status: ACCOMPLISHED</color></size>";
    }

    public void SaveCertificatePNG()
    {
        string fileName = $"NASA_Junior_Astronaut_Certificate_{DateTime.Now:HHmmss}.png";
        ScreenCapture.CaptureScreenshot(fileName);
        Debug.Log("Certificate PNG saved to project folder: " + fileName);
    }

    public void CloseCertificate()
    {
        if (certificatePanel != null)
            certificatePanel.SetActive(false);
    }
}