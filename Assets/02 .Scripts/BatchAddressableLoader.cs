using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

public class BatchAddressableLoader : MonoBehaviour
{
    [SerializeField] private Transform parentTransform;
    private List<GameObject> instances = new List<GameObject>();

    public void LoadAllBatch()
    {
        // "batch" 라벨이 붙은 모든 GameObject 프리팹을 로드해서 인스턴스화
        Addressables.LoadAssetsAsync<GameObject>(
            "batch",                 // label
            prefab =>                
            {
                var inst = Instantiate(prefab, parentTransform);
                instances.Add(inst);
            }
        ).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                Debug.Log("Batch assets loaded.");
            else
                Debug.LogError("Failed to load batch assets.");
        };
    }

    public void ReleaseAllBatch()
    {
        foreach (var inst in instances)
            Addressables.ReleaseInstance(inst);
        instances.Clear();
        Debug.Log("Released all batch instances.");
    }
}
