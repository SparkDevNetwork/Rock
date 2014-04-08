// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class MiscCommunication : Rock.Migrations.RockMigration
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
    "The HTML to inject into email contents when the communication is a Bulk Email.  Contents will be placed wherever the 'Unsubcribe HTML' merge field is used, or if not used, at the end of the email in email contents.",
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
