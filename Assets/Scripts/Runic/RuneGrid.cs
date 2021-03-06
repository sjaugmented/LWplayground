﻿using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Solvers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LW.Runic
{
    [RequireComponent(typeof(GridObjectCollection))]
    [RequireComponent(typeof(RadialView))]
    [RequireComponent(typeof(SolverHandler))]

    public class RuneGrid : MonoBehaviour
    {
        IList<Transform> childList = new List<Transform>();
        void Start()
		{
            GetComponent<RadialView>().enabled = false ;
		}

        public void UpdateCollection()
		{
            GetComponent<GridObjectCollection>().enabled = true;
            GetComponent<GridObjectCollection>().UpdateCollection();
        }

        public void PositionGrid()
		{
            StartCoroutine("OnOffRadial");
		}

        private IEnumerator OnOffRadial()
		{
            GetComponent<RadialView>().enabled = true;
            yield return new WaitForSeconds(1);
            GetComponent<RadialView>().enabled = false;
            GetComponent<GridObjectCollection>().enabled = false;

        }
    }
}
