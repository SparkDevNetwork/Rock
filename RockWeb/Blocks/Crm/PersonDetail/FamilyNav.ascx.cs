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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    [DisplayName( "Family Navigation" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows you to switch between the members of the family the person belongs to." )]

    public partial class FamilyNav : Rock.Web.UI.PersonBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            BuildDropDown();
        }

        protected void BuildDropDown()
        {
            var sb = new StringBuilder(
            $@"<a href=""#"" id=""familyDropdownNav"" class=""profile-toggle"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"">
                    <img src=""{Person.GetPersonPhotoUrl( this.Person )}"" class=""avatar mr-2 flex-shrink-0"" alt=""""></img>
                    <span class=""d-none d-sm-inline text-nowrap font-weight-semibold"">{this.Person.FullName}<i class=""fa fa-chevron-down ml-2""></i></span>
                </a>

                <ul class=""dropdown-menu"" aria-labelledby=""familyDropdownNav"">");

            sb.AppendLine();

            var familyMembers = GetFamilyMembers();
            foreach ( var familyMember in familyMembers )
            {
                sb.Append( CreateFamilyMemberListItem( familyMember ) );
            }

            sb.AppendLine( "</ul>" );
            litFamilyMemberNav.Text = sb.ToString();
        }

        private string CreateFamilyMemberListItem( GroupMember groupMember )
        {
            var personLink = FormatPersonLink( groupMember.Person.Id.ToString() );
            var familyMemberListItem = $@"
                <li>
                    <a href=""{personLink}"">
                        <img src=""{Person.GetPersonPhotoUrl( groupMember.PersonId )}"" alt="""" class=""avatar"">
                        <span class=""name"">
                            {groupMember.Person.FullName}
                        </span>
                    </a>
                </li>";

            return familyMemberListItem;
        }

        private List<GroupMember> GetFamilyMembers()
        {
            var members = new GroupMemberService( new RockContext() )
                .Queryable( "GroupRole,Person", true )
                .Where( m => m.GroupId == this.Person.PrimaryFamilyId && m.PersonId != this.Person.Id )
                .OrderBy( m => m.GroupRole.Order )
                .ToList();

            var orderedMembers = new List<GroupMember>();

            // Add adult males
            orderedMembers.AddRange( members
                .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) && m.Person.Gender == Gender.Male )
                .OrderByDescending( m => m.Person.Age ) );

            // Add adult females
            orderedMembers.AddRange( members
                .Where( m => m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) && m.Person.Gender != Gender.Male )
                .OrderByDescending( m => m.Person.Age ) );

            // Add non-adults
            orderedMembers.AddRange( members
                .Where( m => !m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ) )
                .OrderByDescending( m => m.Person.Age ) );

            return orderedMembers;
        }

        protected string FormatPersonLink( string personId )
        {
            var currentPersonId = Person.Id.ToString();

            if ( PageCache.PageContexts.ContainsKey( Person.TypeName ) )
            {
                currentPersonId = PageParameter( PageCache.PageContexts[Person.TypeName] );
            }

            // Look for a subpage route (anything after the "/Person/{id}" part of the URL)
            var subpageRoute = Request.UrlProxySafe().AbsolutePath.ReplaceCaseInsensitive( ResolveRockUrl( $"~/Person/{currentPersonId}" ), "" );

            // If the path is different, then append it onto the link
            if ( subpageRoute != Request.UrlProxySafe().AbsolutePath )
            {
                return ResolveRockUrl( string.Format( "~/Person/{0}{1}", personId, subpageRoute ) );
            }
            else
            {
                return ResolveRockUrl( string.Format( "~/Person/{0}", personId ) );
            }
        }
    }
}