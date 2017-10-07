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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Communication List Subscribe" )]
    [Category( "Communication" )]
    [Description( "Block that allows a person to manage the communication lists that they are subscribed to" )]
    [GroupCategoryField( "Communication List Categories", "Select the categories of the communication lists to display, or select none to show all.", true, Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST, required: false, order: 1 )]
    public partial class CommunicationListSubscribe : RockBlock
    {
        #region fields

        Dictionary<int, GroupMember> personCommunicationListsMember = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
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
                BindRepeater();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindRepeater();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptCommunicationLists control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptCommunicationLists_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var group = e.Item.DataItem as Rock.Model.Group;

            if ( group != null )
            {
                var cbCommunicationListIsSubscribed = e.Item.FindControl( "cbCommunicationListIsSubscribed" ) as RockCheckBox;
                var rblCommunicationPreference = e.Item.FindControl( "rblCommunicationPreference" ) as RockRadioButtonList;

                cbCommunicationListIsSubscribed.Text = group.GetAttributeValue( "PublicName" );
                var groupMember = personCommunicationListsMember.GetValueOrNull( group.Id );
                if ( cbCommunicationListIsSubscribed.Text.IsNullOrWhiteSpace() )
                {
                    cbCommunicationListIsSubscribed.Text = group.Name;

                }

                cbCommunicationListIsSubscribed.Checked = groupMember != null && groupMember.GroupMemberStatus == GroupMemberStatus.Active;

                CommunicationType communicationType = CurrentPerson.CommunicationPreference == CommunicationType.SMS ? CommunicationType.SMS : CommunicationType.Email;
                if ( groupMember != null )
                {
                    groupMember.LoadAttributes();
                    var groupMemberCommunicationType = ( CommunicationType? ) groupMember.GetAttributeValue( "PreferredCommunicationMedium" ).AsIntegerOrNull();
                    if ( groupMemberCommunicationType.HasValue )
                    {
                        // if GroupMember record has SMS or Email specified, that takes precedence over their Person.CommunicationPreference
                        if ( groupMemberCommunicationType.Value == CommunicationType.SMS )
                        {
                            communicationType = CommunicationType.SMS;
                        }
                        else if ( groupMemberCommunicationType.Value == CommunicationType.Email )
                        {
                            communicationType = CommunicationType.Email;
                        }
                    }
                }

                rblCommunicationPreference.SetValue( ( int ) communicationType );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            nbResult.Text = "#TODO# Email Preferences Updated";
            nbResult.NotificationBoxType = NotificationBoxType.Success;
            nbResult.Visible = true;
            pnlCommunicationPreferences.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            nbResult.Text = "No changes made to email preferences";
            nbResult.NotificationBoxType = NotificationBoxType.Info;
            nbResult.Visible = true;
            pnlCommunicationPreferences.Visible = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the repeater.
        /// </summary>
        protected void BindRepeater()
        {
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var categoryService = new CategoryService( rockContext );

            int communicationListGroupTypeId = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ).Id;
            var communicationListQry = groupService.Queryable().Where( a => a.GroupTypeId == communicationListGroupTypeId );

            var categoryGuids = this.GetAttributeValue( "CommunicationListCategories" ).SplitDelimitedValues().AsGuidList();

            var communicationLists = communicationListQry.ToList();
            var viewableCommunicationLists = new List<Group>();
            foreach ( var communicationList in communicationLists )
            {
                communicationList.LoadAttributes( rockContext );
                if ( !categoryGuids.Any() )
                {
                    // if no categories where specified, only show lists that the person has VIEW auth
                    if ( communicationList.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                    {
                        viewableCommunicationLists.Add( communicationList );
                    }
                }
                else
                {
                    Guid? categoryGuid = communicationList.GetAttributeValue( "Category" ).AsGuidOrNull();
                    if ( categoryGuid.HasValue && categoryGuids.Contains( categoryGuid.Value ) )
                    {
                        viewableCommunicationLists.Add( communicationList );
                    }
                }
            }

            viewableCommunicationLists = viewableCommunicationLists.OrderBy( a =>
            {
                var name = a.GetAttributeValue( "PublicName" );
                if ( name.IsNullOrWhiteSpace() )
                {
                    name = a.Name;
                }

                return name;
            } ).ToList();

            var groupIds = viewableCommunicationLists.Select( a => a.Id ).ToList();
            var personId = this.CurrentPersonId.Value;

            personCommunicationListsMember = new GroupMemberService( rockContext ).Queryable()
                .Where( a => groupIds.Contains( a.GroupId ) && a.PersonId == personId )
                .GroupBy( a => a.GroupId )
                .ToList().ToDictionary( k => k.Key, v => v.FirstOrDefault() );

            rptCommunicationLists.DataSource = viewableCommunicationLists;
            rptCommunicationLists.DataBind();

            nbNoCommunicationLists.Visible = !viewableCommunicationLists.Any();
            pnlCommunicationPreferences.Visible = viewableCommunicationLists.Any();
        }

        #endregion

        
    }
}