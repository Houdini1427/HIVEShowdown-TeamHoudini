using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

using Photon.Pun;
using Photon.Realtime;

using TMPro;

namespace Com.MyCompany.MyGame
{
    public class GameManager : MonoBehaviourPunCallbacks
    {

        #region Public Fields

        public static GameManager Instance;

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;

        public GameObject reportPanel;

        [SerializeField]
        public TMP_Text playerHealthText;
        [SerializeField]
        public TMP_Text rewardText;
        [SerializeField]
        public Slider playerHealthSlider;
        [SerializeField]
        public TMP_Text healthStakeTxt;
        [SerializeField]
        public TMP_Text totalRewardTxt;
        [SerializeField]
        public TMP_Text stakeRewardTxt;
        [SerializeField]
        public GameObject successImage;

        private float _tickImgTime = 0.0f;
        private const float TickImageTimeout = 10.0f;
        private bool tickTriggered = false;

        private float _playerTime = 0.5f;
        private bool playerStarted = false;

        private float currentReward;
        private float currentHealth;
        private float currentStakeDamageReward;
        const string playerNamePrefKey = "PlayerName";
        const string stakeAmtPrefKey = "StakeAmt";

        #endregion

        #region Public Methods

        public void UpdateGamePanel(float Health, float CurrentReward, float CurrentStakeDamageReward)
        {
            playerHealthText.text = (int)(Health*100) + "%";
            rewardText.text = CurrentReward + " HIVE";
            playerHealthSlider.value = Health;
            currentReward = CurrentReward;
            currentHealth = Health;
            currentStakeDamageReward = CurrentStakeDamageReward;
        }

        public void ReturnReward()
        {   
            Debug.Log("Total Reward: "+currentReward);
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                string username = PlayerPrefs.GetString(playerNamePrefKey);
                StartCoroutine(Hive.Instance.PostData(username, "Returning Reward", (float)Math.Round(currentReward, 3)));
            }
            
        }

        public void ReturnCallbackSuccess()
        {
            Debug.Log("Reached Success Callback");
            successImage.SetActive(true);
            _tickImgTime = TickImageTimeout;
            tickTriggered = true;
        }

        public void Update()
        {
            if (_playerTime >= 0)
            {
                _playerTime -= Time.deltaTime;
            }
            else
            {
                if (!playerStarted)
                {
                    playerStarted = true;
                    PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                }
            }
            if (_tickImgTime >= 0)
            {
                _tickImgTime -= Time.deltaTime;
            }
            else
            {
                if (tickTriggered)
                {
                    tickTriggered = false;
                    SceneManager.LoadScene(0);
                }
            }
        }

        public void ReturnCallbackFail()
        {
            Debug.Log("Reached Fail Callback");
        }

        public void SetupReport(float stakeDamage, float stakeHealth)
        {
            successImage.SetActive(false);
            reportPanel.SetActive(true);
            healthStakeTxt.text = "Stake Remaining: "+(float)Math.Round(stakeHealth, 3);
            totalRewardTxt.text = "Total Reward: "+(float)Math.Round(stakeHealth+stakeDamage, 3);
            stakeRewardTxt.text = "Stake Damage Rewarded: "+(float)Math.Round(stakeDamage, 3);
        }

        public void LeaveRoom(float stakeDamage, float stakeHealth)
        {
            SetupReport(stakeDamage, stakeHealth);
            // Leave Photon Room
            PhotonNetwork.LeaveRoom();

        }

        public void LeaveButtonCall()
        {
            float _stake;
            if (PlayerPrefs.HasKey(stakeAmtPrefKey))
            {
                _stake = Utilities.StringToFloat(PlayerPrefs.GetString(stakeAmtPrefKey));
            }
            else{
                return;
            }

            LeaveRoom(currentStakeDamageReward, currentHealth*_stake);
        }

        #endregion

        #region Private Methods

        void LoadArena()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
                return;
            }
            Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
            // PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
            PhotonNetwork.LoadLevel("Room for 1");
        }

        #endregion

        #region Photon Callbacks

        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting

            // if (PhotonNetwork.IsMasterClient)
            // {
            //     Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            //     LoadArena();
            // }
        }

        public override void OnPlayerLeftRoom(Player other)
        {
            Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

            // if (PhotonNetwork.IsMasterClient)
            // {
            //     Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            //     LoadArena();
            // }
        }

        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            // SceneManager.LoadScene(0);
        }

        void Start()
        {
            Instance = this;
            // LoadArena();
            reportPanel.SetActive(false);
            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
            }
            else
            {
                // Debug.LogFormat("We are Instantiating LocalPlayer from {0}", Application.loadedLevelName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                if (PlayerManager.LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                    // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                    // PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }
        }

        #endregion
    }
}
