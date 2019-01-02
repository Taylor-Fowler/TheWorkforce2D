using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TheWorkforce.Crafting;
using UnityEngine;
using TheWorkforce.World;
using TheWorkforce.Items;

namespace TheWorkforce.Static_Classes
{
    public static class AssetProcessor
    {
        // TODO: Add a method that returns the directoryInfo requested only if the directory exists
        //       More error checking and handling!

        #region Private Members
        private const string BasePath = "Assets/Resources/";
        private static readonly Vector2 _texturePivot = new Vector2(0.5f, 0.5f);
        #endregion

        #region TileSet Loading
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
                
                // NOTE: Commented logging of Tile Precedence
                // Debug.Log(tileSetSettings.Id + " Precedence: " + tileSetSettings.Precedence);
            }

            return tileSets.ToArray();
        }
        #endregion

        #region Item Loading
        /// <summary>
        /// Loads all of the different item types into the item manager
        /// </summary>
        public static void LoadItems(ItemManager itemManager)
        {
            //LoadCraftingComponents(itemManager);
            LoadRawMaterials(itemManager);
        }

        private static void LoadCraftingComponents(ItemManager itemManager)
        {
            //DirectoryInfo[] itemDirectories = new DirectoryInfo(BasePath + "Items/Crafting Components").GetDirectories();
            
            //foreach (var directory in itemDirectories)
            //{
            //    var settingsFileContents = GetJsonFile(directory);

            //    ItemSettings itemSettings = JsonConvert.DeserializeObject<ItemSettings>(settingsFileContents);
            //    FileInfo textureFile = directory.GetFiles("*.png")[0];
                
            //    CraftingComponent craftingComponent = new CraftingComponent();
            //    craftingComponent.InitialiseItem(itemSettings.Id, itemSettings.Name, itemSettings.Description, LoadSpriteFromTexture(LoadTexture(textureFile)), itemSettings.MaxStackSize);
                
            //    itemManager.Add(craftingComponent);
            //}
        }

        private static void LoadRawMaterials(ItemManager itemManager)
        {
            DirectoryInfo[] itemDirectories = new DirectoryInfo(BasePath + "Items/Raw Materials").GetDirectories();
            
            foreach (var directory in itemDirectories)
            {
                var settingsFileContents = GetJsonFile(directory);
                JObject jObject = JObject.Parse(settingsFileContents);
                RawMaterial rawMaterial = new RawMaterial();

                bool success = true;

                foreach(var entityProperty in jObject.Properties())
                {
                    if (entityProperty.Name == typeof(Item).Name)
                    {
                        InitialiseItem(rawMaterial, entityProperty, directory.GetFiles("*.png")[0]);
                    }
                    else
                    {
                        success = InitialiseProperty(entityProperty, rawMaterial);
                        if(!success)
                        {
                            Debug.Log("[AssetProcessor] - LoadRawMaterials(ItemManager) \n"
                                    + "Error Loading Item Property: " + entityProperty.Name);
                            break;
                        }
                    }
                }

                if(success)
                {
                    itemManager.Add(rawMaterial);
                }
            }
        }

        private static void InitialiseItem(Item item, JProperty itemData, FileInfo textureFile)
        {
            Sprite sprite = LoadSpriteFromTexture(LoadTexture(textureFile), Tile.PX_SIZE * 2, Tile.PX_SIZE * 2, Tile.PX_SIZE * 2);
            item.InitialiseItem
                (
                    (int)itemData.Value["Id"],
                    (string)itemData.Value["Name"],
                    (string)itemData.Value["Description"],
                    (int)itemData.Value["ItemType"],
                    sprite,
                    (int)itemData.Value["MaxStackSize"]
                );
        }

        //private static T CreateProperty<T>() where T : class
        //{
        //    return Activator.CreateInstance(typeof(T)) as T;
        //}

        private static bool InitialiseProperty(JProperty propertyData, RawMaterial rawMaterial)
        {
            if(propertyData.Name == typeof(BaseGenerationSettings).Name)
            {
                InitialiseProperty(new BaseGenerationSettings(), propertyData, rawMaterial);
            }
            else if (propertyData.Name == typeof(BaseHarvestSettings).Name)
            {
                InitialiseProperty(new BaseHarvestSettings(), propertyData, rawMaterial);
            }
            else
            {
                return false;
            }

            return true;
        }

        private static void InitialiseProperty(BaseGenerationSettings type, JProperty propertyData, IGeneratable generatable)
        {
            type.InitialiseGenerationSettings
                (
                    (float)propertyData.Value["MaximumMoisture"],
                    (float)propertyData.Value["MinimumMoisture"],
                    (float)propertyData.Value["MaximumElevation"],
                    (float)propertyData.Value["MinimumElevation"]
                );

            generatable.SetGenerationSettings(type);
        }

        private static void InitialiseProperty(BaseHarvestSettings type, JProperty propertyData, IHarvestable harvestable)
        {
            type.InitialiseHarvestSettings
                (
                    (EToolType)(int)propertyData.Value["HarvestTool"],
                    (float)propertyData.Value["Strength"],
                    (float)propertyData.Value["BaseCapacity"],
                    (float)propertyData.Value["CapacityModifier"]
                );

            harvestable.SetHarvestSettings(type);
        }

        //private static void InitialiseProperty<BaseGenerationSettings>(JProperty propertyData, IGeneratable generatable) 
        //{

        //}

        private static RawMaterial InitialiseRawMaterial()
        {
            return null;
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

        #region EToolTypeSprite Loading
        public static void LoadEToolTypeSprites()
        {
            const string path = BasePath + "UI/ToolType";
            DirectoryInfo directory = new DirectoryInfo(path);

            List<Sprite> sprites = new List<Sprite>();

            foreach(var spriteName in Enum.GetNames(typeof(EToolType)))
            {
                FileInfo textureFile = directory.GetFiles(spriteName + ".png")[0];
                Sprite sprite = LoadSpriteFromTexture(LoadTexture(textureFile), Tile.PX_SIZE * 2, Tile.PX_SIZE * 2, Tile.PX_SIZE * 2);
                sprites.Add(sprite);
            }

            AssetProvider.InitialiseEToolTypeSprites(sprites.ToArray());
        }
        #endregion

        #region Sprite & Texture Utility Functions
        private static Sprite LoadSpriteFromTexture(Texture2D texture, int width = Tile.PX_SIZE, int height = Tile.PX_SIZE, int pixelsPerUnit = Tile.PX_SIZE, int x = 0, int y = 0)
        {
            return Sprite.Create(texture, new Rect(x, y, width, height), _texturePivot, pixelsPerUnit, 0, SpriteMeshType.FullRect);
        }

        private static Texture2D LoadTexture(FileInfo textureFile, int width = Tile.PX_SIZE, int height = Tile.PX_SIZE)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.LoadImage(File.ReadAllBytes(textureFile.FullName));
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            return texture;
        }
        #endregion


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
