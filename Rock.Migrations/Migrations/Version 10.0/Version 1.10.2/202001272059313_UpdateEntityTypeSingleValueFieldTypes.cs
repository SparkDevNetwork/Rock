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
    public partial class UpdateEntityTypeSingleValueFieldTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.ConnectionActivityType", "39356C8F-B69E-4744-906C-0A182671B9F8" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.ConnectionOpportunity", "B188B729-FE6D-498B-8871-65AB8FD1E11E" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.ConnectionStatus", "EC381D5D-729F-4581-A8F7-8C1FCE44DA98" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.ConnectionType", "50DA6F25-E81E-46E8-A773-4B479B4FB9E6" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.ContentChannel", "D835A0EC-C8DB-483A-A37C-E8FB6E956C3D" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.ContentChannelType", "2B58514E-47F8-4740-A72C-B862B030855B" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.DataView", "BD72BBF1-0269-407E-BDBE-EEED4F1F207F" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.DefinedType", "BC48720C-3610-4BCF-AE66-D255A17F1CDF" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.EventCalendar", "EC0D9528-1A22-404E-A776-566404987363" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.InteractionChannel", "5EE5D193-60B6-4808-9BE9-C5FFDDF444E4" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.Metric", "AD52248B-B8C6-436E-9B57-E0BA4B42603E" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.NoteType", "E3FF88AC-13F6-4DF8-8371-FC0D7FD9A571" );
            RockMigrationHelper.UpdateEntityTypeSingleValueFieldType( "Rock.Model.Report", "B7FA826C-3367-4BF2-90E5-8C6730079D82" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // nothing to do here
        }
    }
}
