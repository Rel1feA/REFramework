using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace RECode.REFramework
{
    public enum InteractionType
    {
        Started,
        Performed,
        Canceled
    }

    public class InputActionPackage
    {
        public Action<InputAction.CallbackContext> inputAction;
        private UnityAction action;

        public InputActionPackage(UnityAction _action)
        {
            action += _action;
            inputAction += ((c) => { action?.Invoke(); });
        }

        public void AddAction(UnityAction _action)
        {
            action += _action;
        }

        public void RemoveAction(UnityAction _action)
        {
            action -= _action;
            if(action==null)
            {
                inputAction = null;
            }
        }
    }

    public class InputManager : MonoSingleton<InputManager>
    {
        [Header("配置文件")]
        [SerializeField] private InputActionAsset inputActions;

        private Dictionary<string,InputAction>inputActionDic=new Dictionary<string,InputAction>();
        private Dictionary<string,InputActionMap>actionMapDic=new Dictionary<string,InputActionMap>();
        private Dictionary<string,InputActionPackage> inputPackageDic=new Dictionary<string,InputActionPackage>();

        private string currentActiveMap;
        private InputDevice lastUsedDevice;

        public InputDevice LastUsedDevice { get { return lastUsedDevice; } }


        protected override void Awake()
        {
            base.Awake();
            CacheActions();
            SwitchActionMap(InputConstants.Map_Gameplay);
            lastUsedDevice=Keyboard.current;
            InputSystem.onEvent += OnInputEvent;
        }

        private void OnDestroy()
        {
            InputSystem.onEvent -= OnInputEvent;
        }

        private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
        {
            if (!(device is Keyboard)&&!(device is Gamepad))
                return;
            lastUsedDevice = device;
        }

        private void CacheActions()
        {
            inputActionDic.Clear();
            actionMapDic.Clear();
            foreach(InputActionMap map in inputActions.actionMaps)
            {
                actionMapDic[map.name] = map;
                foreach(InputAction action in map.actions)
                {
                    string fullKey=$"{map.name}_{action.name}";
                    if(!inputActionDic.ContainsKey(fullKey))
                    {
                        inputActionDic[fullKey] = action;
                    }
                }
            }
        }

        private InputAction GetInputAction(string actionName, string mapName = null)
        {
            string key;
            if (mapName != null)
            {
                key = $"{mapName}_{actionName}";
            }
            else
            {
                key = $"{currentActiveMap}_{actionName}";
            }
            if (!inputActionDic.TryGetValue(key, out InputAction inputAction))
            {
                Debug.LogWarning($"InputManager: 未找到名为 {actionName} 的 InputAction");
                return null;
            }
            return inputAction;
        }

        private string GetKey(string actionName,InteractionType type)
        {
            string key;
            switch (type)
            {
                case InteractionType.Started:
                    key = $"{actionName}_{InputConstants.Type_Started}";
                    break;
                case InteractionType.Performed:
                    key = $"{actionName}_{InputConstants.Type_Performed}";
                    break;
                case InteractionType.Canceled:
                    key = $"{actionName}_{InputConstants.Type_Canceled}";
                    break;
                default:
                    key = "Error";
                    break;
            }
            return key;
        }


        public void BindAction(string actionName,UnityAction action,InteractionType type,string mapName=null)
        {
            InputAction inputAction = GetInputAction(actionName,mapName);
            if (inputAction != null)
            {
                string key=GetKey(actionName,type);
                if (!inputPackageDic.ContainsKey(key))
                {
                    InputActionPackage package = new InputActionPackage(action);
                    inputPackageDic[key] = package;
                    switch (type)
                    {
                        case InteractionType.Started:
                            inputAction.started += inputPackageDic[key].inputAction;
                            break;
                        case InteractionType.Performed:
                            inputAction.performed += inputPackageDic[key].inputAction;
                            break;
                        case InteractionType.Canceled:
                            inputAction.canceled += inputPackageDic[key].inputAction;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    inputPackageDic[key].AddAction(action);
                }
            }
        }

        public void UnBindAction(string actionName, UnityAction action, InteractionType type, string mapName = null)
        {
            InputAction inputAction = GetInputAction(actionName, mapName);
            if (inputAction != null)
            {
                string key=GetKey(actionName,type);
                if (!inputPackageDic.ContainsKey(key))
                {
                    Debug.Log($"未找到名为{key}的绑定，无法删除");
                    return;
                }
                else
                {
                    inputPackageDic[key].RemoveAction(action);
                    if (inputPackageDic[key].inputAction==null)
                    {
                        switch (type)
                        {
                            case InteractionType.Started:
                                inputAction.started -= inputPackageDic[key].inputAction;
                                break;
                            case InteractionType.Performed:
                                inputAction.performed -= inputPackageDic[key].inputAction;
                                break;
                            case InteractionType.Canceled:
                                inputAction.canceled -= inputPackageDic[key].inputAction;
                                break;
                            default:
                                break;
                        }
                        inputPackageDic.Remove(key);
                    }
                }
            }
        }

        public Vector2 GetAxis(string actionName)
        {
            InputAction inputAction=GetInputAction(actionName);
            if(inputAction!=null)
            {
                return inputAction.ReadValue<Vector2>();
            }
            return Vector2.zero;
        }

        public float GetFloat(string actionName)
        {
            InputAction inputAction = GetInputAction(actionName);
            if (inputAction != null)
            {
                return inputAction.ReadValue<float>();
            }
            return 0;
        }

        public bool GetKeyDown(string actionName)
        {
            InputAction inputAction = GetInputAction(actionName);
            if( inputAction != null )
            {
                return inputAction.WasPressedThisFrame();
            }
            return false;
        }

        public bool GetKeyUp(string actionName)
        {
            InputAction inputAction = GetInputAction(actionName);
            if (inputAction != null)
            {
                return inputAction.WasReleasedThisFrame();
            }
            return false;
        }

        public bool GetKey(string actionName)
        {
            InputAction inputAction = GetInputAction(actionName);
            if (inputAction != null)
            {
                return inputAction.IsPressed();
            }
            return false;
        }

        public void SwitchActionMap(string mapName)
        {
            if(string.IsNullOrEmpty(mapName)) return;
            if (!string.IsNullOrEmpty(currentActiveMap) && actionMapDic.TryGetValue(currentActiveMap, out InputActionMap oldMap))
            {
                oldMap.Disable();
            }
            if(actionMapDic.TryGetValue(mapName, out InputActionMap newMap))
            {
                newMap.Enable();
                currentActiveMap = mapName;
            }
            else
            {
                Debug.LogError($"未找到名为{mapName}的InputActionMap");
            }
        }

        public void DisableInputMap(string mapName)
        {
            if(actionMapDic.TryGetValue(mapName, out InputActionMap targetMap))
            {
                targetMap.Disable();
            }
            else
            {
                Debug.LogError($"未找到名为{mapName}的InputActionMap");
            }
        }

        public void DisableAllInputMap()
        {
            foreach(var map in actionMapDic.Values)
            {
                map.Disable();
            }
        }

        private int GetRealIndexInAction(string actionName,int index,string mapName=null)
        {
            InputAction inputAction = GetInputAction(actionName, mapName);
            if (inputAction == null)
            {
                Debug.LogError($"你输入的Action名{mapName}/{actionName}错误");
                return -1;
            }
            for(int i=0;i<inputAction.controls.Count;i++)
            {
                if (inputAction.controls[i].device==lastUsedDevice)
                {
                    return inputAction.GetBindingIndexForControl(inputAction.controls[i]) + index;
                }
            }
            return -1;
        }

        public void StartRebind(string actionName,int index,UnityAction<bool> onComplete=null,string mapName=null,bool isRealIndex=true)
        {
            InputAction inputAction=GetInputAction(actionName,mapName);
            if(inputAction==null)
            {
                onComplete(false);
                Debug.LogError($"你输入的Action名{mapName}/{actionName}错误，无法改键");
                return;
            }
            if(!isRealIndex)index=GetRealIndexInAction(actionName,index,mapName);
            inputAction.Disable();
            inputAction.PerformInteractiveRebinding(index)
                .OnMatchWaitForAnother(0.1f)
                .WithControlsExcluding("<Mouse>")
                .WithControlsExcluding("<Pointer>")
                .WithoutIgnoringNoisyControls()
                .OnComplete((op) =>
                {
                    op.Dispose();
                    inputAction.Enable();
                    onComplete?.Invoke(true);
                })
                .OnCancel((op) =>
                {
                    op.Dispose();
                    inputAction.Enable();
                    onComplete?.Invoke(false);
                })
                .Start();
        }

        public string GetBindingName(string actionName, int index = 0,string mapName=null,bool toHumanReadAble=true,bool isRealIndex=true)
        {
            InputAction inputAction = GetInputAction(actionName, mapName);
            if (inputAction == null)
            {
                Debug.LogError($"你输入的Action名{mapName}/{actionName}错误");
                return string.Empty;
            }
            if (!isRealIndex) index = GetRealIndexInAction(actionName, index, mapName);
            string bindingName = inputAction.bindings[index].effectivePath;
            if (toHumanReadAble)
            {
                return InputControlPath.ToHumanReadableString(bindingName, InputControlPath.HumanReadableStringOptions.OmitDevice);
            }
            else
            {
                return bindingName;
            }
        }
    }
}


