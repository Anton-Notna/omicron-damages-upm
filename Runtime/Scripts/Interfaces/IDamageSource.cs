using UnityEngine;

namespace OmicronDamages
{
    public interface IDamageSource
    {
        public Ray Orientation { get; }
    }
}