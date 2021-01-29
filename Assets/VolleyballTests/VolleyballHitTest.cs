using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
// using MLAgents;
using Scripts;

namespace Tests
{
    public class VolleyballHitTest
    {
        // VolleyballArea area;
        [SetUp]
        public void Init()
        {
            GameObject areaObject = MonoBehaviour
            .Instantiate(Resources.Load<GameObject>("Prefabs/VollleyballTraining"));       
        }


        // A Test behaves as an ordinary method
        [Test]
        public void VolleyballHitTestSimplePasses()
        {
            // Use the Assert class to test conditions
            Assert.Fail();
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
