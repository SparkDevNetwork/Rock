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
    public partial class AddMeasurementClassificationValueToMetric : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Metric", "MeasurementClassificationValueId", c => c.Int() );
            CreateIndex( "dbo.Metric", "MeasurementClassificationValueId" );
            AddForeignKey( "dbo.Metric", "MeasurementClassificationValueId", "dbo.DefinedValue", "Id" );

            Sql( @"
UPDATE [dbo].[Category]
SET [IconCssClass] = 'icon-fw fa fa-calendar-week'
WHERE [Guid] = '64b29ade-144d-4e84-96cc-a79398589733'
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.Metric", "MeasurementClassificationValueId", "dbo.DefinedValue" );
            DropIndex( "dbo.Metric", new[] { "MeasurementClassificationValueId" } );
            DropColumn( "dbo.Metric", "MeasurementClassificationValueId" );
        }
    }
}
