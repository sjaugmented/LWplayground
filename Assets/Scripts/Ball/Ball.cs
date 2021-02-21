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
        // tennis ball
        // each hit changes color and sends OSC
        // can catch in forcefield between hands and manipulate floats
        // receive OSC to explode orb on downbeat

        [SerializeField] AudioClip conjureFX;
        [SerializeField] AudioClip destroyFX;
        [SerializeField] float magnetRange = 0.1f;
        [SerializeField] float stopRange = 0.06f;
        [SerializeField] float magnetism = 2;

        public float distanceToHand;

        HandTracking hands;

        void Start()
        {
            GetComponent<AudioSource>().PlayOneShot(conjureFX);
            hands = GameObject.FindGameObjectWithTag("HandTracking").GetComponent<HandTracking>();
        }

        void Update()
        {
            distanceToHand = Vector3.Distance(transform.position, hands.rightPalm.Position);

            if (distanceToHand < magnetRange && distanceToHand > stopRange) {
                // gentle push toward hand
                // TODO needs stop/brakes
                transform.LookAt(hands.rightPalm.Position);
                GetComponent<Rigidbody>().AddForce(transform.forward * magnetism);
            }
        }

        public void DestroySelf() {
            GetComponentInChildren<MeshExploder>().Explode();
            if (!GetComponent<AudioSource>().isPlaying) {
                GetComponent<AudioSource>().PlayOneShot(destroyFX);
            }
            GetComponentInChildren<MeshRenderer>().enabled = false;
            var particles = GetComponentInChildren<ParticleSystem>().emission;
            particles.enabled = false;
        }
    }
}

