// Assets/Editor/BatchAddressableSetup.cs
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class BatchAddressableSetup : EditorWindow
{
    [SerializeField] private List<GameObject> targets = new List<GameObject>();
    [SerializeField] private bool includeChildren = false; // 자식까지 처리 옵션

    private ReorderableList goList;
    private SerializedObject so;
    private AddressableAssetSettings settings;
    private AddressableAssetGroup group;

    [MenuItem("Tools/Addressables/Batch Setup")]
    public static void ShowWindow() => GetWindow<BatchAddressableSetup>("Batch Addressables");

    private void OnEnable()
    {
        so = new SerializedObject(this);

        // Addressables Default Local Group 준비
        settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        group = settings.FindGroup("Default Local Group")
             ?? settings.CreateGroup(
                    "Default Local Group",
                    false, false, true,
                    null,
                    typeof(BundledAssetGroupSchema)
                );

        // ReorderableList 초기화 (씬 오브젝트 허용)
        goList = new ReorderableList(so, so.FindProperty("targets"), true, true, true, true);
        goList.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "Hierarchy Objects to Addressablize");
        goList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var elem = goList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            elem.objectReferenceValue = EditorGUI.ObjectField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                elem.objectReferenceValue,
                typeof(GameObject),
                true
            );
        };
        goList.onAddCallback = list =>
        {
            goList.serializedProperty.arraySize++;
            goList.index = goList.serializedProperty.arraySize - 1;
        };
    }

    private void OnGUI()
    {
        so.Update();
        goList.DoLayoutList();

        // includeChildren 옵션 노출
        includeChildren = EditorGUILayout.Toggle("Include Children", includeChildren);

        so.ApplyModifiedProperties();

        GUILayout.Space(10);
        if (GUILayout.Button("Process Addressables"))
            ProcessAll();
    }

    private void ProcessAll()
    {
        // 1) 풀 생성
        List<GameObject> pool = new List<GameObject>();
        if (includeChildren)
        {
            foreach (var go in targets.Where(x => x != null))
                pool.AddRange(go.GetComponentsInChildren<Transform>(true)
                                .Select(t => t.gameObject));
        }
        else
        {
            pool.AddRange(targets.Where(x => x != null));
        }

        // 2) 자식 제외 필터: 부모로 등록된 오브젝트의 자손은 무시
        var roots = pool.Where(go => !pool.Any(parent =>
            go != parent && go.transform.IsChildOf(parent.transform)
        ));

        // 3) 실제 처리
        foreach (var go in roots)
            ProcessSingle(go);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Batch Addressables", "완료됐다, My son.", "OK");
    }

    private void ProcessSingle(GameObject go)
    {
        const string baseFolder = "Assets/Addressables";
        if (!AssetDatabase.IsValidFolder(baseFolder))
            AssetDatabase.CreateFolder("Assets", "Addressables");
        const string prefabFolder = baseFolder + "/Prefabs";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
            AssetDatabase.CreateFolder(baseFolder, "Prefabs");

        string prefabPath = $"{prefabFolder}/{go.name}.prefab";

        // SaveAsPrefabAsset로 변경
        PrefabUtility.SaveAsPrefabAsset(
            go,
            prefabPath,
            out bool success
        );
        if (!success)
            Debug.LogError($"[{go.name}] Prefab 저장 실패 at {prefabPath}");

        // Addressable 등록
        string guid = AssetDatabase.AssetPathToGUID(prefabPath);
        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = $"prefabs/{go.name}";
        if (!entry.labels.Contains("batch"))
            entry.labels.Add("batch");

        // 4) UI Image 안의 Sprite/Atlas 처리
        foreach (var img in go.GetComponentsInChildren<Image>(true))
        {
            var spr = img.sprite;
            if (spr == null) continue;

            string assetPath = AssetDatabase.GetAssetPath(spr);
            if (assetPath.EndsWith(".spriteatlas"))
                RegisterAsset(assetPath, $"atlases/{Path.GetFileNameWithoutExtension(assetPath)}");
            else if (AssetDatabase.IsSubAsset(spr))
                RegisterAsset(assetPath, $"atlases/{Path.GetFileNameWithoutExtension(assetPath)}");
            else
                RegisterAsset(assetPath, $"sprites/{spr.name}");
        }
    }


    private void ProcessRecursively(GameObject go)
    {
        ProcessSingle(go);
        foreach (Transform child in go.transform)
            ProcessRecursively(child.gameObject);
    }

    private void RegisterAsset(string path, string address)
    {
        string guid = AssetDatabase.AssetPathToGUID(path);
        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = address;
        if (!entry.labels.Contains("batch")) entry.labels.Add("batch");
    }
}
