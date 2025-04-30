using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AddressableImageEntry
{
    public string name = "Image";

    [SerializeField] private Image targetImage;
    [SerializeField] private AssetReference atlasReference;
    [SerializeField] private string atlasSpriteName;
    [SerializeField] private AssetReferenceSprite spriteReference;

    public void Load()
    {
        if (atlasReference.RuntimeKeyIsValid() && !string.IsNullOrEmpty(atlasSpriteName))
        {
            atlasReference.LoadAssetAsync<SpriteAtlas>()
                .Completed += OnAtlasLoaded;
        }
        else if (spriteReference.RuntimeKeyIsValid())
        {
            spriteReference.LoadAssetAsync()
                .Completed += OnSpriteLoaded;
        }
        else
        {
            Debug.LogError($"[{name}] 할당된 참조 없음");
        }
    }

    private void OnAtlasLoaded(AsyncOperationHandle<SpriteAtlas> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var atlas = handle.Result;
            Sprite sp = atlas.GetSprite(atlasSpriteName);
            if (sp != null)
                targetImage.sprite = sp;
            else
                Debug.LogError($"[{name}] Sprite '{atlasSpriteName}' Atlas 없음");
        }
        else
        {
            Debug.LogError($"[{name}] Atlas 로드 실패.");
        }
    }

    private void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
            targetImage.sprite = handle.Result;
        else
            Debug.LogError($"[{name}] Sprite 참조 실패.");
    }
}

public class AddressableManager : MonoBehaviour
{
    [SerializeField] private AssetReferenceGameObject[] objectRefs;
    [SerializeField] private AddressableImageEntry[] imageEntries;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        StartCoroutine(InitAddressables());
    }

    IEnumerator InitAddressables()
    {
        var initHandle = Addressables.InitializeAsync();
        yield return initHandle;
    }

    public void OnClickGetResources()
    {
        foreach (var objRef in objectRefs)
        {
            objRef.InstantiateAsync()
                  .Completed += handle =>
                  {
                      if (handle.Status == AsyncOperationStatus.Succeeded)
                          spawnedObjects.Add(handle.Result);
                      else
                          Debug.LogError("GameObject 인스턴스화");
                  };
        }

        foreach (var entry in imageEntries)
        {
            entry.Load();
        }
    }

    public void ReleaseResources()
    {
        foreach (var go in spawnedObjects)
            Addressables.ReleaseInstance(go);
        spawnedObjects.Clear();
    }
}
