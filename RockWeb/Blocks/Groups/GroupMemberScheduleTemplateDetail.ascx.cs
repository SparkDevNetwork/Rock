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
using System.ComponentModel;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;

namespace RockWeb.Blocks.Groups
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Group Member Schedule Template Detail" )]
    [Category( "Groups" )]
    [Description( "Displays the details of a group member schedule template." )]
    public partial class GroupMemberScheduleTemplateDetail : RockBlock, IDetailBlock
    {
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
                ShowDetail( PageParameter( "GroupMemberScheduleTemplateId" ).AsInteger() );
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
            // nothing to do
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            var groupMemberScheduleTemplateService = new GroupMemberScheduleTemplateService( rockContext );

            GroupMemberScheduleTemplate groupMemberScheduleTemplate;

            int groupMemberScheduleTemplateId = hfGroupMemberScheduleTemplateId.Value.AsInteger();

            if ( groupMemberScheduleTemplateId == 0 )
            {
                groupMemberScheduleTemplate = new GroupMemberScheduleTemplate();
                groupMemberScheduleTemplateService.Add( groupMemberScheduleTemplate );
            }
            else
            {
                groupMemberScheduleTemplate = groupMemberScheduleTemplateService.Get( groupMemberScheduleTemplateId );
            }

            groupMemberScheduleTemplate.Name = tbName.Text;

            if ( groupMemberScheduleTemplate.Schedule == null )
            {
                groupMemberScheduleTemplate.Schedule = new Schedule();
            }

            groupMemberScheduleTemplate.Schedule.iCalendarContent = sbSchedule.iCalendarContent;

            rockContext.SaveChanges();

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupMemberScheduleTemplateId">The group member schedule template identifier.</param>
        public void ShowDetail( int groupMemberScheduleTemplateId )
        {
            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            GroupMemberScheduleTemplate groupMemberScheduleTemplate = null;

            if ( !groupMemberScheduleTemplateId.Equals( 0 ) )
            {
                groupMemberScheduleTemplate = new GroupMemberScheduleTemplateService( new RockContext() ).Get( groupMemberScheduleTemplateId );
                lActionTitle.Text = ActionTitle.Edit( GroupMemberScheduleTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
                pdAuditDetails.SetEntity( groupMemberScheduleTemplate, ResolveRockUrl( "~" ) );
            }

            if ( groupMemberScheduleTemplate == null )
            {
                groupMemberScheduleTemplate = new GroupMemberScheduleTemplate { Id = 0 };
                lActionTitle.Text = ActionTitle.Add( GroupMemberScheduleTemplate.FriendlyTypeName ).FormatAsHtmlTitle();

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            hfGroupMemberScheduleTemplateId.Value = groupMemberScheduleTemplate.Id.ToString();
            tbName.Text = groupMemberScheduleTemplate.Name;
            if ( groupMemberScheduleTemplate.Schedule != null )
            {
                sbSchedule.iCalendarContent = groupMemberScheduleTemplate.Schedule.iCalendarContent;
            }
            else
            {
                sbSchedule.iCalendarContent = string.Empty;
            }

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( GroupMemberScheduleTemplate.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( GroupMemberScheduleTemplate.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            btnSave.Visible = !readOnly;
        }

        #endregion
    }
}