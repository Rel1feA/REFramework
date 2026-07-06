using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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

        private string currentActiveMap="";


        protected override void Awake()
        {
            base.Awake();
            CacheActions();
            SwitchActionMap(InputConstants.Map_Gameplay);
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
    }
}


