using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine.InputSystem;

namespace ThrowEverything
{
    internal class InputSettings : LcInputActions
    {
        public static readonly InputSettings Instance = new();

        [InputAction("<Keyboard>/r", Name = "ThrowEverything: Throw Item")]
        public InputAction ThrowItem { get; set; }
    }
}
