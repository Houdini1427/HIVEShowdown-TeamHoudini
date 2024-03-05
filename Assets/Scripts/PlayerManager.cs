using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

using Photon.Pun;

namespace Com.MyCompany.MyGame
{
    /// <summary>
    /// Player manager.
    /// Handles fire Input and Beams.
    /// </summary>
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region Private Fields

        [SerializeField]
        private PlayerInput _input;

        private Animator animator;

        bool IsFiring;
        public float Stake;
        public float Power = 0.1f;
        const string stakeAmtPrefKey = "StakeAmt";

        private float TotalStakeDmg = 0.0f;

        // public Dictionary<string, float> stakeOwnerMap = new Dictionary<string, float>();
        #endregion

        #region Public Fields
        
        [Tooltip("The current Health of our player")]
        public float Health = 1f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        [Tooltip("The Player's UI GameObject Prefab")]
        [SerializeField]
        public GameObject PlayerUiPrefab;

        #endregion

        #region Private Methods

        #if UNITY_5_4_OR_NEWER
        void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }
        #endif

        void SetPower(){
            if (Stake == null){
                return;
            }
            Power = Utilities.GetPower(Stake);
            Debug.Log("Power set for the player: " + Power.ToString(), this);
        }

        #endregion

        #region IPunObservable implementation

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(IsFiring);
                stream.SendNext(Health);
            }
            else
            {
                // Network player, receive data
                this.IsFiring = (bool)stream.ReceiveNext();
                this.Health = (float)stream.ReceiveNext();
            }
        }

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            if (PlayerPrefs.HasKey(stakeAmtPrefKey)){
                // Stake = (float)Math.Round(float.Parse(PlayerPrefs.GetString(stakeAmtPrefKey)), 3);
                this.Stake = Utilities.StringToFloat(PlayerPrefs.GetString(stakeAmtPrefKey));
                SetPower();
                Debug.Log("Stake set for the player: " + Stake.ToString(), this);
            }
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during initialization phase.
        /// </summary>
        void Start()
        {
            animator = this.gameObject.GetComponent<Animator>();
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

            if (_cameraWork != null)
            {
                if (photonView.IsMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }

            if (PlayerUiPrefab != null)
            {
                GameObject _uiGo =  Instantiate(PlayerUiPrefab);
                _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
            }
            else
            {
                Debug.LogWarning("<Color=Red><a>Missing</a></Color> PlayerUiPrefab reference on player Prefab.", this);
            }
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            if (photonView.IsMine)
            {
                ProcessInputs();
                if (Health <= 0f)
                {
                    animator.SetTrigger("Death");
                    GameManager.Instance.LeaveRoom(TotalStakeDmg, Stake*Health);
                }
                GameManager.Instance.UpdateGamePanel(Health, GetCurrentReward(), TotalStakeDmg);
            }
        }

        void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 5f, 0f);
            }
            GameObject _uiGo = Instantiate(this.PlayerUiPrefab);
            _uiGo.SendMessage("SetTarget", this, SendMessageOptions.RequireReceiver);
        }

        #endregion

        #region Custom

        public void AddStakeDmg(float stakeDmg, string stakeOwner)
        {
            if (!photonView.IsMine) return;
            TotalStakeDmg += stakeDmg;
            Debug.Log(stakeDmg + " stake damage to " + stakeOwner);
        }

        public void TakeDmg(float _damage)
        {
            if (!photonView.IsMine) return;
            Debug.Log("Took Damage: " + _damage);
            animator.SetTrigger("Hit");
            if (Health > 0)
            {
                Health -= _damage;
                if (Health < 0) Health = 0;
            }
            else Health = 0;
        }

        public float GetCurrentReward()
        {
            return Health*Stake + TotalStakeDmg;
        }

        void ProcessInputs()
        {
            // if (_input)
            // {
            //     if (!IsFiring)
            //     {
            //         IsFiring = true;
            //     }
            // }
            // if (Input.GetButtonUp("Fire1"))
            // {
            //     if (IsFiring)
            //     {
            //         IsFiring = false;
            //     }
            // }
        }

        #endregion
    }
}
