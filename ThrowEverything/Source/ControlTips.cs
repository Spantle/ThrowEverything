using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ThrowEverything.Models;

namespace ThrowEverything
{
    internal class ControlTips
    {
        internal static void Set(GrabbableObject item)
        {
            try
            {
                ChargingThrow chargingThrow = State.GetChargingThrow();
                Item itemProperties = item.itemProperties;

                int len = 1;
                if (itemProperties.toolTips != null)
                {
                    len = itemProperties.toolTips.Length + 1;
                    if (len > 3)
                    {
                        // the game has a total limit of 4 tooltips that can be shown
                        // excluding the drop tooltip which is always shown by force, this means that itemProperties.toolTips can have a max of only 3
                        // an item with 3 tooltips is the shotgun
                        // if we try to inject our own tooltip if there's already 3/4 tooltips, one of the item's will not be shown
                        // in this scenario we will not show the throw tooltip to avoid the item's tooltips from being hidden
                        return;
                    }
                }
                string[] tt = new string[len];
                string name = itemProperties.itemName;
                if (chargingThrow.isCharging)
                {
                    int percentage = chargingThrow.GetChargedPercentage();
                    tt[0] = $"Throw {name} : [{percentage}%]";
                }
                else
                {
                    string key = InputSettings.Instance.ThrowItem.bindings[0].path.Split('/').Last().ToUpper();
                    tt[0] = $"Throw {name} : [{key}]";
                }

                for (int i = 1; i < tt.Length; i++)
                {
                    tt[i] = itemProperties.toolTips[i - 1];
                }

                HUDManager.Instance.ChangeControlTipMultiple(tt, holdingItem: true, itemProperties);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"failed to inject control tips");
                Plugin.Logger.LogWarning(e);
            }
        }
    }
}
