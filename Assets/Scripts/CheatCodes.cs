using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class CheatCodeEntry
{
    public string cheatCode;
    public UnityEvent<string> @event;
}

public class CheatCodes : MonoBehaviour
{
    [SerializeField]
    private string compositeString;

    [SerializeField,
    Range(.1f, 2f)]
    private float clearAfter = .75f;

    [SerializeField]
    private List<CheatCodeEntry> cheatCodes;

    private InputAction _anyKeyWait;
    private Coroutine _clearTimer;
    private float _timeUntilClear;

    private static readonly Dictionary<string, string> _controls = new()
    {
            {"/<Mouse>/leftButton", "LMB"}, {"/<Mouse>/middleButton", "MMB"}, {"/<Mouse>/rightButton", "RMB"},
            {"/<Gamepad>/buttonNorth", "GBN"}, {"/<Gamepad>/buttonSouth", "GBS"}, {"/<Gamepad>/buttonEast", "GBE"},
            {"/<Gamepad>/buttonWest", "GBW"}, {"/<Gamepad>/start", "GBT"}, {"/<Gamepad>/select", "GBL"},
            {"/<Gamepad>/rightStick/up", "GRSU"}, {"/<Gamepad>/rightStick/down", "GRSD"},
            {"/<Gamepad>/rightStick/left", "GRSL"}, {"/<Gamepad>/rightStick/right", "GRSR"},
            {"/<Gamepad>/leftStick/up", "GLSU"}, {"/<Gamepad>/leftStick/down", "GLSD"},
            {"/<Gamepad>/leftStick/left", "GLSL"}, {"/<Gamepad>/leftStick/right", "GLSR"},
            {"/<Gamepad>/dpad/up", "GDU"}, {"/<Gamepad>/dpad/down", "GDD"}, {"/<Gamepad>/dpad/left", "GDL"},
            {"/<Gamepad>/dpad/right", "GDR"}, {"/<Gamepad>/leftShoulder", "GLS"}, {"/<Gamepad>/rightShoulder", "GRS"},
            {"/<Gamepad>/leftTrigger", "GLT"}, {"/<Gamepad>/rightTrigger", "GRT"},
    };


    private void Awake()
    {
        _anyKeyWait = new InputAction(type: InputActionType.Button);
        foreach (var control in _controls)
        {
            _anyKeyWait.AddBinding(control.Key);
        }
        _anyKeyWait.AddBinding("/<Gamepad>/<button>");
    }

    private void OnEnable()
    {
        _anyKeyWait.performed += InputMouseGamepad;
        Keyboard.current.onTextInput += InputKey;
        _anyKeyWait.Enable();
        _clearTimer = StartCoroutine(ClearTimer());
    }

    private void OnDisable()
    {
        StopCoroutine(_clearTimer);
        _anyKeyWait.performed -= InputMouseGamepad;
        Keyboard.current.onTextInput -= InputKey;
        _anyKeyWait.Disable();
    }

    private void InputKey(char inputChar)
    {
        compositeString += inputChar.ToString().ToLower();
        _timeUntilClear = 0;
        CheckCodes();
    }

    private void InputMouseGamepad(InputAction.CallbackContext ctx)
    {
        foreach (var control in _controls)
        {
            var controlName = control.Key.Replace("/<Gamepad>/", string.Empty)
                    .Replace("/<Mouse>/", string.Empty);
            if (ctx.control.path.EndsWith(controlName))
            {
                compositeString += control.Value;
                _timeUntilClear = 0f;
            }
        }
        CheckCodes();
    }

    private void CheckCodes()
    {
        foreach (var cheat in cheatCodes)
        {
            if (compositeString.Equals(cheat.cheatCode))
                cheat.@event.Invoke(compositeString);
        }
    }

    private IEnumerator ClearTimer()
    {
        while (true)
        {
            if (compositeString == string.Empty)
                yield return null;

            _timeUntilClear += Time.deltaTime;

            if (_timeUntilClear >= clearAfter)
            {
                _timeUntilClear -= clearAfter;
                compositeString = string.Empty;
            }
            yield return null;
        }
    }

    public void DebugLog(string str) => Debug.Log(str);
}