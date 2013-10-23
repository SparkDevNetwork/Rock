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
using Rock.Web;
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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string groupTypeId = PageParameter( "groupTypeId" );
            string roleId = PageParameter( "roleId" );
            if ( !string.IsNullOrWhiteSpace( roleId ) )
            {
                if (string.IsNullOrWhiteSpace(groupTypeId))
                {
                    ShowDetail( "roleId", int.Parse( roleId ) );
                }
                else
                {
                    ShowDetail( "roleId", int.Parse( roleId ), int.Parse( groupTypeId ) );
                }
            }

            else
            {
                upDetail.Visible = false;
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? roleId = PageParameter( pageReference, "roleId" ).AsInteger();
            if ( roleId != null )
            {
                GroupTypeRole role = new GroupTypeRoleService().Get( roleId.Value );
                if ( role != null )
                {
                    breadCrumbs.Add( new BreadCrumb( role.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Role", pageReference ) );
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
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                GroupTypeRoleService groupRoleService = new GroupTypeRoleService();
                GroupTypeRole role;

                int groupRoleId = int.Parse( hfRoleId.Value );

                if ( groupRoleId == 0 )
                {
                    int groupTypeId =hfGroupTypeId.ValueAsInt();
                    role = new GroupTypeRole();
                    role.IsSystem = false;
                    role.GroupTypeId = groupTypeId;

                    var orders = groupRoleService.Queryable()
                        .Where( d => d.GroupTypeId == groupTypeId )
                        .Select( d => d.Order )
                        .ToList();

                    role.Order = orders.Any() ? orders.Max() + 1 : 0;
                }
                else
                {
                    role = groupRoleService.Get( groupRoleId );
                }

                role.Name = tbName.Text;
                role.Description = tbDescription.Text;

                role.MinCount = tbMinCount.Text.AsInteger();
                role.MaxCount = tbMaxCount.Text.AsInteger();

                // validate Min/Max count comparison
                if ( role.MinCount != null && role.MaxCount != null )
                {
                    if ( role.MinCount > role.MaxCount )
                    {
                        tbMinCount.ShowErrorMessage( "Min Count cannot be larger than Max Count" );
                        return;
                    }
                }

                if ( !role.IsValid )
                {
                    return;
                }

                role.LoadAttributes();

                RockTransactionScope.WrapTransaction( () =>
                {
                    if ( role.Id.Equals( 0 ) )
                    {
                        groupRoleService.Add( role, CurrentPersonId );
                    }

                    groupRoleService.Save( role, CurrentPersonId );

                    Rock.Attribute.Helper.GetEditValues( phAttributes, role );
                    Rock.Attribute.Helper.SaveAttributeValues( role, CurrentPersonId );
                } );

                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfRoleId.Value.Equals( "0" ) )
            {
                // Cancelling on Add
                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["groupTypeId"] = hfGroupTypeId.Value;
                NavigateToParentPage( qryString );
            }
            else
            {
                // Cancelling on Edit
                GroupTypeRoleService roleService = new GroupTypeRoleService();
                GroupTypeRole role = roleService.Get( int.Parse( hfRoleId.Value ) );

                Dictionary<string, string> qryString = new Dictionary<string, string>();
                qryString["groupTypeId"] = role.GroupTypeId.ToString();
                NavigateToParentPage( qryString );
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            ShowDetail( itemKey, itemKeyValue, null );
        }
        
        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue, int? groupTypeId )
        {
            if ( !itemKey.Equals( "roleId" ) )
            {
                return;
            }

            GroupTypeRole groupRole = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                groupRole = new GroupTypeRoleService().Get( itemKeyValue );
            }
            else
            {
                if ( groupTypeId.HasValue )
                {
                    groupRole = new GroupTypeRole { Id = 0 };
                    groupRole.GroupTypeId = groupTypeId.Value;
                }
            }

            if ( groupRole == null )
            {
                return;
            }

            hfGroupTypeId.Value = groupRole.GroupTypeId.ToString();
            hfRoleId.Value = groupRole.Id.ToString();

            if ( groupRole.Id.Equals( 0 ) )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( GroupTypeRole.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = groupRole.Name.FormatAsHtmlTitle();
            }

            tbName.Text = groupRole.Name;
            tbDescription.Text = groupRole.Description;
            tbSortOrder.Text = groupRole.Order != null ? groupRole.Order.ToString() : string.Empty;
            tbMaxCount.Text = groupRole.MaxCount != null ? groupRole.MaxCount.ToString() : string.Empty;
            tbMinCount.Text = groupRole.MinCount != null ? groupRole.MinCount.ToString() : string.Empty;

            // render UI based on Authorized and IsSystem

            nbEditModeMessage.Text = string.Empty;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( GroupTypeRole.FriendlyTypeName );
                btnSave.Visible = false;
                btnCancel.Text = "Close";
            }
            else
            {
                btnSave.Visible = true;
                btnCancel.Text = "Cancel";
            }

            bool readOnly = false;
            if ( groupRole.IsSystem )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.System( GroupTypeRole.FriendlyTypeName );
            }

            tbName.ReadOnly = readOnly;
        }

        #endregion

    }
}
