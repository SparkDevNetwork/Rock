using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;

namespace RockWeb.Plugins.com_centralaz.Accountability
{
    /// <summary>
    /// Displays the details of an Accountability Group Type.
    /// </summary>
    [DisplayName( "Accountability Group Type Detail" )]
    [Category( "com_centralaz > Accountability" )]
    [Description( "Displays the details of an Accountability Group Type." )]

    public partial class AccountabilityGroupTypeDetail : Rock.Web.UI.RockBlock
    {

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            lbDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", Group.FriendlyTypeName );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlGroupList );
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
                string groupTypeId = PageParameter( "GroupTypeId" );
                if ( !string.IsNullOrWhiteSpace( groupTypeId ) )
                {
                    ShowDetail( groupTypeId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? groupTypeId = PageParameter( pageReference, "groupTypeId" ).AsIntegerOrNull();
            if ( groupTypeId != null )
            {
                GroupType groupType = new GroupTypeService( new RockContext() ).Get( groupTypeId.Value );
                if ( groupType != null )
                {
                    breadCrumbs.Add( new BreadCrumb( groupType.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Accountability Group Type", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            ShowEditDetails( GetGroupType( hfGroupTypeId.Value.AsInteger() ) );
        }

        /// <summary>
        /// Handles the Click event of the lbDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();

            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            AuthService authService = new AuthService( rockContext );
            GroupType groupType = groupTypeService.Get( int.Parse( hfGroupTypeId.Value ) );

            if ( groupType != null )
            {
                if ( !groupType.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    maDeleteWarning.Show( "You are not authorized to delete this group type.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !groupTypeService.CanDelete( groupType, out errorMessage ) )
                {
                    maDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                groupTypeService.Delete( groupType );

                rockContext.SaveChanges();

            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            GroupType groupType;
            RockContext rockContext = new RockContext();
            GroupTypeService groupTypeService = new GroupTypeService( rockContext );
            AttributeService attributeService = new AttributeService( rockContext );
            AttributeQualifierService attributeQualifierService = new AttributeQualifierService( rockContext );

            int groupTypeId = int.Parse( hfGroupTypeId.Value );

            if ( groupTypeId == 0 )
            {
                groupType = new GroupType();
                groupType.IsSystem = false;
                groupType.Name = string.Empty;
            }
            else
            {
                groupType = groupTypeService.Get( groupTypeId );
            }

            //Add roles
            GroupTypeRole role = new GroupTypeRole();
            role.IsSystem = true;
            role.Name = "Member";
            role.Description = "Member of an accountability group";
            role.Order = 0;
            role.IsLeader = false;
            groupType.Roles.Add( role );

            role = new GroupTypeRole();
            role.IsSystem = true;
            role.Name = "Leader";
            role.Description = "The Accountability Group Leader";
            role.Order = 1;
            role.IsLeader = true;
            groupType.Roles.Add( role );

            groupType.Name = dtbName.Text;
            groupType.Description = dtbDescription.Text;
            groupType.GroupTypePurposeValue = new DefinedValueService( rockContext ).GetByDefinedTypeGuid( Rock.SystemGuid.DefinedType.GROUPTYPE_PURPOSE.AsGuid() ).Where( a => a.Value == "Accountability Group" ).FirstOrDefault();
            groupType.InheritedGroupTypeId = GroupTypeCache.Read( com.centralaz.Accountability.SystemGuid.GroupType.ACCOUNTABILITY_GROUP_TYPE.AsGuid() ).Id;
            if ( !Page.IsValid )
            {
                return;
            }

            if ( !groupType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            if ( groupType.Id.Equals( 0 ) )
            {
                groupTypeService.Add( groupType );
            }
            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["GroupTypeId"] = groupType.Id.ToString();

            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            if ( hfGroupTypeId.Value.Equals( "0" ) )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();

            }
            else
            {
                // Cancelling on Edit.  Return to Details
                ShowReadonlyDetails( GetGroupType( hfGroupTypeId.Value.AsInteger() ) );
            }
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowReadonlyDetails( GetGroupType( hfGroupTypeId.Value.AsInteger() ) );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="parentGroupId">The parent group identifier.</param>
        public void ShowDetail( int groupTypeId )
        {
            GroupType groupType = null;

            bool editAllowed = true;

            if ( !groupTypeId.Equals( 0 ) )
            {
                groupType = GetGroupType( groupTypeId );
                if ( groupType != null )
                {
                    editAllowed = groupType.IsAuthorized( Authorization.EDIT, CurrentPerson );
                }
            }

            if ( groupType == null )
            {
                groupType = new GroupType { Id = 0, Name = "", Description = "" };
            }

            pnlDetails.Visible = true;

            hfGroupTypeId.Value = groupType.Id.ToString();

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( GroupType.FriendlyTypeName );
            }

            if ( groupType.IsSystem )
            {
                nbEditModeMessage.Text = EditModeMessage.System( GroupType.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lbEdit.Visible = false;
                lbDelete.Visible = false;
                ShowReadonlyDetails( groupType );
            }
            else
            {
                lbEdit.Visible = true;
                lbDelete.Visible = !groupType.IsSystem;
                if ( groupType.Id > 0 )
                {
                    ShowReadonlyDetails( groupType );
                }
                else
                {
                    ShowEditDetails( groupType );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowEditDetails( GroupType groupType )
        {
            if ( groupType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( GroupType.FriendlyTypeName ).FormatAsHtmlTitle();

            }
            else
            {
                lReadOnlyTitle.Text = groupType.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            dtbName.Text = groupType.Name;
            dtbDescription.Text = groupType.Description;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="group">The group.</param>
        private void ShowReadonlyDetails( GroupType groupType )
        {
            SetEditMode( false );

            hfGroupTypeId.SetValue( groupType.Id );
            lReadOnlyTitle.Text = groupType.Name.FormatAsHtmlTitle();

            lGroupDescription.Text = groupType.Description;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Gets the group.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns></returns>
        private GroupType GetGroupType( int groupTypeId )
        {
            string key = string.Format( "GroupType:{0}", groupTypeId );
            GroupType groupType = RockPage.GetSharedItem( key ) as GroupType;
            if ( groupType == null )
            {
                groupType = new GroupTypeService( new RockContext() ).Queryable()
                    .Where( g => g.Id == groupTypeId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, groupType );
            }

            return groupType;
        }


        #endregion
    }
}