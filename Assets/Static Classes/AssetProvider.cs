using System;
using UnityEngine;
using TheWorkforce.Items;

namespace TheWorkforce.Static_Classes
{
    public static class AssetProvider
    {
        public const float TILE_ITEM_Z_INDEX = -0.6f;

        public static Sprite[] EToolTypeSprites { get; private set; }

        public static void InitialiseEToolTypeSprites(Sprite[] sprites)
        {
            if(sprites.Length == Enum.GetNames(typeof(EToolType)).Length)
            {
                EToolTypeSprites = new Sprite[sprites.Length];

                for(int i = 0; i < sprites.Length; i++)
                {
                    EToolTypeSprites[i] = sprites[i];
                }
            }
        }
    }
}