using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

using TMPro;


namespace Com.MyCompany.MyGame
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        #region Private Serializable Fields

        /// <summary>
        /// The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created.
        /// </summary>
        [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
        [SerializeField]
        private byte maxPlayersPerRoom = 4;

        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject controlPanel1;
        [Tooltip("The Ui Panel to let the user enter name, connect and play")]
        [SerializeField]
        private GameObject controlPanel2;
        [Tooltip("The UI Label to inform the user that the connection is in progress")]
        [SerializeField]
        private GameObject progressLabel;

        [SerializeField]
        private TMP_Text messageText;

        private float _messageTimeoutDelta;
        [SerializeField]
        private const float MessageTimeout = 2.0f;

        #endregion

        #region Private Fields

        /// <summary>
        /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
        /// </summary>
        string gameVersion = "1";

        /// <summary>
        /// Keep track of the current process. Since connection is asynchronous and is based on several callbacks from Photon,
        /// we need to keep track of this to properly adjust the behavior when we receive call back by Photon.
        /// Typically this is used for the OnConnectedToMaster() callback.
        /// </summary>
        bool isConnecting;
        const string playerNamePrefKey = "PlayerName";
        const string stakeAmtPrefKey = "StakeAmt";

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            // #Critical
            // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            // Connect();
            progressLabel.SetActive(false);
            controlPanel1.SetActive(true);
            controlPanel2.SetActive(false);
            messageText.enabled = false;
            _messageTimeoutDelta = 0;
        }

        void Update()
        {
            if (_messageTimeoutDelta >= 0)
            {
                _messageTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                messageText.enabled = false;
            }
        }

        #endregion


        #region Public Methods

        public void Login()
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                string username = PlayerPrefs.GetString(playerNamePrefKey);
                Hive.Instance.ConnectWallet(username);
            }
            
        }

        /// <summary>
        /// Start the connection process.
        /// - If already connected, we attempt joining a random room
        /// - if not yet connected, Connect this application instance to Photon Cloud Network
        /// </summary>
        public void Connect()
        {
   
            if (PlayerPrefs.HasKey(stakeAmtPrefKey))
            {
                string stakeAmt = PlayerPrefs.GetString(stakeAmtPrefKey);
                string username = PlayerPrefs.GetString(playerNamePrefKey);
                float stakeFloat = Utilities.StringToFloat(stakeAmt);
                if (stakeFloat < 0.5f)
                {
                    return;
                }
                Hive.Instance.Transfer(username, stakeFloat, "Transfering Stake to Contract Account");
            }
            else{
                return;
            }

            // progressLabel.SetActive(true);
            // controlPanel2.SetActive(false);
            // // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
            // if (PhotonNetwork.IsConnected)
            // {
            //     // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            //     PhotonNetwork.JoinRandomRoom();
            // }
            // else
            // {
            //     // #Critical, we must first and foremost connect to Photon Online Server.
            //     isConnecting = PhotonNetwork.ConnectUsingSettings();
            //     PhotonNetwork.GameVersion = gameVersion;
            // }
        }

        public void LoginCallback(string response)
        {
            Debug.Log("Reached Callback "+response);
            if (response == "not_installed")
            {
                _messageTimeoutDelta = MessageTimeout;
                messageText.text = "This game requires Hive Keychain to work. Kindly install Hive Keychain.";
                messageText.enabled = true;
                return;
            }
            if (response == "login_failed")
            {
                _messageTimeoutDelta = MessageTimeout;
                messageText.text = "Wrong Credentials. Please check your username with your Hive keychain Wallet.";
                messageText.enabled = true;
                return;
            }
            controlPanel1.SetActive(false);
            controlPanel2.SetActive(true);
        }

        public void ConnectCallback(string response)
        {
            // TODO: Remove this when deploying
            // response = "true";
            // This Statement is to be removed
            
            if (response == "true")
            {
                progressLabel.SetActive(true);
                controlPanel2.SetActive(false);
                // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
                if (PhotonNetwork.IsConnected)
                {
                    // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
                    PhotonNetwork.JoinRandomRoom();
                }
                else
                {
                    // #Critical, we must first and foremost connect to Photon Online Server.
                    isConnecting = PhotonNetwork.ConnectUsingSettings();
                    PhotonNetwork.GameVersion = gameVersion;
                }
            }
            else{
                _messageTimeoutDelta = MessageTimeout;
                messageText.text = response;
                messageText.enabled = true;
            }
            
        }

    #endregion

    #region MonoBehaviourPunCallbacks Callbacks

    public override void OnConnectedToMaster()
    {
        if (isConnecting){
            Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
            // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
            PhotonNetwork.JoinRandomRoom();
            isConnecting = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        progressLabel.SetActive(false);
        controlPanel2.SetActive(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            Debug.Log("We load the 'Room for 1' ");

            // #Critical
            // Load the Room Level.
            PhotonNetwork.LoadLevel("Room for 1");
        }
    
    }

    #endregion

    }
}
