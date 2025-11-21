namespace Code.Scripts.Plants.Powers
{
    public class CloverPower : LegumePower
    {
        protected override float Radius => 6f;

        public const int EffectPercent = 5;
        protected override float EffectStrength => EffectPercent / 100.0f;
    }
}
