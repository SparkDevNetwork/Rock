using System;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class AttributeMatrixEditor : CompositeControl, IHasValidationGroup
    {
        #region Controls

        private HiddenField _hfAttributeMatrixGuid;
        private Grid _gMatrixItems;

        private Panel _pnlEditMatrixItem;
        private HiddenField _hfMatrixItemId;
        private DynamicPlaceholder _phMatrixItemAttributes;
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
            }
        }

        #endregion Properties

        #region Overridden Control Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // HiddenField to store which AttributeMatrix record we are editing
            _hfAttributeMatrixGuid = new HiddenField { ID = "_hfAttributeMatrixGuid" };
            this.Controls.Add( _hfAttributeMatrixGuid );

            // Grid with view of MatrixItems
            _gMatrixItems = new Grid { ID = "_gMatrixItems", DisplayType = GridDisplayType.Light };
            this.Controls.Add( _gMatrixItems );
            _gMatrixItems.DataKeyNames = new string[] { "Id" };
            _gMatrixItems.Actions.AddClick += gMatrixItems_AddClick;
            _gMatrixItems.Actions.ShowAdd = true;
            _gMatrixItems.IsDeleteEnabled = true;
            _gMatrixItems.GridReorder += gMatrixItems_GridReorder;

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
            _pnlEditMatrixItem = new Panel { ID = "_pnlEditMatrixItem", Visible = false, CssClass = "well" };
            _hfMatrixItemId = new HiddenField { ID = "_hfMatrixItemId" };
            _pnlEditMatrixItem.Controls.Add( _hfMatrixItemId );

            _phMatrixItemAttributes = new DynamicPlaceholder { ID = "_phMatrixItemAttributes" };
            _pnlEditMatrixItem.Controls.Add( _phMatrixItemAttributes );

            if ( tempAttributeMatrixItem != null )
            {
                Rock.Attribute.Helper.AddEditControls( tempAttributeMatrixItem, _phMatrixItemAttributes, false );
            }

            _btnSaveMatrixItem = new LinkButton { ID = "_btnSaveMatrixItem", CssClass = "btn btn-primary", Text = "Save Matrix Item" };
            _btnSaveMatrixItem.Click += btnSaveMatrixItem_Click;
            _pnlEditMatrixItem.Controls.Add( _btnSaveMatrixItem );

            _btnCancelMatrixItem = new LinkButton { ID = "_btnCancelMatrixItem", CssClass = "btn btn-link", Text = "Cancel" };
            _btnCancelMatrixItem.Click += btnCancelMatrixItem_Click;
            _pnlEditMatrixItem.Controls.Add( _btnCancelMatrixItem );

            this.Controls.Add( _pnlEditMatrixItem );
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
                    attributeMatrixItem.AttributeMatrix = new AttributeMatrixService( rockContext ).Get( _hfAttributeMatrixGuid.Value.AsGuid() );
                }

                if ( this.AttributeMatrixTemplateId.HasValue && attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplateId != this.AttributeMatrixTemplateId )
                {
                    attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplateId = this.AttributeMatrixTemplateId.Value;
                    attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplate = new AttributeMatrixTemplateService( rockContext ).Get( attributeMatrixItem.AttributeMatrix.AttributeMatrixTemplateId );
                }

                attributeMatrixItem.LoadAttributes();
            }

            _phMatrixItemAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( attributeMatrixItem, _phMatrixItemAttributes, true );

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
            Guid attributeMatrixGuid = _hfAttributeMatrixGuid.Value.AsGuid();

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
                attributeMatrixItem.AttributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid );
                attributeMatrixItemService.Add( attributeMatrixItem );
            }

            attributeMatrixItem.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( _phMatrixItemAttributes, attributeMatrixItem );
            rockContext.SaveChanges();
            attributeMatrixItem.SaveAttributeValues( rockContext );

            _gMatrixItems.Visible = true;
            _pnlEditMatrixItem.Visible = false;

            BindMatrixItemsGrid( attributeMatrixItem.AttributeMatrix );
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
        /// Binds the matrix items grid.
        /// </summary>
        /// <param name="attributeMatrix">The attribute matrix.</param>
        public void BindMatrixItemsGrid( AttributeMatrix attributeMatrix )
        {
            foreach ( var attributeMatrixItem in attributeMatrix.AttributeMatrixItems )
            {
                attributeMatrixItem.LoadAttributes();
            }

            _gMatrixItems.DataSource = attributeMatrix.AttributeMatrixItems.ToList();
            _gMatrixItems.DataBind();
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

                BindMatrixItemsGrid( attributeMatrix );
            }
        }

        /// <summary>
        /// Handles the GridReorder event of the gMatrixItems control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gMatrixItems_GridReorder( object sender, GridReorderEventArgs e )
        {
            Guid attributeMatrixGuid = _hfAttributeMatrixGuid.Value.AsGuid();

            var rockContext = new RockContext();
            var attributeMatrix = new AttributeMatrixService( rockContext ).Get( attributeMatrixGuid );
            var service = new AttributeMatrixItemService( rockContext );
            var items = service.Queryable().Where( a => a.AttributeMatrixId == attributeMatrix.Id ).OrderBy( i => i.Order ).ToList();
            service.Reorder( items, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindMatrixItemsGrid( attributeMatrix );
        }

        #endregion MatrixItemsGrid
    }
}
