using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LW.HSL
{
    
    public class HSLOrbController : MonoBehaviour
    {
        Color orbColor;

        ColorPicker colorPicker;
        Material material;
        void Start()
        {
            colorPicker = GameObject.FindGameObjectWithTag("ColorPicker").GetComponent<ColorPicker>();
            material = GetComponentInChildren<HSLOrbID>().gameObject.GetComponent<Renderer>().material;
        }

        void Update()
        {
            transform.LookAt(2 * transform.position - Camera.main.transform.position);
            orbColor = colorPicker.PreviewColor;
            
            material.color = orbColor;
            material.SetColor("_EmissionColor", orbColor);
        }
    }
}


