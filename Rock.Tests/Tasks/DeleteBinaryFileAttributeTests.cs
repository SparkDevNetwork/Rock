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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Configuration;
using Rock.Model;
using Rock.Tasks;
using Rock.Tests.Shared.TestFramework;

namespace Rock.Tests.Tasks
{
    [TestClass]
    public class DeleteBinaryFileAttributeTests
    {
        /// <summary>
        /// A binary file that is not referenced by any attribute or attribute
        /// value should be deleted when the task runs with the default
        /// (non-contains) search.
        /// </summary>
        [TestMethod]
        public void Execute_ShouldDeleteBinaryFile_WhenFileHasNoReferences()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var binaryFileGuid = Guid.NewGuid();

                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = binaryFileGuid,
                    FileName = "unreferenced.txt",
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = binaryFileGuid,
                    UseContainsSearch = false,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var fileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == binaryFileGuid );

                Assert.IsFalse( fileStillExists, "The binary file should have been deleted." );
            }
        }

        /// <summary>
        /// A binary file that is still referenced as the default value of a
        /// binary-file attribute should not be deleted when the task runs with
        /// the default (non-contains) search.
        /// </summary>
        [TestMethod]
        public void Execute_ShouldNotDeleteBinaryFile_WhenAttributeDefaultValueMatches()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var binaryFileGuid = Guid.NewGuid();

                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = binaryFileGuid,
                    FileName = "referenced.txt",
                } );

                // The task only considers attributes whose field type is one of
                // the known binary-file field types, so seed that field type.
                var binaryFileFieldType = new FieldType
                {
                    Id = 1,
                    Guid = SystemGuid.FieldType.BINARY_FILE.AsGuid(),
                };

                rockContext.Set<FieldType>().Add( binaryFileFieldType );

                // An attribute whose default value points at the binary file
                // should keep the file alive.
                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    FieldTypeId = binaryFileFieldType.Id,
                    DefaultValue = binaryFileGuid.ToString(),
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = binaryFileGuid,
                    UseContainsSearch = false,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var fileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == binaryFileGuid );

                Assert.IsTrue( fileStillExists, "The binary file should not have been deleted because it is still referenced." );
            }
        }

        /// <summary>
        /// A binary file that is still referenced by an attribute value should
        /// not be deleted when the task runs with the default (non-contains)
        /// search.
        /// </summary>
        [TestMethod]
        public void Execute_ShouldNotDeleteBinaryFile_WhenAttributeValueMatches()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var binaryFileGuid = Guid.NewGuid();

                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = binaryFileGuid,
                    FileName = "referenced.txt",
                } );

                var binaryFileFieldType = new FieldType
                {
                    Id = 1,
                    Guid = SystemGuid.FieldType.BINARY_FILE.AsGuid(),
                };

                rockContext.Set<FieldType>().Add( binaryFileFieldType );

                // The attribute's default value is empty so that only the
                // attribute value branch can keep the file alive.
                var binaryFileAttribute = new Rock.Model.Attribute
                {
                    Id = 1,
                    FieldTypeId = binaryFileFieldType.Id,
                    DefaultValue = string.Empty,
                };

                rockContext.Set<Rock.Model.Attribute>().Add( binaryFileAttribute );

                // The Attribute navigation property must be set explicitly. The
                // mock DbSet does not perform EF relationship fixup, so the
                // task's "a.Attribute.FieldTypeId" lookup relies on this.
                rockContext.Set<AttributeValue>().Add( new AttributeValue
                {
                    Id = 1,
                    AttributeId = binaryFileAttribute.Id,
                    Attribute = binaryFileAttribute,
                    Value = binaryFileGuid.ToString(),
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = binaryFileGuid,
                    UseContainsSearch = false,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var fileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == binaryFileGuid );

                Assert.IsTrue( fileStillExists, "The binary file should not have been deleted because it is still referenced by an attribute value." );
            }
        }

        /// <summary>
        /// A binary file whose guid appears within a delimited attribute default
        /// value (e.g. "42,&lt;guid&gt;") should not be deleted when the task
        /// runs with the contains search.
        /// </summary>
        [TestMethod]
        public void Execute_ShouldNotDeleteBinaryFile_WhenContainsSearchAttributeDefaultValueMatches()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var binaryFileGuid = Guid.NewGuid();

                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = binaryFileGuid,
                    FileName = "referenced.txt",
                } );

                var binaryFileFieldType = new FieldType
                {
                    Id = 1,
                    Guid = SystemGuid.FieldType.BINARY_FILE.AsGuid(),
                };

                rockContext.Set<FieldType>().Add( binaryFileFieldType );

                // The guid is embedded within a larger delimited value so only a
                // contains search will find it.
                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    FieldTypeId = binaryFileFieldType.Id,
                    DefaultValue = $"42,{binaryFileGuid}",
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = binaryFileGuid,
                    UseContainsSearch = true,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var fileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == binaryFileGuid );

                Assert.IsTrue( fileStillExists, "The binary file should not have been deleted because its guid is contained in an attribute default value." );
            }
        }

        /// <summary>
        /// A binary file whose guid appears within a delimited attribute value
        /// (e.g. "42,&lt;guid&gt;") should not be deleted when the task runs
        /// with the contains search.
        /// </summary>
        [TestMethod]
        public void Execute_ShouldNotDeleteBinaryFile_WhenContainsSearchAttributeValueMatches()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var binaryFileGuid = Guid.NewGuid();

                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = binaryFileGuid,
                    FileName = "referenced.txt",
                } );

                var binaryFileFieldType = new FieldType
                {
                    Id = 1,
                    Guid = SystemGuid.FieldType.BINARY_FILE.AsGuid(),
                };

                rockContext.Set<FieldType>().Add( binaryFileFieldType );

                // The attribute's default value is empty so that only the
                // attribute value branch can keep the file alive.
                var binaryFileAttribute = new Rock.Model.Attribute
                {
                    Id = 1,
                    FieldTypeId = binaryFileFieldType.Id,
                    DefaultValue = string.Empty,
                };

                rockContext.Set<Rock.Model.Attribute>().Add( binaryFileAttribute );

                // The Attribute navigation property must be set explicitly. The
                // mock DbSet does not perform EF relationship fixup, so the
                // task's "a.Attribute.FieldTypeId" lookup relies on this.
                rockContext.Set<AttributeValue>().Add( new AttributeValue
                {
                    Id = 1,
                    AttributeId = binaryFileAttribute.Id,
                    Attribute = binaryFileAttribute,
                    Value = $"42,{binaryFileGuid}",
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = binaryFileGuid,
                    UseContainsSearch = true,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var fileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == binaryFileGuid );

                Assert.IsTrue( fileStillExists, "The binary file should not have been deleted because its guid is contained in an attribute value." );
            }
        }

        /// <summary>
        /// A binary file whose guid only appears within a delimited attribute
        /// default value (e.g. "42,&lt;guid&gt;") should still be deleted when
        /// the task runs with the default (non-contains) search, because the
        /// exact-match comparison does not treat the delimited value as a
        /// reference. This verifies the distinction between the contains and
        /// non-contains searches.
        /// </summary>
        [TestMethod]
        public void Execute_ShouldDeleteBinaryFile_WhenNonContainsSearchAndOnlyDelimitedDefaultValueMatches()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var binaryFileGuid = Guid.NewGuid();

                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = binaryFileGuid,
                    FileName = "unreferenced.txt",
                } );

                var binaryFileFieldType = new FieldType
                {
                    Id = 1,
                    Guid = SystemGuid.FieldType.BINARY_FILE.AsGuid(),
                };

                rockContext.Set<FieldType>().Add( binaryFileFieldType );

                // The guid is embedded within a larger delimited value, so an
                // exact-match (non-contains) search should not consider this a
                // reference and the file should be deleted.
                rockContext.Set<Rock.Model.Attribute>().Add( new Rock.Model.Attribute
                {
                    Id = 1,
                    FieldTypeId = binaryFileFieldType.Id,
                    DefaultValue = $"42,{binaryFileGuid}",
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = binaryFileGuid,
                    UseContainsSearch = false,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var fileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == binaryFileGuid );

                Assert.IsFalse( fileStillExists, "The binary file should have been deleted because the non-contains search does not match a delimited value." );
            }
        }

        /// <summary>
        /// When the requested binary file does not exist the task should return
        /// without deleting anything.
        /// </summary>
        [TestMethod]
        public void Execute_ShouldNotDeleteAnything_WhenBinaryFileDoesNotExist()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var existingFileGuid = Guid.NewGuid();
                var missingFileGuid = Guid.NewGuid();

                // Seed an unrelated file to confirm the task does not touch it
                // when the requested file cannot be found.
                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = existingFileGuid,
                    FileName = "unrelated.txt",
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = missingFileGuid,
                    UseContainsSearch = false,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var unrelatedFileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == existingFileGuid );

                Assert.IsTrue( unrelatedFileStillExists, "The unrelated binary file should not have been deleted." );
            }
        }

        /// <summary>
        /// A binary file that is not referenced by any attribute or attribute
        /// value should be deleted when the task runs with the contains search.
        /// This verifies the contains branch falls through to the delete when no
        /// reference is found.
        /// </summary>
        [TestMethod]
        public void Execute_ShouldDeleteBinaryFile_WhenContainsSearchAndNoReferences()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var binaryFileGuid = Guid.NewGuid();

                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = binaryFileGuid,
                    FileName = "unreferenced.txt",
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = binaryFileGuid,
                    UseContainsSearch = true,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var fileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == binaryFileGuid );

                Assert.IsFalse( fileStillExists, "The binary file should have been deleted." );
            }
        }

        /// <summary>
        /// A binary file referenced by an attribute value whose field type is
        /// not one of the binary-file field types (e.g. Text) should still be
        /// deleted, because only the binary-file field types are considered when
        /// looking for references.
        /// </summary>
        [TestMethod]
        public void Execute_ShouldDeleteBinaryFile_WhenAttributeValueFieldTypeIsNotBinaryFile()
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var binaryFileGuid = Guid.NewGuid();

                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = binaryFileGuid,
                    FileName = "referenced.txt",
                } );

                // Seed a binary-file field type so the list of binary-file field
                // type ids is populated, proving the file survives or not based on
                // the field type filter rather than an empty list.
                rockContext.Set<FieldType>().Add( new FieldType
                {
                    Id = 1,
                    Guid = SystemGuid.FieldType.BINARY_FILE.AsGuid(),
                } );

                var textFieldType = new FieldType
                {
                    Id = 2,
                    Guid = SystemGuid.FieldType.TEXT.AsGuid(),
                };

                rockContext.Set<FieldType>().Add( textFieldType );

                // A text attribute whose value happens to match the file guid
                // should not count as a reference.
                var textAttribute = new Rock.Model.Attribute
                {
                    Id = 1,
                    FieldTypeId = textFieldType.Id,
                    DefaultValue = string.Empty,
                };

                rockContext.Set<Rock.Model.Attribute>().Add( textAttribute );

                rockContext.Set<AttributeValue>().Add( new AttributeValue
                {
                    Id = 1,
                    AttributeId = textAttribute.Id,
                    Attribute = textAttribute,
                    Value = binaryFileGuid.ToString(),
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = binaryFileGuid,
                    UseContainsSearch = false,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var fileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == binaryFileGuid );

                Assert.IsFalse( fileStillExists, "The binary file should have been deleted because the referencing attribute value is not a binary-file field type." );
            }
        }

        /// <summary>
        /// A binary file referenced by an attribute value of any of the
        /// binary-file field types should not be deleted.
        /// </summary>
        /// <param name="fieldTypeGuid">The unique identifier of the binary-file field type to test.</param>
        [TestMethod]
        [DataRow( SystemGuid.FieldType.AUDIO_FILE )]
        [DataRow( SystemGuid.FieldType.BACKGROUNDCHECK )]
        [DataRow( SystemGuid.FieldType.BINARY_FILE )]
        [DataRow( SystemGuid.FieldType.FILE )]
        [DataRow( SystemGuid.FieldType.IMAGE )]
        [DataRow( SystemGuid.FieldType.VIDEO_FILE )]
        public void Execute_ShouldNotDeleteBinaryFile_WhenAttributeValueFieldTypeIsBinaryFile( string fieldTypeGuid )
        {
            using ( var scope = TestHelper.CreateScopedRockAppWithMockDatabase() )
            {
                var rockContext = scope.App.CreateRockContext();
                var binaryFileGuid = Guid.NewGuid();

                rockContext.Set<BinaryFile>().Add( new BinaryFile
                {
                    Id = 1,
                    Guid = binaryFileGuid,
                    FileName = "referenced.txt",
                } );

                var binaryFileFieldType = new FieldType
                {
                    Id = 1,
                    Guid = fieldTypeGuid.AsGuid(),
                };

                rockContext.Set<FieldType>().Add( binaryFileFieldType );

                var binaryFileAttribute = new Rock.Model.Attribute
                {
                    Id = 1,
                    FieldTypeId = binaryFileFieldType.Id,
                    DefaultValue = string.Empty,
                };

                rockContext.Set<Rock.Model.Attribute>().Add( binaryFileAttribute );

                // The Attribute navigation property must be set explicitly. The
                // mock DbSet does not perform EF relationship fixup, so the
                // task's "a.Attribute.FieldTypeId" lookup relies on this.
                rockContext.Set<AttributeValue>().Add( new AttributeValue
                {
                    Id = 1,
                    AttributeId = binaryFileAttribute.Id,
                    Attribute = binaryFileAttribute,
                    Value = binaryFileGuid.ToString(),
                } );

                var message = new DeleteBinaryFileAttribute.Message
                {
                    BinaryFileGuid = binaryFileGuid,
                    UseContainsSearch = false,
                };

                new DeleteBinaryFileAttribute().Execute( message );

                var fileStillExists = rockContext.Set<BinaryFile>()
                    .Any( f => f.Guid == binaryFileGuid );

                Assert.IsTrue( fileStillExists, $"The binary file should not have been deleted because it is referenced by an attribute value of field type {fieldTypeGuid}." );
            }
        }
    }
}
