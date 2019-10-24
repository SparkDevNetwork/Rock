using System;
using Rock.Utility;
using Xunit;

namespace Rock.Tests.Rock.Utility
{
    public class RockDynamicTests
    {
        [Fact]
        public void StoreKeyGetKeyValueMatches()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;
            Assert.Equal( 123, rockDynamic["A"] );
        }

        [Fact]
        public void SetPropertyGetKeyValueMatches()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic.A = 123;
            Assert.Equal( 123, rockDynamic["A"] );
        }

        [Fact]
        public void StoreKeyGetPropertyValueMatches()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;
            Assert.Equal( 123, rockDynamic.A );
        }

        [Fact]
        public void GetUnsetPropertyFails()
        {
            dynamic rockDynamic = new RockDynamic();
            Assert.ThrowsAny<Exception>( () => rockDynamic.NotSet );
        }

        [Fact]
        public void SetPropertyContainsKey()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic.A = 123;
            Assert.True( rockDynamic.ContainsKey( "A" ) );
        }

        [Fact]
        public void StoreKeyContainsKey()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;

            Assert.True( rockDynamic.ContainsKey( "A" ) );
        }

        [Fact]
        public void ContainsKeyFailsForUnsetKey()
        {
            dynamic rockDynamic = new RockDynamic();

            Assert.False( rockDynamic.ContainsKey( "A" ) );
        }

        [Fact]
        public void ContainsKeyValuePair()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;

            Assert.True( rockDynamic.Contains( new System.Collections.Generic.KeyValuePair<string, object>( "A", 123 ) ) );
        }

        [Fact]
        public void ContainsKeyValuePairForDifferentValue()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["A"] = 123;

            Assert.False( rockDynamic.Contains( new System.Collections.Generic.KeyValuePair<string, object>( "A", 456 ) ) );
        }

        [Fact]
        public void StoreKeyIsCaseSensitive()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic["a"] = 123;
            Assert.False( rockDynamic.ContainsKey( "A" ) );
        }

        [Fact]
        public void SetPropertyIsCaseSensitive()
        {
            dynamic rockDynamic = new RockDynamic();
            rockDynamic.a = 123;
            Assert.ThrowsAny<Exception>( () => rockDynamic.A );
        }
    }
}
