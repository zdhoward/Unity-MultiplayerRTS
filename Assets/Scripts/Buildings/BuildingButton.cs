using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using Mirror;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Building building = null;
    [SerializeField] Image icon = null;
    [SerializeField] TMP_Text priceLabel = null;
    [SerializeField] LayerMask floorLayerMask = new LayerMask();

    Camera mainCamera;
    RTSPlayer player;
    GameObject buildingPreviewInstance;
    Renderer buildingRendererInstance;

    void Start()
    {
        mainCamera = Camera.main;
        icon.sprite = building.GetIcon();
        priceLabel.text = building.GetPrice().ToString();
    }

    void Update()
    {
        if (player == null)
            player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        if (buildingPreviewInstance == null)
            return;

        UpdateBuildingPreview();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

        buildingPreviewInstance.SetActive(false);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingPreviewInstance == null)
            return;

        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayerMask))
        {
            // place building
            player.CmdTryPlaceBuilding(building.GetId(), hit.point);
        }

        Destroy(buildingPreviewInstance);
    }

    void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorLayerMask))
            return;

        buildingPreviewInstance.transform.position = hit.point;

        if (!buildingPreviewInstance.activeSelf)
            buildingPreviewInstance.SetActive(true);
    }
}
