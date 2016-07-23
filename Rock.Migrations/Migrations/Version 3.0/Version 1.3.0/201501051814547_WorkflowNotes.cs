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
    public partial class WorkflowNotes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.WorkflowActionForm", "AllowNotes", c => c.Boolean());

            Sql( @"
    ALTER TABLE [dbo].[AttributeValue] DROP COLUMN [ValueAsDateTime]

    ALTER TABLE [dbo].[AttributeValue] ADD [ValueAsDateTime] AS CASE 
            -- make sure it isn't a big value or a date range, etc
            WHEN LEN([value]) <= 33
                THEN CASE 
                        -- is it an ISO-8601
                        WHEN VALUE LIKE '____-__-__T__:__:__%'
                            THEN CONVERT(DATETIME, CONVERT(DATETIMEOFFSET, [value]))
                        -- is it some other value SQL Date
                        WHEN ISDATE([VALUE]) = 1
                            THEN CONVERT(DATETIME, [VALUE])
                        ELSE NULL
                        END
            ELSE NULL    
            END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.WorkflowActionForm", "AllowNotes");
        }
    }
}
