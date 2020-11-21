﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LW.Core;
using System;

namespace LW.HoverDrums
{
    public class HoverDrummer : MonoBehaviour
    {
        [SerializeField] float castDelay = 3f;
        [SerializeField] float maxXAxisDist = 0.5f;
        [SerializeField] List<GameObject> drumVariants;
        [SerializeField] List<float> colorVariants;

        public int drumsLeftToCast;
        Vector3 orbCastRotOffset = new Vector3(60, 0, 0);

        HandTracking handtracking;
        CastOrigins castOrigins;
        HoverDrumController drumController;
        float timeSinceLastCast = Mathf.Infinity;

        public int drumShape = 0;
        public int drumColor = 0;

        private void Start()
        {
            handtracking = GameObject.FindGameObjectWithTag("Handtracking").GetComponent<HandTracking>();
            castOrigins = FindObjectOfType<CastOrigins>();
            drumsLeftToCast = drumVariants.Count * colorVariants.Count;
        }

        private void Update()
        {
            timeSinceLastCast += Time.deltaTime;

            if (drumsLeftToCast == 0) return;

            if (handtracking.palmsOut && handtracking.rightOpen && handtracking.leftOpen)
            {
                CastOrb();
            }
        }

        private void CastOrb()
        {
            Vector3 castOrigin = Vector3.Lerp(handtracking.rtMiddleKnuckle.Position, handtracking.ltMiddleKnuckle.Position, 0.5f);

            Quaternion palmsRotationMid = Quaternion.Slerp(handtracking.rightPalm.Rotation, handtracking.leftPalm.Rotation, 0.5f);
            Quaternion castRotation = palmsRotationMid * Quaternion.Euler(orbCastRotOffset);

            if (timeSinceLastCast >= castDelay)
            {
                timeSinceLastCast = 0;

                GameObject drum = Instantiate(drumVariants[drumShape], castOrigin, castRotation);
                drum.GetComponent<HoverDrumController>().SetDrumColor(colorVariants[drumColor]);

                NextVariant();

                float spellForceRange = 1 - (castOrigins.palmDist / maxXAxisDist);
                float spellForce = spellForceRange * 10;
                if (spellForce < 1) spellForce = 2;
                drum.GetComponent<HoverDrumController>().force = spellForce;

            }
        }

        private void NextVariant()
        {
            if (drumColor < colorVariants.Count - 1)
            {
                drumColor++;
                drumsLeftToCast--;
            }
            else
            {
                drumColor = 0;
                if (drumShape < drumVariants.Count - 1)
                {
                    drumShape++;
                }
                else return;
            }
        }

        private void Reset()
        {
            // clear all drums
            
            // reset shape and color ints
            drumShape = 0;
            drumColor = 0;
        }
    }
}

