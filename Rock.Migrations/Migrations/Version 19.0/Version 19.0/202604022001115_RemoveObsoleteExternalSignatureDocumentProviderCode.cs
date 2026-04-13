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
    public partial class RemoveObsoleteExternalSignatureDocumentProviderCode : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
                -- Remove the obsolete Rock.Jobs.ProcessSignatureDocuments job
                DELETE FROM [ServiceJob] WHERE [Guid] = '77B2F2D4-D188-4716-9A79-F93AD4673F8C'

                -- Inactivate any SignatureDocumentTemplate that is using an external provider 
                -- (since those templates won't work anymore)
                UPDATE [SignatureDocumentTemplate]
                SET
                    [IsActive] = 0,
                    [Name] = LEFT([Name], 100 - LEN(' (Legacy)')) + ' (Legacy)'
                WHERE
                    [ProviderEntityTypeId] IS NOT NULL
                    AND [LavaTemplate] IS NULL
                    AND [IsActive] = 1;

                -- Perform cleanup for HistoryLog block header that contains invalid HTML/icon
                DECLARE @BlockTypeId INT;
                DECLARE @AttributeId INT;

                SELECT @BlockTypeId = [Id]
                FROM [BlockType]
                WHERE [Guid] = 'C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0';

                IF @BlockTypeId IS NOT NULL
                BEGIN
                    SELECT @AttributeId = [Id]
                    FROM [Attribute]
                    WHERE [Key] = 'Heading'
                        AND [EntityTypeQualifierColumn] = 'BlockTypeId'
                        AND [EntityTypeQualifierValue] = CAST(@BlockTypeId AS VARCHAR(50));

                    IF @AttributeId IS NOT NULL
                    BEGIN
                        UPDATE [AttributeValue]
                        SET
                            [Value] = REPLACE([Value], '<i class=''fa fa-history''></i> ', ''),
                            [IsPersistedValueDirty] = 1
                        WHERE [AttributeId] = @AttributeId
                            AND [Value] LIKE '%<i class=''fa fa-history''></i> %';
                    END
                END
            " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
