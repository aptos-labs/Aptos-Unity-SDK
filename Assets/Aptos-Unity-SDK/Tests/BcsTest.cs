using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Aptos.Utilities.BCS;

public class BcsTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void BcsTestSimplePasses()
    {
        //Assert.AreEqual(1, 2);
        //Assert.AreEqual(1, 1);
        byte[] test = Serialization.Serialize("TEST");
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator BcsTestWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
