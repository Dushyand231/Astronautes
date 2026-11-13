using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OutpostManager : MonoBehaviour
{
    [Header("Mission State")]
    public int currentSol = 1;
    public int maxSols = 7;
    public bool isCommanderMode = false; // False = Cadet (7-10 yrs), True = Commander (11-14 yrs)

    [Header("Outpost Resources (0 to 100)")]
    public float oxygenAndFood = 80f;
    public float crewHealth = 100f;
    public float radiationDose = 10f; // High = Danger! (Max 100 = Fail)
    public int scienceScore = 0;

    [Header("Power Allocation Sliders (Total = 100%)")]
    public Slider lifeSupportSlider;
    public Slider shieldingSlider;
    public Slider scienceSlider;

    [Header("3D Visual Feedback Objects")]
    public GameObject shieldDomeVisual;
    public Light outpostMainLight;
    public ParticleSystem dustStormEffect;

    [Header("UI Text References (Member 3 Link Pannuvanga)")]
    public TMP_Text solText;
    public TMP_Text statusText;
    public TMP_Text eventLogText;
    public TMP_Text powerSplitText;

    private bool isUpdatingSliders = false;
    private string currentCrisis = "Normal"; // Normal, SolarFlare, DustStorm

    void Start()
    {
        // Initial 100% split: 40% Life Support, 30% Shield, 30% Science
        lifeSupportSlider.value = 40f;
        shieldingSlider.value = 30f;
        scienceSlider.value = 30f;

        lifeSupportSlider.onValueChanged.AddListener(delegate { BalanceSliders(lifeSupportSlider); });
        shieldingSlider.onValueChanged.AddListener(delegate { BalanceSliders(shieldingSlider); });
        scienceSlider.onValueChanged.AddListener(delegate { BalanceSliders(scienceSlider); });

        TriggerSolEvent();
        UpdateVisualsAndUI();
    }

    // ZERO-SUM TRADE-OFF LOGIC: Oru slider yethuna matha rendu koraiyum (Sum always 100)
    void BalanceSliders(Slider changedSlider)
    {
        if (isUpdatingSliders) return;
        isUpdatingSliders = true;

        float remaining = 100f - changedSlider.value;

        if (changedSlider == lifeSupportSlider)
        {
            float otherSum = shieldingSlider.value + scienceSlider.value;
            if (otherSum > 0)
            {
                shieldingSlider.value = remaining * (shieldingSlider.value / otherSum);
                scienceSlider.value = remaining * (scienceSlider.value / otherSum);
            }
            else
            {
                shieldingSlider.value = remaining / 2f;
                scienceSlider.value = remaining / 2f;
            }
        }
        else if (changedSlider == shieldingSlider)
        {
            float otherSum = lifeSupportSlider.value + scienceSlider.value;
            if (otherSum > 0)
            {
                lifeSupportSlider.value = remaining * (lifeSupportSlider.value / otherSum);
                scienceSlider.value = remaining * (scienceSlider.value / otherSum);
            }
            else
            {
                lifeSupportSlider.value = remaining / 2f;
                scienceSlider.value = remaining / 2f;
            }
        }
        else if (changedSlider == scienceSlider)
        {
            float otherSum = lifeSupportSlider.value + shieldingSlider.value;
            if (otherSum > 0)
            {
                lifeSupportSlider.value = remaining * (lifeSupportSlider.value / otherSum);
                shieldingSlider.value = remaining * (shieldingSlider.value / otherSum);
            }
            else
            {
                lifeSupportSlider.value = remaining / 2f;
                shieldingSlider.value = remaining / 2f;
            }
        }

        isUpdatingSliders = false;
        UpdateVisualsAndUI();
    }

    // "NEXT SOL" BUTTON CLICK PANNUMBODHU NADAKKURA SIMULATION
    public void AdvanceToNextSol()
    {
        if (currentSol > maxSols) return;

        float lifePower = lifeSupportSlider.value;
        float shieldPower = shieldingSlider.value;
        float sciencePower = scienceSlider.value;

        // Dust Storm irundha total power efficiency 30% drop aagum!
        float efficiency = (currentCrisis == "DustStorm") ? 0.7f : 1.0f;
        lifePower *= efficiency;
        shieldPower *= efficiency;
        sciencePower *= efficiency;

        // 1. Life Support Trade-Off Calculation
        if (lifePower >= 35f)
            oxygenAndFood = Mathf.Clamp(oxygenAndFood + (lifePower - 35f) * 0.5f, 0f, 100f);
        else
            oxygenAndFood = Mathf.Clamp(oxygenAndFood - (35f - lifePower) * 0.8f, 0f, 100f);

        // 2. Radiation Shielding Trade-Off Calculation (NASA DONKI Storm impact)
        float incomingRadiation = (currentCrisis == "SolarFlare") ? 45f : 15f;
        if (isCommanderMode) incomingRadiation += 10f;

        float netRadiation = incomingRadiation - (shieldPower * 0.6f);
        radiationDose = Mathf.Clamp(radiationDose + netRadiation, 0f, 100f);

        // 3. Science Output
        scienceScore += Mathf.RoundToInt(sciencePower * 2f);

        // 4. Crew Health Penalty if O2 low or Radiation high
        if (oxygenAndFood < 25f) crewHealth -= 25f;
        if (radiationDose > 60f) crewHealth -= 20f;
        crewHealth = Mathf.Clamp(crewHealth, 0f, 100f);

        // Check Win/Lose
        if (crewHealth <= 0f || oxygenAndFood <= 0f || radiationDose >= 100f)
        {
            eventLogText.text = "MISSION FAILED! Outpost life support or radiation limits breached. Try a better power balance!";
            return;
        }

        if (currentSol == maxSols)
        {
            eventLogText.text = $"MISSION ACCOMPLISHED! You survived 7 Sols on Mars! Final Science Score: {scienceScore}";
            return;
        }

        currentSol++;
        TriggerSolEvent();
        UpdateVisualsAndUI();
    }

    void TriggerSolEvent()
    {
        // Grounded in NASA DONKI & Mars Weather scenarios
        if (currentSol == 2 || currentSol == 5)
        {
            currentCrisis = "SolarFlare";
            eventLogText.text = "[NASA DONKI ALERT] M-Class Solar Flare detected! Radiation surging — boost Shielding Power, or risk crew health!";
        }
        else if (currentSol == 4 || currentSol == 6)
        {
            currentCrisis = "DustStorm";
            eventLogText.text = "[MARS WEATHER ALERT] Regional Dust Storm! Solar power dropped by 30%. Prioritize Life Support!";
        }
        else
        {
            currentCrisis = "Normal";
            eventLogText.text = $"[SOL {currentSol}] Clear Martian skies (-60°C). Good window to boost Science & Rover scans!";
        }
    }

    void UpdateVisualsAndUI()
    {
        // 3D Visual Feedback
        if (shieldDomeVisual != null)
            shieldDomeVisual.SetActive(shieldingSlider.value >= 35f); // Shield 35% mela irundha 3D dome theriyum!

        if (outpostMainLight != null)
            outpostMainLight.intensity = (lifeSupportSlider.value >= 30f) ? 1.5f : 0.4f;

        if (dustStormEffect != null)
        {
            if (currentCrisis == "DustStorm" && !dustStormEffect.isPlaying) dustStormEffect.Play();
            else if (currentCrisis != "DustStorm" && dustStormEffect.isPlaying) dustStormEffect.Stop();
        }

        // UI Update
        if (solText != null) solText.text = $"SOL {currentSol} / {maxSols}";
        if (statusText != null)
            statusText.text = $"Oxygen/Food: {oxygenAndFood:F0}%  |  Crew Health: {crewHealth:F0}%  |  Radiation: {radiationDose:F0} mSv  |  Science: {scienceScore}";
        if (powerSplitText != null)
            powerSplitText.text = $"Life Support: {lifeSupportSlider.value:F0}%  |  Shield: {shieldingSlider.value:F0}%  |  Science: {scienceSlider.value:F0}%";
    }
}