using System;
using UnityEngine;

// SCRUM-395: a placed blueprint. Fill it with material boxes and it builds the real object.
public class Blueprint : MonoBehaviour
{
    // Anyone can listen: customers (built tables), UI, sounds
    public static event Action<FurnitureData, GameObject> Built;
    public event Action<int, int> MaterialsChanged;

    public FurnitureData Data { get; private set; }
    public int MaterialsIn { get; private set; }
    public int MaterialsNeeded => Data.materialBoxes;
    public bool IsFull => MaterialsIn >= MaterialsNeeded;

    public void Init(FurnitureData data)
    {
        Data = data;
        name = "Blueprint_" + data.furnitureName;
        Debug.Log($"{name}: needs {MaterialsNeeded} material box(es)");
    }

    // Returns false if it is already full
    public bool AddMaterial()
    {
        if (IsFull) return false;

        MaterialsIn++;
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
            BakeryProgress.Instance.AddBuiltItem(Data.name, transform.position, transform.rotation);

        Built?.Invoke(Data, built);
        Destroy(gameObject);
    }

    [ContextMenu("Test: add 1 material")]
    private void TestAdd() => AddMaterial();
}