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
    public partial class UpdateLoginAttributes : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE [Page]
        SET
	     [InternalName] = 'System Emails'
	    ,[PageTitle] = 'System Emails'
	    ,[BrowserTitle] = 'System Emails'
        ,[IconCssClass] = 'fa fa-envelope'
    WHERE
	    [Guid] = '89B7A631-EA6F-4DA3-9380-04EE67B63E9E'
            
" );
            DeleteBlockAttribute( "BD6CB735-C86A-4D0D-BDA8-FBF1AAA261E9" );
            DeleteBlockAttribute( "9B22FDA2-A821-4CD6-9ED6-C95DD9E04107" );

            // Attrib for BlockType: Login:Confirm Caption
            AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Confirm Caption", "ConfirmCaption", "",
                "The text (HTML) to display when a user's account needs to be confirmed.", 2,
                @"
Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.
", "9B22FDA2-A821-4CD6-9ED6-C95DD9E04107" );
            // Attrib for BlockType: Login:Locked Out Caption
            AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Locked Out Caption", "LockedOutCaption", "",
                "The text (HTML) to display when a user's account has been locked.", 5,
                @"
Sorry, your account has been locked.  Please contact our office at {{ GlobalAttribute.OrganizationPhone }} or email {{ GlobalAttribute.OrganizationEmail }} to resolve this.  Thank-you. 
", "BD6CB735-C86A-4D0D-BDA8-FBF1AAA261E9" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DeleteBlockAttribute( "BD6CB735-C86A-4D0D-BDA8-FBF1AAA261E9" );
            DeleteBlockAttribute( "9B22FDA2-A821-4CD6-9ED6-C95DD9E04107" );

            // Attrib for BlockType: Login:Locked Out Caption
            AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Locked Out Caption", "LockedOutCaption", "", "", 5, @"Sorry, your account has temporarily been locked.  Please contact our office get it reactivated.", "BD6CB735-C86A-4D0D-BDA8-FBF1AAA261E9" );
            // Attrib for BlockType: Login:Confirm Caption
            AddBlockTypeAttribute( "7B83D513-1178-429E-93FF-E76430E038E4", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Confirm Caption", "ConfirmCaption", "", "", 2, @"Thank-you for logging in, however, we need to confirm the email associated with this account belongs to you. We've sent you an email that contains a link for confirming.  Please click the link in your email to continue.", "9B22FDA2-A821-4CD6-9ED6-C95DD9E04107" );
        }
    }
}
