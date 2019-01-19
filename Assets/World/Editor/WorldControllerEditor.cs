using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace TheWorkforce
{
    [CustomEditor(typeof(WorldController))]
    public class WorldControllerEditor : Editor
    {
        private int _selectedIndex = -1;
        private GUIStyle _buttonStyle;

        public override void OnInspectorGUI()
        {
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fixedHeight = 50
            };


            WorldController worldController = target as WorldController;
            var chunkControllers = GetControllersOrdered(worldController.ChunkControllers);

            GUILayout.BeginVertical("Box");
            _selectedIndex = GUILayout.SelectionGrid(_selectedIndex, ChunkController.ChunkPositionStrings(chunkControllers).ToArray(), Chunk.KEEP_LOADED, _buttonStyle);
            GUILayout.EndVertical();

            if (_selectedIndex != -1)
            {
                GUILayout.BeginVertical("Box");
                ChunkController selected = chunkControllers[_selectedIndex];
                Editor editor = CreateEditor(selected);
                editor.OnInspectorGUI();
                GUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private ChunkController[,] GetControllersAsGrid(IEnumerable<ChunkController> chunkControllers)
        {
            ChunkController[,] grid = new ChunkController[Chunk.KEEP_LOADED, Chunk.KEEP_LOADED];

            List<ChunkController> ccs = new List<ChunkController>(chunkControllers);
            ccs.Sort((cc1, cc2) =>
            {
                if (cc1.Chunk.Position.x < cc2.Chunk.Position.x) return -1;
                else if (cc1.Chunk.Position.x > cc2.Chunk.Position.x) return 1;

                if (cc1.Chunk.Position.y < cc2.Chunk.Position.y) return -1;
                return 1;
            });

            for (int x = 0; x < Chunk.KEEP_LOADED; x++)
                for (int y = 0; y < Chunk.KEEP_LOADED; y++)
                {
                    grid[x, y] = ccs[x * Chunk.KEEP_LOADED + y];
                }

            return grid;
        }

        private ChunkController[] GetControllersOrdered(IEnumerable<ChunkController> chunkControllers)
        {
            List<ChunkController> ccs = new List<ChunkController>(chunkControllers);
            ccs.Sort((cc1, cc2) =>
            {
                if (cc1.Chunk.Position.y < cc2.Chunk.Position.y) return 1;
                else if (cc1.Chunk.Position.y > cc2.Chunk.Position.y) return -1;

                if (cc1.Chunk.Position.x < cc2.Chunk.Position.x) return -1;
                return 1;

            });

            return ccs.ToArray();
        }
    }

}