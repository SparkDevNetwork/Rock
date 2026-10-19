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
    /// Tests the marker that keeps REST clients from setting properties that
    /// only Rock may set, such as where a BinaryFile is stored.
    /// </summary>
    [TestClass]
    public class NotClientWritableAttributeTests
    {
        [TestMethod]
        public void GetPropertyNames_BinaryFile_ReturnsOnlyTheStorageProperties()
        {
            var names = NotClientWritableAttribute.GetPropertyNames( typeof( BinaryFile ) );

            CollectionAssert.AreEquivalent(
                new[] { nameof( BinaryFile.Path ), nameof( BinaryFile.StorageEntityTypeId ), nameof( BinaryFile.StorageEntitySettings ) },
                names.ToArray() );
        }

        [TestMethod]
        public void GetPropertyNames_ModelWithoutAttribute_ReturnsEmpty()
        {
            Assert.AreEqual( 0, NotClientWritableAttribute.GetPropertyNames( typeof( DefinedValue ) ).Count );
            Assert.AreEqual( 0, NotClientWritableAttribute.GetPropertyNames( typeof( BinaryFileType ) ).Count );
        }

        [TestMethod]
        public void ClearAndRestore_ModelWithoutAttribute_DoNotTouchTheContext()
        {
            // A null context would throw if the helpers asked it for the
            // entry, so this proves models without the attribute are skipped
            // before EF runs change detection.
            var definedValue = new DefinedValue();

            NotClientWritableAttribute.ClearValues( null, definedValue, typeof( DefinedValue ) );
            NotClientWritableAttribute.RestoreOriginalValues( null, definedValue, typeof( DefinedValue ) );
        }

        [TestMethod]
        public void GetPropertyNames_SameType_ReturnsCachedResult()
        {
            var first = NotClientWritableAttribute.GetPropertyNames( typeof( BinaryFile ) );
            var second = NotClientWritableAttribute.GetPropertyNames( typeof( BinaryFile ) );

            Assert.AreSame( first, second );
        }

        [TestMethod]
        public void MarkedProperties_AreAllNullable()
        {
            // ClearValues sets every marked property to null, so a marked
            // property must be a reference type or a nullable value type.
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
                .Where( p => p.IsDefined( typeof( NotClientWritableAttribute ), true ) )
                .ToList();

            Assert.AreEqual( 3, markedProperties.Count );

            foreach ( var property in markedProperties )
            {
                var type = property.PropertyType;
                var isNullable = !type.IsValueType || Nullable.GetUnderlyingType( type ) != null;

                Assert.IsTrue( isNullable, $"{property.DeclaringType.FullName}.{property.Name} must be nullable to carry NotClientWritable." );
            }
        }
    }
}
