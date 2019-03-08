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
using Rock.Checkr.Constants;
using Rock.Plugin;
using Rock.SystemGuid;

namespace Rock.Migrations
{
    [MigrationNumber( 6, "1.8.0" )]
    public class Checkr_DeactivatePackages : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            string bioActions = SqlScalar( string.Format( @"
                DECLARE @BlockId int
                SET @BlockId = (SELECT [Id] FROM [Block] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                SELECT TOP(1) Value
                FROM [AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @BlockId", Block.BIO, SystemGuid.Attribute.BIO_WORKFLOWACTION ) ).ToStringSafe().ToUpperInvariant();

            if (bioActions.Contains( WorkflowType.PROTECTMYMINISTRY.ToUpperInvariant() ) )
            {
                Sql( string.Format( @"
                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                UPDATE [DefinedValue]
                SET
                    [IsActive] = 0
                WHERE
                    [DefinedTypeId] = @DefinedTypeId AND [ForeignId] = 2", DefinedType.BACKGROUND_CHECK_TYPES ) );
            }
            else if ( bioActions.Contains( CheckrSystemGuid.CHECKR_WORKFLOW_TYPE.ToUpperInvariant() ) )
            {
                Sql( string.Format( @"
                DECLARE @DefinedTypeId int
                SET @DefinedTypeId = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}')

                UPDATE [DefinedValue]
                SET
                    [IsActive] = 0
                WHERE
                    [DefinedTypeId] = @DefinedTypeId AND [ForeignId] = 1", DefinedType.BACKGROUND_CHECK_TYPES ) );
            }
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
