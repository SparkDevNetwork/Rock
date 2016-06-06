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
    public partial class AudioVideoFieldTypes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add/Update FieldTypes for Audio and Video
            Sql( "DELETE FROM [FieldType] where [Guid] in ('0D842975-7439-4D2E-BB94-BAD8DDF22260', 'FA398F9D-5B01-41EA-9A93-112F910A277D')" );
            RockMigrationHelper.UpdateFieldType( "Audio File", "Field where you can upload an audio file that will be displayed in an audio player.", "Rock", "Rock.Field.Types.AudioFileFieldType", "9772CB1F-3BC4-432E-80DD-D635CDB2DA32" );
            RockMigrationHelper.UpdateFieldType( "Audio Url", "Field where you can specify the url for an audio file that will be displayed in an audio player.", "Rock", "Rock.Field.Types.AudioUrlFieldType", "3B2D8714-421C-4CB8-A892-58B83521EF8A" );

            RockMigrationHelper.UpdateFieldType( "Video File", "Field where you can upload a video file that will be displayed in a video player.", "Rock", "Rock.Field.Types.VideoFileFieldType", "F1F5B59D-F086-4627-A94A-DFA7E67950F3" );
            RockMigrationHelper.UpdateFieldType( "Video Url", "Field where you can specify the url for a video file that will be displayed in a video player.", "Rock", "Rock.Field.Types.VideoUrlFieldType", "E6FD57F3-1704-4E96-91A7-3D3E85346393" );

            // add 
            RockMigrationHelper.UpdateFieldType( "Encrypted Text", "", "Rock", "Rock.Field.Types.EncryptedTextFieldType", "36167F3E-8CB2-44F9-9022-102F171FBC9A" );
            RockMigrationHelper.UpdateFieldType( "Filter Date", "", "Rock", "Rock.Field.Types.FilterDateFieldType", "4F879A48-63DA-446F-837B-7458799298C0" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
