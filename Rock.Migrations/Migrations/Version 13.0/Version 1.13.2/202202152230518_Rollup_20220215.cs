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
    public partial class Rollup_20220215 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.DefinedValue", "CategoryId", c => c.Int());
            AddColumn("dbo.DefinedType", "CategorizedValuesEnabled", c => c.Boolean());
            CreateIndex("dbo.DefinedValue", "CategoryId");
            AddForeignKey("dbo.DefinedValue", "CategoryId", "dbo.Category", "Id");

            UpAddElectronicSignatureReceiptSystemCommunication();
            UpAddPhotoReleaseSignatureDocumentTemplate();

            UpdateEntityTypeLinkUrlLavaTemplate_Up();
            FixSystemCommunicationLavaSyntax_Up();
            FixSchedulingConfirmationLavaSyntax_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.DefinedValue", "CategoryId", "dbo.Category");
            DropIndex("dbo.DefinedValue", new[] { "CategoryId" });
            DropColumn("dbo.DefinedType", "CategorizedValuesEnabled");
            DropColumn("dbo.DefinedValue", "CategoryId");

            DownRemovePhotoReleaseSignatureDocumentTemplate();
            DownRemoveElectronicSignatureReceiptSystemCommunication();
        }

        private void UpAddElectronicSignatureReceiptSystemCommunication()
        {
            RockMigrationHelper.UpdateSystemCommunication( "System",
                "Electronic Signature Receipt",
                "", // from
                "", // fromName
                "", // to
                "", // cc
                "", // bcc
                "Electronic Signature Receipt from {{ 'Global' | Attribute:'OrganizationName'}}", // subject
                                                                                                  // body
                @"{{ 'Global' | Attribute:'EmailHeader' }}

<p>{{ SignatureDocument.SignedByPersonAlias.Person.FullName | Default:'Hello' }},</p>
<p>Attached is your copy of the '{{ SignatureDocument.Name }}' {{ SignatureDocument.SignatureDocumentTemplate.DocumentTerm | Downcase }} you signed on {{ 'Now' | Date:'MMMM d, yyyy' }}. Please retain this copy for your records.
</p>
<p>
<strong>Signed by:</strong> 
{% if SignatureDocument.SignedByPersonAlias %}
   {{ SignatureDocument.SignedByPersonAlias.Person.FullName }}
{% else %}
   {{ SignatureDocument.SignedName }}
{% endif %}<br>
<strong>Signed on:</strong> {{ SignatureDocument.SignedDateTime }}<br>
{% if SignatureDocument.AppliesToPersonAlias %}
<strong>Applies to:</strong> {{ SignatureDocument.AppliesToPersonAlias.Person.FullName | Default:'-' }}<br>
{% endif %}
<strong>Reference code:</strong> {{ SignatureDocument.SignatureVerificationHash }}
</p>

{{ 'Global' | Attribute:'EmailFooter' }}",
                SystemGuid.SystemCommunication.SYSTEM_ELECTRONIC_SIGNATURE_RECEIPT );
        }

        private void UpAddPhotoReleaseSignatureDocumentTemplate()
        {
            Sql( @"
                DECLARE @CompletionSystemCommunicationId int 
                    SET @CompletionSystemCommunicationId = (SELECT [Id] FROM [SystemCommunication] WHERE [guid] = '224A0E80-069B-463C-8187-E13682F8A550')

                DECLARE @DigitalSignedBinaryFileTypeId int = (
                    SELECT TOP 1 [Id]
                    FROM [BinaryFileType]
                    WHERE [Guid] = '40871411-4E2D-45C2-9E21-D9FCBA5FC340' ) -- Rock.SystemGuid.BinaryFiletype.DIGITALLY_SIGNED_DOCUMENTS

                DECLARE @Id int
                SET @Id = (SELECT [Id] FROM [SignatureDocumentTemplate] WHERE [guid] = 'E982C45E-CDD3-481E-8664-85AD36500F1B')
                IF @Id IS NULL
                BEGIN
                    INSERT INTO [SignatureDocumentTemplate]
                               ([Name]
                               ,[Description]
                               ,[ProviderEntityTypeId]
                               ,[ProviderTemplateKey]
                               ,[BinaryFileTypeId]
                               ,[CreatedDateTime]
                               ,[ModifiedDateTime]
                               ,[Guid]
                               ,[InviteSystemEmailId]
                               ,[InviteSystemCommunicationId]
                               ,[LavaTemplate]
                               ,[IsActive]
                               ,[DocumentTerm]
                               ,[SignatureType]
                               ,[CompletionSystemCommunicationId])
                    VALUES
                               ('Photo Release'
                               ,'An example ""Photo Release"" agreement which can be used when the SignedByPerson and AppliesToPerson are provided.'
                               ,null
                               ,''
                               ,@DigitalSignedBinaryFileTypeId
                               ,GetDate()
                               ,GetDate()
                               ,'E982C45E-CDD3-481E-8664-85AD36500F1B'
                               ,null
                               ,null
                               ,
'<html>
    <head>
        <link rel=""stylesheet"" href=""https://cdn.simplecss.org/simple.min.css""> 
    </head>
    <body>
        <img src=""{{ ''Global'' | Attribute:''PublicApplicationRoot'' }}{{ ''Global'' | Attribute:''EmailHeaderLogo'' }}""> 
        <p>I, {{ Workflow | Attribute:''SignedByPerson'' }}, hereby grant, voluntarily and with full understanding, to {{ ''Global'' | Attribute: ''OrganizationName'' }} (""Church""), a license to the following:</p>
        <ol>
            <li>Use and storage of ""{{ Workflow | Attribute:''AppliesToPerson'' | Append:''&#39;s'' }}"" name and image, by means of digital or film photography, video photography, audio recording or other documentation, with respect to the activity, namely (""Activity""), of Church.</li>
            <li>Use of any stored data including ""{{ Workflow | Attribute:''AppliesToPerson'' | Append:''&#39;s'' }}"" name and image in printed publications of Church.</li>
            <li>Use of any stored data including ""{{ Workflow | Attribute:''AppliesToPerson'' | Append:''&#39;s'' }}"" name and image in electronic publications of Church.</li>
" +
@"            <li>Use of any stored data including ""{{ Workflow | Attribute:''AppliesToPerson'' | Append:''&#39;s'' }}"" name and image in any Web site created by or for Church for its sole benefit.</li>
            <li>If I am signing this agreement on behalf of a minor child, I hereby warrant that I am the legal parent or guardian of the child and that I have the legal authority to sign this agreement on behalf of the child.</li>
            <li>If a dispute over this agreement or any claim for damages arises, I agree to resolve the matter through a mutually acceptable alternative dispute resolution process. If I cannot agree with Church upon such a process, the dispute will be submitted to a three-member arbitration panel for resolution in accordance with the rules of the American Arbitration Association for final resolution.</li>
        </ol>
    </body>
</html>'
                               , 1
                               ,'Release'
                               ,0
                               ,@CompletionSystemCommunicationId)
                    END
" );
        }

        private void DownRemovePhotoReleaseSignatureDocumentTemplate()
        {
            RockMigrationHelper.DeleteByGuid( "E982C45E-CDD3-481E-8664-85AD36500F1B", "SignatureDocumentTemplate" );
        }

        private void DownRemoveElectronicSignatureReceiptSystemCommunication()
        {
            RockMigrationHelper.DeleteSystemCommunication( SystemGuid.SystemCommunication.SYSTEM_ELECTRONIC_SIGNATURE_RECEIPT );
        }
    
        /// <summary>
        /// MP: EntityTypeLinkUrlLavaTemplate
        /// </summary>
        private void UpdateEntityTypeLinkUrlLavaTemplate_Up()
        {
            // Update EntityType so that Workflow and Registration have LinkUrlLavaTemplate's
            Sql( @"
                UPDATE [EntityType]
                SET [LinkUrlLavaTemplate] = '~/Workflow/{{ Entity.Id }}'
                WHERE [Guid] = '" + Rock.SystemGuid.EntityType.WORKFLOW + "'" );

            Sql( @"
                UPDATE [EntityType]
                SET [LinkUrlLavaTemplate] = '~/web/event-registrations/{{ Entity.RegistrationInstanceId }}/registration/{{ Entity.Id }}'
                WHERE [Guid] = '" + Rock.SystemGuid.EntityType.REGISTRATION + "'" );
        }

        /// <summary>
        /// DL: Fix Lava Syntax for System Communications
        /// </summary>
        private void FixSystemCommunicationLavaSyntax_Up()
        {
            string searchText;
            string replaceText;

            // Workflow Form Notification Template.
            // Replace '&&' operator with the Liquid operator 'and'.
            searchText = "attribute.IsRequired && attribute.Value";
            replaceText = "attribute.IsRequired and attribute.Value";

            Sql( $@"
                UPDATE [SystemCommunication]
                SET [Body] = REPLACE([Body],'{searchText}','{replaceText}')
                WHERE [Guid] = '{SystemGuid.SystemCommunication.WORKFLOW_FORM_NOTIFICATION}'
                " );

            // Group Attendance Reminder Template.
            // Replace the use of the '\' escape character in a string with the "EscapeDataString" filter.
            searchText = @"Date:'yyyy-MM-ddTHH\%3amm\%3ass'".Replace( "'", "''" );
            replaceText = @"Date:'yyyy-MM-ddTHH:mm:ss' | EscapeDataString".Replace( "'", "''" );

            Sql( $@"
                UPDATE [SystemCommunication]
                SET [Body] = REPLACE([Body],'{searchText}','{replaceText}')
                WHERE [Guid] = '{SystemGuid.SystemCommunication.GROUP_ATTENDANCE_REMINDER}'
                " );

            // Attendance Summary Notification Template.
            // Replace invalid '- %}' sequence with the corrected ' -%}'.
            searchText = "- %}";
            replaceText = " -%}";

            Sql( $@"
                UPDATE [SystemCommunication]
                SET [Body] = REPLACE([Body],'{searchText}','{replaceText}')
                WHERE [Guid] = '{SystemGuid.SystemCommunication.ATTENDANCE_NOTIFICATION}'
                " );
        }

        /// <summary>
        /// DL: Fix Lava for Scheduling Confirmation
        /// </summary>
        private void FixSchedulingConfirmationLavaSyntax_Up()
        {
            string searchText;
            string replaceText;

            // Scheduling Confirmation Template.
            // Update Obsolete Model References.
            searchText = "attendance.Location.Name";
            replaceText = "attendance.Occurrence.Location.Name";

            Sql( $@"
                UPDATE [SystemCommunication]
                SET [Body] = REPLACE([Body],'{searchText}','{replaceText}')
                WHERE [Guid] IN ('{SystemGuid.SystemCommunication.SCHEDULING_REMINDER}','{SystemGuid.SystemCommunication.SCHEDULING_CONFIRMATION}')
                " );

            searchText = "attendance.Schedule.Name";
            replaceText = "attendance.Occurrence.Schedule.Name";

            Sql( $@"
                UPDATE [SystemCommunication]
                SET [Body] = REPLACE([Body],'{searchText}','{replaceText}')
                WHERE [Guid] IN ('{SystemGuid.SystemCommunication.SCHEDULING_REMINDER}','{SystemGuid.SystemCommunication.SCHEDULING_CONFIRMATION}')
                " );
        }
    }
}
