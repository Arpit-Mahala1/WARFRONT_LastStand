using System.Collections;
using UnityEngine;

public class EngineerController : MonoBehaviour
{
    public GameObject sandbagPrefab;
    public GameObject turretPrefab;
    public float buildTimeSandbag = 5f;
    public float buildTimeTurret = 10f;
    public float repairRate = 15f;
    public float buildRange = 12f;

    private bool isBusy;
    private UnitMovement unitMovement;
    private Camera mainCamera;

    private void Start()
    {
        GameManager.Instance.RegisterEngineerCount(1);
        UnitHealth unitHealth = GetComponent<UnitHealth>();
        if (unitHealth != null)
        {
            unitHealth.OnDeath += () => GameManager.Instance.RegisterEngineerCount(-1);
        }
        
        unitMovement = GetComponent<UnitMovement>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (unitMovement != null && unitMovement.IsSelected && !isBusy)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TryBuildSandbag();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TryBuildTurret();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TryRepair();
            }
        }
    }

    private void TryBuildSandbag()
    {
        if (GetMouseGroundPosition(out Vector3 targetPos))
        {
            if (Vector3.Distance(transform.position, targetPos) <= buildRange)
            {
                BuildSandbag(targetPos);
            }
            else
            {
                Debug.Log("Target out of build range.");
            }
        }
    }

    private void TryBuildTurret()
    {
        if (GetMouseGroundPosition(out Vector3 targetPos))
        {
            if (Vector3.Distance(transform.position, targetPos) <= buildRange)
            {
                BuildTurret(targetPos);
            }
            else
            {
                Debug.Log("Target out of build range.");
            }
        }
    }

    private void TryRepair()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            StructureHealth structure = hit.collider.GetComponentInParent<StructureHealth>();
            if (structure != null)
            {
                if (Vector3.Distance(transform.position, structure.transform.position) <= buildRange)
                {
                    RepairStructure(structure);
                }
                else
                {
                    Debug.Log("Target structure out of build range.");
                }
            }
        }
    }

    private bool GetMouseGroundPosition(out Vector3 position)
    {
        position = Vector3.zero;
        if (mainCamera == null) mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        // Attempt to raycast against everything. You could filter by ground layer if needed.
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            position = hit.point;
            return true;
        }
        return false;
    }

    public void BuildSandbag(Vector3 position)
    {
        if (isBusy || sandbagPrefab == null || !GameManager.Instance.SpendSupply(30))
            return;

        StartCoroutine(BuildStructure(position, sandbagPrefab, buildTimeSandbag));
    }

    public void BuildTurret(Vector3 position)
    {
        if (isBusy || turretPrefab == null || !GameManager.Instance.SpendSupply(80))
            return;

        StartCoroutine(BuildStructure(position, turretPrefab, buildTimeTurret));
    }

    public void RepairStructure(StructureHealth structure)
    {
        if (structure == null || isBusy)
            return;

        StartCoroutine(RepairCoroutine(structure));
    }

    private IEnumerator BuildStructure(Vector3 position, GameObject prefab, float buildTime)
    {
        isBusy = true;
        
        if (unitMovement != null)
            unitMovement.StopMoving();

        float elapsed = 0f;
        while (elapsed < buildTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Instantiate(prefab, position, Quaternion.identity);
        isBusy = false;
    }

    private IEnumerator RepairCoroutine(StructureHealth structure)
    {
        isBusy = true;
        
        if (unitMovement != null)
            unitMovement.StopMoving();

        while (structure != null && !structure.isDestroyed && structure.currentHealth < structure.maxHealth)
        {
            structure.Repair(repairRate * Time.deltaTime);
            yield return null;
        }

        isBusy = false;
    }
}
