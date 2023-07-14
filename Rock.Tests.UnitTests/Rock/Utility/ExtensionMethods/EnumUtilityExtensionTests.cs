using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using Rock.ViewModels.Utility;

namespace Rock.Tests.UnitTests.Rock.Utility.ExtensionMethods
{
    [TestClass]
    public class EnumUtilityExtensionTests
    {
        private enum TestEnum
        {
            TestElementOne,
            TestElementTwo
        }

        private enum TestEnumWithDescription
        {
            [System.ComponentModel.Description( "Test Element One Description" )]
            TestElementOne,

            [System.ComponentModel.Description( "Test Element Two Description" )]
            TestElementTwo
        }

        [TestMethod]
        public void ConvertEnumToListItemBagList()
        {
            var expectedListItemBagList = new ListItemBag[] {
                new ListItemBag { Text = "Test Element One", Value = "0" },
                new ListItemBag { Text = "Test Element Two", Value = "1" }
            };

            var actualListItemBag = typeof( TestEnum ).ToEnumListItemBag();

            Assert.AreEqual( expectedListItemBagList.Length, actualListItemBag.Count);
            Assert.AreEqual( expectedListItemBagList[0].Text, actualListItemBag[0].Text );
            Assert.AreEqual( expectedListItemBagList[1].Text, actualListItemBag[1].Text );
            Assert.AreEqual( expectedListItemBagList[0].Value, actualListItemBag[0].Value );
            Assert.AreEqual( expectedListItemBagList[1].Value, actualListItemBag[1].Value );
        }

        [TestMethod]
        public void ConvertEnumWithDescriptionToListItemBagList()
        {
            var expectedListItemBagList = new ListItemBag[] {
                new ListItemBag { Text = "Test Element One Description", Value = "0" },
                new ListItemBag { Text = "Test Element Two Description", Value = "1" }
            };

            var actualListItemBag = typeof( TestEnumWithDescription ).ToEnumListItemBag();

            Assert.AreEqual( expectedListItemBagList.Length, actualListItemBag.Count);
            Assert.AreEqual( expectedListItemBagList[0].Text, actualListItemBag[0].Text );
            Assert.AreEqual( expectedListItemBagList[1].Text, actualListItemBag[1].Text );
            Assert.AreEqual( expectedListItemBagList[0].Value, actualListItemBag[0].Value );
            Assert.AreEqual( expectedListItemBagList[1].Value, actualListItemBag[1].Value );
        }
    }
}
