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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.church_ccv.Groups
{
    [DisplayName( "Next Steps Group Member Detail" )]
    [Category( "CCV > Groups" )]
    [Description( "Displays the details of the given Next Steps group member " )]
    public partial class NextStepsGroupMemberDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbErrorMessage.Text = string.Empty;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "GroupMemberId" ).AsInteger() );
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveGroupMember();

            if ( cvGroupMember.IsValid )
            {
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["GroupId"] = hfGroupId.Value;
                NavigateToParentPage( qryString );
            }
        }

        /// <summary>
        /// Saves the group member.
        /// </summary>
        private void SaveGroupMember()
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();

                GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                GroupMember groupMember;

                int groupMemberId = int.Parse( hfGroupMemberId.Value );

                // load existing group member
                groupMember = groupMemberService.Get( groupMemberId );

                var optOutReason = ddlOptOutReason.SelectedValueAsEnumOrNull<OptOutReason>();
                if ( !optOutReason.HasValue )
                {
                    groupMember.GroupMemberStatus = rblActivePendingStatus.SelectedValueAsEnum<GroupMemberStatus>();
                }
                else
                {
                    groupMember.GroupMemberStatus = GroupMemberStatus.Inactive;
                }

                groupMember.LoadAttributes();
                if ( optOutReason.HasValue )
                {
                    groupMember.SetAttributeValue( "OptOutReason", optOutReason.ConvertToInt().ToString() );
                    if ( dpFollowUpDate.Visible && dpFollowUpDate.SelectedDate.HasValue )
                    {
                        groupMember.SetAttributeValue( "FollowUpDate", dpFollowUpDate.SelectedDate.Value.ToString( "o" ) );
                    }
                    else
                    {
                        groupMember.SetAttributeValue( "FollowUpDate", null );
                    }
                }
                else
                {
                    groupMember.SetAttributeValue( "OptOutReason", null );
                    groupMember.SetAttributeValue( "FollowUpDate", null );
                }

                if ( !Page.IsValid )
                {
                    return;
                }

                // see if there's a valid email address
                if ( string.IsNullOrEmpty( ebEmailAddress.Text ) == true ||
                    ebEmailAddress.Text.IsValidEmail() )
                {
                    groupMember.Person.Email = ebEmailAddress.Text;
                }

                // set their home and mobile phone
                var homeNumberType = Rock.Web.Cache.DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) );
                var mobileNumberType = Rock.Web.Cache.DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );

                groupMember.Person.UpdatePhoneNumber( homeNumberType.Id, pnHome.CountryCode, pnHome.Text, null, null, rockContext );
                groupMember.Person.UpdatePhoneNumber( mobileNumberType.Id, pnMobile.CountryCode, pnMobile.Text, null, null, rockContext );

                // set their anniversary date
                groupMember.Person.AnniversaryDate = dpAnniversaryDate.SelectedDate;

                // if the groupMember IsValid is false, and the UI controls didn't report any errors, it is probably because the custom rules of GroupMember didn't pass.
                // So, make sure a message is displayed in the validation summary
                cvGroupMember.IsValid = groupMember.IsValid;

                if ( !cvGroupMember.IsValid )
                {
                    cvGroupMember.ErrorMessage = groupMember.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                    return;
                }

                // using WrapTransaction because there are three Saves
                rockContext.WrapTransaction( () =>
                {
                    if ( groupMember.Id.Equals( 0 ) )
                    {
                        groupMemberService.Add( groupMember );
                    }

                    rockContext.SaveChanges();
                    groupMember.SaveAttributeValues( rockContext );
                } );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfGroupMemberId.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["GroupId"] = hfGroupId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                GroupMemberService groupMemberService = new GroupMemberService( new RockContext() );
                GroupMember groupMember = groupMemberService.Get( int.Parse( hfGroupMemberId.Value ) );

                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["GroupId"] = groupMember.GroupId.ToString();
                NavigateToParentPage( qryString );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        public void ShowDetail( int groupMemberId )
        {
            ShowDetail( groupMemberId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        /// <param name="groupId">The group id.</param>
        public void ShowDetail( int groupMemberId, int? groupId )
        {
            var rockContext = new RockContext();
            GroupMember groupMember = new GroupMemberService( rockContext ).Get( groupMemberId );

            if ( groupMember == null )
            {
                if ( groupMemberId > 0 )
                {
                    nbErrorMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Warning;
                    nbErrorMessage.Title = "Warning";
                    nbErrorMessage.Text = "Group Member not found. Group Member may have been moved to another group or deleted.";
                }
                else
                {
                    nbErrorMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                    nbErrorMessage.Title = "Invalid Request";
                    nbErrorMessage.Text = "An incorrect querystring parameter was used.  A valid GroupMemberId parameter is required.";
                }

                pnlEditDetails.Visible = false;
                return;
            }

            pnlEditDetails.Visible = true;

            hfGroupId.Value = groupMember.GroupId.ToString();
            hfGroupMemberId.Value = groupMember.Id.ToString();

            LoadDropDowns();

            lPersonName.Text = groupMember.Person.ToString();
            lGroupRole.Text = groupMember.GroupRole.ToString();
            rblActivePendingStatus.SetValue( groupMember.GroupMemberStatus.ConvertToInt() );
            groupMember.LoadAttributes();
            var optOutReason = groupMember.GetAttributeValue( "OptOutReason" ).ConvertToEnumOrNull<OptOutReason>();
            if ( optOutReason.HasValue )
            {
                ddlOptOutReason.SetValue( optOutReason.ConvertToInt() );
            }
            else
            {
                ddlOptOutReason.SetValue( string.Empty );
            }

            // get their email
            ebEmailAddress.Text = groupMember.Person.Email;


            // get their home and mobile phone numbers
            var homeNumberType = Rock.Web.Cache.DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME ) );
            var mobileNumberType = Rock.Web.Cache.DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) );

            var homephone = groupMember.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == homeNumberType.Id ).FirstOrDefault();
            if ( homephone != null )
            {
                pnHome.Text = homephone.NumberFormatted;
            }
            
            var mobilephone = groupMember.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == mobileNumberType.Id ).FirstOrDefault();
            if ( mobilephone != null )
            {
                pnMobile.Text = mobilephone.NumberFormatted;
            }

            // get their anniversary date
            dpAnniversaryDate.SelectedDate = groupMember.Person.AnniversaryDate;

            var followUpDate = groupMember.GetAttributeValue( "FollowUpDate" );

            dpFollowUpDate.SelectedDate = followUpDate.AsDateTime();

            SetControlVisibilities();
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            rblActivePendingStatus.Items.Clear();
            rblActivePendingStatus.Items.Add( new ListItem( GroupMemberStatus.Active.ConvertToString(), GroupMemberStatus.Active.ConvertToInt().ToString() ) );
            rblActivePendingStatus.Items.Add( new ListItem( GroupMemberStatus.Pending.ConvertToString(), GroupMemberStatus.Pending.ConvertToInt().ToString() ) );

            ddlOptOutReason.BindToEnum<OptOutReason>( true );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlOptOutReason control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlOptOutReason_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetControlVisibilities();
        }

        /// <summary>
        /// Sets the control visibilities.
        /// </summary>
        private void SetControlVisibilities()
        {
            dpFollowUpDate.Visible = ddlOptOutReason.SelectedValueAsEnumOrNull<OptOutReason>() == OptOutReason.FollowUpLater;
            rblActivePendingStatus.Visible = ddlOptOutReason.SelectedValueAsEnumOrNull<OptOutReason>() == null;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private enum OptOutReason
        {
            /// <summary>
            /// follow up later
            /// </summary>
            FollowUpLater = 0,

            /// <summary>
            /// do not contact
            /// </summary>
            DoNotContact = 1,

            /// <summary>
            /// completed
            /// </summary>
            Completed = 2
        }

        #endregion
    }
}