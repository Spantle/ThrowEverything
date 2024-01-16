using System;
using System.Collections.Generic;
using System.Text;
using static UnityEngine.InputSystem.InputAction;

namespace ThrowEverything
{
    public class ThrowableItem
    {
        public Action<CallbackContext> throwEvent;
        public GrabbableObject item;

        public ThrowableItem(Action<CallbackContext> throwEvent, GrabbableObject item)
        {
            this.throwEvent = throwEvent;
            this.item = item;
        }
    }
}
