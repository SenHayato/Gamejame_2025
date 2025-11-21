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
    public float _maturationCycle;
    public float _fruitingCycle;
    public string flavorText;
    public PowerKind power;

    [Range(1,3)]
    public int _tier;

    public int _price;
    public int _goldGenerated;
    public float _health;
    public bool _indestructible;
    
    public GrowthRate GrowthRateBand
    {
        // Just some example values
        get {
            if (_maturationCycle <= 5)
            {
                return GrowthRate.Fast;
            }
            return _maturationCycle <= 15 ? GrowthRate.Medium : GrowthRate.Slow;
        }
    }

    public GrowthRate FruitingRateBand
    {
        // Just some example values
        get {
            if (_fruitingCycle <= 10)
            {
                return GrowthRate.Fast;
            }
            return _maturationCycle <= 15 ? GrowthRate.Medium : GrowthRate.Slow;
        }
    }
}
