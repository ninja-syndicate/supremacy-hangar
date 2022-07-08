using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SliderValueToText : MonoBehaviour
{
    [SerializeField] private Slider sliderUI;
    [SerializeField] private TextMeshProUGUI textSliderValue;
    void Start()
    {
        textSliderValue = GetComponent<TextMeshProUGUI>();
        ShowSliderValue();
    }

    //Assign text field to show the value a the slider
    public void ShowSliderValue()
    {

        //Output for whole number
        if (sliderUI.wholeNumbers)
            textSliderValue.text = "(" + sliderUI.value + ")";
        else //Output for floating number (decimal point)
            textSliderValue.text = "(" + sliderUI.value.ToString("F1") + ")";
    }
}