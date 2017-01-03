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
    public partial class DigitalSignature2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RenameTable(name: "dbo.SignatureDocumentType", newName: "SignatureDocumentTemplate");
            RenameColumn(table: "dbo.SignatureDocument", name: "SignatureDocumentTypeId", newName: "SignatureDocumentTemplateId");
            RenameColumn(table: "dbo.RegistrationTemplate", name: "RequiredSignatureDocumentTypeId", newName: "RequiredSignatureDocumentTemplateId");
            RenameColumn(table: "dbo.Group", name: "RequiredSignatureDocumentTypeId", newName: "RequiredSignatureDocumentTemplateId");
            RenameIndex(table: "dbo.Group", name: "IX_RequiredSignatureDocumentTypeId", newName: "IX_RequiredSignatureDocumentTemplateId");
            RenameIndex(table: "dbo.RegistrationTemplate", name: "IX_RequiredSignatureDocumentTypeId", newName: "IX_RequiredSignatureDocumentTemplateId");
            RenameIndex(table: "dbo.SignatureDocument", name: "IX_SignatureDocumentTypeId", newName: "IX_SignatureDocumentTemplateId");
            AddColumn("dbo.SignatureDocumentTemplate", "InviteSystemEmailId", c => c.Int());
            AddColumn("dbo.SignatureDocument", "LastInviteDate", c => c.DateTime());
            AddColumn("dbo.SignatureDocument", "InviteCount", c => c.Int(nullable: false));
            CreateIndex("dbo.SignatureDocumentTemplate", "InviteSystemEmailId");
            AddForeignKey("dbo.SignatureDocumentTemplate", "InviteSystemEmailId", "dbo.SystemEmail", "Id");
            DropColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateFromName");
            DropColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateFromAddress");
            DropColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateSubject");
            DropColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateBody");
            DropColumn("dbo.SignatureDocument", "RequestDate");

            Sql( @"
    --update the block types
    UPDATE[BlockType]
    SET[Path] = '~/Blocks/Core/SignatureDocumentTemplateDetail.ascx'
    WHERE[Path] = '~/Blocks/Core/SignatureDocumentTypeDetail.ascx'

    UPDATE[BlockType]
    SET[Path] = '~/Blocks/Core/SignatureDocumentTemplateList.ascx'
    WHERE[Path] = '~/Blocks/Core/SignatureDocumentTypeList.ascx'

    -- update the page names
    UPDATE[Page] SET
        [PageTitle] = 'Document Templates',
        [BrowserTitle] = 'Document Templates',
	    [InternalName] = 'Document Templates',
	    [IconCssClass] = 'fa fa-pencil-square-o'
    WHERE[Guid] = '7096FA12-07A5-489C-83B0-EE55494A3484'
" );

            //Add the system email
//            RockMigrationHelper.UpdateSystemEmail( "System", "Digital Signature Invite", "", "", "", "", "", "{{  SignatureDocument.SignatureDocumentTemplate.Name }} Signature Request for {{ 'Global' | Attribute:'OrganizationName' }}", @"{{ 'Global' | Attribute:'EmailHeader' }}

//<p>
//    This email contains a secure link to provide a digital signature for the {{  SignatureDocument.SignatureDocumentTemplate.Name }} document. Please 
//    do not forward or share this email, link, or access code with others. If you believe this email was sent to you in 
//    error, please let us know at {{ 'Global' | Attribute:'OrganizationEmail' }}. 
//</p>

//<p>
//    <table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
//     <tr>
//       <td>
    
//    		<div><!--[if mso]>
//    		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
//    			<w:anchorlock/>
//    			<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
//    		  </v:roundrect>
//    		<![endif]--><a href=""{{ InviteLink }}""
//    		style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">Sign Now</a></div>
    
//    	</td>
//     </tr>
//    </table>
//</p>

//<p>
//    This invite will expire in 15 days.
//</p>

//{{ 'Global' | Attribute:'EmailFooter' }}", "791F2DE4-5A59-60AE-4F2F-FDC3EBC4FFA9" );

        }

    /// <summary>
    /// Operations to be performed during the downgrade process.
    /// </summary>
    public override void Down()
        {
            AddColumn("dbo.SignatureDocument", "RequestDate", c => c.DateTime());
            AddColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateBody", c => c.String());
            AddColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateSubject", c => c.String());
            AddColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateFromAddress", c => c.String());
            AddColumn("dbo.SignatureDocumentTemplate", "RequestEmailTemplateFromName", c => c.String());
            DropForeignKey("dbo.SignatureDocumentTemplate", "InviteSystemEmailId", "dbo.SystemEmail");
            DropIndex("dbo.SignatureDocumentTemplate", new[] { "InviteSystemEmailId" });
            DropColumn("dbo.SignatureDocument", "InviteCount");
            DropColumn("dbo.SignatureDocument", "LastInviteDate");
            DropColumn("dbo.SignatureDocumentTemplate", "InviteSystemEmailId");
            RenameIndex(table: "dbo.SignatureDocument", name: "IX_SignatureDocumentTemplateId", newName: "IX_SignatureDocumentTypeId");
            RenameIndex(table: "dbo.RegistrationTemplate", name: "IX_RequiredSignatureDocumentTemplateId", newName: "IX_RequiredSignatureDocumentTypeId");
            RenameIndex(table: "dbo.Group", name: "IX_RequiredSignatureDocumentTemplateId", newName: "IX_RequiredSignatureDocumentTypeId");
            RenameColumn(table: "dbo.Group", name: "RequiredSignatureDocumentTemplateId", newName: "RequiredSignatureDocumentTypeId");
            RenameColumn(table: "dbo.RegistrationTemplate", name: "RequiredSignatureDocumentTemplateId", newName: "RequiredSignatureDocumentTypeId");
            RenameColumn(table: "dbo.SignatureDocument", name: "SignatureDocumentTemplateId", newName: "SignatureDocumentTypeId");
            RenameTable(name: "dbo.SignatureDocumentTemplate", newName: "SignatureDocumentType");
        }
    }
}
