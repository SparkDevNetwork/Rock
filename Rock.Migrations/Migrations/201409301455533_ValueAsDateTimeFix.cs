// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class ValueAsDateTimeFix : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropColumn( "dbo.AttributeValue", "ValueAsDateTime" );
            DropColumn( "dbo.AttributeValue", "ValueAsNumeric" );
            Sql( @"alter table AttributeValue add ValueAsDateTime as case when (len(value) < 50 and ISDATE( value) = 1) then convert(datetime, value) else null end" );
            Sql( @"alter table AttributeValue add ValueAsNumeric as case when (len(value) < 100 and ISNUMERIC( value) = 1 and value not like '%[^0-9.]%') then convert(numeric(38,10), value ) else null end" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
