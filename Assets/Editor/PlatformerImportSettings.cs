using System.IO;
using UnityEditor;
using UnityEngine;

public static class PlatformerImportSettings
{
    const int PPU = 64;

    static readonly string[] BackgroundKeywords =
        { "background", "bg", "back", "sky", "parallax" };

    [MenuItem("Tools/Platformer/Apply 2D Import Settings To Selected Textures")]
    static void Apply()
    {
        var textures = Selection.objects;
        if (textures == null || textures.Length == 0)
        {
            EditorUtility.DisplayDialog("Platformer Import Settings",
                "Select one or more textures in the Project window first.", "OK");
            return;
        }

        int changed = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var obj in textures)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                string nameLower = Path.GetFileNameWithoutExtension(path).ToLower();
                string pathLower = path.ToLower();
                bool isBackground = IsBackground(nameLower) || IsBackground(pathLower);
                bool hasXml = File.Exists(Path.ChangeExtension(path, ".xml"));

                importer.textureType       = TextureImporterType.Sprite;
                importer.spriteImportMode  = hasXml ? SpriteImportMode.Multiple : SpriteImportMode.Single;
                importer.spritePixelsPerUnit = PPU;
                importer.filterMode        = FilterMode.Point;
                importer.mipmapEnabled     = false;
                importer.wrapMode          = isBackground ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

                var settings = importer.GetDefaultPlatformTextureSettings();
                settings.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SetPlatformTextureSettings(settings);

                importer.SaveAndReimport();
                changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        EditorUtility.DisplayDialog("Platformer Import Settings",
            $"Applied 2D settings to {changed} texture(s).", "OK");
        Debug.Log($"[PlatformerImportSettings] Applied to {changed} texture(s).");
    }

    [MenuItem("Tools/Platformer/Apply 2D Import Settings To Selected Textures", true)]
    static bool Validate() => Selection.objects != null && Selection.objects.Length > 0;

    static bool IsBackground(string text)
    {
        foreach (var kw in BackgroundKeywords)
            if (text.Contains(kw)) return true;
        return false;
    }
}
