using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AstroBotAdvisor : MonoBehaviour
{
    [Header("Link Existing Sliders")]
    public Slider lifeSupportSlider;
    public Slider shieldingSlider;
    public Slider scienceSlider;

    [Header("Link NASA Feed (Optional)")]
    public NasaLiveFeed nasaFeed;

    [Header("Astro-Bot UI Text")]
    public TMP_Text astroBotText;

    private float updateTimer = 0f;

    void Start()
    {
        if (lifeSupportSlider != null) lifeSupportSlider.onValueChanged.AddListener(_ => EvaluateMission());
        if (shieldingSlider != null) shieldingSlider.onValueChanged.AddListener(_ => EvaluateMission());
        if (scienceSlider != null) scienceSlider.onValueChanged.AddListener(_ => EvaluateMission());
        EvaluateMission();
    }

    void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= 3f)
        {
            updateTimer = 0f;
            EvaluateMission();
        }
    }

    public void EvaluateMission()
    {
        if (astroBotText == null || lifeSupportSlider == null || shieldingSlider == null || scienceSlider == null)
            return;

        float life = lifeSupportSlider.value;
        float shield = shieldingSlider.value;
        float science = scienceSlider.value;
        bool flareActive = nasaFeed != null && nasaFeed.realSolarFlareActive;

        // Priority 1: Live NASA Solar Flare Warning
        if (flareActive && shield < 0.45f)
        {
            astroBotText.color = new Color(1f, 0.35f, 0.25f); // Alert Coral Red
            astroBotText.text = "<b>[ASTRO-BOT ALERT]</b> Live NASA DONKI Solar Flare detected! Boost <b>Shielding</b> above 45% immediately to block cosmic radiation.";
        }
        // Priority 2: Low Oxygen / Life Support Warning
        else if (life < 0.25f)
        {
            astroBotText.color = new Color(1f, 0.75f, 0.2f); // Warning Amber
            astroBotText.text = "<b>[ASTRO-BOT]</b> Habitat O2 & thermal loop dropping! Keep <b>Life Support</b> at least 25% so astronauts stay safe.";
        }
        // Priority 3: High Science Discovery Praise
        else if (science >= 0.45f && life >= 0.25f && shield >= 0.25f)
        {
            astroBotText.color = new Color(0.2f, 0.95f, 0.85f); // Cyan Discovery
            astroBotText.text = "<b>[ASTRO-BOT]</b> Optimal Rover & Lab power! Perseverance MOXIE & spectrometers running at peak Mars discovery rate.";
        }
        // Default: Educational Mars Fact
        else
        {
            astroBotText.color = Color.white;
            astroBotText.text = "<b>[ASTRO-BOT CO-PILOT]</b> Mars atmosphere is 95% CO2 with thin radiation shielding. Balance power across O2, Magnetic Dome & Science!";
        }
    }
}