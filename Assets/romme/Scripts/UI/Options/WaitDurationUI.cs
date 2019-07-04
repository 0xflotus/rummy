﻿using UnityEngine;
using UnityEngine.UI;
using romme.Utility;

namespace romme.UI
{
    [RequireComponent(typeof(Text))]
    public class WaitDurationUI : MonoBehaviour
    {
        private Text text;
        public bool isDrawWaitDuration = false;

        private void Start()
        {
            text = GetComponent<Text>();
        }

        private void Update()
        {
            text.text = isDrawWaitDuration ? Tb.I.GameMaster.DrawWaitDuration.ToString() : Tb.I.GameMaster.PlayWaitDuration.ToString();
        }
    }

}