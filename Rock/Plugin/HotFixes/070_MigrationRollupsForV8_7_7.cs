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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///Migration 
    /// </summary>
    [MigrationNumber( 70, "1.8.6" )]
    public class MigrationRollupsForV8_7_7 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpDatesPhotoRequestCommunicationTemplate();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }


        /// <summary>
        ///NA: Updates the "Photo Request Template" communication template
        /// </summary>
        private void UpDatesPhotoRequestCommunicationTemplate()
        {
            // Updates the "Photo Request Template" communication template
            Sql( @"DECLARE @CommunicationTemplateId int = (SELECT [Id] FROM [CommunicationTemplate] WHERE [Guid] = 'B9A0489C-A823-4C5C-A9F9-14A206EC3B88')
    DECLARE @Message nvarchar(MAX)

    IF @CommunicationTemplateId IS NOT NULL
    BEGIN
        SET @Message = (SELECT [Message] FROM [CommunicationTemplate] WHERE [Id] = @CommunicationTemplateId)
        SET @Message = Replace(@Message,'OptOut/{{ Person.UrlEncodedKey }}','OptOut/{{ Person | PersonActionIdentifier:''OptOut'' }}')
        SET @Message = Replace(@Message,'Unsubscribe/{{ Person.UrlEncodedKey }}','Unsubscribe/{{ Person | PersonActionIdentifier:''Unsubscribe'' }}')
        BEGIN
            UPDATE
                [CommunicationTemplate]
            SET
                [Message] = @Message
            WHERE
                [Id] = @CommunicationTemplateId
        END
    END
" );
        }



    }
}
