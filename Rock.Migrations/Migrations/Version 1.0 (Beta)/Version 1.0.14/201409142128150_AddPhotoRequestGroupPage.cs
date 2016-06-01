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
    public partial class AddPhotoRequestGroupPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // fix improper casing in application group type
            Sql( @"UPDATE [GroupType]
                    SET [GroupTerm] = 'Group', [GroupMemberTerm] = 'Member'
                    WHERE [Guid] = '3981CF6D-7D15-4B57-AACE-C0E25D28BD49'" );


            
            // add pages for photo request group 
            RockMigrationHelper.AddPage( "325B50D6-545D-461A-9CB7-72B001E82F21", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Photo Request Application Group", "", "372BAF1A-F619-46FC-A69A-61E2A0A82F0E", "fa fa-users" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "372BAF1A-F619-46FC-A69A-61E2A0A82F0E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Member Detail", "", "34491B77-E94D-4DA6-9E74-0F6086522E4C", "" ); // Site:Rock RMS
            // Add Block to Page: Photo Request Application Group, Site: Rock RMS
            RockMigrationHelper.AddBlock( "372BAF1A-F619-46FC-A69A-61E2A0A82F0E", "", "88B7EFA9-7419-4D05-9F88-38B936E61EDD", "Group Member List", "Main", "", "", 0, "B99901FD-E852-4FCF-8F9B-0870984D59AE" );

            // Add Block to Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "34491B77-E94D-4DA6-9E74-0F6086522E4C", "", "AAE2E5C3-9279-4AB0-9682-F4D19519D678", "Group Member Detail", "Main", "", "", 0, "E7C10335-2BEE-47AF-AC6C-AEC791585418" );

            // Attrib for BlockType: Data View Detail:Database Timeout
            RockMigrationHelper.AddBlockTypeAttribute( "EB279DF9-D817-4905-B6AC-D9883F0DA2E4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Database Timeout", "DatabaseTimeout", "", "The number of seconds to wait before reporting a database timeout.", 0, @"180", "266C0335-48AF-4B74-9BA8-DADEBFAAC471" );

            // Attrib Value for Block:Group Member List, Attribute:Group Page: Photo Request Application Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B99901FD-E852-4FCF-8F9B-0870984D59AE", "9F2D3674-B780-4CD3-B4AB-3DF3EA21905A", @"42" );

            // Attrib Value for Block:Group Member List, Attribute:Detail Page Page: Photo Request Application Group, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B99901FD-E852-4FCF-8F9B-0870984D59AE", "E4CCB79C-479F-4BEE-8156-969B2CE05973", @"34491b77-e94d-4da6-9e74-0f6086522e4c" );

            // update page title of the giving history page
            Sql( @"UPDATE [Page]
                    SET [PageTitle] = 'Giving History', [InternalName] = 'Giving History', [BrowserTitle] = 'Giving History'
                    WHERE [Guid] = '621E03C5-6586-450B-A340-CC7D9727B69A'" );


            // disable debug mode on profile managing page
            Sql( @"UPDATE [AttributeValue]
                    SET [Value] = 'False'
                    WHERE [Guid] = 'F86E5A4A-F92A-428B-956D-FFAA6508443E'" );

            // update the label on the giving history block
            Sql( @"UPDATE [AttributeValue]
                    SET [Value] = 'Accounts'
                    WHERE [Guid] = '49758E66-1158-4C84-B204-05AA604D21AA'" );

            Sql( @"UPDATE [DefinedValue]
                    SET [Description] = N'Rock uses Google Maps to help visualize where your ministry happens.  To enable this, each organization must get a Google Maps key. To complete this do the following:
                    <ol>
                    <li>Visit the <a target=""_blank"" href=""https://code.google.com/apis/console"">Google APIs Console</a> and log in with the Google Account you wish to tie the key to (perhaps not your personal account).</li>
                    <li>Click the <em>APIs</em> link under APIs & auth from the left-hand menu.</li>
                    <li>Activate the <em>Google Maps JavaScript API v3</em> service.</a></li>
                    <li>Click the <em>Credentials</em> link from the left-hand menu. Your API key is available from the <em>Public API access</em> section. Press the ""Create new Key"" button if you don''t yet have a key. Maps API applications use Maps API applications use the Key for browser apps.</li>
                    <li>Enter this key under <span class=""navigation-tip"">Administration > General Settings > Global Attributes > Google API Key</span>.</li>
                    </ol>
                    This key will allow your organization 25,000 transactions a day.  That''s quite a bit, but if you think you need more you can apply for a free <a href=""http://www.google.com/earth/outreach/grants/software/mapsapi.html"">Google Maps Grant</a>.'
                    WHERE [Guid] = '92fa16fa-39e3-4364-9412-aa322c9ef01a'
            " );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Data View Detail:Database Timeout
            RockMigrationHelper.DeleteAttribute( "266C0335-48AF-4B74-9BA8-DADEBFAAC471" );
            // Remove Block: Group Member Detail, from Page: Group Member Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "E7C10335-2BEE-47AF-AC6C-AEC791585418" );
            // Remove Block: Group Member List, from Page: Photo Request Application Group, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "B99901FD-E852-4FCF-8F9B-0870984D59AE" );
            RockMigrationHelper.DeletePage( "34491B77-E94D-4DA6-9E74-0F6086522E4C" ); //  Page: Group Member Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( "372BAF1A-F619-46FC-A69A-61E2A0A82F0E" ); //  Page: Photo Request Application Group, Layout: Full Width, Site: Rock RMS
        }
    }
}
