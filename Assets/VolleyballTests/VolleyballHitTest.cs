﻿using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Volleyball;

namespace Tests
{
    public class VolleyballHitTest
    {
        VolleyballArea area;
        [SetUp]
        public void Init()
        {
            Physics.gravity = Vector3.zero;
            GameObject areaObject = MonoBehaviour
            .Instantiate(Resources.Load<GameObject>("Prefabs/VollleyballTraining")); 
            area = areaObject.GetComponent<VolleyballArea>();      
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(area.gameObject);
        }

        [Test]
        public void VolleyballAreaIsLoaded()
        {
            Assert.IsNotNull(area.gameObject);
        }

        // A Test behaves as an ordinary method
        [Test]
        public void VolleyballHitTestSimplePasses()
        {

            // Use the Assert class to test conditions
            Assert.Pass();
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator VolleyballHitTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            
            yield return null;
        }
    }
}
