using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MyCompany.MyGame
{
    public class BodyCollider : MonoBehaviour
    {
        #region Private fields

        [Tooltip("Hit Multiplier")]
        [SerializeField]
        private float HitMultiplier = 1.0f;

        [SerializeField]
        private PlayerManager target;

        #endregion
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void TakeHit(float _damage)
        {
            target.TakeDmg(_damage);
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
