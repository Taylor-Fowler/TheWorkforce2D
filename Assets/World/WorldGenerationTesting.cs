// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Networking;

// public class LocalTilesMessage : MessageBase
// {
//     public int[] Tiles;
// }

// public class WorldGenerationTesting : NetworkBehaviour
// {
//     public int Width;
//     public int Height;
//     public Texture2D[] TilesetTextures;

//     public TerrainTileset[] Tilesets;

//     public Tile[,] Tiles;

//     private void Awake()
//     {
// 	    this.Tiles = new Tile[this.Width, this.Height];
//         this.Tilesets = new TerrainTileset[this.TilesetTextures.Length];

//         for(int i = 0; i < this.TilesetTextures.Length; i++)
//         {
//             this.Tilesets[i] = new TerrainTileset(this.TilesetTextures[i], 32, 32, (1 - i) * 0.05f, i);
//         }
//     }

//     public override void OnStartServer()
//     {
//         base.OnStartServer();
//         this.SpawnBaseTiles();
//         this.SpawnPadding();
//     }

//     public override void OnStartLocalPlayer()
//     {
//         base.OnStartLocalPlayer();
//         if(!this.isServer)
//         {
//             CmdGetWorld();
//             Debug.Log("playyer");
//         }
//         else
//         {
//             Debug.Log("server");
//         }
//     }

//     [TargetRpc]
//     public void TargetReceiveWorld(NetworkConnection target, LocalTilesMessage message)
//     {
//         this.RecreateBaseTiles(message.Tiles);
//         this.SpawnPadding();
//     }

//     [Command]
//     public void CmdGetWorld()
//     {
//         int[] tileIDs = new int[this.Width * this.Height];
//         for(int x = 0; x < this.Width; x++)
//             for(int y = 0; y < this.Height; y++)
//             {
//                 tileIDs[x * this.Width + y] = this.Tiles[x, y].TilesetID;
//             }

//         TargetReceiveWorld(this.connectionToClient, new LocalTilesMessage { Tiles = tileIDs });
//     }

//     private void SpawnBaseTiles()
//     {
// 	    int halfWidth = (int)(this.Width * 0.5f);
// 	    int halfHeight = (int)(this.Height * 0.5f);
	
// 	    for(int x = 0; x < this.Width; x++)
// 	    {
// 	        for(int y = 0; y < this.Height; y++)
// 	        {
// 		        GameObject go = new GameObject();
// 		        go.transform.position = new Vector3(-halfWidth + x, -halfHeight + y, 0.1f);
// 		        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
// 		        renderer.sprite = this.Type(x, y);
// 		        this.Tiles[x, y] = go.AddComponent<Tile>();
//                 this.Tiles[x, y].TilesetID = this.IsStone(this.GetNoise(x, y)) ? this.Tilesets[1].ID : this.Tilesets[0].ID;
// 	        }
// 	    }
//     }

//     private void RecreateBaseTiles(int[] tileIDs)
//     {
//         int halfWidth = (int)(this.Width * 0.5f);
//         int halfHeight = (int)(this.Height * 0.5f);

//         for (int x = 0; x < this.Width; x++)
//             for(int y = 0; y < this.Height; y++)
//             {
//                 GameObject go = new GameObject();
//                 SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();

//                 go.transform.position = new Vector3(-halfWidth + x, -halfHeight + y, 0.1f);
//                 renderer.sprite = TerrainTileset.GetTileset(tileIDs[x * this.Width + y]).Tiles[TerrainTileset.CENTRAL];
//                 this.Tiles[x, y] = go.AddComponent<Tile>();
//                 this.Tiles[x, y].TilesetID = tileIDs[x * this.Width + y];
//             }
//     }

//     private void SpawnPadding()
//     {
//         Dictionary<TerrainTileset, TilePadding> surroundingTilePadding = new Dictionary<TerrainTileset, TilePadding>();

//         int xPosition, yPosition;

//         for (int x = 0; x < this.Width; x++)
//         {
//             for (int y = 0; y < this.Height; y++)
//             {
//                 // top to bottom
//                 for (int yOffset = 1; yOffset >= -1; yOffset--)
//                 {
//                     yPosition = y + yOffset;
//                     if (yPosition < 0)
//                         continue;
//                     if (yPosition >= this.Height)
//                         continue;

//                     // go from left to right
//                     for (int xOffset = -1; xOffset <= 1; xOffset++)
//                     {
//                         // the actual position of the neighbouring tile
//                         xPosition = x + xOffset;
//                         // if the neighbouring tile isnt loaded, go to the next tile
//                         if (xPosition < 0)
//                             continue;
//                         if (xPosition >= this.Width)
//                             break;
//                         // the tile we're interested in, so skip it
//                         if (xOffset == 0 && yOffset == 0)
//                             continue;

//                         TerrainTileset neighbour = TerrainTileset.GetTileset(this.Tiles[xPosition, yPosition].TilesetID);
//                         TerrainTileset current = TerrainTileset.GetTileset(this.Tiles[x, y].TilesetID);

//                         if (neighbour != current && neighbour.Precedence > current.Precedence)
//                         {
//                             if (!surroundingTilePadding.ContainsKey(neighbour))
//                             {
//                                 surroundingTilePadding.Add(neighbour, new TilePadding());
//                             }
//                             surroundingTilePadding[neighbour].Enable(xOffset, yOffset);
//                         }
//                     }
//                 }
//                 // Instantiate the padding
//                 foreach (var pair in surroundingTilePadding)
//                 {
//                     Sprite[] sprites = pair.Key.GetSprites(pair.Value);

//                     foreach (var sprite in sprites)
//                     {
//                         GameObject gameObject = new GameObject();
//                         gameObject.transform.SetParent(this.Tiles[x, y].transform);
//                         gameObject.transform.position = this.Tiles[x, y].transform.position + new Vector3(0, 0, -pair.Key.Precedence);
//                         gameObject.AddComponent<SpriteRenderer>().sprite = sprite;
//                     }
//                 }

//                 surroundingTilePadding.Clear();
//             }
//         }
//     }

    

//     private bool IsStone(float noise)
//     {
// 	    return noise < 0.5f;
//     }

//     private Sprite Type(int x, int y)
//     {
// 	    float noise = this.GetNoise(x, y);

// 	    if(this.IsStone(noise)) return this.Tilesets[1].Tiles[TerrainTileset.CENTRAL];
// 	    return this.Tilesets[0].Tiles[TerrainTileset.CENTRAL];
//     }

//     private float GetNoise(int x, int y)
//     {
// 	    return Mathf.PerlinNoise((float)x * 1.6f, (float)y * 1.6f);
//     }
// }
