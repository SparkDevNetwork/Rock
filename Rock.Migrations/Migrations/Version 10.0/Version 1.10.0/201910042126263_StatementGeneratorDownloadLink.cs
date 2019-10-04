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
    public partial class StatementGeneratorDownloadLink : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixDiscProfilePersonAttribute();
            UpdateStatementGeneratorDownloadLinkUp();
            CleanupMigrationHistory();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateStatementGeneratorDownloadLinkDown();
        }

        /// <summary>
        /// ED: Add the Defined Type "DISC Results" to the "DISC Profile" person attribute.
        /// </summary>
        private void FixDiscProfilePersonAttribute()
        {
            Sql( $@"
                DECLARE @DiscProfileAttributeId INT = (SELECT [Id] FROM Attribute WHERE [Guid] = '{Rock.SystemGuid.Attribute.PERSON_DISC_PROFILE}')
                DECLARE @DiscResultsDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{Rock.SystemGuid.DefinedType.DISC_RESULTS_TYPE}')
                DECLARE @DefinedTypeAttributeQualifierId INT = (SELECT [Id] FROM [AttributeQualifier] WHERE [AttributeId] = @DiscProfileAttributeId AND [Key] = 'definedtype')
                IF (@DefinedTypeAttributeQualifierId IS NULL)
                BEGIN
                    -- We don't have he qualifier so insert it.
                    INSERT INTO AttributeQualifier([IsSystem], [AttributeId], [Key], [Value], [Guid])
                    VALUES(1, @DiscProfileAttributeId, 'definedtype', @DiscResultsDefinedTypeId, NEWID())
                END
                ELSE
                BEGIN
                    -- We have a qualifier so make sure the value is correct
                    UPDATE [AttributeQualifier]
                    SET [Value] = @DiscResultsDefinedTypeId
                    WHERE [Id] = @DefinedTypeAttributeQualifierId
                END" );
        }

        private void UpdateStatementGeneratorDownloadLinkUp()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.10.0/statementgenerator.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }
        
        private void UpdateStatementGeneratorDownloadLinkDown()
        {
            Sql( @"
                DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.8.0/statementgenerator.exe'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// Cleanups the migration history records except the last one.
        /// </summary>
        private void CleanupMigrationHistory()
        {
            Sql( @"
                UPDATE [dbo].[__MigrationHistory]
                SET [Model] = 0x
                WHERE MigrationId < (SELECT TOP 1 MigrationId FROM __MigrationHistory ORDER BY MigrationId DESC)" );
        }
    }
}
