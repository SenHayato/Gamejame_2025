using System;
using System.Collections.Generic;
using Code.Scripts.Plants.Powers;
using Code.Scripts.Plants.Powers.PowerExtension;
using UnityEngine;

namespace Code.Scripts.Managers
{
    public class PlantManager : MonoBehaviour
    {
        #region Editor Fields
        [SerializeField] private List<PlantData> _plantData;
        [SerializeField] public Camera _camera;
        #endregion

        #region Singleton
        public static PlantManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                CornPowerAoe = new PlantAoeState(PowerKind.Corn);
                LegumePowerAoe = new PlantAoeState(PowerKind.Clover);
                BananaPowerAoe = new PlantAoeState(PowerKind.Banana);
                ChiliPowerAoe = new PlantAoeState(PowerKind.Chili);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            CornPowerAoe.Start();
            LegumePowerAoe.Start();
            BananaPowerAoe.Start();
            ChiliPowerAoe.Start();
        }

        public PlantAoeState CornPowerAoe;
        public PlantAoeState LegumePowerAoe;
        public PlantAoeState BananaPowerAoe;
        public PlantAoeState ChiliPowerAoe;
        #endregion

        #region Methods
        public PlantData GetPlantData(string plantName)
        {
            return _plantData.Find(plant => plant.name == plantName);
        }

        private void Update()
        {
            if (!_camera) return;
            CornPowerAoe.Tick(_camera);
            LegumePowerAoe.Tick(_camera);
            BananaPowerAoe.Tick(_camera);
            ChiliPowerAoe.Tick(_camera);
        }

        #endregion
    }
}
