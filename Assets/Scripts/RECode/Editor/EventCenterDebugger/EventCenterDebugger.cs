
using RECode.REFramework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class EventCenterDebugger : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private ListView eventListView;
    private ListView listenerListView;
    private Button refreshBTN;

    private List<string> eventList=new List<string>();
    private List<string> listenerList=new List<string>();

    [MenuItem("Window/REEditor/EventCenterDebugger")]
    public static void OpenWindow()
    {
        EventCenterDebugger wnd = GetWindow<EventCenterDebugger>();
        wnd.titleContent = new GUIContent("EventCenterDebugger");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;
        m_VisualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Scripts/RECode/Editor/EventCenterDebugger/EventCenterDebugger.uxml");
        m_VisualTreeAsset.CloneTree(root);
        eventListView = root.Q<ListView>("EventList");
        listenerListView = root.Q<ListView>("ListenerList");
        refreshBTN = root.Q<Button>("RefreshBTN");

        eventListView.makeItem = MakeEventListViewItem;
        eventListView.bindItem = BindEventListViewItem;
        eventListView.selectionChanged += GetOnEventListViewChange;

        listenerListView.makeItem = MakeListenerListViewItem;
        listenerListView.bindItem = BindListenerListViewItem;

        UpdateEventListView();

        refreshBTN.clicked += UpdateEventListView;
    }

    private void UpdateEventListView()
    {
        eventList.Clear();
        foreach (string key in EventCenter.Instance.EventDic.Keys)
        {
            eventList.Add(key);
        }
        eventListView.itemsSource = eventList;
        eventListView.RefreshItems();
    }

    private VisualElement MakeEventListViewItem()
    {
        Label label = new Label();
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        label.style.marginLeft = 5;
        label.style.fontSize = 15;
        label.style.color = Color.white;
        label.style.paddingBottom = 4;
        label.style.paddingTop = 4;
        label.style.paddingLeft = 2;
        label.style.paddingRight = 2;
        return label;
    }

    private void BindEventListViewItem(VisualElement ve, int index)
    {
        Label label=ve as Label;

        label.text=eventList[index];
    }

    private void GetOnEventListViewChange(IEnumerable<object> obj)
    {
        foreach (object item in obj)
        {
            string key = item as string;
            UpdateListenerListView(key);
        }
    }

    private void UpdateListenerListView(string key)
    {
        listenerList.Clear();
        if (key==string.Empty)
        {
            return;
        }
        IEventInfo eventInfo= EventCenter.Instance.EventDic[key];
        for (int i = 0; i < eventInfo.GetDelegates().Length; i++)
        {
            Delegate del = eventInfo.GetDelegates()[i];
            string methodName = del.Method.Name;          
            object target = del.Target;
            string targetInfo = target != null ? target.GetType().Name : "静态方法";
            listenerList.Add($"[{i}] 方法名: {methodName}, 所属类型: {targetInfo}");
        }
        listenerListView.itemsSource = listenerList;
        listenerListView.RefreshItems();
    }


    private VisualElement MakeListenerListViewItem()
    {
        Label label = new Label();
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        label.style.marginLeft = 5;
        label.style.fontSize = 13;
        label.style.color = Color.white;
        label.style.paddingBottom = 4;
        label.style.paddingTop = 4;
        label.style.paddingLeft = 2;
        label.style.paddingRight = 2;
        return label;
    }

    private void BindListenerListViewItem(VisualElement ve, int index)
    {
        Label label=ve as Label;
        label.text = listenerList[index];
    }
}
