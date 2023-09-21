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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Rollup_20230921 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ReplaceBinaryFileTypeForPersonPhotos();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        // JMH: Replaces the DEFAULT BinaryFileType associated with Person.Photos with the PERSON_IMAGE BinaryFileType.
        private void ReplaceBinaryFileTypeForPersonPhotos()
        {
            Sql( $@"DECLARE @DefaultBinaryFileTypeId AS INT = (SELECT TOP 1 [Id] FROM [BinaryFileType] WHERE [Guid] = '{Rock.SystemGuid.BinaryFiletype.DEFAULT}')
                    DECLARE @PersonImageBinaryFileTypeId AS INT = (SELECT TOP 1 [Id] FROM [BinaryFileType] WHERE [Guid] = '{Rock.SystemGuid.BinaryFiletype.PERSON_IMAGE}')

                    UPDATE BF
                       SET BF.[BinaryFileTypeId] = @PersonImageBinaryFileTypeId
                      FROM [BinaryFile] BF
                     INNER JOIN [Person] P ON BF.[Id] = P.[PhotoId]
                     WHERE BF.[BinaryFileTypeId] = @DefaultBinaryFileTypeId" );
        }
    }
}
