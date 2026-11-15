using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AstroBotAdvisor : MonoBehaviour
{
    public Slider lifeSupportSlider;
    public Slider shieldingSlider;
    public Slider scienceSlider;
    public NasaLiveFeed nasaFeed;
    public TMP_Text astroBotText;

    private float timer = 0f;

    void Start()
    {
        if (lifeSupportSlider != null) lifeSupportSlider.onValueChanged.AddListener(_ => UpdateAdvice());
        if (shieldingSlider != null) shieldingSlider.onValueChanged.AddListener(_ => UpdateAdvice());
        if (scienceSlider != null) scienceSlider.onValueChanged.AddListener(_ => UpdateAdvice());
        UpdateAdvice();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 3f) { timer = 0f; UpdateAdvice(); }
    }

    public void UpdateAdvice()
    {
        if (astroBotText == null || lifeSupportSlider == null || shieldingSlider == null || scienceSlider == null) return;

        float life = lifeSupportSlider.value;
        float shield = shieldingSlider.value;
        float science = scienceSlider.value;
        bool flare = nasaFeed != null && nasaFeed.realSolarFlareActive;

        if (flare && shield < 0.45f)
        {
            astroBotText.color = new Color(1f, 0.35f, 0.25f);
            astroBotText.text = "<b>[ASTRO-BOT ALERT]</b> Live NASA DONKI Solar Flare! Boost <b>Shielding</b> > 45% to protect crew.";
        }
        else if (life < 0.25f)
        {
            astroBotText.color = new Color(1f, 0.75f, 0.2f);
            astroBotText.text = "<b>[ASTRO-BOT]</b> Habitat O2 dropping! Keep <b>Life Support</b> at least 25%.";
        }
        else if (science >= 0.45f && life >= 0.25f && shield >= 0.25f)
        {
            astroBotText.color = new Color(0.2f, 0.95f, 0.85f);
            astroBotText.text = "<b>[ASTRO-BOT]</b> Optimal power split! Perseverance MOXIE & spectrometers at peak discovery.";
        }
        else
        {
            astroBotText.color = Color.white;
            astroBotText.text = "<b>[ASTRO-BOT]</b> Mars air is 95% CO2. Balance power across O2, Magnetic Shield & Science!";
        }
    }
}