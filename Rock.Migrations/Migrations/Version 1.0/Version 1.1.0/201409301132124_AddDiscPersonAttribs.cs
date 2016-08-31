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
    public partial class AddDiscPersonAttribs : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //Add Category
            RockMigrationHelper.UpdateCategory( "5997C8D3-8840-4591-99A5-552919F90CBD", "DISC", "fa fa-bar-chart", "DISC Score Person Attributes", "0B187C81-2106-4875-82B6-FBF1277AE23B" );

            // now update the new DISC category attribute to be for entity type Person
            Sql( @"
	                DECLARE @PersonEntityTypeId INT
	                SET @PersonEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '72657ED8-D16E-492E-AC12-144C5E7567E7')

                    UPDATE [Category]
                      SET [EntityTypeQualifierColumn] = 'EntityTypeId'
                          ,[EntityTypeQualifierValue] = CAST(@PersonEntityTypeId as varchar)
                    WHERE [Guid] = '0B187C81-2106-4875-82B6-FBF1277AE23B'" );

            //Add Attributes
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Adaptive D", "AdaptiveD", "fa fa-bar-chart", "Adaptive Dominance: is bottom line oriented, makes quick decisions, wants direct answers.", 1, string.Empty, "EDE5E199-37BE-424F-A788-5CDCC064157C" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Adaptive I", "AdaptiveI", "fa fa-bar-chart", "Adaptive Influence: very people oriented, has a lot of friends, wants opportunity to talk.", 2, string.Empty, "7F0A1794-0150-413B-9AE1-A6B0D6373DA6" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Adaptive S", "AdaptiveS", "fa fa-bar-chart", "Adaptive Steadiness: does not like change, wants limited responsibility and sincere appreciation.", 3, string.Empty, "2512DAC6-BBC4-4D0E-A01D-E92F94C534BD" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Adaptive C", "AdaptiveC", "fa fa-bar-chart", "Adaptive Cautiousness: is detail oriented, wants no sudden changes, won't make decision.", 4, string.Empty, "4A2E1539-4ECC-40B9-9EBD-C0C84EC8DA36" );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Natural D", "NaturalD", "fa fa-bar-chart", "Natural Dominance: is bottom line oriented, makes quick decisions, wants direct answers.", 5, string.Empty, "86670F7D-07BA-4ECE-9BB9-9D94B5FB5F26" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Natural I", "NaturalI", "fa fa-bar-chart", "Natural Influence: very people oriented, has a lot of friends, wants opportunity to talk", 6, string.Empty, "3EFF4FEF-EE4C-40E2-8DBD-80F3276852DA" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Natural S", "NaturalS", "fa fa-bar-chart", "Natural Steadiness: does not like change, wants limited responsibility and sincere appreciation.", 7, string.Empty, "FA4341B4-28C7-409E-A101-548BB5759BE6" );
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.INTEGER, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Natural C", "NaturalC", "fa fa-bar-chart", "Natural Cautiousness: is detail oriented, wants no sudden changes, won't make decision.", 8, string.Empty, "3A10ECFB-8CAB-4CCA-8B29-298756CD3251" );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DATE, "0B187C81-2106-4875-82B6-FBF1277AE23B", "Last Save Date", "LastSaveDate", "fa fa-bar-chart", "The date the person took the DISC test.", 9, string.Empty, "990275DB-611B-4D2E-94EA-3FFA1186A5E1" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //Delete Attributes
            RockMigrationHelper.DeleteAttribute( "990275DB-611B-4D2E-94EA-3FFA1186A5E1" );

            RockMigrationHelper.DeleteAttribute( "3A10ECFB-8CAB-4CCA-8B29-298756CD3251" );
            RockMigrationHelper.DeleteAttribute( "FA4341B4-28C7-409E-A101-548BB5759BE6" );
            RockMigrationHelper.DeleteAttribute( "3EFF4FEF-EE4C-40E2-8DBD-80F3276852DA" );
            RockMigrationHelper.DeleteAttribute( "86670F7D-07BA-4ECE-9BB9-9D94B5FB5F26" );

            RockMigrationHelper.DeleteAttribute( "4A2E1539-4ECC-40B9-9EBD-C0C84EC8DA36" );
            RockMigrationHelper.DeleteAttribute( "2512DAC6-BBC4-4D0E-A01D-E92F94C534BD" );
            RockMigrationHelper.DeleteAttribute( "7F0A1794-0150-413B-9AE1-A6B0D6373DA6" );
            RockMigrationHelper.DeleteAttribute( "EDE5E199-37BE-424F-A788-5CDCC064157C" );

            //Delete Category
            RockMigrationHelper.DeleteCategory( "0B187C81-2106-4875-82B6-FBF1277AE23B" );
        }
    }
}
