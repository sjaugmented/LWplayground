﻿using System.Collections;
using UnityEngine;
using LW.Core;
using static UnityEngine.ParticleSystem;

namespace LW.Runic
{
    public class RuneController : MonoBehaviour
    {
        [SerializeField] AudioClip castFX;
        [SerializeField] AudioClip singleTouchFX;
        [SerializeField] AudioClip doubleTouchFX;
        [SerializeField] ParticleSystem particles;
        [SerializeField] NodeRingController nodeRing;
        [SerializeField] GameObject nodeIndicators;
        bool oscTest = false;

        public float force = 1;
        public string address1;
        public string address2;
        
        Material runeMaterial;
        public Material RuneMaterial
		{
            get { return runeMaterial; }
		}

        bool isTouched = false;
        
        int runeColors;
        int siblingIndex;
        float defaultOSCValue;

        Renderer renderer;
        RunicDirector director;
        HandTracking handtracking;
        EmissionModule emission;

        void Start()
        {
            renderer = GetComponentInChildren<Renderer>();
            director = GameObject.FindGameObjectWithTag("Director").GetComponent<RunicDirector>();
            handtracking = GameObject.FindGameObjectWithTag("Handtracking").GetComponent<HandTracking>();

			GetComponent<Rigidbody>().AddForce(transform.forward * force);
            GetComponent<AudioSource>().PlayOneShot(castFX);

            GameObject.FindGameObjectWithTag("OSC").GetComponent<OSC>().SetAddressHandler(address1 + "/receive", OnReceiveOSC);
            GameObject.FindGameObjectWithTag("OSC").GetComponent<OSC>().SetAllMessageHandler(OnReceiveOSC);

            emission = particles.emission;
            emission.enabled = false;

            defaultOSCValue = GameObject.FindGameObjectWithTag("Caster").GetComponent<RuneCaster>().DefaultOSCValue;

        }

        void Update()
        {
            if (director.Node)
			{
				nodeRing.gameObject.SetActive(true);
                SetMaterialOpacity(0.8f);
			}
			else
			{
				nodeRing.gameObject.SetActive(false);
                SetMaterialOpacity(1);
			}

			if (isTouched)
            {
                renderer.material.color = Color.HSVToRGB(0, 0, 0.2f);
            }
            else
			{
                renderer.material = runeMaterial;
			}

			if (oscTest) StartCoroutine("PlayParticles");

            // ordered within Rune Grid for better gathering
            transform.SetSiblingIndex(siblingIndex);
        }

        private void SetMaterialOpacity(float v)
		{
            Color matColor = runeMaterial.color;
            matColor.a = v;
            runeMaterial.color = matColor;
        }

        public void SetRuneAddressAndMaterial(int runeID, Material material)
        {
            string name = transform.GetChild(0).name;
            runeColors = FindObjectOfType<RuneCaster>().GetRuneColorCount();

            if (name == "Cube") runeID += runeColors;
			if (name == "Diamond") runeID += runeColors * 2;

			address1 = name + runeID + "a".ToString();
            address2 = name + runeID + "b".ToString();
            
            runeMaterial = material;
            MainModule particlesMain = particles.main;
            particlesMain.startColor = runeMaterial.color;

            gameObject.name = runeID + name;

            siblingIndex = runeID - 1; // for rune grid sort order
        }

        public void Touched()
        {
            if (director.Node)
			{
                nodeRing.ActivateNodeRing();
            }

            // if touched with one finger
            if (!handtracking.rightPeace && !handtracking.leftPeace)
            {
                isTouched = true;
				GetComponent<AudioSource>().PlayOneShot(singleTouchFX);
				SendOSCMessage(address1);
            }
            else
            {
                StartCoroutine("TwoFingerTouchFlicker");
				GetComponent<AudioSource>().PlayOneShot(doubleTouchFX);
				SendOSCMessage(address2);
            }
        }

        public void NotTouched()
        {
            isTouched = false;
        }

        public void Gazed()
		{
            if (director.Gaze)
			{
                nodeRing.ActivateNodeRing();
            }
		}

        public void SendOSCMessage(string address)
        {
            OscMessage message = new OscMessage();
            message.address = address;
            message.values.Add(defaultOSCValue);
            GameObject.FindGameObjectWithTag("OSC").GetComponent<OSC>().Send(message);
            Debug.Log(this.gameObject.name + " sending OSC:" + message); // todo remove
        }

        public void SendOSCMessage(string address, float value)
		{
            OscMessage message = new OscMessage();
            message.address = address;
            message.values.Add(value);
            GameObject.FindGameObjectWithTag("OSC").GetComponent<OSC>().Send(message);
            Debug.Log(this.gameObject.name + " sending OSC:" + message); // todo remove
        }

        void OnReceiveOSC(OscMessage message)
        {
            Debug.Log("OSC received: " + message);
            StartCoroutine("PlayParticles");
        }

        void OnCollisionEnter(Collision collision)
		{
            if (collision.collider.CompareTag("Rune"))
			{
                Collider runeCollider = collision.gameObject.GetComponent<Collider>();
                Physics.IgnoreCollision(runeCollider, GetComponent<Collider>());
			}
		}

        private IEnumerator TwoFingerTouchFlicker()
		{
            isTouched = true;
            yield return new WaitForSeconds(0.05f);
            isTouched = false;
            yield return new WaitForSeconds(0.2f);
            isTouched = true;
            yield return new WaitForSeconds(0.2f);
            isTouched = false;
		}

        private IEnumerator PlayParticles()
        {
            Debug.Log("pulsing");
            emission.enabled = true;
            yield return new WaitForSeconds(0.3f);
            emission.enabled = false;
        }
    }
}
