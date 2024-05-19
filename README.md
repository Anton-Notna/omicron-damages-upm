# Omicron Damages
The motivation for writing this repository is to create a flexible and simple damage system that can be reused in different projects.

This package involves writing your own code.

# Installation
Omicron Damages is the upm package, so the install is similar to other upm packages:
1. Open `Window/PackageManager`
2. Click `+` in the right corner and select `Add package from git url...`
3. Paste link to this package `https://github.com/Anton-Notna/omicron-damages-upm.git` and click `Add`

# Usage
The central point is the interface “IDamageable<TData>”.
First, we need to define what kind of data will be passed to the Damageable object. Let's create a simple structure:
```csharp
namespace Core.Damages
{
    public struct DamageData
    {
        public int Amount;
        public DamageType Type;
    }

    public enum DamageType
    {
        Melee = 0,
        Gun = 1,
        Magic = 2,
    }
}
```

Now, let's create a Damageable class that can interact with this data:
```csharp
namespace Core.Damages
using OmicronDamages;
using UnityEngine;

namespace Core.Damages
{
    public class Health : IDamageable<DamageData>
    {
        [SerializeField]
        private int _currentHealth = 10;

        public void TakeDamage(DamagePoint point, DamageData data)
        {
            _currentHealth -= data.Amount;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, int.MaxValue);
        }
    }
}
```

Okay, now we have to do damage to these objects somehow. For this we can use the abstract class “SphereScanDamager<TData>”.
It allows us to register damage between spheres based on the position of the previous and current frame, so it won't miss any “IDamageable”.

Let's write an implementation that works with our damage structure:
```csharp
using OmicronDamages;

namespace Core.Damages
{
    public class SphereScanDamager : SphereScanDamager<DamageData> { }
}
```

Finally, we need to call “SphereScanDamager.Activate(...)”:
```csharp
using OmicronDamages;
using System.Collections;
using UnityEngine;

namespace Core.Damages
{
    public class DebugDamager : MonoBehaviour
    {
        [SerializeField]
        private float _activeTime;
        [SerializeField]
        private float _delay;
        [SerializeField]
        private GameObject _gfx;
        [SerializeField]
        private SphereScanDamager _damager;
        [SerializeField]
        private int _damageAmount = 10;

        private IEnumerator Start()
        {
            var active = new WaitForSeconds(_activeTime);
            var delay = new WaitForSeconds(_delay);
            TransformDamageSource source = new TransformDamageSource(transform, Vector3.forward);
            DamageData data = new DamageData() { Amount = _damageAmount };
            while (true)
            {
                _damager.Activate(source, data, _activeTime);
                _gfx.SetActive(true);
                yield return active;

                _gfx.SetActive(false);
                yield return delay;
            }
        }
    }
}
```
