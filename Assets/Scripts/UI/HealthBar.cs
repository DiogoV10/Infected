using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    //public Slider slider;
    public Image image;
    public TMP_Text text;

    public void SetHealth(float health)
    {
        image.fillAmount = health / 100;
        text.text = Mathf.Ceil(health).ToString();
    }
}
