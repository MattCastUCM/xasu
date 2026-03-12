#if ENABLE_INPUT_SYSTEM
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Xasu.HighLevel;
using static UnityEngine.InputSystem.InputAction;

public static class InputTrackerExtensions
{
    private static readonly Dictionary<InputAction, Action<CallbackContext>> registeredPresses = new Dictionary<InputAction, Action<CallbackContext>>();
    private static readonly Dictionary<InputAction, Action<CallbackContext>> registeredReleases = new Dictionary<InputAction, Action<CallbackContext>>();

    /// <summary>
    /// Extension method to easily add input tracking to Unity's new Input System actions.
    /// </summary>
    public static void RegisterAnalytics(this InputAction inputAction)
    {
        if (registeredPresses.ContainsKey(inputAction) || registeredReleases.ContainsKey(inputAction))
        {
            throw new InvalidOperationException($"The input action '{inputAction.name}' is already registered for analytics. Please unregister it before registering again.");
        }

        inputAction.performed += registeredPresses[inputAction] = SendPressed;
        inputAction.canceled += registeredReleases[inputAction] = SendReleased;
    }

    public static void RegisterAnalytics(this InputAction inputAction, string name)
    {
        if (registeredPresses.ContainsKey(inputAction) || registeredReleases.ContainsKey(inputAction))
        {
            throw new InvalidOperationException($"The input action '{inputAction.name}' is already registered for analytics. Please unregister it before registering again.");
        }

        inputAction.performed += registeredPresses[inputAction] = (context) => SendPressed(context, name);
        inputAction.canceled += registeredReleases[inputAction] = (context) => SendReleased(context, name);

    }

    public static void UnregisterAnalytics(this InputAction inputAction)
    {
        if (!registeredPresses.ContainsKey(inputAction) || !registeredReleases.ContainsKey(inputAction))
        {
            throw new InvalidOperationException($"The input action '{inputAction.name}' is not registered for analytics. Please register it before trying to unregister.");
        }

        inputAction.performed -= registeredPresses[inputAction];
        registeredPresses.Remove(inputAction);
        inputAction.canceled -= registeredReleases[inputAction];
        registeredReleases.Remove(inputAction);
    }

    private static void SendPressed(CallbackContext context) => SendPressed(context, context.action.name);
    private static void SendPressed(CallbackContext context, string name)
    {
        InputTracker.Instance.Pressed(name, InputTypeFromControl(context.control));
    }

    private static void SendReleased(CallbackContext context) => SendReleased(context, context.action.name);
    private static void SendReleased(CallbackContext context, string name)
    {
        InputTracker.Instance.Released(name, InputTypeFromControl(context.control));
    }

    private static InputTracker.InputType InputTypeFromControl(InputControl control)
    {
        if (control is Keyboard)
            return InputTracker.InputType.Keyboard;
        if (control is Mouse)
            return InputTracker.InputType.Mouse;
        if (control is Touchscreen)
            return InputTracker.InputType.Touchscreen;
        // Default to Button if the control type is not recognized
        return InputTracker.InputType.Button;
    }
}
#endif
