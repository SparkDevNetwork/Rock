﻿// <copyright>
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// The main Person Profile block the main information about a person
    /// </summary>
    [DisplayName( "Person Bio Summary" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Person name, picture, and badges." )]

    [SecurityAction( Authorization.VIEW_PROTECTION_PROFILE, "The roles and/or users that can view the protection profile alert for the selected person." )]

    #region Block Attributes

    [BadgesField(
        "Badges",
        Key = AttributeKey.Badges,
        Description = "The label badges to display in this block.",
        IsRequired = false,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "7249D05F-0FD1-4F44-88EB-AD46DEB1DAEA" )]
    public partial class BioSummary : PersonBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string Badges = "Badges";
        }

        private static class PageParameterKey
        {
            public const string PersonId = "PersonId";
        }

        private static class SharedItemKey
        {
            public const string GroupType = "GroupType";
            public const string ShowOnlyPrimaryGroupMembers = "ShowOnlyPrimaryGroupMembers";
        }

        #endregion Attribute Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( pnlContent );

            if ( Person == null )
            {
                return;
            }

            // If this is the empty person, check for an old Alias record and if found, redirect permanent to it instead.
            if ( Person.Id == 0 )
            {
                //referring to aliasPersonId as person might be merged
                var personId = this.PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

                if ( personId.HasValue )
                {
                    var personAlias = new PersonAliasService( new RockContext() ).GetByAliasId( personId.Value );
                    if ( personAlias != null )
                    {
                        var pageReference = RockPage.PageReference;
                        pageReference.Parameters.AddOrReplace( PageParameterKey.PersonId, personAlias.PersonId.ToString() );
                        Response.RedirectPermanent( pageReference.BuildUrl(), false );
                    }
                }
            }

            if ( Person.IsDeceased )
            {
                pnlContent.Attributes.Add( "class","card card-profile card-profile-bio card-profile-bio-condensed deceased" );
            }
            else
            {
                pnlContent.Attributes.Add("class","card card-profile card-profile-bio card-profile-bio-condensed");
            }

            // Set the browser page title to include person's name
            RockPage.BrowserTitle = Person.FullName;

            ShowProtectionLevel();
            ShowBadgeList();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // dont' show if there isn't a person, or if it is a 'Nameless" person record type
                if ( Person == null || Person.Id == 0 || Person.RecordTypeValueId == DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ) )
                {
                    pnlContent.Visible = false;
                    return;
                }

                ShowPersonImage();
                ShowPersonName();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // reload the page if block settings where changed
            Response.Redirect( Request.RawUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        #endregion Base Control Methods

        #region Methods

        private void ShowProtectionLevel()
        {
            if ( Person.AccountProtectionProfile > Rock.Utility.Enums.AccountProtectionProfile.Low )
            {
                string acctProtectionLevel = $@"
                    <div class=""protection-profile"">
                        <span class=""profile-label"">Protection Profile: {Person.AccountProtectionProfile.ConvertToString( true )}</span>
                        <i class=""fa fa-fw fa-lock"" onmouseover=""$(this).parent().addClass('is-hovered')"" onmouseout=""$(this).parent().removeClass('is-hovered')""></i>
                    </div>";

                litAccountProtectionLevel.Text = acctProtectionLevel;
            }
        }

        private void ShowPersonImage()
        {
            lImage.Text = $@"<img src=""{Person.GetPersonPhotoUrl( Person, 400 )}&Style=icon"" alt class=""img-profile"">";
        }

        private string GetPersonName()
        {
            // Check if this record represents a Business.
            bool isBusiness = false;

            if ( Person.RecordTypeValueId.HasValue )
            {
                int recordTypeValueIdBusiness = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                isBusiness = ( Person.RecordTypeValueId.Value == recordTypeValueIdBusiness );
            }

            if ( isBusiness )
            {
                return $@"<h1 class=""person-name is-business"">{Person.LastName}</h1>";
            }

            // Prefix with Title if they have a Title with IsFormal=True
            string titleText = string.Empty;
            if ( Person.TitleValueId.HasValue )
            {
                var personTitleValue = DefinedValueCache.Get( Person.TitleValueId.Value );
                if ( personTitleValue != null && personTitleValue.GetAttributeValue( "IsFormal" ).AsBoolean() )
                {
                    titleText = $"{personTitleValue.Value} ";
                }
            }

            string nameText =  $"{titleText}{Person.NickName} {Person.LastName}";

            return $@"<h1 class=""person-name"">{nameText}</h1>";
        }

        private void ShowBadgeList()
        {
            string badgeList = GetAttributeValue( AttributeKey.Badges );
            if ( !string.IsNullOrWhiteSpace( badgeList ) )
            {
                foreach ( string badgeGuid in badgeList.SplitDelimitedValues() )
                {
                    Guid guid = badgeGuid.AsGuid();
                    if ( guid != Guid.Empty )
                    {
                        var badge = BadgeCache.Get( guid );
                        if ( badge != null )
                        {
                            blStatus.BadgeTypes.Add( badge );
                        }
                    }
                }
            }
        }

        private void ShowPersonName()
        {
            var groupTypeId = GroupTypeCache.GetId( RockPage.GetSharedItem( SharedItemKey.GroupType )?.ToString()?.AsGuid() ?? Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
            var showOnlyPrimaryGroupMembers = RockPage.GetSharedItem( SharedItemKey.ShowOnlyPrimaryGroupMembers )?.ToString()?.AsBoolean() ?? false;

            var groupMemberService = new GroupMemberService( new RockContext() );
            var orderedGroupMemberList = groupMemberService.GetSortedGroupMemberListForPerson( this.Person.Id, groupTypeId.Value, showOnlyPrimaryGroupMembers ).ToList();

            var personNameHtml = GetPersonName();
            if ( !orderedGroupMemberList.Any() )
            {
                lName.Text = personNameHtml;
                return;
            }

            var sb = new StringBuilder(
                $@"<div class=""dropdown dropdown-family"">
<a href=""#"" class=""profile-toggle"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""true"">
{personNameHtml}
<i class=""fa fa-chevron-down ml-2""></i>
</a>
<ul class=""dropdown-menu"">" );

            foreach ( var groupMember in orderedGroupMemberList )
            {
                sb.Append( CreateGroupMemberListItem( groupMember ) );
            }

            sb.AppendLine( "</ul>" );
            sb.AppendLine( "</div>" );

            lName.Text = sb.ToString();
        }

        private string CreateGroupMemberListItem( GroupMember groupMember )
        {
            var personLink = FormatPersonLink( groupMember.Person.Id.ToString() );
            var groupMemberListItem = $@"
                <li>
                    <a href=""{personLink}"">
                        <img src=""{Person.GetPersonPhotoUrl( groupMember.Person.Initials, groupMember.Person.PhotoId, groupMember.Person.Age, groupMember.Person.Gender, groupMember.Person.RecordTypeValueId, groupMember.Person.AgeClassification )}&Style=icon"" alt="""" class=""avatar"">
                        <span class=""name"">
                            {groupMember.Person.FullName}
                        </span>
                    </a>
                </li>";

            return groupMemberListItem;
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

        #endregion Methods

    }
}