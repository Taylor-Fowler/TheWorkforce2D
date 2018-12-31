using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheWorkforce.Items
{
    public class ToolSlot
    {
        //#region Custom Event Declarations
        //public event DirtyHandler
        //#endregion

        #region Public Properties
        public ITool Tool { get; private set; }
        public readonly EToolType Allowed;
        #endregion


        #region Constructor
        public ToolSlot(EToolType toolAllowed)
        {
            Allowed = toolAllowed;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Try to add a valid tool into the slot, replacing the item previously in the slot
        /// </summary>
        /// <param name="tool">The tool to add to the tool slot, if it is null then the operation fails. Use <see cref="Empty()"/> to remove an item without replacing</param>
        /// <returns>A pair containing the success status of the operation and the item that was removed in the case of a successful addition</returns>
        public KeyValuePair<bool, ITool> Add(ITool tool)
        {
            if(tool != null && tool.ToolType == Allowed)
            {
                KeyValuePair<bool, ITool> successReplacing = new KeyValuePair<bool, ITool>(true, Tool);
                Tool = tool;

                return successReplacing;
            }
            
            return new KeyValuePair<bool, ITool>(false, null);
        }

        /// <summary>
        /// Removes the tool stored in the slot and replaces it with a null reference
        /// </summary>
        /// <returns>The tool that was previously stored in the slot</returns>
        public ITool Empty()
        {
            ITool removeFromSlot = Tool;
            Tool = null;

            return removeFromSlot;
        }
        #endregion
    }

}