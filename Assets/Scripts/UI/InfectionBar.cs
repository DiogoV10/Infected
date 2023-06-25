using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InfectionBar : MonoBehaviour
{
    public Image image;
    public TMP_Text text;

    public void SetInfection(float infection)
    {
        image.fillAmount = infection / 100;
        text.text = Mathf.Ceil(infection).ToString();
    }
}
