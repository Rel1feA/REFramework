using RECode.REFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private void OnEnable()
    {
        EventCenter.Instance.AddListener("PlayerMove", Move);
        EventCenter.Instance.AddListener<(string, string, int)>("PlayerTest", Test);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveListener("PlayerMove", Move);
        EventCenter.Instance.RemoveListener<(string, string, int)>("PlayerTest", Test);
    }

    public void Move()
    {
        Debug.Log("Player Move");
    }

    public void Test((string s1,string s2,int i) args)
    {
        Debug.Log($"{args.s1},{args.s2},{args.i}");
    }
}
