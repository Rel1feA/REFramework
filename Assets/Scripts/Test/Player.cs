using RECode.REFramework;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Player : MonoBehaviour
{
    private CharacterController controller;

    [SerializeField]
    private float speed;
    [SerializeField]
    private float gravity = 9.8f;

    private Vector2 input;

    private void Awake()
    {
        controller= GetComponent<CharacterController>();
    }

    private void Update()
    {
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        controller.Move(input * speed*Time.deltaTime);
        controller.Move(Vector2.down*gravity*Time.deltaTime);
    }

    private void OnEnable()
    {
        EventCenter.Instance.AddListener<(string, string, int)>("PlayerTest", Test);
    }

    private void OnDisable()
    {
        EventCenter.Instance.RemoveListener<(string, string, int)>("PlayerTest", Test);
    }


    public void Test((string s1,string s2,int i) args)
    {
        UnityEngine.Debug.Log($"{args.s1},{args.s2},{args.i}");
    }
}
