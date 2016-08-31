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
    public partial class MiscCommunication : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.PhoneNumber", "PersonId", "dbo.Person");
            AddForeignKey("dbo.PhoneNumber", "PersonId", "dbo.Person", "Id", cascadeDelete: true);

            Sql( @"
    -- Update layout to FullWidthPanel
    UPDATE [Page] SET [LayoutId] = (SELECT [Id] FROM [Layout] WHERE [Guid] = '195BCD57-1C10-4969-886F-7324B6287B75')
    WHERE [Guid] = '5B4A4DF6-17BB-4C99-B5B7-6DC9C896BC8E'

    -- Switch SMS Number Names and Descriptions
    UPDATE DV SET
	    [Name] = DV.[Description],
	    [Description] = DV.[Name]
    FROM [DefinedValue] DV
	    INNER JOIN [DefinedType] DT ON DT.[Id] = DV.[DefinedTypeId]
    WHERE DT.[Guid] = '611BDE1F-7405-4D16-8626-CCFEDB0E62BE'
	    AND DV.[Name] NOT LIKE '+%'
" );

            AddEntityAttribute( "Rock.Communication.Channel.Email", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "", "", "Unsubscribe HTML", "",
    "The HTML to inject into email contents when the communication is a Bulk Communication.  Contents will be placed wherever the 'Unsubcribe HTML' merge field is used, or if not used, at the end of the email in email contents.",
    2, @"
<p style=''float: right;''>
    <small><a href=''{{ GlobalAttribute.PublicApplicationRoot }}Unsubscribe/{{ Person.PrimaryAlias.UrlEncodedKey }}''>Unsubscribe</a></small>
</p>
", "2942EFCB-9BCF-4A16-9A78-D6149E2EAAD3" );
            AddEntityAttribute( "Rock.Communication.Channel.Email", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "", "", "Default Plain Text", "",
    "The text to use when the plain text field is left blank.",
    3, @"
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ GlobalAttribute.PublicApplicationRoot }}GetCommunication.ashx?c={{ Communication.Id }}&p={{ Person.UrlEncodedKey }}
", "FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88" );

            AddAttributeValue( "2942EFCB-9BCF-4A16-9A78-D6149E2EAAD3", 0, @"
<p style=''float: right;''>
    <small><a href=''{{ GlobalAttribute.PublicApplicationRoot }}Unsubscribe/{{ Person.UrlEncodedKey }}''>Unsubscribe</a></small>
</p>
", "B6E2738F-CEB3-4BE3-ACD7-62EF875D5FAE" );
            AddAttributeValue( "FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88", 0, @"
Unfortunately, you cannot view the contents of this email as it contains formatting that is not supported 
by your email client.  

You can view an online version of this email here: 
{{ GlobalAttribute.PublicApplicationRoot }}GetCommunication.ashx?c={{ Communication.Id }}&p={{ Person.UrlEncodedKey }}
", "F44929D6-B5D1-4D6F-8A57-29C2FCFC80E2" );

            // Give Approve authorization to Communication Admins for Communication block
            AddSecurityAuthForBlock( "BD9B2F32-AB18-4761-80C9-FDA4DBEEA9EC", 0, "Approve", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", Model.SpecialRole.None, "25A49D1C-B27B-4F31-80E7-A08F443FA425" );

            // Give Edit authorization to Communication Admins for Communication block
            AddSecurityAuthForBlock( "8B080D88-D088-4D09-9D74-576B485549A2", 0, "Edit", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", Model.SpecialRole.None, "29CD0576-4885-43E8-97A7-33A1BE9421B4" );

            // Give Approve authorization to Communication Admins for Ad Detail block
            AddSecurityAuthForBlock( "EEA597FB-BE71-4A8E-A671-9C80856FB761", 0, "Approve", true, "B1906B7D-1A1E-41B9-BBA4-F4482CECAF7B", Model.SpecialRole.None, "E4A488B7-0AB6-4C39-B66E-5C7E92646C8F" );

            // Add attribute to components to disable ordering
            UpdateBlockTypeAttribute("21F5F466-59BC-40B2-8D73-7314D936C3CB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Component Container", "ComponentContainer", "", "The Rock Extension Managed Component Container to manage", 1, "", "259AF14D-0214-4BE4-A7BF-40423EA07C99");
            UpdateBlockTypeAttribute( "21F5F466-59BC-40B2-8D73-7314D936C3CB", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Support Ordering", "SupportOrdering", "", "Should user be allowed to re-order list of components?", 2, "True", "A4889D7B-87AA-419D-846C-3E618E79D875" );
            AddBlockAttributeValue( "803CE253-3ADA-4C2A-B62F-EC4D5B7B7257", "A4889D7B-87AA-419D-846C-3E618E79D875", "True" );
            AddBlockAttributeValue( "73CE9F13-43F1-4DD4-AA5B-70A48C5A6D85", "A4889D7B-87AA-419D-846C-3E618E79D875", "True" );
            AddBlockAttributeValue( "10D2886B-40F6-47EE-B137-23595FAC224D", "A4889D7B-87AA-419D-846C-3E618E79D875", "True" );
            AddBlockAttributeValue( "B6F6DBF7-96CA-4A6A-AFB3-ED2278EEB70E", "A4889D7B-87AA-419D-846C-3E618E79D875", "False" );
            AddBlockAttributeValue( "190A276D-81B5-4EF6-B1EB-1ABC68B4D770", "A4889D7B-87AA-419D-846C-3E618E79D875", "True" );
            AddBlockAttributeValue( "FA8EA948-150A-4668-BDEF-E3669FAC695E", "A4889D7B-87AA-419D-846C-3E618E79D875", "True" );
            AddBlockAttributeValue( "8966CAFE-D8FC-4703-8960-17CB5807A3B8", "A4889D7B-87AA-419D-846C-3E618E79D875", "True" );
            AddBlockAttributeValue( "F87CE2AA-8B89-4D2D-9EF4-3C2CC8D48440", "A4889D7B-87AA-419D-846C-3E618E79D875", "False" );
            AddBlockAttributeValue( "8C707818-ECB1-4E40-8F2C-6E9802E6BA73", "A4889D7B-87AA-419D-846C-3E618E79D875", "True" );
            AddBlockAttributeValue( "66024082-B8B4-43A8-A94E-F313A0998596", "A4889D7B-87AA-419D-846C-3E618E79D875", "True" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.PhoneNumber", "PersonId", "dbo.Person");
            AddForeignKey("dbo.PhoneNumber", "PersonId", "dbo.Person", "Id");
        }
    }
}
