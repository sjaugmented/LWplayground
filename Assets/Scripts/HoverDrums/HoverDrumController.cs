﻿using UnityEngine;

namespace LW.HoverDrums
{
    public class HoverDrumController : MonoBehaviour
    {
        public float force = 1;
        public string address;
        
        float colorVal;
        bool isTouched = false;

        //OSC osc;
        //Rigidbody rigidBody;
        Renderer renderer;

        void Start()
        {
            renderer = GetComponentInChildren<Renderer>();
            GetComponent<Rigidbody>().AddForce(transform.forward * force);
            //osc = GameObject.FindGameObjectWithTag("OSC").GetComponent<OSC>();
        }

        void Update()
        {
            if (isTouched)
            {
                renderer.material.color = Color.white;
            }
            else renderer.material.color = Color.HSVToRGB(colorVal, 1, 1);
        }

        public void SetDrumColor(float color)
        {
            colorVal = color;
        }

        public void SetDrumAddress(int drumNum)
        {
            address = transform.GetChild(0).name + drumNum.ToString();
        }

        public void Touched()
        {
            isTouched = true;
            SendOSCMessage(address);
        }

        public void NotTouched()
        {
            isTouched = false;
        }

        private void SendOSCMessage(string address)
        {
            OscMessage message = new OscMessage();
            message.address = address;
            message.values.Add(1);
            GameObject.FindGameObjectWithTag("OSC").GetComponent<OSC>().Send(message);
            Debug.Log(this.gameObject.name + " sending OSC:" + message); // todo remove
        }
    }
}
