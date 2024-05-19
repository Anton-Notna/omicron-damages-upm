using System;
using UnityEngine;

namespace OmicronDamages
{
    [Serializable]
    public class TransformDamageSource : IDamageSource
    {
        [SerializeField]
        private Transform _transform;
        [SerializeField]
        private Vector3 _localDirection = Vector3.forward;

        public TransformDamageSource(Transform transform, Vector3 localDirection)
        {
            _transform = transform;
            _localDirection = localDirection;
        }

        public Ray Orientation => new Ray(_transform.position, _transform.TransformDirection(_localDirection));
    }
}