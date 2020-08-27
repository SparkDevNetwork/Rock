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
    public partial class AddDocumentsToPersonMerge : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdatePersonMergeSproc();
            UpdateOrphanedDocuments();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// Updates the person merge sproc to update the documents for the person.
        /// </summary>
        private void UpdatePersonMergeSproc()
        {
            Sql( MigrationSQL._202008272036178_AddDocumentsToPersonMerge_spCrm_PersonMerge );
        }

        /// <summary>
        /// Updates person documents that were orphaned during a merge to the new person if it can be found.
        /// </summary>
        private void UpdateOrphanedDocuments()
        {
            Sql( @"
                DECLARE @PersonEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person' )

                UPDATE d
                SET [EntityId] = pa.[PersonId]
                FROM [Document] d
                JOIN [DocumentType] dt ON dt.[Id] = d.[DocumentTypeId]
                JOIN [PersonAlias] pa ON pa.[AliasPersonId] = d.[EntityId]
                WHERE dt.[EntityTypeId] = @PersonEntityTypeId
	                AND d.[EntityId] NOT IN (SELECT [Id] FROM [dbo].[Person])" );
        }
    }
}
