﻿using LW.Core;
using UnityEngine;

public class StaffTracker : MonoBehaviour
{
    HandTracking handTracker;

    // Start is called before the first frame update
    void Start()
    {
        handTracker = GetComponent<HandTracking>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
