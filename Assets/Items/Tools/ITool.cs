using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Items
{
    public interface ITool
    {
        EToolType ToolType { get; }
        int Level { get; }
        int UsesLeft { get; }

        void InitialiseTool(EToolType toolType, int level, int usesLeft);
    }
}
