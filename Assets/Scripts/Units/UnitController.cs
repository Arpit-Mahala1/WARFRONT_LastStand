using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public LayerMask groundLayer;

    private readonly List<UnitMovement> selectedUnits = new List<UnitMovement>();
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (groundLayer == 0)
            groundLayer = ~0;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            HandleLeftClick();

        if (Input.GetMouseButtonDown(1))
            HandleRightClick();
    }

    private void HandleLeftClick()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f))
            return;

        UnitMovement unit = hit.collider.GetComponentInParent<UnitMovement>();
        if (unit != null && unit.gameObject.CompareTag("PlayerUnit"))
        {
            bool shiftHeld = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (!shiftHeld)
                ClearSelection();

            if (selectedUnits.Contains(unit))
            {
                if (shiftHeld)
                    DeselectUnit(unit);
            }
            else
            {
                SelectUnit(unit);
            }

            return;
        }

        if (!Input.GetKey(KeyCode.LeftShift) && (groundLayer == 0 || (groundLayer.value & (1 << hit.collider.gameObject.layer)) != 0))
            ClearSelection();
    }

    private void HandleRightClick()
    {
        if (selectedUnits.Count == 0)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
            return;

        selectedUnits.RemoveAll(unit => unit == null);
        if (selectedUnits.Count == 0)
            return;

        Vector3 baseDestination = hit.point;
        float radius = 1.5f;

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            UnitMovement unit = selectedUnits[i];
            float angle = i * Mathf.PI * 2f / selectedUnits.Count;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            unit.MoveToPosition(baseDestination + offset);
        }
    }

    private void SelectUnit(UnitMovement unit)
    {
        if (unit == null)
            return;

        if (!selectedUnits.Contains(unit))
        {
            selectedUnits.Add(unit);
            unit.SetSelected(true);
        }
    }

    private void DeselectUnit(UnitMovement unit)
    {
        if (unit == null)
            return;

        selectedUnits.Remove(unit);
        unit.SetSelected(false);
    }

    private void ClearSelection()
    {
        for (int i = selectedUnits.Count - 1; i >= 0; i--)
        {
            if (selectedUnits[i] != null)
                selectedUnits[i].SetSelected(false);
        }

        selectedUnits.Clear();
    }
}
