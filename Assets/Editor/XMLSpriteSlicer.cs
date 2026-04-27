using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class XMLSpriteSlicer
{
    [MenuItem("Assets/Slice Sprite using XML", true)]
    static bool Validate() => Selection.activeObject is Texture2D;

    [MenuItem("Assets/Slice Sprite using XML", priority = 100)]
    static void Execute()
    {
        var texture = (Texture2D)Selection.activeObject;
        string texPath = AssetDatabase.GetAssetPath(texture);
        string xmlPath = Path.ChangeExtension(texPath, ".xml");

        if (!File.Exists(xmlPath))
        {
            EditorUtility.DisplayDialog("XML Sprite Slicer",
                $"No XML file found at:\n{xmlPath}", "OK");
            return;
        }

        var importer = (TextureImporter)AssetImporter.GetAtPath(texPath);
        importer.textureType       = TextureImporterType.Sprite;
        importer.spriteImportMode  = SpriteImportMode.Multiple;
        importer.mipmapEnabled     = false;

        // Apply import settings first so the data provider can open the asset
        importer.SaveAndReimport();

        // Parse XML
        int texH = texture.height;
        var doc  = new XmlDocument();
        doc.Load(xmlPath);

        // Support both <TextureAtlas> and bare <root> wrappers
        XmlNodeList nodes = doc.GetElementsByTagName("SubTexture");
        if (nodes.Count == 0)
        {
            EditorUtility.DisplayDialog("XML Sprite Slicer",
                "No <SubTexture> nodes found in the XML file.", "OK");
            return;
        }

        var rects = new List<SpriteRect>(nodes.Count);
        var seen  = new Dictionary<string, int>();

        foreach (XmlNode node in nodes)
        {
            string name  = node.Attributes["name"]?.Value   ?? "sprite";
            float  x     = float.Parse(node.Attributes["x"]?.Value      ?? "0");
            float  y     = float.Parse(node.Attributes["y"]?.Value      ?? "0");
            float  w     = float.Parse(node.Attributes["width"]?.Value  ?? "0");
            float  h     = float.Parse(node.Attributes["height"]?.Value ?? "0");

            // Strip file extension from name if present (e.g. "idle_0.png" → "idle_0")
            name = Path.GetFileNameWithoutExtension(name);

            // Deduplicate names
            if (seen.TryGetValue(name, out int count))
            {
                seen[name] = count + 1;
                name = $"{name}_{count}";
            }
            else
            {
                seen[name] = 1;
            }

            // Convert top-left origin (XML) → bottom-left origin (Unity)
            float unityY = texH - y - h;

            rects.Add(new SpriteRect
            {
                name      = name,
                rect      = new Rect(x, unityY, w, h),
                pivot     = new Vector2(0.5f, 0.5f),
                alignment = SpriteAlignment.Center,
                border    = Vector4.zero,
                spriteID  = GUID.Generate()
            });
        }

        // Write via Sprite Data Provider API
        var factory      = new SpriteDataProviderFactories();
        factory.Init();
        var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();
        dataProvider.SetSpriteRects(rects.ToArray());
        dataProvider.Apply();

        importer.SaveAndReimport();

        Debug.Log($"[XMLSpriteSlicer] Sliced {rects.Count} sprites from {Path.GetFileName(xmlPath)}");
        EditorUtility.DisplayDialog("XML Sprite Slicer",
            $"Done — {rects.Count} sprites sliced from {Path.GetFileName(xmlPath)}.", "OK");
    }
}
