using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Testing
{
    public class TestMemoryInitialisation : MonoBehaviour
    {
        public bool UseMonobehaviours = true;
        public World World;
        public WorldGeneration WorldGeneration;
        public GameObject WorldGameObject;


        private void Awake()
        {
            TerrainTileSet.InitialiseTileSets();
            World = new World(784893570);
            WorldGeneration = new WorldGeneration(World.Seed, World.NegativeXSeed, World.NegativeYSeed);
            WorldGameObject = new GameObject("WorldGameObject");

            if(UseMonobehaviours)
            {
                InitialiseMonos();
            }
            else
            {
                InitialiseNonMonos();
            }
        }

        private void InitialiseMonos()
        {
            List<Vector2> ChunkPositions = new List<Vector2>();
            List<ChunkController> ChunkControllers = new List<ChunkController>();

            for (int x = 0; x < 20; ++x)
            {
                for (int z = 0; z < 20; ++z)
                {
                    ChunkPositions.Add(new Vector2(x, z));
                    var chunkController = new GameObject("Chunk Controller: " + x + ", " + z).AddComponent<ChunkController>();
                    chunkController.transform.SetParent(WorldGameObject.transform);
                    ChunkControllers.Add(chunkController);
                }
            }
            var Chunks = WorldGeneration.GenerateChunks(ChunkPositions);

            for (int i = 0; i < ChunkControllers.Count; ++i)
            {
                ChunkControllers[i].SetChunk(Chunks[i], World);
            }
        }

        private void InitialiseNonMonos()
        {
            List<Vector2> ChunkPositions = new List<Vector2>();
            List<ChunkController> ChunkControllers = new List<ChunkController>();

            for (int x = 0; x < 25; ++x)
            {
                for (int z = 0; z < 25; ++z)
                {
                    ChunkPositions.Add(new Vector2(x, z));
                }
            }
            var Chunks = WorldGeneration.GenerateChunks(ChunkPositions);

            for (int x = 0; x < 5; x++)
            {
                for (int z = 0; z < 5; z++)
                {
                    var chunkController = new GameObject("Chunk Controller: " + x + ", " + z).AddComponent<ChunkController>();
                    chunkController.transform.SetParent(WorldGameObject.transform);
                    ChunkControllers.Add(chunkController);
                    //chunkController.SetChunk(Chunks[10 + x * 20 + 10 + z], World);
                    //chunkController.SetChunk(Chunks[10 + x + (z * 25)], World);
                    chunkController.SetChunk(Chunks[x + (z * 25)], World);
                }
            }
        }
    }
}
