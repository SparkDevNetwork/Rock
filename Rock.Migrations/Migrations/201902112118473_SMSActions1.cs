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
    public partial class SMSActions1 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( $@"UPDATE [DefinedType]
SET [Name] = 'Text To Workflow (Legacy)'
, [Description] = 'Matches SMS phones and keywords to launch workflows of various types. This method of launching workflows from incoming text messages is now considered legacy. Please look at migrating to the new SMS Actions instead.'
WHERE [Guid] = '{ Rock.SystemGuid.DefinedType.TEXT_TO_WORKFLOW }'" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $@"UPDATE [DefinedType]
SET [Name] = 'Text To Workflow'
, [Description] = 'Matches SMS phones and keywords to launch workflows of various types'
WHERE [Guid] = '{ Rock.SystemGuid.DefinedType.TEXT_TO_WORKFLOW }'" );
        }
    }
}
