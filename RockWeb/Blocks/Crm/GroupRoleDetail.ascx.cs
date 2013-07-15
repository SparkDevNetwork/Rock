//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// 
    /// </summary>
    public partial class GroupRoleDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        private GroupRole _groupRole = null;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string roleIdParam = PageParameter( "groupRoleId" );
            if ( !string.IsNullOrWhiteSpace( roleIdParam ) )
            {
                int roleId = int.MinValue;
                if (int.TryParse(roleIdParam, out roleId) && !roleId.Equals(0))
                {
                    _groupRole = new GroupRoleService().Get( roleId );
                }
                else
                {
                    _groupRole = new GroupRole { Id = 0 };
                }

                _groupRole.LoadAttributes();

                phAttributes.Controls.Clear();
                Rock.Attribute.Helper.AddEditControls( _groupRole, phAttributes, !Page.IsPostBack );
            }

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
                if ( _groupRole != null )
                {
                    pnlDetails.Visible = true;
                    ShowDetail( _groupRole );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            GroupTypeService groupTypeService = new GroupTypeService();
            List<GroupType> groupTypes = groupTypeService.Queryable().OrderBy( a => a.Name ).ToList();
            ddlGroupType.DataSource = groupTypes;
            ddlGroupType.DataBind();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "groupRoleId" ) )
            {
                return;
            }

            pnlDetails.Visible = true;

            GroupRole groupRole = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                groupRole = new GroupRoleService().Get( itemKeyValue );
            }
            else
            {
                groupRole = new GroupRole { Id = 0 };
            }

            ShowDetail( groupRole );

        }

        public void ShowDetail(GroupRole groupRole)
        {
            if ( groupRole.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( GroupRole.FriendlyTypeName );
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( GroupRole.FriendlyTypeName );
            }

            LoadDropDowns();

            hfGroupRoleId.Value = groupRole.Id.ToString();
            tbName.Text = groupRole.Name;
            tbDescription.Text = groupRole.Description;
            ddlGroupType.SelectedValue = groupRole.GroupTypeId.ToString();
            tbSortOrder.Text = groupRole.SortOrder != null ? groupRole.SortOrder.ToString() : string.Empty;
            tbMaxCount.Text = groupRole.MaxCount != null ? groupRole.MaxCount.ToString() : string.Empty;
            tbMinCount.Text = groupRole.MinCount != null ? groupRole.MinCount.ToString() : string.Empty;

            // render UI based on Authorized and IsSystem
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( GroupRole.FriendlyTypeName );
            }

            if ( groupRole.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlySystem( GroupRole.FriendlyTypeName );
            }

            if ( readOnly )
            {
                lActionTitle.Text = ActionTitle.View( GroupRole.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            tbName.ReadOnly = readOnly;
            tbDescription.ReadOnly = readOnly;
            ddlGroupType.Enabled = !readOnly;
            tbSortOrder.ReadOnly = readOnly;
            tbMaxCount.ReadOnly = readOnly;
            tbMinCount.ReadOnly = readOnly;
            btnSave.Visible = !readOnly;
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            GroupRole groupRole;
            GroupRoleService groupRoleService = new GroupRoleService();

            int groupRoleId = int.Parse( hfGroupRoleId.Value );

            if ( groupRoleId == 0 )
            {
                groupRole = new GroupRole();
                groupRole.IsSystem = false;
                groupRoleService.Add( groupRole, CurrentPersonId );
            }
            else
            {
                groupRole = groupRoleService.Get( groupRoleId );
            }

            groupRole.LoadAttributes();

            groupRole.Name = tbName.Text;
            groupRole.Description = tbDescription.Text;
            groupRole.GroupTypeId = int.Parse( ddlGroupType.SelectedValue );
            groupRole.SortOrder = tbSortOrder.Text.AsInteger();
            groupRole.MinCount = tbMinCount.Text.AsInteger();
            groupRole.MaxCount = tbMaxCount.Text.AsInteger();

            // validate Control values
            if ( !tbSortOrder.IsValid )
            {
                return;
            }

            if ( !tbMaxCount.IsValid )
            {
                return;
            }

            if ( !tbMinCount.IsValid )
            {
                return;
            }

            // validate Min/Max count comparison
            if ( groupRole.MinCount != null && groupRole.MaxCount != null )
            {
                if ( groupRole.MinCount > groupRole.MaxCount )
                {
                    tbMinCount.ShowErrorMessage( "Min Count cannot be larger than Max Count" );
                    return;
                }
            }

            if ( !groupRole.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            RockTransactionScope.WrapTransaction( () =>
            {
                groupRoleService.Save( groupRole, CurrentPersonId );
                Rock.Attribute.Helper.GetEditValues( phAttributes, groupRole );
                Rock.Attribute.Helper.SaveAttributeValues( groupRole, CurrentPersonId );
            } );

            NavigateToParentPage();
        }

        #endregion
    }
}
