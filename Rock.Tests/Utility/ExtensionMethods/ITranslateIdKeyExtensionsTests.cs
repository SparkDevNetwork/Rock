using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.ViewModels.Utility;

namespace Rock.Tests.Utility.ExtensionMethods
{
    [TestClass]
    public class ITranslateIdKeyExtensionsTests
    {
        [TestMethod]
        public void TranslateIdToIdKey_WithNullInstance_DoesNotThrow()
        {
            ITranslateIdKey instance = null;

            instance.TranslateIdToIdKey();
        }

        [TestMethod]
        public void TranslateIdToIdKey_WithNullId_DoesNotSetIdKey()
        {
            var instance = new TestTranslateIdKey
            {
                Id = null
            };

            instance.TranslateIdToIdKey();

            Assert.IsNull( instance.IdKey );
            Assert.IsNull( instance.Id );
        }

        [TestMethod]
        public void TranslateIdToIdKey_WithValidId_SetsIdKeyAndNullsId()
        {
            var instance = new TestTranslateIdKey
            {
                Id = 456
            };

            instance.TranslateIdToIdKey();

            Assert.IsNotNull( instance.IdKey );
            Assert.IsNotEmpty( instance.IdKey );
            Assert.AreEqual( 456.AsIdKey(), instance.IdKey );
            Assert.IsNull( instance.Id );
        }

        [TestMethod]
        public void TranslateIdToIdKey_WithMultipleItems_TranslatesAllItems()
        {
            var instances = new TestTranslateIdKey[]
            {
                new TestTranslateIdKey { Id = 123 },
                new TestTranslateIdKey { Id = null },
                new TestTranslateIdKey { Id = 789 }
            };

            instances.TranslateIdToIdKey();

            Assert.AreEqual( 123.AsIdKey(), instances[0].IdKey );
            Assert.IsNull( instances[0].Id );

            Assert.IsNull( instances[1].IdKey );
            Assert.IsNull( instances[1].Id );

            Assert.AreEqual( 789.AsIdKey(), instances[2].IdKey );
            Assert.IsNull( instances[2].Id );
        }

        private class TestTranslateIdKey : ITranslateIdKey
        {
            public int? Id { get; set; }

            public string IdKey { get; set; }
        }
    }
}
