using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EffectBar : MonoBehaviour
{
    public TextMeshProUGUI ValueText;
    public Image ValueBar;

    float value = 0, maxValue = GameMaster.effectBarValueMax;
    float lerpSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ValueText.text = value + "%";

        lerpSpeed = 10f * Time.deltaTime;
        BarFiller();
        ColorChanger();
    }

    void BarFiller()
    {
        ValueBar.fillAmount = Mathf.Lerp(ValueBar.fillAmount, value / maxValue, lerpSpeed);
    }

    void ColorChanger()
    {
        ValueBar.color = Color.Lerp(Color.yellow, Color.red, (value / maxValue));
    }

    public void AddValue(int v)
    {
        value += v;
        value = Mathf.Clamp(value, 0, maxValue);
    }

    public void SetValue(int v)
    {
        value = v;
        value = Mathf.Clamp(value, 0, maxValue);
    }

}
