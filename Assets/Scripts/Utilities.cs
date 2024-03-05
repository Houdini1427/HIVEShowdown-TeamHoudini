using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Com.MyCompany.MyGame
{
    
    public class Utilities : MonoBehaviour
    {
        public static float GetPower(float Stake)
        {
            float Power = (float)(1/(1 + (float)Math.Exp(-Stake/10))) - 0.4f;
            Power = (float)Math.Round(Power, 3);
            return Power;
        }

        public static float StringToFloat(string Stake)
        {
            float stakeFloat = (float)Math.Round(float.Parse(Stake), 3);
            return stakeFloat;
        }
    }
}
