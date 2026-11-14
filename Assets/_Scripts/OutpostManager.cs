using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public struct NasaMissionEvent
{
    public string title;
    public string crisisType; // "Normal", "SolarFlare", "DustStorm", "ScienceWindow"
    public string nasaSource;
    [TextArea] public string kidFriendlyBrief;
}

public class OutpostManager : MonoBehaviour
{
    [Header("Mission State")]
    public int currentSol = 1;
    public int maxSols = 7;
    public bool isCommanderMode = false;

    [Header("Outpost Resources (0 to 100)")]
    public float oxygenAndFood = 80f;
    public float crewHealth = 100f;
    public float radiationDose = 10f;
    public int scienceScore = 0;

    [Header("Power Allocation Sliders (Total = 100%)")]
    public Slider lifeSupportSlider;
    public Slider shieldingSlider;
    public Slider scienceSlider;
    public Button nextSolButton;
    public Button replayButton;
    public TMP_Text modeButtonText;

    [Header("3D Visual Feedback Objects")]
    public GameObject shieldDomeVisual;
    public Light outpostMainLight;
    public ParticleSystem dustStormEffect;

    

    [Header("UI Text References")]
    public TMP_Text solText;
    public TMP_Text statusText;
    public TMP_Text eventLogText;
    public TMP_Text powerSplitText;
    public TMP_Text dataAttributionText;

    private bool isUpdatingSliders = false;
    private bool isTransmitting = false;
    private NasaMissionEvent activeEvent;
    private List<NasaMissionEvent> nasaEventPool;

    void Start()
    {
        InitNasaDataset();

        if (replayButton != null)
            replayButton.gameObject.SetActive(false);

        if (modeButtonText != null)
            modeButtonText.text = isCommanderMode ? "MODE: COMMANDER (11-14y)" : "MODE: CADET (7-10y)";

        lifeSupportSlider.value = 40f;
        shieldingSlider.value = 30f;
        scienceSlider.value = 30f;

        lifeSupportSlider.onValueChanged.AddListener(delegate { BalanceSliders(lifeSupportSlider); });
        shieldingSlider.onValueChanged.AddListener(delegate { BalanceSliders(shieldingSlider); });
        scienceSlider.onValueChanged.AddListener(delegate { BalanceSliders(scienceSlider); });

        TriggerSolEvent();
        UpdateVisualsAndUI();
    }

    void InitNasaDataset()
    {
        nasaEventPool = new List<NasaMissionEvent>
        {
            new NasaMissionEvent {
                title = "Clear Martian Skies (-60°C)",
                crisisType = "Normal",
                nasaSource = "NASA Curiosity REMS Weather Dataset",
                kidFriendlyBrief = "Calm winds at Gale Crater! Great time to power up Science & Rover scans, but keep Oxygen above 35%."
            },
            new NasaMissionEvent {
                title = "M-Class Solar Flare Detected!",
                crisisType = "SolarFlare",
                nasaSource = "NASA DONKI Space Weather DB (FLR-M5.4)",
                kidFriendlyBrief = "High-energy solar radiation is hitting Mars! Boost Shielding Power (45%+) or crew radiation risk spikes (NASA OSDR Data)."
            },
            new NasaMissionEvent {
                title = "Regional Dust Storm Approaching",
                crisisType = "DustStorm",
                nasaSource = "NASA Mars Reconnaissance Orbiter (MRO)",
                kidFriendlyBrief = "Thick red dust blocks sunlight — Solar Power drops by 30%! Cut Science power to keep Life Support alive."
            },
            new NasaMissionEvent {
                title = "Subsurface Ice Sample Signal",
                crisisType = "ScienceWindow",
                nasaSource = "NASA Perseverance SHERLOC Data",
                kidFriendlyBrief = "Sensors found water-ice traces nearby! Allocate 40%+ to Science for double research points, at the cost of tonight's shield."
            }
        };
    }

    void BalanceSliders(Slider changedSlider)
    {
        if (isUpdatingSliders) return;
        isUpdatingSliders = true;

        float remaining = 100f - changedSlider.value;

        if (changedSlider == lifeSupportSlider)
        {
            float otherSum = shieldingSlider.value + scienceSlider.value;
            shieldingSlider.value = (otherSum > 0) ? remaining * (shieldingSlider.value / otherSum) : remaining / 2f;
            scienceSlider.value = (otherSum > 0) ? remaining * (scienceSlider.value / otherSum) : remaining / 2f;
        }
        else if (changedSlider == shieldingSlider)
        {
            float otherSum = lifeSupportSlider.value + scienceSlider.value;
            lifeSupportSlider.value = (otherSum > 0) ? remaining * (lifeSupportSlider.value / otherSum) : remaining / 2f;
            scienceSlider.value = (otherSum > 0) ? remaining * (scienceSlider.value / otherSum) : remaining / 2f;
        }
        else if (changedSlider == scienceSlider)
        {
            float otherSum = lifeSupportSlider.value + shieldingSlider.value;
            lifeSupportSlider.value = (otherSum > 0) ? remaining * (lifeSupportSlider.value / otherSum) : remaining / 2f;
            shieldingSlider.value = (otherSum > 0) ? remaining * (shieldingSlider.value / otherSum) : remaining / 2f;
        }

        isUpdatingSliders = false;
        UpdateVisualsAndUI();
    }

