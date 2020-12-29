﻿using LW.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LW.Runic
{
    public class RuneCaster : MonoBehaviour
    {
        [Header("DevMode controls")]
        [SerializeField] bool devMode = false; // TODO remove
        [Range(7.5f, 50)] float force = 10f; // TODO make private

        [Header("Controller Settings")]
        [SerializeField] float castDelay = 0.5f; //TODO hardcode
        [SerializeField] float maxPalmDist = 0.5f; //TODO hardcode
        //[SerializeField] float resetWindow = 2; //TODO hardcode

        [Header("Hook Ups")]
        [SerializeField] GameObject masterRune;
        [SerializeField] AudioClip resetFX;
        [SerializeField] AudioClip gatherFX;

        float proximitySensor = Mathf.Infinity;
        float timeSinceLastCast = Mathf.Infinity;
        float resetTimer = 5;
        bool readyToGather = false;

        public RuneType runeType; // TODO private; easy shape switching in inspector
        int runeTypeIndex = 0; // automates rune selection
        
        [SerializeField] List<Color> runeColors = new List<Color>();
        int runeColorIndex = 0;
        
        // stores live drums, for dev purposes only TODO make private
        List<RuneController> liveRunes = new List<RuneController>();

        HandTracking handtracking;
        CastOrigins castOrigins;
        RunicDirector director;
        RuneBelt runeBelt;
        AudioSource audio;

        private void Start()
        {
            handtracking = GameObject.FindGameObjectWithTag("Handtracking").GetComponent<HandTracking>();
            castOrigins = GameObject.FindGameObjectWithTag("Handtracking").GetComponent<CastOrigins>();
            director = GameObject.FindGameObjectWithTag("Director").GetComponent<RunicDirector>();
            runeBelt = GetComponent<RuneBelt>();
            audio = GetComponent<AudioSource>();

            runeBelt.ResetAllRuneAmmo(runeColors.Count);
            masterRune.SetActive(false);
        }

        private void Update()
        {
            timeSinceLastCast += Time.deltaTime;
            resetTimer += Time.deltaTime;
            runeType = (RuneType)runeTypeIndex;
            proximitySensor += Time.deltaTime;

            if (director.currentMode == RunicDirector.Mode.Build)
			{
                
            }

            #region DEV MODE
            if (devMode)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    Reset();
                }

                if (Input.GetKeyDown(KeyCode.G))
                {
                    GatherRunes();
                }

                force += Input.mouseScrollDelta.y;

                if (Input.GetMouseButtonDown(0) && proximitySensor > 0)
                {
                    CastRune();
                }

                if (Input.GetKeyDown(KeyCode.Period)) runeTypeIndex++;
                if (Input.GetKeyDown(KeyCode.Comma)) runeTypeIndex--;

            }
            #endregion

            if (proximitySensor > 0.1f)
			{
                ////// Set Rune Type
                if (handtracking.palmsOpposed && handtracking.rightFist && handtracking.leftFist)
                {
                    masterRune.SetActive(true);
                    SelectRuneType();
                }
                else masterRune.SetActive(false);

                ////// Casting
                if (handtracking.palmsOut && handtracking.rightOpen && handtracking.leftOpen)
                {
                    CastRune();
                }
            }
            else
			{
                masterRune.SetActive(false);
			}


            #region Gather & Reset - activates Build Mode (!!Build mode deprecated for now!!)
            ////// Gather Runes
            // prime the gather runes method
            if (!handtracking.twoHands && handtracking.rightRockOn)
            {
                if (!readyToGather)
                {
                    resetTimer = 0;
                    readyToGather = true;
                }
            }
            else { readyToGather = false; }

            // trigger the gather runes method
            if (resetTimer < 2 && !handtracking.twoHands && handtracking.rightFist)
            {
                GatherRunes();
                //director.currentMode = RunicDirector.Mode.Build;
                resetTimer = Mathf.Infinity;
            }

            ////// Reset Interface
            if (handtracking.palmsIn && handtracking.rightFist && handtracking.leftFist)
            {
                Reset();
                //director.currentMode = RunicDirector.Mode.Build;
            }
			#endregion
		}

		private void SelectRuneType()
		{
            int totalRunes = runeBelt.GetRuneSlots();
            float staffAng = handtracking.GetStaffForCamUp();            
            float slotSize = 180 / totalRunes; // size of selectable area based on number of Rune Types

            for (int i = 0; i < totalRunes; i++)
			{
                if (staffAng < (180 - slotSize * i) && staffAng > (180 - slotSize * (i+1)))
				{
                    runeTypeIndex = i;
				}
			}

            // display masterRune with proper child
            foreach (Transform child in masterRune.transform)
			{
                child.gameObject.SetActive(false);
			}
            masterRune.transform.GetChild(runeTypeIndex).gameObject.SetActive(true);

            masterRune.transform.position = castOrigins.midpointhandtracking;
            masterRune.transform.LookAt(Camera.main.transform);
		}

		private void CastRune()
        {
            Vector3 castOrigin = Vector3.Lerp(handtracking.rightPalm.Position, handtracking.leftPalm.Position, 0.5f);
            Quaternion handRotation = Quaternion.Slerp(handtracking.rightPalm.Rotation, handtracking.leftPalm.Rotation, 0.5f);
            Quaternion castRotation = handRotation * Quaternion.Euler(60, 0, 0); // rotational offset - so casts go OUT instead of UP along the hand.Z axis

            if (timeSinceLastCast >= castDelay && runeBelt.GetCurrentRuneAmmo(runeType) > 0)
            {
                timeSinceLastCast = 0;
                GameObject rune;

                GameObject runePrefab = runeBelt.GetRunePrefab(runeType);

                if (devMode)
                {
                    rune = Instantiate(runePrefab, Camera.main.transform.position, Camera.main.transform.rotation);
                }
                else
                {
                    rune = Instantiate(runePrefab, castOrigin, castRotation);
                }


                runeColorIndex = runeColors.Count - runeBelt.GetCurrentRuneAmmo(runeType);

                // reduce ammo
                runeBelt.ReduceCurrentRuneAmmo(runeType);
                
                int runeID = runeColors.Count - runeBelt.GetCurrentRuneAmmo(runeType);

                RuneController currentRune = rune.GetComponent<RuneController>();
                currentRune.SetRuneAddressAndColor(runeID, runeColors[runeColorIndex]);

                float spellForce = (1 - (castOrigins.palmDist / maxPalmDist)) * 50;
                if (spellForce < 7.5f) spellForce = 7.5f;
                // set rune casting force and color
                if (devMode) currentRune.force = force;
                else currentRune.force = spellForce;
                // add rune to list of live drums
                liveRunes.Add(currentRune);

                //add to DrumContainer parent
                currentRune.transform.SetParent(FindObjectOfType<RuneGrid>().transform);

                //SetNextRuneColor();
            }
        }

		private void SetNextRuneColor()
        {
            if (runeColorIndex < runeColors.Count - 1)
            {
                runeColorIndex++;
            }
            else
            {
                runeColorIndex = 0;
            }
        }

        private void GatherRunes()
		{
            if (!audio.isPlaying) audio.PlayOneShot(gatherFX);
            RuneGrid grid = FindObjectOfType<RuneGrid>();
            grid.UpdateCollection();
            grid.PositionGrid();
		}

        private void Reset()
        {
            if (liveRunes.Count == 0) return;
            
            if (!audio.isPlaying)
            {
                audio.PlayOneShot(resetFX);
            }

            // clear all runes
            for (int i = 0; i < liveRunes.Count; i++)
            {
                StartCoroutine("DropAndDestroy", liveRunes[i]);
                liveRunes.Remove(liveRunes[i]);
            }

            // reset ammo counts, id, shape, color
            runeBelt.ResetAllRuneAmmo(runeColors.Count);
            runeColorIndex = 0;
        }

        private IEnumerator DropAndDestroy(RuneController drum)
        {
            if (!drum) yield break;
            drum.GetComponent<Rigidbody>().useGravity = true;
            yield return new WaitForSeconds(2);
            Destroy(drum.gameObject);
        }

        public void TriggerProximitySensor()
		{
            proximitySensor = 0;
		}

        public int GetRuneColorCount()
		{
            return runeColors.Count;
		}
    }
}

