using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    private List<UnitMovement> selectedUnits;

    private void Awake()
    {
        selectedUnits = new List<UnitMovement>();
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
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        UnitMovement unit = hit.collider.GetComponent<UnitMovement>();
        if (unit == null || !hit.collider.CompareTag("PlayerUnit"))
            return;

        if (!Input.GetKey(KeyCode.LeftShift))
            ClearSelection();

        if (!selectedUnits.Contains(unit))
            selectedUnits.Add(unit);
    }

    private void HandleRightClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Ground"))
            return;

        foreach (UnitMovement unit in selectedUnits)
            unit.MoveToPosition(hit.point);
    }

    private void ClearSelection()
    {
        selectedUnits.Clear();
    }
}
