using System;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;
using Rock.Utility;

namespace Rock.Tests.Rock.Utility
{
    [TestClass]
    public class RockDynamicTests
    {
        [TestMethod]
        public void StoreKeyGetKeyValueMatches()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;
            Assert.That.AreEqual( 123, (int)rockDynamic["A"] );
        }

        [TestMethod]
        public void SetPropertyGetKeyValueMatches()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic.A = 123;
            Assert.That.AreEqual( 123, (int)rockDynamic["A"] );
        }

        [TestMethod]
        public void StoreKeyGetPropertyValueMatches()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;
            Assert.That.AreEqual( 123, (int)rockDynamic.A );
        }

        [TestMethod]
        public void GetUnsetPropertyFails()
        {
            dynamic rockDynamic = new RockDynamic();

            Assert.That.ThrowsException<RuntimeBinderException>( () => rockDynamic.NotSet );
        }

        [TestMethod]
        public void SetPropertyContainsKey()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic.A = 123;
            Assert.That.IsTrue( ( bool ) rockDynamic.ContainsKey( "A" ) );
        }

        [TestMethod]
        public void StoreKeyContainsKey()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;

            Assert.That.IsTrue( (bool) rockDynamic.ContainsKey( "A" ) );
        }

        [TestMethod]
        public void ContainsKeyFailsForUnsetKey()
        {
            dynamic rockDynamic = new RockDynamic();

            Assert.That.IsFalse( (bool)rockDynamic.ContainsKey( "A" ) );
        }

        [TestMethod]
        public void ContainsKeyValuePair()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;

            Assert.That.IsTrue( (bool)rockDynamic.Contains( new System.Collections.Generic.KeyValuePair<string, object>( "A", 123 ) ) );
        }

        [TestMethod]
        public void ContainsKeyValuePairForDifferentValue()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;

            Assert.That.IsFalse( (bool)rockDynamic.Contains( new System.Collections.Generic.KeyValuePair<string, object>( "A", 456 ) ) );
        }

        [TestMethod]
        public void StoreKeyIsCaseSensitive()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["a"] = 123;
            Assert.That.IsFalse( (bool)rockDynamic.ContainsKey( "A" ) );
        }

        [TestMethod]
        public void SetPropertyIsCaseSensitive()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic.a = 123;
            Assert.That.ThrowsException<RuntimeBinderException>( () => rockDynamic.A );
        }
    }
}
