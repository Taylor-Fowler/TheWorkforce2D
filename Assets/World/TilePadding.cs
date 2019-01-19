namespace TheWorkforce
{
    public class TilePadding
    {
        public const int 
            NORTH_WEST = 0,
            NORTH = 1,
            NORTH_EAST = 2,
            WEST = 3,
            EAST = 4,
            SOUTH_WEST = 5,
            SOUTH = 6,
            SOUTH_EAST = 7;
    
        public bool NW, N, NE, W, E, SW, S, SE;
    
        /// <summary>
        ///     Checks to see if each edge is enabled and then disables the corners attached to that edge eg. Northern Edge is
        ///     connected to
        ///     the North Eastern and North Western corners.
        /// </summary>
        public void ApplyFilter()
        {
            if (N)
            {
                NW = false;
                NE = false;
            }
    
            if (E)
            {
                NE = false;
                SE = false;
            }
    
            if (S)
            {
                SE = false;
                SW = false;
            }
    
            if (W)
            {
                NW = false;
                SW = false;
            }
        }
    
        public void Enable(int xOffset, int yOffset)
        {
            switch (xOffset)
            {
                // WEST
                case -1:
                    switch (yOffset)
                    {
                        // SOUTH
                        case -1:
                            SW = true;
                            break;
                        // CENTRAL
                        case 0:
                            W = true;
                            break;
                        // NORTH
                        case 1:
                            NW = true;
                            break;
                    }
    
                    break;
                // CENTRAL
                case 0:
                    switch (yOffset)
                    {
                        // SOUTH
                        case -1:
                            S = true;
                            break;
                        // CENTRAL - NEVER
                        case 0:
                            break;
                        // NORTH
                        case 1:
                            N = true;
                            break;
                    }
    
                    break;
                // EAST
                case 1:
                    switch (yOffset)
                    {
                        // SOUTH
                        case -1:
                            SE = true;
                            break;
                        // CENTRAL
                        case 0:
                            E = true;
                            break;
                        // NORTH
                        case 1:
                            NE = true;
                            break;
                    }
    
                    break;
            }
        }
    
        public int EdgeInformation()
        {
            if (N && E && S && W) return TerrainTileSet.ALL_EDGES;
            if (N && E && S) return TerrainTileSet.U_WEST;
            if (N && E && W) return TerrainTileSet.U_SOUTH;
            if (N && S && W) return TerrainTileSet.U_EAST;
            if (N && E) return TerrainTileSet.NORTH_EAST_CORNER;
            if (N && S) return TerrainTileSet.HORIZONTAL_EDGES;
            if (N && W) return TerrainTileSet.NORTH_WEST_CORNER;
            if (E && S && W) return TerrainTileSet.U_NORTH;
            if (E && S) return TerrainTileSet.SOUTH_EAST_CORNER;
            if (E && W) return TerrainTileSet.VERTICAL_EDGES;
            if (S && W) return TerrainTileSet.SOUTH_WEST_CORNER;
            if (N) return TerrainTileSet.NORTH;
            if (E) return TerrainTileSet.EAST;
            if (S) return TerrainTileSet.SOUTH;
            if (W) return TerrainTileSet.WEST;
            return -1;
        }
    
        public int CornerInformation()
        {
            if (NW && NE && SW && SE) return TerrainTileSet.ALL_CORNERS;
            if (NW && NE && SW) return TerrainTileSet.NE_NW_SW;
            if (NW && NE && SE) return TerrainTileSet.NE_NW_SE;
            if (NW && SW && SE) return TerrainTileSet.NW_SE_SW;
            if (NW && NE) return TerrainTileSet.NE_NW;
            if (NW && SW) return TerrainTileSet.NW_SW;
            if (NW && SE) return TerrainTileSet.NW_SE;
            if (NE && SW && SE) return TerrainTileSet.NE_SE_SW;
            if (NE && SW) return TerrainTileSet.NE_SW;
            if (NE && SE) return TerrainTileSet.NE_SE;
            if (SW && SE) return TerrainTileSet.SE_SW;
            if (NW) return TerrainTileSet.NORTH_WEST;
            if (NE) return TerrainTileSet.NORTH_EAST;
            if (SW) return TerrainTileSet.SOUTH_WEST;
            if (SE) return TerrainTileSet.SOUTH_EAST;
            return -1;
        }
    }
}