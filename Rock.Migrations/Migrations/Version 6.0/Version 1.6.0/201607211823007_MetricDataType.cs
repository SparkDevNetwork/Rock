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
    public partial class MetricDataType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Metric", "NumericDataType", c => c.Int(nullable: false));
            Sql( @"
    UPDATE [Metric] SET [NumericDataType] = 1
" );

            // Add Salvations and Adult Attendance metrics
            RockMigrationHelper.UpdateCategory( "3D35C859-DF37-433F-A20A-0FFD0FCB9862", "Weekly Metrics", "", "", "64B29ADE-144D-4E84-96CC-A79398589733" );
            Sql( @"
    DECLARE @SourceValueTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '1D6511D6-B15D-4DED-B3C4-459CD2A7EC0E' )
    DECLARE @CategoryId int = ( SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '64B29ADE-144D-4E84-96CC-A79398589733' )
    DECLARE @CampusEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name]= 'Rock.Model.Campus' )
    DECLARE @ScheduleEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name]= 'Rock.Model.Schedule' )

    INSERT INTO [Metric] ( [IsSystem], [Title], [SubTitle], [Description], [IconCssClass], [IsCumulative], [SourceValueTypeId], [SourceSql], [YAxisLabel], [Guid], [NumericDataType] )
    VALUES 
	    ( 0, 'Salvations', '', '', '', 0, @SourceValueTypeId, '', 'Salvations', '34EA42B9-1142-43DA-8A8B-AA1864A1CA72', 0 ),
	    ( 0, 'Adult Attendance', '', '', '', 0, @SourceValueTypeId, '', 'Attendance', '0D126800-2FDA-4B34-96FD-9BAE76F3A89A', 0 )

    DECLARE @SalvationMetricId int = ( SELECT TOP 1 [Id] FROM [Metric] WHERE [Guid] = '34EA42B9-1142-43DA-8A8B-AA1864A1CA72' )
    DECLARE @AttendanceMetricId int = ( SELECT TOP 1 [Id] FROM [Metric] WHERE [Guid] = '0D126800-2FDA-4B34-96FD-9BAE76F3A89A' )

    INSERT INTO [MetricCategory] ( [MetricId], [CategoryId], [Order], [Guid] )
    VALUES 
	    ( @AttendanceMetricId, @CategoryId, 0, newid() ),
	    ( @SalvationMetricId, @CategoryId, 1, newid() )

    INSERT INTO [MetricPartition] ( [MetricId], [Label], [EntityTypeId], [IsRequired], [Order], [Guid] )
    SELECT [Id], 'Campus', @CampusEntityTypeId, 1, 0, newid()
    FROM [Metric]
    WHERE [Id] IN ( @AttendanceMetricId, @SalvationMetricId )

    INSERT INTO [MetricPartition] ( [MetricId], [Label], [EntityTypeId], [IsRequired], [Order], [Guid] )
    SELECT [Id], 'Service', @ScheduleEntityTypeId, 1, 1, newid()
    FROM [Metric]
    WHERE [Id] IN ( @AttendanceMetricId, @SalvationMetricId )
