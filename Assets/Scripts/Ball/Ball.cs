﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LW.Core;

namespace LW.Ball{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(Rigidbody))]
    public class Ball : MonoBehaviour
    {
        // TODO
        // can catch in forcefield between hands and manipulate floats

        [SerializeField] string oscAddress;
        [SerializeField] AudioClip conjureFX;
        [SerializeField] AudioClip destroyFX;
        [SerializeField] float destroyDelay = 0.5f;
        [SerializeField] float magnetRange = 0.1f;
        // [SerializeField] float stopRange = 0.06f;
        [SerializeField] float magneticForce = 2;
        [SerializeField] float bounceForce = 1;
        [SerializeField] float touchFrequency = 1;
        float touchTimer = Mathf.Infinity;
        bool touchToggled = false;
        [SerializeField] float forceMult = 10000;
        [SerializeField] float killForce = 1000;

        public float distanceToRtHand, distanceToLtHand;

        float hueVal = Mathf.Epsilon;
        int oscVal = 0;
        bool gravity;

        NewTracking tracking;
        CastOrigins castOrigins;
        OSC osc;
        BallCaster caster;
        ForceField forceField;

        void Start()
        {
            GetComponent<AudioSource>().PlayOneShot(conjureFX);
            tracking = GameObject.FindGameObjectWithTag("HandTracking").GetComponent<NewTracking>();
            castOrigins = GameObject.FindGameObjectWithTag("HandTracking").GetComponent<CastOrigins>();
            osc = GameObject.FindGameObjectWithTag("OSC").GetComponent<OSC>();
            caster = GameObject.FindGameObjectWithTag("Caster").GetComponent<BallCaster>();

            gravity = GetComponent<Rigidbody>().useGravity;
            forceField = GameObject.FindGameObjectWithTag("ForceField").GetComponent<ForceField>();

            // TODO
            // lens test
            GameObject.FindGameObjectWithTag("OSC").GetComponent<OSC>().SetAddressHandler(oscAddress + "/receive", OnReceiveOSC);
            GameObject.FindGameObjectWithTag("OSC").GetComponent<OSC>().SetAllMessageHandler(OnReceiveOSC);
        }

        void Update()
        {
            touchTimer += Time.deltaTime;
            
            distanceToRtHand = Vector3.Distance(transform.position, tracking.GetRtPalm.Position);
            distanceToLtHand = Vector3.Distance(transform.position, tracking.GetLtPalm.Position);

            // TODO 
            // remote test
            // refactor for better readability
            //gravity = forceField ? !forceField.Caught : true;

            //if (!gravity)
            //{
            //    transform.position = castOrigins.PalmsMidpoint;
            //    return;
            //}

            if (distanceToRtHand < magnetRange)
            {
                Magnetism(tracking.GetRtPalm.Position);
            }
            if (distanceToLtHand < magnetRange) {
                Magnetism(tracking.GetLtPalm.Position);
            }
        }

        private void OnCollisionEnter(Collision other) {
            float collisionForce = other.impulse.magnitude * forceMult / Time.fixedDeltaTime;
            Debug.Log("FORCE>>>>>>>>>>>>>>>>>>>>>" + collisionForce);

            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("ForceField"))
            {
                if (other.gameObject.CompareTag("Player")) {
                    if (!touchToggled)
                    {
                        touchTimer = 0;
                        touchToggled = true;
                    }

                    if (touchTimer > touchFrequency)
                    {
                        hueVal += 0.1388f; // 1/5 of 360
                        if (hueVal > 1)
                        {
                            hueVal -= 1;
                        }

                        var ballMaterial = GetComponentInChildren<Renderer>().material;
                        ballMaterial.color = Color.HSVToRGB(hueVal, 1, 1);

                        oscVal += 1;
                        SendOSC(oscAddress, oscVal);
                        touchToggled = false;
                    }
                }
            }
            else
            {
                if (collisionForce >= killForce)
                {
                    //caster.StartCoroutine("DestroyBall");
                    Debug.Log(other.collider.gameObject.layer);
                }
            }
            
        }

        private void SendOSC(string address, float val) {
            OscMessage message = new OscMessage();
            message.address = address;
            message.values.Add(val);
            osc.Send(message);
        }

        private void Magnetism(Vector3 attraction)
        {
            transform.LookAt(attraction);
            GetComponent<Rigidbody>().AddForce(transform.forward * magneticForce);
        }

        public bool Handled {get; set;}

        void OnReceiveOSC(OscMessage message)
        {
            Debug.Log("OSC received: " + message);
            // TODO rethink this logic of having caster destroy Ball
            StartCoroutine("DestroySelf");
        }

        IEnumerator DestroySelf() {
            SendOSC("byeBall", 1);
            GetComponentInChildren<MeshExploder>().Explode();
            if (!GetComponent<AudioSource>().isPlaying) {
                GetComponent<AudioSource>().PlayOneShot(destroyFX);
            }
            GetComponentInChildren<MeshRenderer>().enabled = false;
            yield return new WaitForSeconds(destroyDelay);
            caster.Ball = false;
            Destroy(gameObject);
        }

    }
}


