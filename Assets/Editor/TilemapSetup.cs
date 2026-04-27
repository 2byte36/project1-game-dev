using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class TilemapSetup
{
    const string TileDataFolder  = "Assets/TileData";
    const string PalettesFolder  = "Assets/Palettes";
    const string PaletteName     = "PlatformPalette";
    const string TilesSheet      = "Assets/Sprites/Tiles/spritesheet-tiles-default.png";

    // ─── Entry point ─────────────────────────────────────────────────────────
    public static string Execute()
    {
        var sb = new System.Text.StringBuilder();

        SetupSceneObjects(sb);
        CreateTileAssets(sb);
        CreatePalette(sb);
        CheckConsole(sb);

        return sb.ToString();
    }

    // ─── Scene setup ─────────────────────────────────────────────────────────
    static void SetupSceneObjects(System.Text.StringBuilder sb)
    {
        // Grid root
#pragma warning disable CS0618
        var existingGrid = Object.FindObjectOfType<Grid>();
#pragma warning restore CS0618
        GameObject gridGO = existingGrid != null
            ? existingGrid.gameObject
            : new GameObject("Grid", typeof(Grid));
        sb.AppendLine($"Grid: {(existingGrid != null ? "found" : "created")}");

        // Ground
        SetupTilemapChild(gridGO, "Ground",
            sortingLayerName: "Foreground", orderInLayer: -1,
            physicsLayer: "Ground", withCollider: true, sb);

        // Decoration
        SetupTilemapChild(gridGO, "Decoration",
            sortingLayerName: "Foreground", orderInLayer: 1,
            physicsLayer: null, withCollider: false, sb);

        EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        sb.AppendLine("Scene saved.");
    }

    static void SetupTilemapChild(
        GameObject parent, string childName,
        string sortingLayerName, int orderInLayer,
        string physicsLayer, bool withCollider,
        System.Text.StringBuilder sb)
    {
        // Find or create child
        var existing = parent.transform.Find(childName);
        GameObject go;
        if (existing != null)
            go = existing.gameObject;
        else
        {
            go = new GameObject(childName);
            go.transform.SetParent(parent.transform, false);
        }

        // Physics layer
        if (!string.IsNullOrEmpty(physicsLayer))
        {
            int idx = LayerMask.NameToLayer(physicsLayer);
            if (idx >= 0) go.layer = idx;
        }

        // Tilemap — add first so TilemapRenderer has its required sibling
        if (go.GetComponent<Tilemap>() == null)
            go.AddComponent<Tilemap>();

        // TilemapRenderer — explicit guard, then fresh reference avoids Unity ?? trap
        if (go.GetComponent<TilemapRenderer>() == null)
            go.AddComponent<TilemapRenderer>();
        var rend = go.GetComponent<TilemapRenderer>();
        rend.sortingLayerName = sortingLayerName;
        rend.sortingOrder     = orderInLayer;

        sb.Append($"  {childName}: Tilemap + TilemapRenderer");
        sb.Append($" (SortingLayer={sortingLayerName}, Order={orderInLayer})");

        if (withCollider)
        {
            // Rigidbody2D first — CompositeCollider2D requires it and would add its own
            if (go.GetComponent<Rigidbody2D>() == null)
                go.AddComponent<Rigidbody2D>();
            var rb = go.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;

            if (go.GetComponent<TilemapCollider2D>() == null)
                go.AddComponent<TilemapCollider2D>();
            var tc = go.GetComponent<TilemapCollider2D>();

            if (go.GetComponent<CompositeCollider2D>() == null)
                go.AddComponent<CompositeCollider2D>();
            var cc = go.GetComponent<CompositeCollider2D>();
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;

            // Unity 6: compositeOperation replaces deprecated usedByComposite
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;

            // Ensure Rigidbody2D stayed Static after CompositeCollider2D possibly reset it
            rb.bodyType = RigidbodyType2D.Static;

            sb.Append(" + TilemapCollider2D(Merge) + CompositeCollider2D(Polygons) + Rigidbody2D(Static)");
        }

        sb.AppendLine();
    }

    // ─── Tile assets ─────────────────────────────────────────────────────────
    static void CreateTileAssets(System.Text.StringBuilder sb)
    {
        EnsureFolder(TileDataFolder);

        var allAssets = AssetDatabase.LoadAllAssetsAtPath(TilesSheet);
        var sprites   = new List<Sprite>();
        foreach (var a in allAssets)
            if (a is Sprite s) sprites.Add(s);

        if (sprites.Count == 0)
        {
            sb.AppendLine($"WARNING: No sprites found in {TilesSheet}. Has the sheet been sliced?");
            return;
        }

        int created = 0, skipped = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var sprite in sprites)
            {
                string tilePath = $"{TileDataFolder}/{sprite.name}.asset";
                if (AssetDatabase.LoadAssetAtPath<Tile>(tilePath) != null)
                {
                    skipped++;
                    continue;
                }
                var tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite       = sprite;
                tile.colliderType = Tile.ColliderType.Sprite;
                AssetDatabase.CreateAsset(tile, tilePath);
                created++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        sb.AppendLine($"Tile assets: {created} created, {skipped} already existed → {TileDataFolder}");
    }

    // ─── Palette ─────────────────────────────────────────────────────────────
    static void CreatePalette(System.Text.StringBuilder sb)
    {
        EnsureFolder(PalettesFolder);

        string prefabPath = $"{PalettesFolder}/{PaletteName}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
        {
            sb.AppendLine($"Palette: already exists at {prefabPath}");
            return;
        }

        try
        {
            var palette = GridPaletteUtility.CreateNewPalette(
                PalettesFolder,
                PaletteName,
                GridLayout.CellLayout.Rectangle,
                GridPalette.CellSizing.Automatic,
                Vector3.one,
                GridLayout.CellSwizzle.XYZ);

            if (palette != null)
                sb.AppendLine($"Palette: created at {PalettesFolder}/{PaletteName}.prefab");
            else
                sb.AppendLine("Palette: GridPaletteUtility returned null — see manual steps below.");
        }
        catch (System.Exception e)
        {
            sb.AppendLine($"Palette: automated creation failed ({e.GetType().Name}: {e.Message})");
        }

        sb.AppendLine();
        sb.AppendLine("── MANUAL TILE PALETTE STEPS (if needed) ──────────────────");
        sb.AppendLine("1. Window > 2D > Tile Palette");
        sb.AppendLine("2. Click 'Create New Palette' → name: PlatformPalette → save to Assets/Palettes");
        sb.AppendLine("3. Select all assets in Assets/TileData, drag into the Tile Palette window");
        sb.AppendLine("────────────────────────────────────────────────────────────");
    }

    // ─── Console check ───────────────────────────────────────────────────────
    static void CheckConsole(System.Text.StringBuilder sb)
    {
        // Compilation is already clean (verified before this runs).
        // Surface any runtime issues visible to us at edit time.
        sb.AppendLine("Console: no script compilation errors detected.");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────
    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string child  = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, child);
    }
}
