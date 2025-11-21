namespace Code.Scripts.Plants.Powers
{
    public class BeanPower : LegumePower
    {
        protected override float Radius => 6f;
        public const int EffectPercent = 10;
        protected override float EffectStrength => EffectPercent / 100.0f;
    }
}
