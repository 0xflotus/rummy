﻿using UnityEngine;
using UnityEngine.UI;
using romme.Utility;

namespace romme.UI.Options
{

    public class ApplyMinSum : MonoBehaviour
    {
        public Text CurrentSumText;
        public InputField NewMinSumInput;

        private void Start()
        {            
            CurrentSumText.text = Tb.I.GameMaster.MinimumLaySum.ToString();
        }

        public void ApplyNewMinSum()
        {            
            int newMinSum = 0;
            int.TryParse(NewMinSumInput.text, out newMinSum);
            if(newMinSum < 0)
                return;

            Tb.I.GameMaster.MinimumLaySum = newMinSum;
            CurrentSumText.text = newMinSum.ToString();
        }
        
    }

}
