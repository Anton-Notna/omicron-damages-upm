using UnityEngine;

namespace OmicronDamages
{
    public struct DamagePoint
    {
        public Ray Direction;
        public Ray Hit;

        public Quaternion HitOrientation
        {
            get
            {
                Vector3 up = Hit.direction == Vector3.up ? Vector3.forward : Vector3.up;
                return Quaternion.LookRotation(Hit.direction, up);
            }
        }

        public Quaternion DirectionOrientation
        {
            get
            {
                Vector3 up = Direction.direction == Vector3.up ? Vector3.forward : Vector3.up;
                return Quaternion.LookRotation(Direction.direction, up);
            }
        }
    }
}