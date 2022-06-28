using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SliderValueToText : MonoBehaviour
{
    [SerializeField] private Slider sliderUI;
    [SerializeField] private TextMeshProUGUI textSliderValue;

    [SerializeField] private bool isAudio = false;
    void Start()
    {
        textSliderValue = GetComponent<TextMeshProUGUI>();
        ShowSliderValue();
    }

    //Assign text field to show the value a the slider
    public void ShowSliderValue()
    {
        float sliderValue = isAudio ? sliderUI.value * 100 : sliderUI.value;

        //Output for whole number
        if (sliderUI.wholeNumbers)
            textSliderValue.text = "(" + sliderUI.value + ")";
        else //Output for floating number (decimal point)
            textSliderValue.text = isAudio ? "(" + sliderValue.ToString("F0") + ")" : "(" + sliderValue.ToString("F1") + ")";
    }
}