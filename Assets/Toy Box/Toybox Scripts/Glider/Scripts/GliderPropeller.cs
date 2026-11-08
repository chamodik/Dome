/*
 * Copyright (c) 2023 TORISOUP
 * Released under the MIT license
 * https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace TORISOUP.Gliders
{
    public class GliderPropeller : UdonSharpBehaviour
    {
        [Header("推進力の強さ")] [Range(0, 10.0f)] [SerializeField]
        private float _propellerPower = 1f;
        
        [Header("最大高さ制限")] [Space(20)] public float _limitHeight = 100;
        [Header("高さ制限を可視化する（UnityEditor時）")] public bool _drawGizmo = false;

        [Header("放置時に初期位置に戻す")] [SerializeField] [Space(20)]
        private bool _enableAutoReset = true;

        [Header("初期位置に戻るまでの待ち時間")] [SerializeField]
        private float _autoResetSeconds = 30f;

        [SerializeField] [Space(20)] private Transform _propeller;
        [SerializeField] private VRC_Pickup _vrcPickup;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private bool _isUsing;
        private bool _isGrab;
        private bool _isUsingRemote;
        private VRCPlayerApi _player;
        private float _maxY;
        private bool _isGrounded;
        private float _idleSeconds = 0;
        private float _currentRotateSpeed = 0;

        private void Start()
        {
            _startPosition = transform.localPosition;
            _startRotation = transform.localRotation;
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                _player = player;
            }
        }

        public override void OnPickup()
        {
            _isGrab = true;
            _isUsingRemote = false;
            _idleSeconds = 0;
            _maxY = _player.GetPosition().y;
        }

        public override void OnDrop()
        {
            _isGrab = false;
            _isUsing = false;
            _isUsingRemote = false;
            _idleSeconds = 0;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Rpc_RemoteUnUse));
        }

        public override void OnPickupUseDown()
        {
            _isUsing = true;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Rpc_RemoteUse));
        }

        public override void OnPickupUseUp()
        {
            _isUsing = false;
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Rpc_RemoteUnUse));
        }

        public override void OnOwnershipTransferred(VRCPlayerApi player)
        {
            _isUsing = false;
            _isUsingRemote = false;
        }

        private void Update()
        {
            if (_player == null) return;
            if (_enableAutoReset && Networking.IsOwner(gameObject) && !_isGrab)
            {
                if (_idleSeconds < _autoResetSeconds)
                {
                    _idleSeconds += Time.deltaTime;
                    if (_idleSeconds >= _autoResetSeconds)
                    {
                        transform.localPosition = _startPosition;
                        transform.localRotation = _startRotation;
                    }
                }
            }

            Animation();
        }

        private void Animation()
        {
            if (_propeller == null) return;
            if (!_vrcPickup.IsHeld) return;

            var targetSpeed = 0f;
            if (_isUsing || _isUsingRemote)
            {
                targetSpeed = 2500;
            }
            else
            {
                targetSpeed = 750;
            }

            _currentRotateSpeed =
                Mathf.Lerp(_currentRotateSpeed, targetSpeed, 10 * Time.deltaTime);

            _propeller.rotation =
                Quaternion.AngleAxis(_currentRotateSpeed * Time.deltaTime, _propeller.up) *
                _propeller.rotation;
        }

        private void Initialize()
        {
            if (_player == null) return;

            var v = _player.GetVelocity().magnitude;
            // (mv^2)/2 = mgh より現在の速度から達せられる最高地点を計算する
            _maxY = v * v / (Physics.gravity.magnitude * 2) + _player.GetPosition().y;
        }

        void FixedUpdate()
        {
            if (_player == null) return;
            if (!_isGrab) return;

            // 初期化
            if (!_isGrounded && _player.IsPlayerGrounded())
            {
                Initialize();
            }

            _isGrounded = _player.IsPlayerGrounded();
            
            if (_isGrounded && !_isUsing)
            {
                _maxY = _player.GetPosition().y;
                return;
            }

            var currentVel = _player.GetVelocity();

            if (_isUsing)
            {
                if (_maxY < _limitHeight)
                {
                    _maxY += (_propellerPower * 2.5f) * Time.deltaTime;
                }
            }
            
            // 落下分
            var deltaY = (_maxY) - _player.GetPosition().y;
            if (deltaY <= 0)
            {
                _maxY = _player.GetPosition().y;
                return;
            }

            // 位置エネルギーの落下分が運動エネルギーに変化した
            var vel = transform.forward.normalized *
                      Mathf.Sqrt(2 * Physics.gravity.magnitude * deltaY);

            var targetVel = Vector3.Lerp(currentVel, vel, 10f * Time.deltaTime);

            _player.SetVelocity(targetVel);
        }

        public void Rpc_RemoteUse()
        {
            _isUsingRemote = true;
        }

        public void Rpc_RemoteUnUse()
        {
            _isUsingRemote = false;
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!_drawGizmo) return;
            this.UpdateProxy(ProxySerializationPolicy.RootOnly);
            var orig = transform.position;
            var d = new Vector3(orig.x, _limitHeight, orig.z);
            Gizmos.color = new Color(0.8f, 0.8f, 1, 0.85f);
            Gizmos.DrawCube(d, new Vector3(200, 0.01f, 200));
        }
#endif
    }
}