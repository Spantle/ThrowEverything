using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;

namespace ThrowEverything
{
    public class ThrowEverythingInputSettings : LcInputActions
    {
        public static readonly ThrowEverythingInputSettings Instance = new ThrowEverythingInputSettings();

        [InputAction("<Keyboard>/r", Name = "ThrowEverything: Throw Item")]
        public InputAction ThrowItem { get; set; }
    }
}
