using System;
using System.Collections.Generic;
using UnityEngine;

namespace OmicronDamages
{
    public abstract class SphereScanDamager<TData> : MonoBehaviour
    {
        public struct CastReport
        {
            public int SuccessDamages;
            public int FailDamages;
            public int NonDamageableCollisions;

            public CastReport Add(CastReport castReport)
            {
                return new CastReport()
                {
                    SuccessDamages = this.SuccessDamages + castReport.SuccessDamages,
                    FailDamages = this.FailDamages + castReport.FailDamages,
                    NonDamageableCollisions = this.NonDamageableCollisions + castReport.NonDamageableCollisions,
                };
            }
        }

        public delegate void DamagedCallback(IDamageable<TData> damageable, Collider damageableCollider, DamagePoint point, TData data);

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

        private readonly HashSet<Collider> _damaged = new HashSet<Collider>();
        private Collider[] _hits;
        private float _activeTime = float.MinValue;
        private IDamageSource _source;
        private TData _data;

        private Vector3 _previousPosition;
        private Quaternion _previousRotation;

        public CastReport Report { get; private set; }

        public event DamagedCallback Damaged = delegate { };

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
            Report = new CastReport();
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

            Report = Report.Add(Cast());
            CollectTransformData();
        }

        private CastReport Cast()
        {
            CastReport report = new CastReport();

            for (int i = 0; i < _points.Length; i++)
            {
                var point = _points[i];
                Vector3 previous = (_previousRotation * point.LocalOffset) + _previousPosition;
                Vector3 current = transform.TransformPoint(point.LocalOffset);

                int count = Physics.OverlapCapsuleNonAlloc(previous, current, point.Radius, _hits, _layerMask, _triggerInteraction);
                if (_gizmos)
                    Debug.DrawLine(previous, current, _gizmosColor, 0.3f);
              
                if (count == 0)
                    continue;

                for (int c = 0; c < count; c++)
                    report = report.Add(Process(_hits[c], previous));
            }

            return report;
        }

        private CastReport Process(Collider collider, Vector3 point)
        {
            CastReport report = new CastReport();

            if (collider == null)
                return report;

            if (_damaged.Contains(collider))
                return report;

            _damaged.Add(collider);

            if (collider.TryGetComponent(out IDamageable<TData> damageable) == false)
            {
                report.NonDamageableCollisions += 1;
                return report;
            }

            Vector3 closest = collider.ClosestPoint(point);
            Vector3 normal = (point - closest).normalized;

            DamagePoint damage = new DamagePoint()
            {
                Direction = _source.Orientation,
                Hit = new Ray(closest, normal),
            };

            if (damageable.TakeDamage(damage, _data))
                report.SuccessDamages += 1;
            else
                report.FailDamages += 1;

            Damaged.Invoke(damageable, collider, damage, _data);

            return report;
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