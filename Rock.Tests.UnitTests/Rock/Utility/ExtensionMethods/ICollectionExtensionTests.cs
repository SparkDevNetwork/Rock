using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;

namespace Rock.Tests.UnitTests.Rock.Utility.ExtensionMethods
{
    [TestClass]
    public class ICollectionExtensionTests
    {
        [TestMethod]
        public void RemoveAll_ShouldNotExceptionIfParametersAreNull()
        {
            Collection<string> masterCollection = null;
            Collection<string> deleteCollection = null;

            masterCollection.RemoveAll( deleteCollection );

            deleteCollection = new Collection<string> { "test" };

            masterCollection.RemoveAll( deleteCollection );

            masterCollection = new Collection<string> { "test" };
            masterCollection.RemoveAll( null );
        }

        [TestMethod]
        public void RemoveAll_ShouldRemoveCorrectObjects()
        {
            var item1 = "test 1";
            var item2 = "test 2";
            var item3 = "test 3";

            var masterCollection = new Collection<string> { item1, item2, item3 };
            var deleteCollection = new Collection<string> { item2 };

            masterCollection.RemoveAll( deleteCollection );

            Assert.That.AreEqual( 2, masterCollection.Count );
            Assert.That.IsTrue( masterCollection.Contains( item1 ) );
            Assert.That.IsTrue( masterCollection.Contains( item3 ) );
        }

        [TestMethod]
        public void RemoveAll_ShouldDoesNotRemoveIncorrectObjects()
        {
            var item1 = "test 1";
            var item2 = "test 2";
            var item3 = "test 3";
            var item4 = "test 4";

            var masterCollection = new Collection<string> { item1, item2, item3 };
            var deleteCollection = new Collection<string> { item2, item4 };

            masterCollection.RemoveAll( deleteCollection );

            Assert.That.AreEqual( 2, masterCollection.Count );
            Assert.That.IsTrue( masterCollection.Contains( item1 ) );
            Assert.That.IsTrue( masterCollection.Contains( item3 ) );
        }
    }
}
