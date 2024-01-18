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

                string name = itemProperties.itemName;
                string key = InputSettings.Instance.ThrowItem.bindings[0].path.Split('/').Last().ToUpper();
                int len = 1;
                if (itemProperties.toolTips != null)
                {
                    len = itemProperties.toolTips.Length + 1;
                }
                string[] tt = new string[len];
                if (chargingThrow.isCharging)
                {
                    int percentage = chargingThrow.GetChargedPercentage();
                    tt[0] = $"Throw {name} : [{percentage}%]";
                }
                else
                {
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
