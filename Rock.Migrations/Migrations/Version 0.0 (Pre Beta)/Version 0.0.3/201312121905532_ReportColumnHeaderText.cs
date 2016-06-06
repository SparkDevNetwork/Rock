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
    public partial class ReportColumnHeaderText : Rock.Migrations.RockMigration1
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.ReportField", "ColumnHeaderText", c => c.String() );

            // Fix up Category for Report Category Tree and Report Category Detail
            Sql( @"update AttributeValue set Value = 'f1f22d3e-fefa-4c84-9ffa-9e8ace60fce7' where [Guid] = 'CF04A81D-A95F-47E4-BE65-04A641607FA2'" );
            Sql( @"update AttributeValue set Value = 'f1f22d3e-fefa-4c84-9ffa-9e8ace60fce7' where [Guid] = '603B229C-047A-4995-BF7F-2F93450204FB'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.ReportField", "ColumnHeaderText" );
        }
    }
}
