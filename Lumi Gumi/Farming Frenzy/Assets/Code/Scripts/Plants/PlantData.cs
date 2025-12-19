using Code;
using Code.Scripts.Plants.Powers;
using UnityEngine;

[CreateAssetMenu(menuName = "Farming Frenzy/PlantData")]
public class PlantData : ScriptableObject
{
    [Header("Sprites")]
    public Sprite[] _maturationSprite;
    public Sprite[] _growthSprite;
    public Sprite _harvestedSprite;
    public Sprite _cursorSprite;
    public Sprite _roleIcon;

    [Header("Config")]
    public bool _isTree;
    public bool _cannotHarvest;
    public float _goldGenerationFactor = 1;
    public float _dryoutRate;

    [Tooltip("Time (seconds) from Seedling to Mature")]
    public float _maturationCycle;

    [Tooltip("Time (seconds) from Mature to First Fruit")]
    public float _fruitingCycle;

    public string flavorText;
    public PowerKind power;

    [Header("Yield Settings")]
    [Tooltip("Maximum items this plant can hold at once.")]
    [Min(1)] public int _maxYield = 1;

    [Tooltip("Seconds to generate ONE extra item after the first fruit appears.")]
    public float _yieldGenerationRate = 5.0f;

    [Range(1, 3)]
    public int _tier;

    public int _price;
    public int _goldGenerated;
    public float _health;
    public bool _indestructible;

    public GrowthRate GrowthRateBand
    {
        get
        {
            if (_maturationCycle <= 5) return GrowthRate.Fast;
            return _maturationCycle <= 15 ? GrowthRate.Medium : GrowthRate.Slow;
        }
    }

    public GrowthRate FruitingRateBand
    {
        get
        {
            if (_fruitingCycle <= 10) return GrowthRate.Fast;
            return _maturationCycle <= 15 ? GrowthRate.Medium : GrowthRate.Slow;
        }
    }
}