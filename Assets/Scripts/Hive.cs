using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

namespace Com.MyCompany.MyGame
{
    public class Hive : MonoBehaviour
    {

#if UNITY_WEBGL && !UNITY_EDITOR

        [DllImport("__Internal")]
        private static extern void Connect(string username);


        [DllImport("__Internal")]
        private static extern void SignTx(string username, string id, string data, string prompt);
    
        
        [DllImport("__Internal")]
        private static extern void Transfer(string username, string to, string amount, string memo);

#endif

        #region Public Fields

        public static Hive Instance;

        private const string contractAccountUsername = "user1427";

        // URL of the server where you want to post the data
        public string serverURL = "http://localhost:3000/";

        // Data to be posted to the server
        public string recipient = "user1427";
        public string comments = "Returning reward";
        public float quantity = 0.001f;

        #endregion

        void Start()
        {
            Instance = this;
        }

        public void ConnectWallet(string username)
        {
            #if !UNITY_EDITOR && UNITY_WEBGL
            
            if (username == "")
            {
                //create webgl alert
                Application.ExternalEval("alert('Username is empty, please enter your hive username & use hive keychain to sign in')");
            }
            else
            {
                //if not empty, connect to hive keychain
                Debug.Log("Username Sent: " + username);
                Connect(username);
            }
            #endif

        }

        public void Transfer(string username, float amount, string description)
        {
            #if !UNITY_EDITOR && UNITY_WEBGL
            Transfer(username, contractAccountUsername, Math.Round(amount, 3).ToString(), description);
            #endif
        }

        // public void Test()
        // {
        //     #if !UNITY_EDITOR && UNITY_WEBGL
        //     // json {"username":"test","id":"1","data":"test","prompt":"test"}
        //     string json = "{\"username\":\"test\",\"id\":\"1\",\"data\":\"test\",\"prompt\":\"test\"}";
        //     SignTx(username, "test", json ,"Signing a test message");
        //     #endif
        // }

        public void Return(string _recipient, float _quantity)
        {
            recipient = _recipient;
            quantity = (float)Math.Round(_quantity, 3);
            Debug.Log("Reached Hive: "+recipient+" - "+quantity);
            // StartCoroutine(PostData());
        }

        private static void ReturnSuccess()
        {
            Debug.Log("Success for transfer");
        }

        private static void ReturnFail()
        {
            Debug.Log("Fail for transfer");
        }


        public IEnumerator PostData(string recipient, string comments, float quantity)
        {
            Debug.Log("Sending back reward");
            
            // To be Commented;
            // quantity = 0.001f;
            // 
            Debug.Log(recipient);
            Debug.Log(comments);
            Debug.Log(quantity);
            
            NewBehaviourScript obj = new NewBehaviourScript();
            obj.recipient = recipient;
            obj.comments = comments;
            obj.quantity = quantity;


            // Convert the dictionary to a JSON string
            string jsonData = JsonUtility.ToJson(obj);
            Debug.Log(jsonData);
            
            // Create a new UnityWebRequest
            // UnityWebRequest request = UnityWebRequest.Post("http://localhost:3000", jsonData, "application/json");
            UnityWebRequest request = UnityWebRequest.Post("https://e971-124-66-173-224.ngrok-free.app", jsonData, "application/json");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
            
            // Send the request and wait for a response
            yield return request.SendWebRequest();

            // Check for errors
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error posting data: " + request.error);
                GameManager.Instance.ReturnCallbackFail();
            }
            else
            {
                Debug.Log("Data posted successfully!");
                GameManager.Instance.ReturnCallbackSuccess();

            }
        }


    }
}