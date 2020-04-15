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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// This class creates a control to add a defined value to a defined type.
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    public class DefinedValueEditor : CompositeControl, IHasValidationGroup
    {
        private HiddenField _hfDefinedValueId;
        private ValidationSummary _valSummaryValue;
        private Literal _lActionTitleDefinedValue;
        private DataTextBox _tbValueName;
        private DataTextBox _tbValueDescription;
        private AttributeValuesContainer _avcDefinedValueAttributes;
        private LinkButton _btnSave;
        private LinkButton _btnCancel;

        /// <summary>
        /// Gets or sets the defined type identifier.
        /// </summary>
        /// <value>
        /// The defined type identifier.
        /// </value>
        public int DefinedTypeId
        {
            get
            {
                int parsedInt = 0;
                int.TryParse( ViewState["DefinedTypeId"].ToStringSafe(), out parsedInt );
                return parsedInt;
            }

            set
            {
                ViewState["DefinedTypeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the defined type unique identifier.
        /// </summary>
        /// <value>
        /// The defined type unique identifier.
        /// </value>
        public Guid DefinedTypeGuid
        {
            get
            {
                string guid = ViewState["DefinedTypeGuid"] as string;
                if ( guid == null )
                {
                    if ( DefinedTypeId != 0 )
                    {
                        DefinedTypeGuid = DefinedTypeCache.Get( DefinedTypeId ).Guid;
                        return DefinedTypeGuid;
                    }

                    return Guid.Empty;
                }

                return new Guid( guid );
            }

            set
            {
                ViewState["DefinedTypeGuid"] = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the defined value identifier.
        /// </summary>
        /// <value>
        /// The defined value identifier.
        /// </value>
        public int DefinedValueId
        {
            get
            {
                int parsedInt = 0;
                int.TryParse( ViewState["DefinedValueId"].ToStringSafe(), out parsedInt );

                return parsedInt;
            }

            set
            {
                ViewState["DefinedValueId"] = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the defined value unique identifier.
        /// </summary>
        /// <value>
        /// The defined value unique identifier.
        /// </value>
        public Guid DefinedValueGuid
        {
            get
            {
                string guid = ViewState["DefinedValueGuid"] as string;
                if ( guid == null )
                {
                    return Guid.Empty;
                }

                return new Guid( guid );
            }

            set
            {
                ViewState["DefinedValueGuid"] = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get
            {
                return ViewState["Name"].ToString();
            }

            set
            {
                ViewState["Name"] = value;
                _tbValueName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description
        {
            get
            {
                return ViewState["Description"].ToString();
            }

            set
            {
                ViewState["Description"] = value;
                _tbValueDescription.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DefinedValueEditor"/> is hidden.
        /// </summary>
        /// <value>
        ///   <c>true</c> if hidden; otherwise, <c>false</c>.
        /// </value>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is multi selection.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is multi selection; otherwise, <c>false</c>.
        /// </value>
        public bool IsMultiSelection { get; set; }

        /// <summary>
        /// Gets or sets the defined value selector client identifier.
        /// </summary>
        /// <value>
        /// The defined value selector client identifier.
        /// </value>
        public string DefinedValueSelectorClientId { get; set; }

        /// <summary>
        /// Gets or sets the validation group. (Default is RockBlock's BlockValidationGroup)
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get => ViewState["ValidationGroup"] as string ?? "DefinedValueValidationGroup";
            set => ViewState["ValidationGroup"] = value;
        }

        #region Overridden Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            return base.SaveViewState();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            _hfDefinedValueId = new HiddenField();
            _hfDefinedValueId.ID = this.ID + "_hfDefinedValueId";
            Controls.Add( _hfDefinedValueId );

            _valSummaryValue = new ValidationSummary();
            _valSummaryValue.ID = this.ID + "_valSummaryValue ";
            _valSummaryValue.AddCssClass( "alert alert-validation" );
            _valSummaryValue.ValidationGroup = ValidationGroup;
            _valSummaryValue.HeaderText = "Please correct the following:";
            Controls.Add( _valSummaryValue );

            _lActionTitleDefinedValue = new Literal();
            _lActionTitleDefinedValue.ID = this.ID + "_lActionTitleDefinedValue";
            Controls.Add( _lActionTitleDefinedValue );

            _tbValueName = new DataTextBox();
            _tbValueName.ID = this.ID + "_tbValueName";
            _tbValueName.SourceTypeName = "Rock.Model.DefinedValue, Rock";
            _tbValueName.PropertyName = "Value";
            _tbValueName.ValidationGroup = ValidationGroup;
            _tbValueName.Placeholder = "Value";
            _tbValueName.Label = "Value";
            _tbValueName.Required = true;
            _tbValueName.CausesValidation = true;
            Controls.Add( _tbValueName );

            _tbValueDescription = new DataTextBox();
            _tbValueDescription.ID = this.ID + "_tbValueDescription";
            _tbValueDescription.SourceTypeName = "Rock.Model.DefinedValue, Rock";
            _tbValueDescription.PropertyName = "Description";
            _tbValueDescription.ValidationGroup = ValidationGroup;
            _tbValueDescription.Placeholder = "Description";
            _tbValueDescription.Label = " ";
            _tbValueDescription.TextMode = TextBoxMode.MultiLine;
            _tbValueDescription.Rows = 3;
            _tbValueDescription.ValidateRequestMode = ValidateRequestMode.Disabled;
            Controls.Add( _tbValueDescription );

            _avcDefinedValueAttributes = new AttributeValuesContainer();
            _avcDefinedValueAttributes.ID = this.ID + "_avcDefinedValueAttributes";
            _avcDefinedValueAttributes.ValidationGroup = ValidationGroup;
            Controls.Add( _avcDefinedValueAttributes );

            _btnSave = new LinkButton();
            _btnSave.ID = this.ID + "_btnSave";
            _btnSave.Text = "Add";
            _btnSave.CssClass = "btn btn-primary btn-xs";
            _btnSave.ValidationGroup = ValidationGroup;
            _btnSave.CausesValidation = true;
            _btnSave.Click += btnSave_Click;
            Controls.Add( _btnSave );

            _btnCancel = new LinkButton();
            _btnCancel.ID = this.ID + "_btnCancel";
            _btnCancel.Text = "Cancel";
            _btnCancel.CssClass = "btn btn-link btn-xs";
            _btnCancel.CausesValidation = false;
            _btnCancel.OnClientClick = $"javascript:$('.{this.ClientID}-js-defined-value-editor').fadeToggle(400, 'swing', function() {{ $('.{DefinedValueSelectorClientId}-js-defined-value-selector').fadeToggle(); }}); return false;";
            Controls.Add( _btnCancel );

            LoadDefinedValueAttributes();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            writer.AddAttribute( "id", this.ClientID.ToString() );
            writer.AddAttribute( "class", $"{this.ClientID}-js-defined-value-editor well" );
            if ( Hidden )
            {
                writer.AddStyleAttribute( "display", "none" );
            }

            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // Validation Row
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _valSummaryValue.RenderControl( writer );
            writer.RenderEndTag();

            // Title Row
            if ( !string.IsNullOrWhiteSpace( _lActionTitleDefinedValue.Text ) )
            {
                writer.RenderBeginTag( HtmlTextWriterTag.Legend );
                _lActionTitleDefinedValue.RenderControl( writer );
                writer.RenderEndTag(); // row 2
            }

            // Start FieldSet
            writer.RenderBeginTag( HtmlTextWriterTag.Fieldset );

            // Name Description Row
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row-fluid" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbValueName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "span12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbValueDescription.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // Attributes
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "attributes" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _avcDefinedValueAttributes.RenderControl( writer );
            writer.RenderEndTag();

            // End of FieldSet
            writer.RenderEndTag();

            // Buttons
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _btnSave.RenderControl( writer );
            _btnCancel.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag();

            writer.RenderEndTag(); // row

            // End control
            writer.RenderEndTag();
        }

        #endregion Overridden Control Methods

        /// <summary>
        /// Loads the defined value attributes.
        /// </summary>
        public void LoadDefinedValueAttributes()
        {
            DefinedValue definedValue;
            var definedType = DefinedTypeCache.Get( DefinedTypeId );

            if ( DefinedValueId == 0 )
            {
                definedValue = new DefinedValue { Id = 0 };
                definedValue.DefinedTypeId = DefinedTypeId;
            }
            else
            {
                definedValue = new DefinedValueService( new RockContext() ).Get( DefinedValueId );
            }

            _hfDefinedValueId.SetValue( definedValue.Id );
            _tbValueName.Text = definedValue.Value;
            _tbValueDescription.Text = definedValue.Description;

            _avcDefinedValueAttributes.ValidationGroup = _valSummaryValue.ValidationGroup;
            _avcDefinedValueAttributes.AddEditControls( definedValue );
        }

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            DefinedValue definedValue;
            var rockContext = new RockContext();
            DefinedValueService definedValueService = new DefinedValueService( rockContext );

            if ( DefinedValueId.Equals( 0 ) )
            {
                definedValue = new DefinedValue { Id = 0 };
                definedValue.DefinedTypeId = DefinedTypeId;
                definedValue.IsSystem = false;

                var orders = definedValueService.Queryable()
                    .Where( d => d.DefinedTypeId == DefinedTypeId )
                    .Select( d => d.Order )
                    .ToList();

                definedValue.Order = orders.Any() ? orders.Max() + 1 : 0;
            }
            else
            {
                definedValue = definedValueService.Get( DefinedValueId );
            }

            definedValue.Value = _tbValueName.Text;
            definedValue.Description = _tbValueDescription.Text;
            _avcDefinedValueAttributes.GetEditValues( definedValue );

            if ( !Page.IsValid )
            {
                return;
            }

            if ( !definedValue.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            rockContext.WrapTransaction( () =>
            {
                if ( definedValue.Id.Equals( 0 ) )
                {
                    definedValueService.Add( definedValue );
                }

                rockContext.SaveChanges();

                definedValue.SaveAttributeValues( rockContext );
            } );

            CreateChildControls();

            if ( this.Parent is DefinedValuePickerWithAdd )
            {
                var picker = this.Parent as DefinedValuePickerWithAdd;

                if ( this.IsMultiSelection )
                {
                    List<int> definedValues = picker.SelectedDefinedValuesId.ToList();
                    definedValues.Add( definedValue.Id );
                    picker.LoadDefinedValues( definedValues.ToArray() );
                }
                else
                {
                    picker.LoadDefinedValues( new int[] { definedValue.Id } );
                }
            }
        }

        #endregion
    }
}
