using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TheWorkforce.Crafting;
using UnityEngine;
using TheWorkforce.World;
using TheWorkforce.Items;
using UnityEngine.Experimental.UIElements;

namespace TheWorkforce.StaticClasses
{
    public static class AssetProcessor
    {
        #region Private Members
        private static Vector2 _texturePivot = new Vector2(0.5f, 0.5f);
        private const string BasePath = "Assets/Resources/";
        #endregion
        
        public static TerrainTileSet[] LoadTerrainTileSets()
        {
            const string path = BasePath + "Terrain/TileSets";

            DirectoryInfo[] tileSetDirectories = new DirectoryInfo(path).GetDirectories();
            List<TerrainTileSet> tileSets = new List<TerrainTileSet>();

            foreach (var directory in tileSetDirectories)
            {
                var settingsFileContents = GetJsonFile(directory);
                
                TerrainTileSetSettings tileSetSettings = JsonConvert.DeserializeObject<TerrainTileSetSettings>(settingsFileContents);
                FileInfo textureFile = directory.GetFiles("*.png")[0];
                Texture2D texture = LoadTexture(textureFile, Tile.PX_SIZE * 6, Tile.PX_SIZE * 6);

                tileSets.Add(new TerrainTileSet(texture, Tile.PX_SIZE, Tile.PX_SIZE, tileSetSettings));
                
                Debug.Log(tileSetSettings.Id + " Precedence: " + tileSetSettings.Precedence);
            }

            return tileSets.ToArray();
        }


        #region Item Loading
        public static IEnumerable<IItem> LoadItems()
        {
            List<IItem> items = new List<IItem>();
            
            LoadCraftingComponents(items);
            LoadRawMaterials(items);

            return items;
        }

        private static void LoadCraftingComponents(ICollection<IItem> items)
        {
            DirectoryInfo[] itemDirectories = new DirectoryInfo(BasePath + "Items/Crafting Components").GetDirectories();
            
            foreach (var directory in itemDirectories)
            {
                var settingsFileContents = GetJsonFile(directory);

                ItemSettings itemSettings = JsonConvert.DeserializeObject<ItemSettings>(settingsFileContents);
                FileInfo textureFile = directory.GetFiles("*.png")[0];
                
                CraftingComponent craftingComponent = new CraftingComponent();
                craftingComponent.InitialiseItem(itemSettings.Id, itemSettings.Name, itemSettings.Description, LoadSpriteFromTexture(LoadTexture(textureFile)), itemSettings.MaxStackSize);
                
                items.Add(craftingComponent);
            }
        }

        private static void LoadRawMaterials(ICollection<IItem> items)
        {
            DirectoryInfo[] itemDirectories = new DirectoryInfo(BasePath + "Items/Raw Materials").GetDirectories();
            
            foreach (var directory in itemDirectories)
            {
                var settingsFileContents = GetJsonFile(directory);

                RawMaterialSettings rawMaterialSettings = JsonConvert.DeserializeObject<RawMaterialSettings>(settingsFileContents);
                FileInfo textureFile = directory.GetFiles("*.png")[0];
                
                RawMaterial rawMaterial = new RawMaterial();
                rawMaterial.InitialiseItem(rawMaterialSettings.Id, rawMaterialSettings.Name, rawMaterialSettings.Description, LoadSpriteFromTexture(LoadTexture(textureFile)), rawMaterialSettings.MaxStackSize);
                rawMaterial.InitialiseHarvestRequirements((EToolType)rawMaterialSettings.HarvestTool, rawMaterialSettings.HarvestSpeed, rawMaterialSettings.HarvestAmount);
                
                items.Add(rawMaterial);
            }
        }
        
        #endregion
        
        #region Recipe Loading
        public static IEnumerable<CraftingRecipe> LoadCraftingRecipes()
        {
            List<CraftingRecipe> recipes = new List<CraftingRecipe>();
            
            DirectoryInfo[] itemDirectories = new DirectoryInfo(BasePath + "Items/").GetDirectories();
            foreach (var subDirectory in itemDirectories)
            {
                if (subDirectory.Name == "Raw Materials")
                {
                    continue;
                }
                
                foreach (var directory in subDirectory.GetDirectories())
                {
                    var recipeFileContents = GetJsonFile(directory, "recipe");

                    CraftingRecipeSettings recipeSettings =
                        JsonConvert.DeserializeObject<CraftingRecipeSettings>(recipeFileContents);
                    
                    CraftingRecipe recipe = new CraftingRecipe(recipeSettings.Id, recipeSettings.ItemsRequired, recipeSettings.ItemsProduced);
                    
                    recipes.Add(recipe);
                }
            }

            return recipes;
        }
        #endregion

        private static Sprite LoadSpriteFromTexture(Texture2D texture, int width = Tile.PX_SIZE, int height = Tile.PX_SIZE, int x = 0, int y = 0)
        {
            return Sprite.Create(texture, new Rect(x, y, width, height), _texturePivot);
        }

        private static Texture2D LoadTexture(FileInfo textureFile, int width = Tile.PX_SIZE, int height = Tile.PX_SIZE)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(File.ReadAllBytes(textureFile.FullName));
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return texture;
        }

        private static string GetJsonFile(DirectoryInfo directory, string fileName = "settings")
        {
            FileInfo fileInfo = directory.GetFiles(fileName + ".json")[0];
            string fileContents;

            using (var reader = new StreamReader(fileInfo.OpenRead()))
            {
                fileContents = reader.ReadToEnd();
            }

            return fileContents;
        }
    }
}