" );

            RockMigrationHelper.AddPage( "0C4B3F4D-53FD-4A65-8C93-3868CE4DA137", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Tools", "", "164C7A7F-8C55-4E20-B582-D84D83174F2C", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "164C7A7F-8C55-4E20-B582-D84D83174F2C", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Weekly Metrics", "", "6E1DDCE6-F941-4AA9-8514-942E76AE3081", "fa fa-signal" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Service Metrics Entry", "Block for easily adding/editing metric values for any metric that has partitions of campus and service time.", "~/Blocks/Reporting/ServiceMetricsEntry.ascx", "Reporting", "535E1879-CD4C-432B-9312-B27B3A668D88" );

            // Add Block to Page: Weekly Metrics, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6E1DDCE6-F941-4AA9-8514-942E76AE3081", "", "535E1879-CD4C-432B-9312-B27B3A668D88", "Service Metrics Entry", "Main", "", "", 0, "2DEEE1BB-C320-4808-91E5-6B52325F3CCD" );
            // Attrib for BlockType: Service Metrics Entry:Schedule Category
            RockMigrationHelper.AddBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Schedule Category", "ScheduleCategory", "", "The schedule category to use for list of service times.", 0, @"", "3A97E33A-E74D-4532-ADD8-1D113906F69B" );
            // Attrib for BlockType: Service Metrics Entry:Metric Categories
            RockMigrationHelper.AddBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "F5334A8E-B7E2-415C-A6EC-A6D8FA5341C4", "Metric Categories", "MetricCategories", "", "Select the metric categories to display (note: only metrics in those categories with a campus and scheudle partition will displayed).", 3, @"", "D034D419-8550-4BEF-B39D-9C38578AE9CF" );
            // Attrib for BlockType: Service Metrics Entry:Weeks Ahead
            RockMigrationHelper.AddBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Weeks Ahead", "WeeksAhead", "", "The number of weeks ahead to display in the 'Week of' selection.", 2, @"0", "ACF0A620-4EF3-4C92-8D05-7BA4AA6AE58F" );
            // Attrib for BlockType: Service Metrics Entry:Weeks Back
            RockMigrationHelper.AddBlockTypeAttribute( "535E1879-CD4C-432B-9312-B27B3A668D88", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Weeks Back", "WeeksBack", "", "The number of weeks back to display in the 'Week of' selection.", 1, @"8", "66A52C8F-1DAB-49B9-9AEC-360C0A94BF8E" );

            // Attrib Value for Block:Service Metrics Entry, Attribute:Schedule Category Page: Weekly Metrics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2DEEE1BB-C320-4808-91E5-6B52325F3CCD", "3A97E33A-E74D-4532-ADD8-1D113906F69B", @"4fecc91b-83f9-4269-ae03-a006f401c47e" );
            // Attrib Value for Block:Service Metrics Entry, Attribute:Metric Categories Page: Weekly Metrics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2DEEE1BB-C320-4808-91E5-6B52325F3CCD", "D034D419-8550-4BEF-B39D-9C38578AE9CF", @"34ea42b9-1142-43da-8a8b-aa1864a1ca72|64b29ade-144d-4e84-96cc-a79398589733,0d126800-2fda-4b34-96fd-9bae76f3a89a|64b29ade-144d-4e84-96cc-a79398589733" );
            // Attrib Value for Block:Service Metrics Entry, Attribute:Weeks Ahead Page: Weekly Metrics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2DEEE1BB-C320-4808-91E5-6B52325F3CCD", "ACF0A620-4EF3-4C92-8D05-7BA4AA6AE58F", @"0" );
            // Attrib Value for Block:Service Metrics Entry, Attribute:Weeks Back Page: Weekly Metrics, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2DEEE1BB-C320-4808-91E5-6B52325F3CCD", "66A52C8F-1DAB-49B9-9AEC-360C0A94BF8E", @"8" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Service Metrics Entry:Weeks Back
            RockMigrationHelper.DeleteAttribute( "66A52C8F-1DAB-49B9-9AEC-360C0A94BF8E" );
            // Attrib for BlockType: Service Metrics Entry:Weeks Ahead
            RockMigrationHelper.DeleteAttribute( "ACF0A620-4EF3-4C92-8D05-7BA4AA6AE58F" );
            // Attrib for BlockType: Service Metrics Entry:Metric Categories
            RockMigrationHelper.DeleteAttribute( "D034D419-8550-4BEF-B39D-9C38578AE9CF" );
            // Attrib for BlockType: Service Metrics Entry:Schedule Category
            RockMigrationHelper.DeleteAttribute( "3A97E33A-E74D-4532-ADD8-1D113906F69B" );
            // Remove Block: Service Metrics Entry, from Page: Weekly Metrics, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "2DEEE1BB-C320-4808-91E5-6B52325F3CCD" );
            RockMigrationHelper.DeleteBlockType( "535E1879-CD4C-432B-9312-B27B3A668D88" ); // Service Metrics Entry
            RockMigrationHelper.DeletePage( "164C7A7F-8C55-4E20-B582-D84D83174F2C" ); //  Page: Tools, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "6E1DDCE6-F941-4AA9-8514-942E76AE3081" ); //  Page: Weekly Metrics, Layout: Full Width, Site: Rock RMS

            Sql( @"
    DELETE [Metric] WHERE [Guid] IN ( '34EA42B9-1142-43DA-8A8B-AA1864A1CA72', '0D126800-2FDA-4B34-96FD-9BAE76F3A89A' )
    DELETE [Category] WHERE [Guid] = '64B29ADE-144D-4E84-96CC-A79398589733'
" );

            DropColumn( "dbo.Metric", "NumericDataType");
        }
    }
}
