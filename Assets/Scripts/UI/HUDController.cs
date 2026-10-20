using UnityEngine;
using UnityEngine.UI;
using Gorkha.Economy;
using Gorkha.Buildings;
using Gorkha.CameraSystem;

namespace Gorkha.UI
{
    /// <summary>
    /// Clash of Clans style top HUD bar displaying kingdom resource counters,
    /// build button, and camera controls.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
#pragma warning disable 0649, 0169
        [Header("Resource Labels")]
        [SerializeField] private Text _foodText;
        [SerializeField] private Text _woodText;
        [SerializeField] private Text _stoneText;
        [SerializeField] private Text _goldText;

        [Header("UI Panels")]
        [SerializeField] private GameObject _buildShopPanel;
        [SerializeField] private GameObject _buildingDetailsPanel;

        [Header("Camera Reference")]
        [SerializeField] private IsometricCameraController _cameraController;
#pragma warning restore 0649, 0169

        private void Start()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged += UpdateHUD;
                UpdateHUD();
            }
        }

        private void OnDestroy()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnResourcesChanged -= UpdateHUD;
            }
        }

        private void UpdateHUD()
        {
            if (_foodText != null) _foodText.text = $"{ResourceManager.Instance.GetAmount(ResourceType.Food):F0}";
            if (_woodText != null) _woodText.text = $"{ResourceManager.Instance.GetAmount(ResourceType.Wood):F0}";
            if (_stoneText != null) _stoneText.text = $"{ResourceManager.Instance.GetAmount(ResourceType.Stone):F0}";
            if (_goldText != null) _goldText.text = $"{ResourceManager.Instance.GetAmount(ResourceType.Gold):F0}";
        }

        public void OpenBuildShop()
        {
            if (_buildShopPanel != null)
            {
                _buildShopPanel.SetActive(true);
            }
        }

        public void CloseBuildShop()
        {
            if (_buildShopPanel != null)
            {
                _buildShopPanel.SetActive(false);
            }
        }

        public void SelectBlueprintToPlace(BuildingData blueprint)
        {
            CloseBuildShop();
            if (BuildingPlacementController.Instance != null)
            {
                BuildingPlacementController.Instance.StartPlacement(blueprint);
            }
        }
    }
}
