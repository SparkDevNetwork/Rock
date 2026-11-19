// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;
using System.Reflection;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Data;
using Rock.Model;

namespace Rock.Tests.Rest
{
    /// <summary>
    /// Tests the marker that limits a REST client to setting file properties
    /// to the stored file or a file the client uploaded.
    /// </summary>
    [TestClass]
    public class UploadedBinaryFileOnlyAttributeTests
    {
        [TestMethod]
        public void GetPropertyNames_Site_ReturnsOnlyTheImageProperties()
        {
            var names = UploadedBinaryFileOnlyAttribute.GetPropertyNames( typeof( Site ) );

            CollectionAssert.AreEquivalent(
                new[]
                {
                    nameof( Site.ConfigurationMobilePhoneBinaryFileId ),
                    nameof( Site.ConfigurationMobileTabletBinaryFileId ),
                    nameof( Site.ThumbnailBinaryFileId ),
                    nameof( Site.FavIconBinaryFileId ),
                    nameof( Site.SiteLogoBinaryFileId )
                },
                names.ToArray() );
        }

        [TestMethod]
        public void GetPropertyNames_CommunicationTemplateAttachment_ReturnsTheFileProperty()
        {
            var names = UploadedBinaryFileOnlyAttribute.GetPropertyNames( typeof( CommunicationTemplateAttachment ) );

            CollectionAssert.AreEquivalent(
                new[] { nameof( CommunicationTemplateAttachment.BinaryFileId ) },
                names.ToArray() );
        }

        [TestMethod]
        public void GetPropertyNames_ModelWithoutAttribute_ReturnsEmpty()
        {
            Assert.AreEqual( 0, UploadedBinaryFileOnlyAttribute.GetPropertyNames( typeof( DefinedValue ) ).Count );
            Assert.AreEqual( 0, UploadedBinaryFileOnlyAttribute.GetPropertyNames( typeof( BinaryFile ) ).Count );
        }

        [TestMethod]
        public void AreValuesAllowed_ModelWithoutAttribute_ReturnsTrueWithoutTouchingTheContext()
        {
            // A null context would throw if the check asked it for the entry,
            // so this proves models without the attribute are skipped before
            // EF runs change detection or the database is queried.
            var definedValue = new DefinedValue();

            Assert.IsTrue( UploadedBinaryFileOnlyAttribute.AreValuesAllowed( null, definedValue, typeof( DefinedValue ), null ) );
        }

        [TestMethod]
        public void GetPropertyNames_SameType_ReturnsCachedResult()
        {
            var first = UploadedBinaryFileOnlyAttribute.GetPropertyNames( typeof( Site ) );
            var second = UploadedBinaryFileOnlyAttribute.GetPropertyNames( typeof( Site ) );

            Assert.AreSame( first, second );
        }

        [TestMethod]
        public void MarkedProperties_AreAllIntegers()
        {
            // The check reads each marked value as a nullable binary file
            // identifier, so a marked property must be an int or an int?.
            Type[] types;
            try
            {
                types = typeof( BinaryFile ).Assembly.GetTypes();
            }
            catch ( ReflectionTypeLoadException ex )
            {
                types = ex.Types.Where( t => t != null ).ToArray();
            }

            var markedProperties = types
                .SelectMany( t => t.GetProperties( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly ) )
                .Where( p => p.IsDefined( typeof( UploadedBinaryFileOnlyAttribute ), true ) )
                .ToList();

            Assert.AreNotEqual( 0, markedProperties.Count );

            foreach ( var property in markedProperties )
            {
                Assert.IsTrue( property.PropertyType == typeof( int ) || property.PropertyType == typeof( int? ), $"{property.DeclaringType.FullName}.{property.Name} must be an int or an int? to carry UploadedBinaryFileOnly." );
            }
        }
    }
}
