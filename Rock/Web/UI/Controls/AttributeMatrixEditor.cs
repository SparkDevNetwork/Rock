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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AttributeMatrixEditor : CompositeControl, INamingContainer, IHasValidationGroup
    {
        #region Controls

        private HiddenField _hfAttributeMatrixGuid;
        private Grid _gMatrixItems;

        private Panel _pnlEditMatrixItem;
        private HiddenField _hfMatrixItemId;
        private NotificationBox _nbWarning;
        private DynamicPlaceholder _phMatrixItemAttributes;
        private Panel _pnlActions;
        private LinkButton _btnSaveMatrixItem;
        private LinkButton _btnCancelMatrixItem;

        #endregion Controls

        #region Properties

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                return ViewState["ValidationGroup"] as string;
            }

            set
            {
                ViewState["ValidationGroup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute matrix template identifier.
        /// </summary>
        /// <value>
        /// The attribute matrix template identifier.
        /// </value>
        public int? AttributeMatrixTemplateId
        {
            get
            {
                return ViewState["AttributeMatrixTemplateId"] as int?;
            }

            set
            {
                ViewState["AttributeMatrixTemplateId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute matrix unique identifier.
        /// </summary>
        /// <value>
        /// The attribute matrix unique identifier.
        /// </value>
        public Guid? AttributeMatrixGuid
        {
            get
            {
                EnsureChildControls();
                return _hfAttributeMatrixGuid.Value.AsGuidOrNull();
            }

            set
            {
                EnsureChildControls();
                _hfAttributeMatrixGuid.Value = value.ToString();
                BindGrid( value );
            }
        }

        #endregion Properties

        #region Overridden Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            EnsureChildControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( this.Page.IsPostBack )
            {
                EnsureChildControls();
                Guid? attributeMatrixGuid = this.AttributeMatrixGuid;
                if ( !attributeMatrixGuid.HasValue )
                {
                    // if the AttributeMatrixGuid isn't known yet, try to scrape it from the PostBack Params
                    attributeMatrixGuid = this.Page.Request.Params[_hfAttributeMatrixGuid.UniqueID].AsGuidOrNull();
                }

                BindGrid( attributeMatrixGuid );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // HiddenField to store which AttributeMatrix record we are editing
            _hfAttributeMatrixGuid = new HiddenField { ID = "_hfAttributeMatrixGuid" };
            this.Controls.Add( _hfAttributeMatrixGuid );

            _nbWarning = new NotificationBox { ID = "_nbWarning", NotificationBoxType = NotificationBoxType.Warning, Dismissable = true };
            this.Controls.Add( _nbWarning );

            // Grid with view of MatrixItems
            _gMatrixItems = new Grid { ID = "_gMatrixItems", DisplayType = GridDisplayType.Light };
            this.Controls.Add( _gMatrixItems );
            _gMatrixItems.DataKeyNames = new string[] { "Id" };
            _gMatrixItems.Actions.AddClick += gMatrixItems_AddClick;
            _gMatrixItems.Actions.ShowAdd = true;
            _gMatrixItems.IsDeleteEnabled = true;
            _gMatrixItems.GridReorder += gMatrixItems_GridReorder;
            _gMatrixItems.GridRebind += _gMatrixItems_GridRebind;

            _gMatrixItems.Columns.Add( new ReorderField() );

            AttributeMatrixItem tempAttributeMatrixItem = null;

            if ( this.AttributeMatrixTemplateId.HasValue )
            {
                tempAttributeMatrixItem = new AttributeMatrixItem();
                tempAttributeMatrixItem.AttributeMatrix = new AttributeMatrix { AttributeMatrixTemplateId = this.AttributeMatrixTemplateId.Value };
                tempAttributeMatrixItem.LoadAttributes();

                foreach ( var attribute in tempAttributeMatrixItem.Attributes.Select( a => a.Value ) )
                {
                    _gMatrixItems.Columns.Add( new AttributeField { DataField = attribute.Key, HeaderText = attribute.Name } );
                }
            }

            DeleteField deleteField = new DeleteField();
            deleteField.Click += gMatrixItems_DeleteClick;
            _gMatrixItems.Columns.Add( deleteField );

            _gMatrixItems.RowSelected += gMatrixItems_RowSelected;

            // Edit Item
            _pnlEditMatrixItem = new Panel { ID = "_pnlEditMatrixItem", Visible = false, CssClass = "well js-validation-group" };
            _hfMatrixItemId = new HiddenField { ID = "_hfMatrixItemId" };
            _pnlEditMatrixItem.Controls.Add( _hfMatrixItemId );

            _phMatrixItemAttributes = new DynamicPlaceholder { ID = "_phMatrixItemAttributes" };
            _pnlEditMatrixItem.Controls.Add( _phMatrixItemAttributes );

            string validationGroup = GetValidationGroupForAttributeControls();

            if ( tempAttributeMatrixItem != null )
            {
                Rock.Attribute.Helper.AddEditControls( tempAttributeMatrixItem, _phMatrixItemAttributes, false, validationGroup );
            }

            _pnlActions = new Panel { ID = "_pnlActions", CssClass = "actions" };
            _pnlEditMatrixItem.Controls.Add( _pnlActions );

            _btnSaveMatrixItem = new LinkButton { ID = "_btnSaveMatrixItem", CssClass = "btn btn-primary btn-sm", Text = "Save", ValidationGroup = validationGroup, CausesValidation = true };
            _btnSaveMatrixItem.Click += btnSaveMatrixItem_Click;
            _pnlActions.Controls.Add( _btnSaveMatrixItem );

            _btnCancelMatrixItem = new LinkButton { ID = "_btnCancelMatrixItem", CssClass = "btn btn-link", Text = "Cancel", CausesValidation = false };
            _btnCancelMatrixItem.Click += btnCancelMatrixItem_Click;
            _pnlActions.Controls.Add( _btnCancelMatrixItem );

            this.Controls.Add( _pnlEditMatrixItem );
        }

        /// <summary>
        /// Handles the GridRebind event of the _gMatrixItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void _gMatrixItems_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid( this.AttributeMatrixGuid );
        }

        /// <summary>
        /// Gets the validation group for attribute controls.
        /// </summary>
        /// <returns></returns>
        private string GetValidationGroupForAttributeControls()
        {
            return $"vgAttributeMatrixEditor_{this.ID}";
        }

        #endregion Overridden Control Methods

        #region EditMatrixItem

        /// <summary>
        /// Handles the AddClick event of the gMatrixItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gMatrixItems_AddClick( object sender, EventArgs e )
        {
            EditMatrixItem( 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gMatrixItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void gMatrixItems_RowSelected( object sender, RowEventArgs e )
        {
            int matrixItemId = e.RowKeyId;
            EditMatrixItem( matrixItemId );
        }

        /// <summary>
        /// Edits the matrix item.
        /// </summary>
        /// <param name="matrixItemId">The matrix item identifier.</param>
        private void EditMatrixItem( int matrixItemId )
        {
            _hfMatrixItemId.Value = matrixItemId.ToString();

            // make a temp attributeMatrixItem to see what Attributes they have
            AttributeMatrixItem attributeMatrixItem = null;

            using ( var rockContext = new RockContext() )
            {
                if ( matrixItemId > 0 )
                {
                    attributeMatrixItem = new AttributeMatrixItemService( rockContext ).Get( matrixItemId );
                }

                if ( attributeMatrixItem == null )
                {
                    attributeMatrixItem = new AttributeMatrixItem();
                    attributeMatrixItem.AttributeMatrix = new AttributeMatrixService( rockContext ).Get( this.AttributeMatrixGuid.Value );
                }

                if ( this.AttributeMatrixTemplateId.HasValue && attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplateId != this.AttributeMatrixTemplateId )
                {
                    attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplateId = this.AttributeMatrixTemplateId.Value;
                    attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplate = new AttributeMatrixTemplateService( rockContext ).Get( attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplateId );
                }

                attributeMatrixItem.LoadAttributes();
            }

            _phMatrixItemAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( attributeMatrixItem, _phMatrixItemAttributes, true, GetValidationGroupForAttributeControls() );

            _gMatrixItems.Visible = false;
            _pnlEditMatrixItem.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSaveMatrixItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnSaveMatrixItem_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var attributeMatrixItemService = new AttributeMatrixItemService( rockContext );
            AttributeMatrixItem attributeMatrixItem = null;
            int attributeMatrixItemId = _hfMatrixItemId.Value.AsInteger();

            if ( attributeMatrixItemId > 0 )
            {
                attributeMatrixItem = attributeMatrixItemService.Get( attributeMatrixItemId );
            }

            if ( attributeMatrixItem == null )
            {
                attributeMatrixItem = new AttributeMatrixItem();
                attributeMatrixItem.AttributeMatrix = new AttributeMatrixService( rockContext ).Get( this.AttributeMatrixGuid.Value );
                attributeMatrixItemService.Add( attributeMatrixItem );
            }

            attributeMatrixItem.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( _phMatrixItemAttributes, attributeMatrixItem );
            rockContext.SaveChanges();
            attributeMatrixItem.SaveAttributeValues( rockContext );

            _gMatrixItems.Visible = true;
            _pnlEditMatrixItem.Visible = false;

            BindGrid( this.AttributeMatrixGuid );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelMatrixItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnCancelMatrixItem_Click( object sender, EventArgs e )
        {
            _gMatrixItems.Visible = true;
            _pnlEditMatrixItem.Visible = false;
        }

        #endregion EditMatrixItem

        #region MatrixItemsGrid

        /// <summary>
        /// Binds the grid.
        /// </summary>
        /// <param name="attributeMatrixGuid">The attribute matrix unique identifier.</param>
        private void BindGrid( Guid? attributeMatrixGuid )
        {
            if ( attributeMatrixGuid.HasValue )
            {
                var rockContext = new RockContext();
                var attributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid.Value );
                if ( attributeMatrix == null )
                {
                    return;
                }

                var attributeMatrixItemList = attributeMatrix.AttributeMatrixItems.OrderBy(a => a.Order).ThenBy(a => a.Id).ToList();

                foreach ( var attributeMatrixItem in attributeMatrixItemList )
                {
                    attributeMatrixItem.LoadAttributes();
                }

                _gMatrixItems.DataSource = attributeMatrixItemList;
                _gMatrixItems.DataBind();

                _gMatrixItems.Actions.ShowAdd = true;

                if ( attributeMatrix.AttributeMatrixTemplate.MinimumRows.HasValue && attributeMatrixItemList.Count < attributeMatrix.AttributeMatrixTemplate.MinimumRows.Value )
                {
                    string itemPhrase = attributeMatrix.AttributeMatrixTemplate.MinimumRows.Value > 1 ? "items are" : "item is";
                    _nbWarning.Text = $"At least {attributeMatrix.AttributeMatrixTemplate.MinimumRows.Value} {itemPhrase} required.";
                    _nbWarning.Visible = true;
                }
                else if ( attributeMatrix.AttributeMatrixTemplate.MaximumRows.HasValue && attributeMatrixItemList.Count >= attributeMatrix.AttributeMatrixTemplate.MaximumRows.Value )
                {
                    string itemPhrase = attributeMatrix.AttributeMatrixTemplate.MaximumRows.Value > 1 ? "items are" : "item is";
                    _nbWarning.Text = $"No more than {attributeMatrix.AttributeMatrixTemplate.MaximumRows.Value} {itemPhrase} allowed.";

                    // only show the warning if they are actually over the limit
                    _nbWarning.Visible = ( attributeMatrix.AttributeMatrixTemplate.MaximumRows.HasValue && attributeMatrixItemList.Count > attributeMatrix.AttributeMatrixTemplate.MaximumRows.Value );

                    // if they are at or over the limit, don't show the Add button
                    _gMatrixItems.Actions.ShowAdd = true;
                }
                else
                {
                    _nbWarning.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the deleteField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void gMatrixItems_DeleteClick( object sender, RowEventArgs e )
        {
            int attributeMatrixItemId = e.RowKeyId;
            var rockContext = new RockContext();
            AttributeMatrixItemService attributeMatrixItemService = new AttributeMatrixItemService( rockContext );
            AttributeMatrixItem attributeMatrixItem = attributeMatrixItemService.Get( attributeMatrixItemId );
            if ( attributeMatrixItem != null )
            {
                var attributeMatrix = attributeMatrixItem.AttributeMatrix;
                attributeMatrixItemService.Delete( attributeMatrixItem );
                rockContext.SaveChanges();

                BindGrid( this.AttributeMatrixGuid );
            }
        }

        /// <summary>
        /// Handles the GridReorder event of the gMatrixItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gMatrixItems_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var attributeMatrix = new AttributeMatrixService( rockContext ).Get( this.AttributeMatrixGuid.Value );
            var service = new AttributeMatrixItemService( rockContext );
            var items = service.Queryable().Where( a => a.AttributeMatrixId == attributeMatrix.Id ).OrderBy( i => i.Order ).ToList();
            service.Reorder( items, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid( this.AttributeMatrixGuid );
        }

        #endregion MatrixItemsGrid
    }
}
