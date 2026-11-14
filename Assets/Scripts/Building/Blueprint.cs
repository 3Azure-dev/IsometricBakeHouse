using System;
using UnityEngine;

// SCRUM-395 / 394: a placed blueprint. Press E with materials and it builds the real object.
public class Blueprint : MonoBehaviour, IInteractable
{
    // Anyone can listen: customers (built tables), UI, sounds
    public static event Action<FurnitureData, GameObject> Built;
    public event Action<int, int> MaterialsChanged;

    public FurnitureData Data { get; private set; }
    public int MaterialsIn { get; private set; }
    public int MaterialsNeeded => Data.materialBoxes;
    public int MaterialsMissing => MaterialsNeeded - MaterialsIn;
    public bool IsFull => MaterialsIn >= MaterialsNeeded;

    public void Init(FurnitureData data)
    {
        Data = data;
        name = "Blueprint_" + data.furnitureName;
        Debug.Log($"{name}: needs {MaterialsNeeded} {data.materialId}");
    }

    // Player presses E here: drop in the carried materials it still needs
    public void Interact()
    {
        if (IsFull || BakeryProgress.Instance == null) return;

        int carried = BakeryProgress.Instance.GetMaterialCount(Data.materialId);
        int give = Mathf.Min(MaterialsMissing, carried);

        if (give <= 0)
        {
            Debug.Log($"{name}: needs {MaterialsMissing} {Data.materialId}, you carry none");
            return;
        }

        BakeryProgress.Instance.TakeMaterial(Data.materialId, give);
        AddMaterial(give);
    }

    public bool AddMaterial(int amount = 1)
    {
        if (IsFull || amount <= 0) return false;

        MaterialsIn = Mathf.Min(MaterialsIn + amount, MaterialsNeeded);
        MaterialsChanged?.Invoke(MaterialsIn, MaterialsNeeded);
        Debug.Log($"{name}: materials {MaterialsIn}/{MaterialsNeeded}");

        if (IsFull) Build();
        return true;
    }

    private void Build()
    {
        GameObject built = Instantiate(Data.prefab, transform.position, transform.rotation);
        built.name = Data.furnitureName;

        if (BakeryProgress.Instance != null)
            BakeryProgress.Instance.AddBuiltItem(Data.itemId, transform.position, transform.rotation);

        Built?.Invoke(Data, built);
        Destroy(gameObject);
    }

    [ContextMenu("Test: add 1 material")]
    private void TestAdd() => AddMaterial(1);
}