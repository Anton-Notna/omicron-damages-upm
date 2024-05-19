namespace OmicronDamages
{
    public interface IDamageable<TData>
    {
        public void TakeDamage(DamagePoint point, TData data);
    }
}