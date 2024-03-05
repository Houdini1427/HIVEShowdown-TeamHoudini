using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

namespace Com.MyCompany.MyGame
{
    
    public class Control : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text functionTxt;

        [SerializeField]
        private TMP_Text keyTxt;

        [SerializeField]
        private string function;
        [SerializeField]
        public string keys;

        // Start is called before the first frame update
        void Start()
        {
            functionTxt.text = function;
            keyTxt.text = keys;
        }
    }
}
