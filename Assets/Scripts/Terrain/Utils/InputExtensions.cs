using UnityEngine.InputSystem;

namespace Terrain.Utils
{
    public static class InputExtensions
    {
        public static bool IsKeyboardAndMouse(this PlayerInput input)
        {
            return input.currentControlScheme == "Keyboard&Mouse";
        }
    }
}