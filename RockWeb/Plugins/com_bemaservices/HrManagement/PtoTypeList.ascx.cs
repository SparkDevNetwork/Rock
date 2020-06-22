// <copyright>
// Copyright by BEMA Information Services
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
using System.ComponentModel;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

using com.bemaservices.HrManagement.Model;

namespace RockWeb.Plugins.com_bemaservices.HrManagement
{
    [DisplayName( "PTO Type List" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Block for viewing PTO Types." )]
    public partial class PtoTypeList : RockBlock, ICustomGridColumns
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
            this.AddConfigurationUpdateTrigger( upnlSettings );

            gPtoTypes.DataKeyNames = new string[] { "Id" };
            gPtoTypes.Actions.ShowAdd = true;
            gPtoTypes.Actions.AddClick += gPtoTypes_Add;
            gPtoTypes.GridRebind += gPtoTypes_GridRebind;

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gPtoTypes.Actions.ShowAdd = canAddEditDelete;
            gPtoTypes.IsDeleteEnabled = canAddEditDelete;

            var deleteField = new DeleteField();
            gPtoTypes.Columns.Add( deleteField );
            deleteField.Click += gPtoTypes_Delete;

            modalPtoType.SaveClick += btnSaveValue_Click;
            modalPtoType.OnCancelScript = string.Format( "$('#{0}').val('');", hfPtoTypeId.ClientID );
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
                ShowDetail();
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
            ShowDetail();
        }

        /// <summary>
        /// Handles the Add event of the gPtoTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPtoTypes_Add( object sender, EventArgs e )
        {
            gPtoTypes_ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gPtoTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPtoTypes_Edit( object sender, RowEventArgs e )
        {
            gPtoTypes_ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gPtoTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gPtoTypes_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var ptoTypeService = new PtoTypeService( rockContext );

            PtoType ptoType = ptoTypeService.Get( e.RowKeyId );

            if ( ptoType != null )
            {
                string errorMessage;
                if ( !ptoTypeService.CanDelete( ptoType, out errorMessage ) )
                {
                    mdGridWarningPtoTypes.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                ptoTypeService.Delete( ptoType );
                rockContext.SaveChanges();
            }

            BindPtoTypesGrid();
        }

        /// <summary>
        /// Handles the Click event of the btnSavePtoType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveValue_Click( object sender, EventArgs e )
        {
            PtoType ptoType;
            var rockContext = new RockContext();
            PtoTypeService ptoTypeService = new PtoTypeService( rockContext );

            int ptoTypeId = hfPtoTypeId.ValueAsInt();

            if ( ptoTypeId.Equals( 0 ) )
            {
                ptoType = new PtoType { Id = 0 };
            }
            else
            {
                ptoType = ptoTypeService.Get( ptoTypeId );
            }

            ptoType.Name = rtbName.Text;
            ptoType.Description = rtbDescription.Text;
            ptoType.IsNegativeTimeBalanceAllowed = rcbIsNegativeTimeBalanceAllowed.Checked;
            ptoType.Color = cpColor.Value;
            ptoType.WorkflowTypeId = wtpWorkflowType.SelectedValueAsInt();
            ptoType.IsActive = cbIsActive.Checked;

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !ptoType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                if ( ptoType.Id.Equals( 0 ) )
                {
                    ptoTypeService.Add( ptoType );
                }

                rockContext.SaveChanges();

            } );

            BindPtoTypesGrid();

            hfPtoTypeId.Value = string.Empty;
            modalPtoType.Hide();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        public void ShowDetail()
        {
            pnlList.Visible = true;
            BindPtoTypesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPtoTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPtoTypes_GridRebind( object sender, EventArgs e )
        {
            BindPtoTypesGrid();
        }

        /// <summary>
        /// Binds the defined ptoTypes grid.
        /// </summary>
        protected void BindPtoTypesGrid()
        {
            var queryable = new PtoTypeService( new RockContext() ).Queryable().OrderBy( a => a.Name );
            var result = queryable.ToList();

            gPtoTypes.DataSource = result;
            gPtoTypes.DataBind();

        }

        /// <summary>
        /// Shows the edit ptoType.
        /// </summary>
        /// <param name="ptoTypeId">The ptoType id.</param>
        protected void gPtoTypes_ShowEdit( int ptoTypeId )
        {
            ShowPtoTypeEdit( ptoTypeId );
        }

        private void ShowPtoTypeEdit( int ptoTypeId )
        {
            PtoType ptoType;

            modalPtoType.SubTitle = String.Format( "Id: {0}", ptoTypeId );

            if ( !ptoTypeId.Equals( 0 ) )
            {
                ptoType = new PtoTypeService( new RockContext() ).Get( ptoTypeId );
            }
            else
            {
                ptoType = new PtoType { Id = 0 };
                ptoType.IsActive = true;
            }

            hfPtoTypeId.SetValue( ptoType.Id );
            rtbName.Text = ptoType.Name;
            rtbDescription.Text = ptoType.Description;
            cbIsActive.Checked = ptoType.IsActive;
            rcbIsNegativeTimeBalanceAllowed.Checked = ptoType.IsNegativeTimeBalanceAllowed;
            cpColor.Value = ptoType.Color;
            wtpWorkflowType.SetValue( ptoType.WorkflowType );

            modalPtoType.Show();
        }

        #endregion
    }
}