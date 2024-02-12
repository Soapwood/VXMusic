using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace UnityEditor.Tilemaps
{
    internal static class TileDragAndDrop
    {
        private enum UserTileCreationMode
        {
            Overwrite,
            CreateUnique,
            Reuse,
        }

        private static readonly string k_TileExtension = "asset";

        private static List<Sprite> GetSpritesFromTexture(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            List<Sprite> sprites = new List<Sprite>();

            foreach (Object asset in assets)
            {
                if (asset is Sprite)
                {
                    sprites.Add(asset as Sprite);
                }
            }

            return sprites;
        }

        private static bool AllSpritesAreSameSize(List<Sprite> sprites)
        {
            if (!sprites.Any())
            {
                return false;
            }

            // If sprites are different sizes (not grid sliced). So we abort.
            for (int i = 1; i < sprites.Count - 1; i++)
            {
                if ((int)sprites[i].rect.width != (int)sprites[i + 1].rect.width ||
                    (int)sprites[i].rect.height != (int)sprites[i + 1].rect.height)
                {
                    return false;
                }
            }
            return true;
        }

        // Input:
        // sheetTextures -> textures containing 2-N equal sized Sprites)
        // singleSprites -> All the leftover Sprites that were in same texture but different sizes or just dragged in as Sprite
        // tiles -> Just plain tiles
        public static Dictionary<Vector2Int, Object> CreateHoverData(List<Texture2D> sheetTextures, List<Sprite> singleSprites, List<TileBase> tiles)
        {
            Dictionary<Vector2Int, Object> result = new Dictionary<Vector2Int, Object>();

            Vector2Int currentPosition = new Vector2Int(0, 0);
            int width = 0;

            if (sheetTextures != null)
            {
                foreach (Texture2D sheetTexture in sheetTextures)
                {
                    Dictionary<Vector2Int, Object> sheet = CreateHoverData(sheetTexture);
                    foreach (KeyValuePair<Vector2Int, Object> item in sheet)
                    {
                        result.Add(item.Key + currentPosition, item.Value);
                    }
                    Vector2Int min = GetMinMaxRect(sheet.Keys.ToList()).min;
                    currentPosition += new Vector2Int(0, min.y - 1);
                }
            }
            if (currentPosition.x > 0)
                currentPosition = new Vector2Int(0, currentPosition.y - 1);

            if (singleSprites != null)
            {
                width = Mathf.RoundToInt(Mathf.Sqrt(singleSprites.Count));
                foreach (Sprite sprite in singleSprites)
                {
                    result.Add(currentPosition, sprite);
                    currentPosition += new Vector2Int(1, 0);
                    if (currentPosition.x >= width)
                        currentPosition = new Vector2Int(0, currentPosition.y - 1);
                }
            }
            if (currentPosition.x > 0)
                currentPosition = new Vector2Int(0, currentPosition.y - 1);

            if (tiles != null)
            {
                width = Math.Max(Mathf.RoundToInt(Mathf.Sqrt(tiles.Count)), width);
                foreach (TileBase tile in tiles)
                {
                    result.Add(currentPosition, tile);
                    currentPosition += new Vector2Int(1, 0);
                    if (currentPosition.x >= width)
                        currentPosition = new Vector2Int(0, currentPosition.y - 1);
                }
            }

            return result;
        }

        // Get all textures that are valid spritesheets. More than one Sprites and all equal size.
        public static List<Texture2D> GetValidSpritesheets(Object[] objects)
        {
            List<Texture2D> result = new List<Texture2D>();
            foreach (Object obj in objects)
            {
                if (obj is Texture2D)
                {
                    Texture2D texture = obj as Texture2D;
                    List<Sprite> sprites = GetSpritesFromTexture(texture);
                    if (sprites.Count() > 1 && AllSpritesAreSameSize(sprites))
                    {
                        result.Add(texture);
                    }
                }
            }
            return result;
        }

        // Get all single Sprite(s) and all Sprite(s) that are part of Texture2D that is not valid sheet (it sprites of varying sizes)
        public static List<Sprite> GetValidSingleSprites(Object[] objects)
        {
            List<Sprite> result = new List<Sprite>();
            foreach (Object obj in objects)
            {
                if (obj is Sprite)
                {
                    result.Add(obj as Sprite);
                }
                else if (obj is Texture2D)
                {
                    Texture2D texture = obj as Texture2D;
                    List<Sprite> sprites = GetSpritesFromTexture(texture);
                    if (sprites.Count == 1 || !AllSpritesAreSameSize(sprites))
                    {
                        result.AddRange(sprites);
                    }
                }
            }
            return result;
        }

        public static List<TileBase> GetValidTiles(Object[] objects)
        {
            List<TileBase> result = new List<TileBase>();
            foreach (Object obj in objects)
            {
                if (obj is TileBase)
                {
                    result.Add(obj as TileBase);
                }
            }
            return result;
        }

        private static Vector2Int GetMinimum(List<Sprite> sprites, Func<Sprite, float> minX, Func<Sprite, float> minY)
        {
            Vector2 minVector = new Vector2(Int32.MaxValue, Int32.MaxValue);
            foreach (var sprite in sprites)
            {
                minVector.x = Mathf.Min(minVector.x, minX(sprite));
                minVector.y = Mathf.Min(minVector.y, minY(sprite));
            }
            return Vector2Int.FloorToInt(minVector);
        }

        public static Vector2Int EstimateGridPixelSize(List<Sprite> sprites)
        {
            if (sprites.Count == 0 || sprites.Any(sprite => sprite == null))
            {
                return Vector2Int.zero;
            }

            if (sprites.Count == 1)
                return Vector2Int.FloorToInt(sprites[0].rect.size);

            return GetMinimum(sprites, s => s.rect.width, s => s.rect.height);
        }

        public static Vector2Int EstimateGridOffsetSize(List<Sprite> sprites)
        {
            if (sprites.Count == 0 || sprites.Any(sprite => sprite == null))
                return Vector2Int.zero;

            if (sprites.Count == 1)
                return Vector2Int.FloorToInt(sprites[0].rect.position);

            return GetMinimum(sprites, s => s.rect.xMin, s => s.rect.yMin);
        }

        public static Vector2Int EstimateGridPaddingSize(List<Sprite> sprites, Vector2Int cellSize, Vector2Int offsetSize)
        {
            if (sprites.Count < 2 || sprites.Any(sprite => sprite == null))
                return Vector2Int.zero;

            var paddingSize = GetMinimum(sprites
                , (s =>
                {
                    var xMin = s.rect.xMin - cellSize.x - offsetSize.x;
                    return xMin >= 0 ? xMin : Int32.MaxValue;
                })
                , (s =>
                {
                    var yMin = s.rect.yMin - cellSize.y - offsetSize.y;
                    return yMin >= 0 ? yMin : Int32.MaxValue;
                })
            );

            // Assume there is no padding if the detected padding is greater than the cell size
            if (paddingSize.x >= cellSize.x)
                paddingSize.x = 0;
            if (paddingSize.y >= cellSize.y)
                paddingSize.y = 0;
            return paddingSize;
        }

        // Turn texture pixel position into integer grid position based on cell size, offset size and padding
        private static Vector2Int GetGridPosition(Sprite sprite, Vector2Int cellPixelSize, Vector2Int offsetSize, Vector2Int paddingSize)
        {
            return new Vector2Int(
                Mathf.FloorToInt((sprite.rect.center.x - offsetSize.x) / (cellPixelSize.x + paddingSize.x)),
                Mathf.FloorToInt(-(sprite.texture.height - sprite.rect.center.y - offsetSize.y) / (cellPixelSize.y + paddingSize.y)) + 1
            );
        }

        // Organizes all the sprites in a single texture nicely on a 2D "table" based on their original texture position
        // Only call this with spritesheet with all Sprites equal size
        public static Dictionary<Vector2Int, Object> CreateHoverData(Texture2D sheet)
        {
            Dictionary<Vector2Int, Object> result = new Dictionary<Vector2Int, Object>();
            List<Sprite> sprites = GetSpritesFromTexture(sheet);
            Vector2Int cellPixelSize = EstimateGridPixelSize(sprites);

            // Get Offset
            Vector2Int offsetSize = EstimateGridOffsetSize(sprites);

            // Get Padding
            Vector2Int paddingSize = EstimateGridPaddingSize(sprites, cellPixelSize, offsetSize);

            foreach (Sprite sprite in sprites)
            {
                Vector2Int position = GetGridPosition(sprite, cellPixelSize, offsetSize, paddingSize);
                result[position] = sprite;
            }

            return result;
        }

        public static Dictionary<Vector2Int, TileBase> ConvertToTileSheet(Dictionary<Vector2Int, Object> sheet)
        {
            Dictionary<Vector2Int, TileBase> result = new Dictionary<Vector2Int, TileBase>();

            string defaultPath = ProjectBrowser.s_LastInteractedProjectBrowser
                ? ProjectBrowser.s_LastInteractedProjectBrowser.GetActiveFolderPath()
                : "Assets";

            // Early out if all objects are already tiles
            if (sheet.Values.ToList().FindAll(obj => obj is TileBase).Count == sheet.Values.Count)
            {
                foreach (KeyValuePair<Vector2Int, Object> item in sheet)
                {
                    result.Add(item.Key, item.Value as TileBase);
                }
                return result;
            }

            UserTileCreationMode userTileCreationMode = UserTileCreationMode.Overwrite;
            string path = "";
            bool multipleTiles = sheet.Count > 1;
            if (multipleTiles)
            {
                bool userInterventionRequired = false;
                path = EditorUtility.SaveFolderPanel("Generate tiles into folder ", defaultPath, "");
                path = FileUtil.GetProjectRelativePath(path);

                // Check if this will overwrite any existing assets
                foreach (var item in sheet.Values)
                {
                    if (item is Sprite)
                    {
                        var tilePath = FileUtil.CombinePaths(path, String.Format("{0}.{1}", item.name, k_TileExtension));
                        if (File.Exists(tilePath))
                        {
                            userInterventionRequired = true;
                            break;
                        }
                    }
                }
                // There are existing tile assets in the folder with names matching the items to be created
                if (userInterventionRequired)
                {
                    var option = EditorUtility.DisplayDialogComplex("Overwrite?", String.Format("Assets exist at {0}. Do you wish to overwrite existing assets?", path), "Overwrite", "Create New Copy", "Reuse");
                    switch (option)
                    {
                        case 0: // Overwrite
                        {
                            userTileCreationMode = UserTileCreationMode.Overwrite;
                        }
                        break;
                        case 1: // Create New Copy
                        {
                            userTileCreationMode = UserTileCreationMode.CreateUnique;
                        }
                        break;
                        case 2: // Reuse
                        {
                            userTileCreationMode = UserTileCreationMode.Reuse;
                        }
                        break;
                    }
                }
            }
            else
            {
                // Do not check if this will overwrite new tile as user has explicitly selected the file to save to
                path = EditorUtility.SaveFilePanelInProject("Generate new tile", sheet.Values.First().name, k_TileExtension, "Generate new tile", defaultPath);
            }

            if (string.IsNullOrEmpty(path))
                return result;

            int i = 0;
            EditorUtility.DisplayProgressBar("Generating Tile Assets (" + i + "/" + sheet.Count + ")", "Generating tiles", 0f);

            try
            {
                MethodInfo createTileMethod = GridPaintActiveTargetsPreferences.GetCreateTileFromPaletteUsingPreferences();
                if (createTileMethod == null)
                    return null;

                foreach (KeyValuePair<Vector2Int, Object> item in sheet)
                {
                    TileBase tile;
                    string tilePath = "";
                    if (item.Value is Sprite)
                    {
                        tile = createTileMethod.Invoke(null, new object[] { item.Value as Sprite }) as TileBase;
                        if (tile == null)
                            continue;

                        tilePath = multipleTiles
                            ? FileUtil.CombinePaths(path, String.Format("{0}.{1}", tile.name, k_TileExtension))
                            : path;
                        // Case 1216101: Fix path slashes for Windows
                        tilePath = FileUtil.NiceWinPath(tilePath);
                        switch (userTileCreationMode)
                        {
                            case UserTileCreationMode.CreateUnique:
                            {
                                if (File.Exists(tilePath))
                                    tilePath = AssetDatabase.GenerateUniqueAssetPath(tilePath);
                                AssetDatabase.CreateAsset(tile, tilePath);
                            }
                            break;
                            case UserTileCreationMode.Overwrite:
                            {
                                AssetDatabase.CreateAsset(tile, tilePath);
                            }
                            break;
                            case UserTileCreationMode.Reuse:
                            {
                                if (File.Exists(tilePath))
                                    tile = AssetDatabase.LoadAssetAtPath<TileBase>(tilePath);
                                else
                                    AssetDatabase.CreateAsset(tile, tilePath);
                            }
                            break;
                        }
                    }
                    else
                    {
                        tile = item.Value as TileBase;
                    }
                    EditorUtility.DisplayProgressBar("Generating Tile Assets (" + i + "/" + sheet.Count + ")", "Generating " + tilePath, (float)i++ / sheet.Count);
                    result.Add(item.Key, tile);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.Refresh();
            return result;
        }

        internal static RectInt GetMinMaxRect(List<Vector2Int> positions)
        {
            if (positions == null || positions.Count == 0)
                return new RectInt();

            return GridEditorUtility.GetMarqueeRect(
                new Vector2Int(positions.Min(p1 => p1.x), positions.Min(p1 => p1.y)),
                new Vector2Int(positions.Max(p1 => p1.x), positions.Max(p1 => p1.y))
            );
        }
    }
}
