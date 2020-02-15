using UnityEngine.InputSystem;

public static class InputExtensions
{
    public static bool IsKeyboardAndMouse(this PlayerInput input)
    {
        return input.currentControlScheme == "Keyboard&Mouse";
    }
}