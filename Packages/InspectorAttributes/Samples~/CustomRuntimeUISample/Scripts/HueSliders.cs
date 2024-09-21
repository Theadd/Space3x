//Create three Sliders ( Create>UI>Slider)
//These are for manipulating the hue, saturation and value levels of the Color.

//Attach this script to a GameObject. Make sure it has a Renderer component.
//Click on the GameObject and attach each of the Sliders and Texts to the fields in the Inspector.

using UnityEngine;
using UnityEngine.UI;

public class HueSliders : MonoBehaviour
{
    float m_Hue;
    float m_Saturation;
    float m_Value;
    //These are the Sliders that control the values. Remember to attach them in the Inspector window.
    public Slider m_SliderHue, m_SliderSaturation, m_SliderValue;

    //Make sure your GameObject has a Renderer component in the Inspector window
    Renderer m_Renderer;

    void Start()
    {
        //Fetch the Renderer component from the GameObject with this script attached
        m_Renderer = GetComponent<Renderer>();

        //Set the maximum and minimum values for the Sliders
        m_SliderHue.maxValue = 1;
        m_SliderSaturation.maxValue = 1;
        m_SliderValue.maxValue = 1;

        m_SliderHue.minValue = 0;
        m_SliderSaturation.minValue = 0;
        m_SliderValue.minValue = 0;
    }

    void Update()
    {
        //These are the Sliders that determine the amount of the hue, saturation and value in the Color
        m_Hue = m_SliderHue.value;
        m_Saturation = m_SliderSaturation.value;
        m_Value = m_SliderValue.value;

        //Create an RGB color from the HSV values from the Sliders
        //Change the Color of your GameObject to the new Color
        m_Renderer.material.color = Color.HSVToRGB(m_Hue, m_Saturation, m_Value);
    }
}
