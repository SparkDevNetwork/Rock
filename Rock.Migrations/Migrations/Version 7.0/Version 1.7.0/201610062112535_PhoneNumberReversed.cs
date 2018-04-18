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
    public partial class PhoneNumberReversed : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    IF COL_LENGTH('PhoneNumber', 'NumberReversed') IS NULL 
        ALTER TABLE [dbo].[PhoneNumber] ADD [NumberReversed] AS( reverse([Number] ) ) PERSISTED
" );

            Sql( @"
    IF NOT EXISTS( SELECT * FROM sys.indexes WHERE name = 'IX_NumberReversed' AND object_id = OBJECT_ID( N'[dbo].[PhoneNumber]' ) ) 
        CREATE NONCLUSTERED INDEX[IX_NumberReversed] ON[dbo].[PhoneNumber]([NumberReversed]) 
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropIndex( "dbo.PhoneNumber", "IX_NumberReversed" );
            DropColumn( "dbo.PhoneNumber", "NumberReversed");
        }
    }
}
