/*
 * Copyright (c) 2020 TORISOUP
 * Released under the MIT license
 * https://opensource.org/licenses/mit-license.php
*/


using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace TORISOUP
{
    public class Glider : UdonSharpBehaviour
    {
        [Header("放置時に初期位置に戻す")] [SerializeField]
        private bool _enableAutoReset = true;

        [Header("初期位置に戻るまでの待ち時間")] [SerializeField]
        private float _autoResetSeconds = 30f;


        [SerializeField] 
        [Header("俊敏性")]
        [Space(20)] 
        private float _quickness = 10;


        private float _glindStartedY;
        private Vector3 _glindStartedVelocity;
        private Vector3 _startPosition;
        private Quaternion _startRotation;
        private bool _isUsing;
        private bool _isGrab;
        private float _idleSeconds;
        private VRCPlayerApi _player;

        private bool _isGrounded;

        private void Start()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }

        private void Update()
        {
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
        }

        public override void OnPickup()
        {
            _player = Networking.LocalPlayer;
            _isGrab = true;
            _idleSeconds = 0;
        }

        public override void OnDrop()
        {
            _isGrab = false;
            _isUsing = false;
            _idleSeconds = 0;
        }

        public override void OnPickupUseDown()
        {
            _isUsing = true;
        }

        public override void OnPickupUseUp()
        {
            _isUsing = false;
        }

        private void Initialize()
        {
            if (_player == null) return;
            var v = _player.GetVelocity().magnitude;
            _glindStartedY = v * v / (Physics.gravity.magnitude * 2) + _player.GetPosition().y;
        }

        /// <summary>
        /// 現在の状態で滑空開始する
        /// </summary>
        public void StartGlide()
        {
            if (!_isGrab) return;
            Initialize();
        }

        void FixedUpdate()
        {
            if (_player == null) return;
            if (!_isGrab) return;

            // 離陸したら自動初期化
            if (_isGrounded && !_player.IsPlayerGrounded())
            {
                StartGlide();
            }

            _isGrounded = _player.IsPlayerGrounded();

            // 地面についたら終わり
            if (_isGrounded) return;

            // ひっくり返しても落下
            if (Vector3.Dot(transform.up, Vector3.down) > 0.5f)
            {
                return;
            }

            var currentVel = _player.GetVelocity();


            if (_isUsing)
            {
                // 速度が早いほどブレーキが効く
                var mag = currentVel.magnitude;
                var dec = Mathf.Lerp(0.5f, 30f, (Mathf.Clamp(mag, 4, 50) - 4) / 46f);

                // ブレーキをかけると全体の力学的エネルギーが減る
                _glindStartedY = _glindStartedY - dec * Time.fixedDeltaTime;
            }

            // 落下分
            var deltaY = _glindStartedY - _player.GetPosition().y;
            if (deltaY <= 0)
            {
                _glindStartedY = _player.GetPosition().y;
                return;
            }
            
            // 位置エネルギーの落下分が運動エネルギーに変化した
            var vel = transform.forward.normalized *
                      Mathf.Sqrt(2 * Physics.gravity.magnitude * deltaY);


            // _quicknessを小さくすると操作性がふわふわになる
            // 大きくすると機敏に動く
            // 1～10 の範囲がよい
            var targetVel = Vector3.Lerp(currentVel, vel, _quickness * Time.fixedDeltaTime);

            _player.SetVelocity(targetVel);
        }
    }
}