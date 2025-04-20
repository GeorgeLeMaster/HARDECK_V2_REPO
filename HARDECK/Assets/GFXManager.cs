using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GFXManager : MonoBehaviour
{

    public static GFXManager instance;

    public LineRenderer LR_movement;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        LR_movement.positionCount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
