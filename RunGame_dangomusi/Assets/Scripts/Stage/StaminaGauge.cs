﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaGauge : MonoBehaviour
{
    // Start is called before the first frame update
    public void Start()
    {

    }

    // Update is called once per frame
    public void Update()
    {

    }

    // Player側でスタミナが増減した時にUI側も減らす
    public void UpdateGauge(float stamina)
    {
        var slider = GetComponent<Slider>();
        slider.value = stamina;
    }

}
