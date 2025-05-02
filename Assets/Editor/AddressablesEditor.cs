// Assets/Editor/AddressablesEditor.cs
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 씬에 있는 모든 UI Image와 AudioSource를 스캔하여
/// Addressable로 등록하고 원본 에셋을 제거/복구할 수 있는 커스텀 에디터 창입니다.
/// Atlas 안의 개별 스프라이트도 이름과 매핑된 Atlas 경로를 기록하여 올바르게 복구합니다.
/// </summary>
public class AddressablesEditor : EditorWindow
{
    // 저장된 원본 매핑
    private List<ImageEntry> imageEntries = new List<ImageEntry>();
    private List<AudioEntry> audioEntries = new List<AudioEntry>();

    // Addressables 세팅 참조
    private AddressableAssetSettings settings;
    private AddressableAssetGroup group;

    [MenuItem("Tools/Addressables/Custom Addressable Editor")]
    public static void ShowWindow() => GetWindow<AddressablesEditor>("Custom Addressable Editor");

    private void OnEnable()
    {
        settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        group = settings.FindGroup("Default Local Group")
             ?? settings.CreateGroup("Default Local Group", false, false, true, null);
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        if (GUILayout.Button("Upload Resources", GUILayout.Height(30))) UploadResources();
        GUILayout.Space(5);
        if (GUILayout.Button("Update Resources", GUILayout.Height(30))) UpdateResources();
    }

    private void UploadResources()
    {
        Debug.Log("가즈아~");
        imageEntries.Clear();
        audioEntries.Clear();

        // 씬의 모든 UI Image 처리
        foreach (var img in FindObjectsOfType<Image>(true))
        {
            var sprite = img.sprite;
            if (sprite == null) continue;

            string recordPath = AssetDatabase.GetAssetPath(sprite);
            if (string.IsNullOrEmpty(recordPath)) continue;

            RegisterAddressable(recordPath);
            Debug.Log($"Registered Image '{img.gameObject.name}' with Sprite '{sprite.name}' at path '{recordPath}'");
            imageEntries.Add(new ImageEntry
            {
                component = img,
                assetPath = recordPath,
                spriteName = sprite.name
            });

            img.sprite = null;
            EditorUtility.SetDirty(img);
        }

        // 씬의 모든 AudioSource 처리
        foreach (var src in FindObjectsOfType<AudioSource>(true))
        {
            var clip = src.clip;
            if (clip == null) continue;

            string clipPath = AssetDatabase.GetAssetPath(clip);
            if (string.IsNullOrEmpty(clipPath)) continue;

            RegisterAddressable(clipPath);
            Debug.Log($"Registered AudioSource '{src.gameObject.name}' with Clip '{clip.name}' at path '{clipPath}'");
            audioEntries.Add(new AudioEntry { component = src, assetPath = clipPath });

            src.clip = null;
            EditorUtility.SetDirty(src);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void UpdateResources()
    {
        Debug.Log("Called Update Resources");

        // 이미지 복구
        foreach (var entry in imageEntries)
        {
            Sprite sprite = null;
            string ext = Path.GetExtension(entry.assetPath).ToLower();
            if (ext == ".spriteatlas")
            {
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(entry.assetPath);
                if (atlas != null)
                    sprite = atlas.GetSprite(entry.spriteName);
            }
            else
            {
                // 다중 스프라이트(스프라이트 시트) 처리: 이름 매칭해서 로드
                var assets = AssetDatabase.LoadAllAssetsAtPath(entry.assetPath);
                foreach (var a in assets)
                {
                    if (a is Sprite s && s.name == entry.spriteName)
                    {
                        sprite = s;
                        break;
                    }
                }
            }

            if (sprite != null)
            {
                entry.component.sprite = sprite;
                EditorUtility.SetDirty(entry.component);
            }
            else
            {
                Debug.LogError($"Failed to load sprite '{entry.spriteName}' from '{entry.assetPath}'");
            }
        }

        // 오디오 클립 복구
        foreach (var entry in audioEntries)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(entry.assetPath);
            if (clip != null)
            {
                entry.component.clip = clip;
                EditorUtility.SetDirty(entry.component);
            }
            else
            {
                Debug.LogError($"Failed to load audio clip from '{entry.assetPath}'");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void RegisterAddressable(string assetPath)
    {
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        var entry = settings.CreateOrMoveEntry(guid, group);
        entry.address = Path.GetFileNameWithoutExtension(assetPath);
    }

    // 내부 매핑 클래스
    private class ImageEntry
    {
        public Image component;
        public string assetPath;
        public string spriteName;
    }
    private class AudioEntry
    {
        public AudioSource component;
        public string assetPath;
    }
}
