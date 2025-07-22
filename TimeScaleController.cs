using UnityEngine;
using UnityEngine.UI;

public class TimeScaleController : MonoBehaviour
{

    public float tScale;
    public GameObject slider;

    private Slider timeSlider;
    private Text displayText; // Optional: shows the value


    void Start()
    {
        return;

        timeSlider = slider.GetComponent<Slider>();
        Time.timeScale = tScale;

        if (timeSlider != null)
        {
            timeSlider.onValueChanged.AddListener(UpdateTimeScale);
            timeSlider.value = Time.timeScale;
        }
    }

    public void UpdateTimeScale(float value)
    {
        Time.timeScale = value;
        //if (displayText != null)
            //displayText.text = "Time Scale: " + value.ToString("F2");
    }
}