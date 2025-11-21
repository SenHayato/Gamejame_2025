using System;
using Code.Scripts.Managers;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Code.Scripts.Plants.Powers
{
    public abstract class Power : MonoBehaviour {}

    [Serializable]
    public enum PowerKind
    {
        None = 1,
        Clover = 2,
        Corn = 3,
        Nettle = 4,
        Bean = 5,
        Banana = 6,
        Chili = 7
    }

    namespace PowerExtension
    {
        public static class PowerExtensions
        {
            private static readonly Object CloverPowerPrefab = Resources.Load("Clover Power");
            private static readonly Object BeanPowerPrefab = Resources.Load("Bean Power");
            private static readonly Object CornPowerPrefab = Resources.Load("Corn Power");
            private static readonly Object NettlePowerPrefab = Resources.Load("Nettle Power");
            private static readonly Object BananaPowerPrefab = Resources.Load("Banana Power");
            private static readonly Object ChiliPowerPrefab = Resources.Load("Chili Power");

            public static void AddTo(this PowerKind kind, GameObject gameObject)
            {
                var pos = gameObject.transform.position;
                var id = Quaternion.identity;
                var parent = gameObject.transform;

                var prefab = kind switch
                {
                    PowerKind.Clover => CloverPowerPrefab,
                    PowerKind.Bean => BeanPowerPrefab,
                    PowerKind.Corn => CornPowerPrefab,
                    PowerKind.Nettle => NettlePowerPrefab,
                    PowerKind.Banana => BananaPowerPrefab,
                    PowerKind.Chili => ChiliPowerPrefab,
                    PowerKind.None => null,
                    _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
                };

                if (prefab)
                {
                    Object.Instantiate(prefab, pos, id, parent);
                }
            }

            public static string Text(this PowerKind kind) => kind switch
            {
                PowerKind.Clover => $"Nearby plants heal \n{CloverPower.EffectPercent}% " +
                                    $"faster (up to {LegumePower.MaxEffectPercent}%)",
                PowerKind.Bean => $"Nearby plants heal \n{BeanPower.EffectPercent}% " +
                                    $"faster (up to {LegumePower.MaxEffectPercent}%)",
                PowerKind.Corn => "Other corn plants in a row\nor column with this one\nfruit" +
                                  $" {CornPower.EffectPercent}% faster (up to {CornPower.MaxEffectPercent}%)",
                PowerKind.Banana =>$"Nearby plants fruit \n{BananaPower.EffectPercent}% " +
                                   $"faster (up to {BananaPower.MaxEffectPercent}%)",
                PowerKind.Nettle => "Animals take damage when\nthey eat this plant",
                PowerKind.Chili => $"Defensive plants do +{ChiliPower.EffectPercent}%\ndamage (does not stack)",
                PowerKind.None => "",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };

            [CanBeNull]
            public static PlantAoeState AoeState(this PowerKind kind)
            {
                switch (kind)
                {
                    case PowerKind.Bean:
                    case PowerKind.Clover:
                        return PlantManager.Instance.LegumePowerAoe;
                    case PowerKind.Corn:
                        return PlantManager.Instance.CornPowerAoe;
                    case PowerKind.Banana:
                        return PlantManager.Instance.BananaPowerAoe;
                    case PowerKind.Chili:
                        return PlantManager.Instance.ChiliPowerAoe;
                    case PowerKind.None:
                    case PowerKind.Nettle:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
                }

                return null;
            }

            public static GameObject PlaceAoeIndicator(this PowerKind kind)
            {
                var indicator = new GameObject();
                kind.AddTo(indicator);

                foreach (Transform child in indicator.transform)
                {
                    foreach (var component in child.GetComponents<Component>())
                    {
                        if (component is Transform or SpriteRenderer) continue;
                        Debug.Log("This component is a", component);
                        Object.Destroy(component);
                    }
                }


                return indicator;
            }
        }
    }
}
