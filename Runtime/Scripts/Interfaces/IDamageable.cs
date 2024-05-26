namespace OmicronDamages
{
    public interface IDamageable<TData>
    {
        public bool TakeDamage(DamagePoint point, TData data);
    }
}