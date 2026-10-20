using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Gorkha.Core
{
    /// <summary>
    /// Unified input wrapper that seamlessly works across New Input System and Legacy Input.
    /// Eliminates InvalidOperationException when project uses New Input System.
    /// </summary>
    public static class GorkhaInput
    {
        public static Vector2 GetMoveAxis()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                float x = 0f;
                float y = 0f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
                return new Vector2(x, y);
            }
#endif
            try
            {
                return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
            catch
            {
                return Vector2.zero;
            }
        }

        public static Vector3 MousePosition
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                var mouse = Mouse.current;
                if (mouse != null)
                {
                    Vector2 pos = mouse.position.ReadValue();
                    return new Vector3(pos.x, pos.y, 0f);
                }
#endif
                try { return Input.mousePosition; } catch { return Vector3.zero; }
            }
        }

        public static bool GetMouseButtonDown(int button)
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (button == 0) return mouse.leftButton.wasPressedThisFrame;
                if (button == 1) return mouse.rightButton.wasPressedThisFrame;
                if (button == 2) return mouse.middleButton.wasPressedThisFrame;
            }
#endif
            try { return Input.GetMouseButtonDown(button); } catch { return false; }
        }

        public static bool GetMouseButton(int button)
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (button == 0) return mouse.leftButton.isPressed;
                if (button == 1) return mouse.rightButton.isPressed;
                if (button == 2) return mouse.middleButton.isPressed;
            }
#endif
            try { return Input.GetMouseButton(button); } catch { return false; }
        }

        public static bool GetMouseButtonUp(int button)
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                if (button == 0) return mouse.leftButton.wasReleasedThisFrame;
                if (button == 1) return mouse.rightButton.wasReleasedThisFrame;
                if (button == 2) return mouse.middleButton.wasReleasedThisFrame;
            }
#endif
            try { return Input.GetMouseButtonUp(button); } catch { return false; }
        }

        public static float GetScrollWheel()
        {
#if ENABLE_INPUT_SYSTEM
            var mouse = Mouse.current;
            if (mouse != null)
            {
                return mouse.scroll.ReadValue().y * 0.01f;
            }
#endif
            try { return Input.GetAxis("Mouse ScrollWheel"); } catch { return 0f; }
        }

        public static bool GetCancelDown()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null) return kb.escapeKey.wasPressedThisFrame;
#endif
            try { return Input.GetKeyDown(KeyCode.Escape); } catch { return false; }
        }

        public static bool IsCtrlHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                return kb.leftCtrlKey.isPressed || kb.rightCtrlKey.isPressed ||
                       kb.leftCommandKey.isPressed || kb.rightCommandKey.isPressed;
            }
#endif
            try
            {
                return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                       Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
            }
            catch { return false; }
        }

        public static bool IsShiftHeld()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                return kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            }
#endif
            try
            {
                return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            }
            catch { return false; }
        }

        public static bool GetKeyDown(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            var kb = Keyboard.current;
            if (kb != null)
            {
                switch (key)
                {
                    case KeyCode.Z: return kb.zKey.wasPressedThisFrame;
                    case KeyCode.X: return kb.xKey.wasPressedThisFrame;
                    case KeyCode.C: return kb.cKey.wasPressedThisFrame;
                    case KeyCode.V: return kb.vKey.wasPressedThisFrame;
                    case KeyCode.Alpha1: return kb.digit1Key.wasPressedThisFrame;
                    case KeyCode.Alpha2: return kb.digit2Key.wasPressedThisFrame;
                    case KeyCode.Alpha3: return kb.digit3Key.wasPressedThisFrame;
                    case KeyCode.Alpha4: return kb.digit4Key.wasPressedThisFrame;
                    case KeyCode.Alpha5: return kb.digit5Key.wasPressedThisFrame;
                    case KeyCode.Alpha6: return kb.digit6Key.wasPressedThisFrame;
                    case KeyCode.Alpha7: return kb.digit7Key.wasPressedThisFrame;
                    case KeyCode.Alpha8: return kb.digit8Key.wasPressedThisFrame;
                    case KeyCode.Alpha9: return kb.digit9Key.wasPressedThisFrame;
                }
            }
#endif
            try { return Input.GetKeyDown(key); } catch { return false; }
        }
    }
}
