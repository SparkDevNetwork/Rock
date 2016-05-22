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
    public partial class MetricCategoryAsEntity : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropTable( "dbo.MetricCategory" );

            CreateTable(
                "dbo.MetricCategory",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    MetricId = c.Int( nullable: false ),
                    CategoryId = c.Int( nullable: false ),
                    Order = c.Int( nullable: false ),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.String( maxLength: 50 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Metric", t => t.MetricId, cascadeDelete: true )
                .ForeignKey( "dbo.Category", t => t.CategoryId, cascadeDelete: true )
                .Index( t => t.MetricId )
                .Index( t => t.CategoryId )
                .Index( t => t.Guid, unique: true );
            
            // Delete old Metric List, Detail and Value blocks
            DeleteBlock( "9126CFA2-9B26-4FBB-BB87-F76514221DBE" );
            DeleteBlock( "816B856A-FDC8-4832-88E7-FA330AFC5D6E" );
            DeleteBlock( "1B9850ED-CD78-47E2-A076-A3BA5D8808EC" );

            // Delete old Metric List and Metric Detail pages
            DeletePage( "574544A8-4831-4DAB-8BCE-B2C9B3D188AF" );
            DeletePage( "84DB9BA0-2725-40A5-A3CA-9A1C043C31B0" );

            // Delete old Metric FrequencyType definedtypes/values
            DeleteDefinedValue( "78CF66EB-1A65-42CC-A05E-3BF6DE515049" );
            DeleteDefinedValue( "338F29E5-05C4-40A5-A669-C098787E2ADF" );
            DeleteDefinedValue( "0BC11625-F8C4-4032-8B27-537D67941489" );
            DeleteDefinedValue( "41663B95-8271-40E9-B1B6-0D14EA45D68D" );
            DeleteDefinedValue( "305CBFA3-3168-40AF-ABCF-F5DFF9DC13C2" );

            DeleteDefinedType( "526CB333-2C64-4486-8469-7F7EA9366254" );

            Sql(@"
UPDATE [Attribute] 
	SET [Description] = 'The opposite relationship (role) that should be added to the related person whenever this relationship type is added. Note: new values will not appear in this list until the group type is saved.  Also, make sure to set the inverse relationship on the relationship selected here.'
    WHERE [Guid] = 'C91148D9-D663-493A-86E8-5000BD281852'");
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropTable( "dbo.MetricCategory" );

            CreateTable(
                "dbo.MetricCategory",
                c => new
                {
                    MetricId = c.Int( nullable: false ),
                    CategoryId = c.Int( nullable: false ),
                    Order = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.MetricId, t.CategoryId } )
                .ForeignKey( "dbo.Metric", t => t.MetricId, cascadeDelete: true )
                .ForeignKey( "dbo.Category", t => t.CategoryId, cascadeDelete: true )
                .Index( t => t.MetricId )
                .Index( t => t.CategoryId );
        }
    }
}
