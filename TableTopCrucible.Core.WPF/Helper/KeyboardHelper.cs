using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace TableTopCrucible.Core.WPF.Helper
{
    public static class KeyboardHelper
    {
        public static bool IsKeyPressed(ModifierKeys key)
         => ((Keyboard.Modifiers & key) == key);
    }
}
