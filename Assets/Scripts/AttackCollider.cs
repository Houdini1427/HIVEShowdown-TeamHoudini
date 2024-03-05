using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class AttackCollider : MonoBehaviour
    {
        #region Private Fields
        
        [SerializeField]
        private PlayerManager target;

        public float HitDamage;

        #endregion

        // Start is called before the first frame update
        void Start()
        {
            HitDamage = target.Power;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void OnTriggerEnter(Collider other)
        {
            PlayerManager otherPlayer = other.gameObject.GetComponent<PlayerManager>();
            if (otherPlayer == null || otherPlayer == target) return;
            otherPlayer.TakeDmg(HitDamage);
            target.AddStakeDmg(otherPlayer.Stake*HitDamage, otherPlayer.photonView.Owner.NickName);
        }

        public void SetTarget(PlayerManager _target)
        {
            if (_target == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> PlayMakerManager target for PlayerUI.SetTarget.", this);
                return;
            }
            target = _target;
        }
    }

}
