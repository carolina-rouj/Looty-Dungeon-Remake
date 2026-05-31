using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public static class DungeonInput
{
#if ENABLE_INPUT_SYSTEM
    private static Keyboard Kb => Keyboard.current;
    private static Mouse Ms => Mouse.current;
#endif

    public static float Horizontal()
    {
        float x = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Kb != null)
        {
            if (Kb.aKey.isPressed || Kb.leftArrowKey.isPressed) x -= 1f;
            if (Kb.dKey.isPressed || Kb.rightArrowKey.isPressed) x += 1f;
        }
#endif
        if (x == 0f)
        {
            if (LegacyKey(KeyCode.A) || LegacyKey(KeyCode.LeftArrow)) x -= 1f;
            if (LegacyKey(KeyCode.D) || LegacyKey(KeyCode.RightArrow)) x += 1f;
        }
        return x;
    }

    public static float Vertical()
    {
        float z = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Kb != null)
        {
            if (Kb.sKey.isPressed || Kb.downArrowKey.isPressed) z -= 1f;
            if (Kb.wKey.isPressed || Kb.upArrowKey.isPressed) z += 1f;
        }
#endif
        if (z == 0f)
        {
            if (LegacyKey(KeyCode.S) || LegacyKey(KeyCode.DownArrow)) z -= 1f;
            if (LegacyKey(KeyCode.W) || LegacyKey(KeyCode.UpArrow)) z += 1f;
        }
        return z;
    }

    public static bool AttackPressed()
    {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Kb != null) pressed |= Kb.spaceKey.wasPressedThisFrame;
        if (Ms != null) pressed |= Ms.leftButton.wasPressedThisFrame;
#endif
        return pressed || LegacyKeyDown(KeyCode.Space) || LegacyMouseDown(0);
    }

    public static bool Pause()   => KeyDown(KeyCode.P);
    public static bool Escape()  => KeyDown(KeyCode.Escape);
    public static bool Submit()  => KeyDown(KeyCode.Return) || KeyDown(KeyCode.KeypadEnter);
    public static bool Confirm() => Submit() || KeyDown(KeyCode.Space);
    public static bool Menu()    => KeyDown(KeyCode.M);
    public static bool Credits() => KeyDown(KeyCode.C);
    public static bool Restart() => KeyDown(KeyCode.R);

    public static int LevelKeyPressed()
    {
        for (int i = 0; i < 10; i++)
        {
            if (NumberKeyDown(i)) return i;
        }
        return -1;
    }

    public static bool KeyDown(KeyCode code)
    {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Kb != null)
        {
            Key key = ToKey(code);
            if (key != Key.None) pressed = Kb[key].wasPressedThisFrame;
        }
#endif
        return pressed || LegacyKeyDown(code);
    }

    private static bool NumberKeyDown(int digit)
    {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        if (Kb != null)
        {
            // OJO: en el enum Key del Input System el orden es Digit1..Digit9, Digit0, por lo
            // que Key.Digit0 + n NO es Digit(n) (Key.Digit0 + 1 seria LeftShift!). Mapeo explicito.
            Key top = TopRowDigit(digit);
            Key pad = NumpadDigit(digit);
            pressed = (top != Key.None && Kb[top].wasPressedThisFrame)
                   || (pad != Key.None && Kb[pad].wasPressedThisFrame);
        }
#endif
        return pressed || LegacyKeyDown(KeyCode.Alpha0 + digit) || LegacyKeyDown(KeyCode.Keypad0 + digit);
    }

#if ENABLE_INPUT_SYSTEM
    private static Key TopRowDigit(int d)
    {
        switch (d)
        {
            case 0: return Key.Digit0;
            case 1: return Key.Digit1;
            case 2: return Key.Digit2;
            case 3: return Key.Digit3;
            case 4: return Key.Digit4;
            case 5: return Key.Digit5;
            case 6: return Key.Digit6;
            case 7: return Key.Digit7;
            case 8: return Key.Digit8;
            case 9: return Key.Digit9;
            default: return Key.None;
        }
    }

    private static Key NumpadDigit(int d)
    {
        switch (d)
        {
            case 0: return Key.Numpad0;
            case 1: return Key.Numpad1;
            case 2: return Key.Numpad2;
            case 3: return Key.Numpad3;
            case 4: return Key.Numpad4;
            case 5: return Key.Numpad5;
            case 6: return Key.Numpad6;
            case 7: return Key.Numpad7;
            case 8: return Key.Numpad8;
            case 9: return Key.Numpad9;
            default: return Key.None;
        }
    }
#endif

    private static bool LegacyKey(KeyCode code)
    {
        try { return Input.GetKey(code); } catch { return false; }
    }

    private static bool LegacyKeyDown(KeyCode code)
    {
        try { return Input.GetKeyDown(code); } catch { return false; }
    }

    private static bool LegacyMouseDown(int button)
    {
        try { return Input.GetMouseButtonDown(button); } catch { return false; }
    }

#if ENABLE_INPUT_SYSTEM
    private static Key ToKey(KeyCode code)
    {
        switch (code)
        {
            case KeyCode.P: return Key.P;
            case KeyCode.M: return Key.M;
            case KeyCode.C: return Key.C;
            case KeyCode.R: return Key.R;
            case KeyCode.Space: return Key.Space;
            case KeyCode.Escape: return Key.Escape;
            case KeyCode.Return: return Key.Enter;
            case KeyCode.KeypadEnter: return Key.NumpadEnter;
            default: return Key.None;
        }
    }
#endif
}
