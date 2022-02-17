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
    public partial class rollup_20220216 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpAddElectronicSignatureReceiptSystemCommunication();
            UpAddPhotoReleaseSignatureDocumentTemplate();
            UpdateStructuredContentEditorDefinedValuesUp();
            UpdateStructuredContentDefinedValuesUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DownRemovePhotoReleaseSignatureDocumentTemplate();
            DownRemoveElectronicSignatureReceiptSystemCommunication();
            UpdateStructuredContentDefinedValuesDown();
        }

        /// <summary>
        /// NA: New Electronic Signature Receipt SystemCommunication & Photo Release Template
        /// </summary>
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

        /// <summary>
        /// NA: New Electronic Signature Receipt SystemCommunication & Photo Release Template
        /// </summary>
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

        /// <summary>
        /// NA: New Electronic Signature Receipt SystemCommunication & Photo Release Template
        /// </summary>
        private void DownRemovePhotoReleaseSignatureDocumentTemplate()
        {
            RockMigrationHelper.DeleteByGuid( "E982C45E-CDD3-481E-8664-85AD36500F1B", "SignatureDocumentTemplate" );
        }

        /// <summary>
        /// NA: New Electronic Signature Receipt SystemCommunication & Photo Release Template
        /// </summary>
        private void DownRemoveElectronicSignatureReceiptSystemCommunication()
        {
            RockMigrationHelper.DeleteSystemCommunication( SystemGuid.SystemCommunication.SYSTEM_ELECTRONIC_SIGNATURE_RECEIPT );
        }

        /// <summary>
        /// CR: Update to Structured Content Editor Class References
        /// </summary>
        private void UpdateStructuredContentEditorDefinedValuesUp()
        {
            string oldReference;
            string newReference;

            // This is for the Structured Content Editor Tools, and will update old class references.
            oldReference = "Rock.UI.StructuredContent.EditorTools";
            newReference = "Rock.UI.StructuredContentEditor.EditorTools";

            Sql( $@"
            UPDATE [DefinedValue]
            SET [Description] = REPLACE([Description],'{oldReference}','{newReference}')
            WHERE [Id] IN (SELECT dv.[Id]
                    FROM [dbo].[DefinedValue] dv
                    INNER JOIN [DefinedType] dt ON dt.[Id] = dv.[DefinedTypeId]
                    WHERE dt.[Guid] = '{SystemGuid.DefinedType.STRUCTURED_CONTENT_EDITOR_TOOLS}')
            " );
        }
    
        /// <summary>
        /// DV: Update the order of the Structured Content items.
        /// </summary>
        private void UpdateStructuredContentDefinedValuesUp()
        {
            RockMigrationHelper.UpdateDefinedValue( "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Default",
                @"{
    header: {
        class: Rock.UI.StructuredContent.EditorTools.Header,
        inlineToolbar: ['link'],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContent.EditorTools.RockImage,
        inlineToolbar: ['link'],
    },
    list: {
        class: Rock.UI.StructuredContent.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContent.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContent.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    warning: Rock.UI.StructuredContent.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContent.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    code: {
        class: Rock.UI.StructuredContent.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    delimiter: Rock.UI.StructuredContent.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContent.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContent.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContent.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                "09B25845-B879-4E69-87E9-003F9380B8DD",
                false,
                null,
                string.Empty,
                0 );

            RockMigrationHelper.UpdateDefinedValue( "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Message Notes",
                @"{
    header: {
        class: Rock.UI.StructuredContent.EditorTools.Header,
        inlineToolbar: ['link'],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContent.EditorTools.RockImage,
        inlineToolbar: ['link'],
    },
    list: {
        class: Rock.UI.StructuredContent.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContent.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContent.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    warning: Rock.UI.StructuredContent.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContent.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    fillin: {
        class: Rock.UI.StructuredContent.EditorTools.Fillin,
        shortcut: 'CMD+SHIFT+F'
    },
    code: {
        class: Rock.UI.StructuredContent.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    note: {
        class: Rock.UI.StructuredContent.EditorTools.Note,
        shortcut: 'CMD+SHIFT+N'
    },
    delimiter: Rock.UI.StructuredContent.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContent.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContent.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContent.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                "31C63FB9-1365-4EEF-851D-8AB9A188A06C",
                false,
                null,
                string.Empty,
                1 );
        }

        /// <summary>
        /// DV: Reset the order of the Structured Content items.
        /// </summary>
        private void UpdateStructuredContentDefinedValuesDown()
        {
            RockMigrationHelper.UpdateDefinedValue( "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Default",
                @"{
    header: {
        class: Rock.UI.StructuredContent.EditorTools.Header,
        inlineToolbar: ['link'],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContent.EditorTools.RockImage,
        inlineToolbar: ['link'],
    },
    list: {
        class: Rock.UI.StructuredContent.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContent.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContent.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    warning: Rock.UI.StructuredContent.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContent.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    code: {
        class: Rock.UI.StructuredContent.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    delimiter: Rock.UI.StructuredContent.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContent.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContent.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContent.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                "09B25845-B879-4E69-87E9-003F9380B8DD",
                false,
                null,
                string.Empty,
                1 );

            RockMigrationHelper.UpdateDefinedValue( "E43AD92C-4DD4-4D78-9852-FCFAEFDF52CA",
                "Message Notes",
                @"{
    header: {
        class: Rock.UI.StructuredContent.EditorTools.Header,
        inlineToolbar: ['link'],
        config: {
            placeholder: 'Header'
        },
        shortcut: 'CMD+SHIFT+H'
    },
    image: {
        class: Rock.UI.StructuredContent.EditorTools.RockImage,
        inlineToolbar: ['link'],
    },
    list: {
        class: Rock.UI.StructuredContent.EditorTools.NestedList,
        inlineToolbar: true,
        shortcut: 'CMD+SHIFT+L'
    },
    checklist: {
        class: Rock.UI.StructuredContent.EditorTools.Checklist,
        inlineToolbar: true,
    },
    quote: {
        class: Rock.UI.StructuredContent.EditorTools.Quote,
        inlineToolbar: true,
        config: {
            quotePlaceholder: 'Enter a quote',
            captionPlaceholder: 'Quote\'s author',
        },
        shortcut: 'CMD+SHIFT+O'
    },
    warning: Rock.UI.StructuredContent.EditorTools.Warning,
    marker: {
        class: Rock.UI.StructuredContent.EditorTools.Marker,
        shortcut: 'CMD+SHIFT+M'
    },
    fillin: {
        class: Rock.UI.StructuredContent.EditorTools.Fillin,
        shortcut: 'CMD+SHIFT+F'
    },
    code: {
        class: Rock.UI.StructuredContent.EditorTools.Code,
        shortcut: 'CMD+SHIFT+C'
    },
    note: {
        class: Rock.UI.StructuredContent.EditorTools.Note,
        shortcut: 'CMD+SHIFT+N'
    },
    delimiter: Rock.UI.StructuredContent.EditorTools.Delimiter,
    inlineCode: {
        class: Rock.UI.StructuredContent.EditorTools.InlineCode,
        shortcut: 'CMD+SHIFT+C'
    },
    embed: Rock.UI.StructuredContent.EditorTools.Embed,
    table: {
        class: Rock.UI.StructuredContent.EditorTools.Table,
        config: {
            defaultHeadings: true
        },
        inlineToolbar: true,
        shortcut: 'CMD+ALT+T'
    }
}",
                "31C63FB9-1365-4EEF-851D-8AB9A188A06C",
                false,
                null,
                string.Empty,
                0 );
        }
    }
}
