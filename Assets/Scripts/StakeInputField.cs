using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


using TMPro;

namespace Com.MyCompany.MyGame
{
    // [RequireComponent(typeof(TMP_InputField))]
    public class StakeInputField : MonoBehaviour
    {
        #region Private Constants
        
        [Tooltip("Text Field")]
        [SerializeField]
        private TMP_Text powerText;

            // Store the PlayerPref Key to avoid typos
        const string stakeAmtPrefKey = "StakeAmt";

        #endregion

        #region MonoBehaviour CallBacks
        // Start is called before the first frame update
        void Start()
        {
            string defaultStake = string.Empty;
            TMP_InputField _inputField = this.GetComponent<TMP_InputField>();
            if (_inputField!=null)
            {
                if (PlayerPrefs.HasKey(stakeAmtPrefKey))
                {
                    defaultStake = PlayerPrefs.GetString(stakeAmtPrefKey);
                    _inputField.text = defaultStake;
                }
            }
        }

        #endregion

        public void SetStakeAmt(string value)
        {
            // #Important
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Stake Amount is null or empty");
                return;
            }

            PlayerPrefs.SetString(stakeAmtPrefKey, value);
            // float Stake = (float)Math.Round(float.Parse(value), 3);
            float Stake = Utilities.StringToFloat(value);
            if (Stake < 0.5f)
            {
                powerText.text = "Invalid: Stake needs to be more than 0.5 HIVE";
                return;
            }
            float Power = Utilities.GetPower(Stake);
            powerText.text = "Provided Power: " + Power.ToString();
        }
    }
}
