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
    public partial class RemoveSMTPAttributes : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    DELETE [Attribute] WHERE [Guid] IN (
        '1C4E71DD-ED38-4586-93CF-A847003EC594',
        '3C5F2BF8-8D8A-46D4-9182-2A25D32851EA',
        '40690F08-1433-4046-8F22-B4B16075F1CF',
        '996B04C9-45E5-4DC1-A84B-27D14B53DCC6',
        '10DD8248-DC68-4206-ABFD-DA4E8BB849E3')
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
