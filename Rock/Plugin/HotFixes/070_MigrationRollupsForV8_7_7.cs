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
            UpDatesPhotoRequestCommunicationTemplate();
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
