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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// An editor control for a set of rows in an Attribute Matrix. Each matrix row is comprised of multiple Attributes Values.
    /// </summary>
    public class AttributeMatrixEditor : CompositeControl, INamingContainer, IRockControl
    {
        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the CSS Icon text.
        /// </summary>
        /// <value>
        /// The CSS icon class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string IconCssClass
        {
            get { return ViewState["IconCssClass"] as string ?? string.Empty; }
            set { ViewState["IconCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }
            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }
            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public virtual bool Required
        {
            get { return ViewState["Required"] as bool? ?? false; }
            set { ViewState["Required"] = value; }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }
            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

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
                EnsureChildControls();
                return _requiredRowCountRangeValidator.ValidationGroup;
            }

            set
            {
                EnsureChildControls();
                _requiredRowCountRangeValidator.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return ( !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid )
                    && _requiredRowCountRangeValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

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
        private HiddenFieldWithValidationProperty _hfRowCount;
        private HiddenFieldRangeValidator _requiredRowCountRangeValidator;

        #endregion Controls

        #region Properties

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

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeMatrixEditor"/> class.
        /// </summary>
        public AttributeMatrixEditor()
            : base()
        {
            RockControlHelper.Init( this );
        }

        #endregion

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

                var matrixItemId = _hfMatrixItemId.Value.AsIntegerOrNull();

                // If the matrixItemId has value we are in edit mode, create the edit mode controls so their state can be preserved,
                // this is necessary since they are created dynamically.
                if ( matrixItemId.HasValue )
                {
                    var postBackControlId = this.Page.Request.Params["__EVENTTARGET"];
                    if ( postBackControlId != _btnSaveMatrixItem.UniqueID )
                    {
                        EditMatrixItem( matrixItemId.Value );
                    }
                }
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

            _hfRowCount = new HiddenFieldWithValidationProperty { ID = "_hfRowCount" };
            this.Controls.Add( _hfRowCount );

            _requiredRowCountRangeValidator = new HiddenFieldRangeValidator();
            _requiredRowCountRangeValidator.ID = _hfRowCount.ID + "_rfv";
            _requiredRowCountRangeValidator.ControlToValidate = _hfRowCount.ID;
            _requiredRowCountRangeValidator.Display = ValidatorDisplay.Dynamic;
            _requiredRowCountRangeValidator.Type = ValidationDataType.Integer;
            _requiredRowCountRangeValidator.CssClass = "validation-error help-inline";
            _requiredRowCountRangeValidator.Enabled = false;

            this.Controls.Add( _requiredRowCountRangeValidator );

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

                AttributeMatrixTemplateService attributeMatrixTemplateService = new AttributeMatrixTemplateService( new RockContext() );
                var attributeMatrixTemplateRanges = attributeMatrixTemplateService.GetSelect( this.AttributeMatrixTemplateId.Value, s => new { s.MinimumRows, s.MaximumRows } );

                // If a value is required, make sure we have a minumum row count of at least 1.
                var minRowCount = attributeMatrixTemplateRanges.MinimumRows.GetValueOrDefault( 0 );
                if ( this.Required
                     && minRowCount < 1 )
                {
                    minRowCount = 1;
                }
                _requiredRowCountRangeValidator.MinimumValue = minRowCount.ToString();

                _requiredRowCountRangeValidator.Enabled = minRowCount > 0;
                if ( minRowCount == 1 )
                {
                    _requiredRowCountRangeValidator.ErrorMessage = "At least 1 row is required.";
                }
                else
                {
                    _requiredRowCountRangeValidator.ErrorMessage = $"At least {minRowCount} rows are required";
                }
            }

            DeleteField deleteField = new DeleteField();
            deleteField.Click += gMatrixItems_DeleteClick;
            _gMatrixItems.Columns.Add( deleteField );

            _gMatrixItems.RowSelected += gMatrixItems_RowSelected;

            // Edit Item
            _pnlEditMatrixItem = new Panel { ID = "_pnlEditMatrixItem", Visible = false, CssClass = "well js-validation-group validation-group" };
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
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            // Set the required field message here because the control label may have been modified during initialization.
            var minRowCount = _requiredRowCountRangeValidator.MinimumValue.AsInteger();

            if ( minRowCount > 0 )
            {
                if ( minRowCount == 1 )
                {
                    _requiredRowCountRangeValidator.ErrorMessage = $"{this.Label} must have at least one entry.";
                }
                else
                {
                    _requiredRowCountRangeValidator.ErrorMessage = $"{this.Label} must have at least {minRowCount} entries.";
                }
            }

            base.RenderControl( writer );
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

            // set the validation group on the controls and save button
            string validationGroup = GetValidationGroupForAttributeControls();
            Rock.Attribute.Helper.AddEditControls( attributeMatrixItem, _phMatrixItemAttributes, true, validationGroup );

            // Make sure to set the validategroup on the save button to match, just in case it changed since CreateChildControls
            _btnSaveMatrixItem.ValidationGroup = validationGroup;

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
            _hfMatrixItemId.Value = null;

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
            _hfMatrixItemId.Value = null;
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

                var attributeMatrixItemList = attributeMatrix.AttributeMatrixItems.OrderBy( a => a.Order ).ThenBy( a => a.Id ).ToList();

                foreach ( var attributeMatrixItem in attributeMatrixItemList )
                {
                    attributeMatrixItem.LoadAttributes();
                }

                _gMatrixItems.DataSource = attributeMatrixItemList;
                _gMatrixItems.RowItemText = "Item";
                _gMatrixItems.DataBind();

                _gMatrixItems.Actions.ShowAdd = true;
                _hfRowCount.Value = attributeMatrixItemList.Count.ToString();

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
                    _nbWarning.Visible = attributeMatrix.AttributeMatrixTemplate.MaximumRows.HasValue && ( attributeMatrixItemList.Count > attributeMatrix.AttributeMatrixTemplate.MaximumRows.Value );

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
