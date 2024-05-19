using System;
using System.Collections.Generic;
using UnityEngine;

namespace OmicronDamages
{
    public abstract class SphereScanDamager<TData> : MonoBehaviour
    {
        [Serializable]
        private struct Point
        {
            [SerializeField]
            public float Radius;
            [SerializeField]
            public Vector3 LocalOffset;
        }

        [SerializeField]
        private int _hitsPerPointLimit = 10;
        [SerializeField]
        private LayerMask _layerMask;
        [SerializeField]
        private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField]
        private Point[] _points;
        [Space]
        [SerializeField]
        private bool _gizmos;
        [SerializeField]
        private Color _gizmosColor = Color.red;

        private readonly HashSet<IDamageable<TData>> _damaged = new HashSet<IDamageable<TData>>();
        private Collider[] _hits;
        private float _activeTime = float.MinValue;
        private IDamageSource _source;
        private TData _data;

        private Vector3 _previousPosition;
        private Quaternion _previousRotation;

        private bool Active => Time.time <= _activeTime;

        public void Activate(IDamageSource source, TData data, float activeDuration)
        {
            if (_hits == null || _hits.Length != _hitsPerPointLimit)
                _hits = new Collider[_hitsPerPointLimit];

            _damaged.Clear();
            CollectTransformData();
            _activeTime = activeDuration + Time.time;
            _source = source;
            _data = data;
        }

        public void Deactivate()
        {
            _activeTime = float.MinValue;
        }

        private void CollectTransformData()
        {
            _previousPosition = transform.position;
            _previousRotation = transform.rotation;
        }

        private void Update()
        {
            if (Active == false)
                return;

            Cast();
            CollectTransformData();
        }

        private bool Cast()
        {
            bool collision = false;
            for (int i = 0; i < _points.Length; i++)
            {
                var point = _points[i];
                Vector3 previous = (_previousRotation * point.LocalOffset) + _previousPosition;
                Vector3 current = transform.TransformPoint(point.LocalOffset);

                int count = Physics.OverlapCapsuleNonAlloc(previous, current, point.Radius, _hits, _layerMask, _triggerInteraction);
                if (_gizmos)
                    Debug.DrawLine(previous, current, _gizmosColor, 0.3f);
              
                if (count > 0)
                    collision = true;
                else
                    continue;

                for (int c = 0; c < count; c++)
                    Process(_hits[c], previous);
            }

            return collision;
        }

        private void Process(Collider collider, Vector3 point)
        {
            if (collider == null)
                return;

            if (collider.TryGetComponent(out IDamageable<TData> damageable) == false)
                return;

            if (_damaged.Contains(damageable))
                return;

            Vector3 closest = collider.ClosestPoint(point);
            Vector3 normal = (point - closest).normalized;

            DamagePoint damage = new DamagePoint()
            {
                Direction = _source.Orientation,
                Hit = new Ray(closest, normal),
            };

            damageable.TakeDamage(damage, _data);
            _damaged.Add(damageable);
        }

        private void OnDrawGizmos()
        {
            if (_gizmos == false)
                return;

            if (_points == null)
                return;

            Color color = _gizmosColor;
            if (Active == false)
            {
                color *= 0.7f;
                color.a = 0.3f;
            }

            Gizmos.color = color;

            for (int i = 0; i < _points.Length; i++)
                Gizmos.DrawWireSphere(transform.TransformPoint(_points[i].LocalOffset), _points[i].Radius);
        }
    }
}