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
    public partial class FixSecurityCodeRandomizedFieldType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"
                DECLARE @SecurityCodeNumericRandomId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'FD72C08A-81E9-4D93-9370-2BA1B4192601')
                DECLARE @BooleanFieldTypeId INT = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '1EDAFDED-DFE6-4334-B019-6EECBA89E05A')

                UPDATE [Attribute] SET [FieldTypeId] = @BooleanFieldTypeId WHERE [Id] = @SecurityCodeNumericRandomId

                UPDATE [AttributeValue] SET [Value] = 'False' WHERE AttributeId = @SecurityCodeNumericRandomId AND [Value] = '0'
                UPDATE [AttributeValue] SET [Value] = 'True' WHERE AttributeId = @SecurityCodeNumericRandomId AND [Value] = '1'
                UPDATE [AttributeValue] SET [Value] = '' WHERE AttributeId = @SecurityCodeNumericRandomId AND [Value] NOT IN ('False', 'True')" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
