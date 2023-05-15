// Copyright © Pixel Crushers. All rights reserved.

using UnityEngine;
using Unity.VisualScripting;

namespace PixelCrushers.DialogueSystem.VisualScriptingSupport
{

    /// <summary>
    /// Registers Lua functions for sending custom events to Visual Scripting.
    /// Also provides C# methods to get Lua.Result values as primitive types.
    /// </summary>
    [AddComponentMenu("Pixel Crushers/Dialogue System/Third Party/Visual Scripting/Dialogue System Visual Scripting Lua")]
    public class DialogueSystemVisualScriptingLua : MonoBehaviour
    {

        protected static bool areFunctionsRegistered = false;

        private bool didIRegisterFunctions = false;

        void OnEnable()
        {
            if (areFunctionsRegistered)
            {
                didIRegisterFunctions = false;
            }
            else
            {
                // Make the functions available to Lua:
                didIRegisterFunctions = true;
                areFunctionsRegistered = true;
                Lua.RegisterFunction(nameof(vsEvent), this, SymbolExtensions.GetMethodInfo(() => vsEvent(string.Empty, string.Empty)));
                Lua.RegisterFunction(nameof(vsEventString), this, SymbolExtensions.GetMethodInfo(() => vsEventString(string.Empty, string.Empty, string.Empty)));
                Lua.RegisterFunction(nameof(vsEventBool), this, SymbolExtensions.GetMethodInfo(() => vsEventBool(string.Empty, string.Empty, false)));
                Lua.RegisterFunction(nameof(vsEventFloat), this, SymbolExtensions.GetMethodInfo(() => vsEventFloat(string.Empty, string.Empty, (double)0)));
                Lua.RegisterFunction(nameof(vsEventInt), this, SymbolExtensions.GetMethodInfo(() => vsEventInt(string.Empty, string.Empty, (double)0)));
            }
        }

        void OnDisable()
        {
            if (didIRegisterFunctions)
            {
                // Remove the functions from Lua:
                didIRegisterFunctions = false;
                areFunctionsRegistered = false;
                Lua.UnregisterFunction(nameof(vsEvent));
                Lua.UnregisterFunction(nameof(vsEventString));
                Lua.UnregisterFunction(nameof(vsEventBool));
                Lua.UnregisterFunction(nameof(vsEventFloat));
                Lua.UnregisterFunction(nameof(vsEventInt));
            }
        }

        public void vsEvent(string gameObjectName, string eventName)
        {
            TriggerVisualScriptingEvent(gameObjectName, eventName, null);
        }

        public void vsEventString(string gameObjectName, string eventName, string arg)
        {
            TriggerVisualScriptingEvent(gameObjectName, eventName, arg);
        }

        public void vsEventBool(string gameObjectName, string eventName, bool arg)
        {
            TriggerVisualScriptingEvent(gameObjectName, eventName, arg);
        }

        public void vsEventFloat(string gameObjectName, string eventName, double arg)
        {
            TriggerVisualScriptingEvent(gameObjectName, eventName, (float)arg);
        }

        public void vsEventInt(string gameObjectName, string eventName, double arg)
        {
            TriggerVisualScriptingEvent(gameObjectName, eventName, (int)arg);
        }

        public void TriggerVisualScriptingEvent(string gameObjectName, string eventName, object arg)
        {
            GameObject go = null;
            try
            {
                go = FindGameObject(gameObjectName);
                if (go == null)
                {
                    if (DialogueDebug.logWarnings) Debug.LogWarning($"Dialogue System: vsEvent() function can't find GameObject '{gameObjectName}' to trigger event '{eventName}'.");
                    return;
                }
                if (arg == null)
                {
                    CustomEvent.Trigger(go, eventName);
                }
                else
                {
                    CustomEvent.Trigger(go, eventName, arg);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e, go);
            }
        }

        private GameObject FindGameObject(string gameObjectName)
        {
            if (string.IsNullOrEmpty(gameObjectName)) return gameObject;
            var go = GameObject.Find(gameObjectName);
            if (go != null) return go;
            if (string.Equals(gameObjectName, "speaker"))
            {
                var sequencer = DialogueManager.instance.GetComponent<Sequencer>();
                if (sequencer != null && sequencer.speaker != null) return sequencer.speaker.gameObject;
            }
            else if (string.Equals(gameObjectName, "listener"))
            {
                var sequencer = DialogueManager.instance.GetComponent<Sequencer>();
                if (sequencer != null && sequencer.speaker != null) return sequencer.listener.gameObject;
            }
            return gameObject;
        }

        /// <summary>
        /// Returns the string value of a Lua Result.
        /// </summary>
        public static string LuaResultAsString(Lua.Result luaResult)
        {
            return luaResult.asString;
        }

        /// <summary>
        /// Returns the bool value of a Lua Result.
        /// </summary>
        public static bool LuaResultAsBool(Lua.Result luaResult)
        {
            return luaResult.asBool;
        }

        /// <summary>
        /// Returns the float value of a Lua Result.
        /// </summary>
        public static float LuaResultAsFloat(Lua.Result luaResult)
        {
            return luaResult.asFloat;
        }

        /// <summary>
        /// Returns the int value of a Lua Result.
        /// </summary>
        public static int LuaResultAsInt(Lua.Result luaResult)
        {
            return luaResult.asInt;
        }
    }
}