    public void AdvanceToNextSol()
    {
        if (currentSol > maxSols || isTransmitting) return;
        StartCoroutine(SimulateSolWithCommDelay());
    }

    IEnumerator SimulateSolWithCommDelay()
    {
        isTransmitting = true;
        if (nextSolButton != null) nextSolButton.interactable = false;

        eventLogText.text = "[COMM DELAY: 14 MIN MARS-EARTH SIGNAL] Transmitting power commands to outpost...";
        yield return new WaitForSeconds(2.0f);

        float efficiency = (activeEvent.crisisType == "DustStorm") ? 0.7f : 1.0f;
        float lifePower = lifeSupportSlider.value * efficiency;
        float shieldPower = shieldingSlider.value * efficiency;
        float sciencePower = scienceSlider.value * efficiency;

        // 1. Life Support Trade-Off
        if (lifePower >= 35f)
            oxygenAndFood = Mathf.Clamp(oxygenAndFood + (lifePower - 35f) * 0.5f, 0f, 100f);
        else
            oxygenAndFood = Mathf.Clamp(oxygenAndFood - (35f - lifePower) * 0.8f, 0f, 100f);

        // 2. Radiation Trade-Off
        float incomingRad = (activeEvent.crisisType == "SolarFlare") ? 45f : 15f;
        if (isCommanderMode) incomingRad += 10f;
        radiationDose = Mathf.Clamp(radiationDose + (incomingRad - shieldPower * 0.6f), 0f, 100f);

        // 3. Science Trade-Off
        int multiplier = (activeEvent.crisisType == "ScienceWindow") ? 4 : 2;
        scienceScore += Mathf.RoundToInt(sciencePower * multiplier);

        // 4. Crew Health Check
        if (oxygenAndFood < 25f) crewHealth -= 25f;
        if (radiationDose > 60f) crewHealth -= 20f;
        crewHealth = Mathf.Clamp(crewHealth, 0f, 100f);

        isTransmitting = false;
        if (nextSolButton != null) nextSolButton.interactable = true;

        UpdateVisualsAndUI();

        if (crewHealth <= 0f || oxygenAndFood <= 0f || radiationDose >= 100f)
        {
            eventLogText.text = "<b>MISSION FAILED!</b> Outpost breached life support or radiation limits (NASA OSDR Threshold). Click Replay to try again!";
            if (nextSolButton != null) nextSolButton.interactable = false;
            if (replayButton != null) replayButton.gameObject.SetActive(true);
            yield break;
        }

        if (currentSol == maxSols)
        {
            string rankBadge = scienceScore >= 700 ? "OUTPOST COMMANDER" : (scienceScore >= 450 ? "MARS SPECIALIST" : "JUNIOR CADET");
            eventLogText.text = $"<b>MISSION ACCOMPLISHED!</b> 7 Sols Survived on Mars!\nFinal Science Score: {scienceScore} | Rank Earned: <b>{rankBadge}</b>";
            if (nextSolButton != null) nextSolButton.interactable = false;
            if (replayButton != null) replayButton.gameObject.SetActive(true);
            yield break;
        }

        currentSol++;
        TriggerSolEvent();
        UpdateVisualsAndUI();
    }

    void TriggerSolEvent()
    {
        if (currentSol == 1)
            activeEvent = nasaEventPool[0];
        else
            activeEvent = nasaEventPool[Random.Range(0, nasaEventPool.Count)];

        eventLogText.text = $"<b>[SOL {currentSol}] {activeEvent.title}</b>\n{activeEvent.kidFriendlyBrief}";
        if (dataAttributionText != null)
            dataAttributionText.text = $"Grounded in: {activeEvent.nasaSource}";
    }

    public void CycleDifficultyMode()
    {
        isCommanderMode = !isCommanderMode;
        if (modeButtonText != null)
            modeButtonText.text = isCommanderMode ? "MODE: COMMANDER (11-14y)" : "MODE: CADET (7-10y)";
        UpdateVisualsAndUI();
    }

    public void RestartMission()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateVisualsAndUI()
    {
        if (shieldDomeVisual != null)
            shieldDomeVisual.SetActive(shieldingSlider.value >= 35f);

        if (outpostMainLight != null)
            outpostMainLight.intensity = (lifeSupportSlider.value >= 30f) ? 1.5f : 0.4f;

        if (dustStormEffect != null)
        {
            if (activeEvent.crisisType == "DustStorm" && !dustStormEffect.isPlaying) dustStormEffect.Play();
            else if (activeEvent.crisisType != "DustStorm" && dustStormEffect.isPlaying) dustStormEffect.Stop();
        }

        string modeLabel = isCommanderMode ? "COMMANDER" : "CADET";
        if (solText != null) solText.text = $"SOL {currentSol}/{maxSols} [{modeLabel}]";
        if (statusText != null)
            statusText.text = $"O2/Food: {oxygenAndFood:F0}% | Health: {crewHealth:F0}% | Radiation: {radiationDose:F0} mSv | Science: {scienceScore}";
        if (powerSplitText != null)
            powerSplitText.text = $"Life Support: {lifeSupportSlider.value:F0}% | Shielding: {shieldingSlider.value:F0}% | Science: {scienceSlider.value:F0}%";
    }
}