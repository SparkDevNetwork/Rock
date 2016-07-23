﻿// <copyright>
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
    public partial class AttributeValueAsDateTimeIndex : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( MigrationSQL._201506130028565_AttributeValueAsDateTimeIndex_CreateColumns );
            Sql( MigrationSQL._201506130028565_AttributeValueAsDateTimeIndex_CreateTrigger );
            Sql( MigrationSQL._201506130028565_AttributeValueAsDateTimeIndex_UpdateValues );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // just enough to let you down and back up
            Sql( "DROP INDEX IX_ValueAsNumeric on AttributeValue" );
            Sql( "DROP INDEX IX_ValueAsDateTime on AttributeValue" );
            Sql( "DROP TRIGGER [dbo].[tgrAttributeValue_InsertUpdate]" );
        }
    }
}
