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
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Mobile.JsonFields;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that allows the user to build a JSON object by specifying
    /// properties and attributes to be included.
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class JsonFieldsBuilder : CompositeControl, IRockControl
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
            get
            {
                return ViewState["Label"] as string ?? string.Empty;
            }

            set
            {
                ViewState["Label"] = value;
            }
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
        public bool Required
        {
            get
            {
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
            }
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
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return true;
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

                return _rblFieldSource.ValidationGroup;
            }
            set
            {
                EnsureChildControls();

                _rblFieldSource.ValidationGroup = value;
                _ddlProperties.ValidationGroup = value;
                _ddlAttributes.ValidationGroup = value;
                _tbKey.ValidationGroup = value;
                _rblAttributeFormatType.ValidationGroup = value;
                _tbKey.ValidationGroup = value;
                _rblFieldFormat.ValidationGroup = value;
                _ceLavaExpression.ValidationGroup = value;
                _lbApply.ValidationGroup = value;
                _lbCancel.ValidationGroup = value;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the field settings.
        /// </summary>
        /// <value>
        /// The field settings.
        /// </value>
        public List<FieldSetting> FieldSettings
        {
            get => _fieldSettings;
            set
            {
                _fieldSettings = value ?? new List<FieldSetting>();

                EnsureChildControls();
                BindFieldGrid();
            }
        }
        private List<FieldSetting> _fieldSettings;

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        /// <value>
        /// The type of the source.
        /// </value>
        public Type SourceType
        {
            get
            {
                var typeName = ( string ) ViewState["SourceType"];

                if ( typeName.IsNotNullOrWhiteSpace() )
                {
                    return Type.GetType( typeName );
                }

                return null;
            }
            set
            {
                ViewState["SourceType"] = value?.AssemblyQualifiedName;
            }
        }

        /// <summary>
        /// The available attributes that can be selected from.
        /// </summary>
        public IEnumerable<AttributeCache> AvailableAttributes
        {
            get
            {
                var availableAttributeIds = ( List<int> ) ViewState[nameof( AvailableAttributes )];
                if ( availableAttributeIds == null )
                {
                    return new List<AttributeCache>();
                }

                return availableAttributeIds.Select( a => AttributeCache.Get( a ) );
            }
            set
            {
                ViewState[nameof( AvailableAttributes )] = value.Select( a => a.Id ).ToList();

                EnsureChildControls();
                BindAttributePicker();
            }
        }

        /// <summary>
        /// The properties that can be selected from.
        /// </summary>
        private List<PropertyInfoSummary> AvailableProperties
        {
            get
            {
                var properties = new List<PropertyInfoSummary>();
                var type = SourceType;

                if ( type != null )
                {
                    var contentChannelProperties = type.GetProperties().OrderBy( p => p.Name );

                    foreach ( var property in contentChannelProperties )
                    {
                        properties.Add( new PropertyInfoSummary
                        {
                            Name = property.Name,
                            Type = property.PropertyType.ToString()
                        } );
                    }
                }

                return properties;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is edit mode.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is edit mode; otherwise, <c>false</c>.
        /// </value>
        private bool IsEditMode
        {
            get => ( bool ) ViewState["IsEditMode"];
            set => ViewState["IsEditMode"] = value;
        }

        /// <summary>
        /// Gets or sets the original edit key.
        /// </summary>
        /// <value>
        /// The original edit key.
        /// </value>
        private string OriginalEditKey
        {
            get => ( string ) ViewState["OriginalEditKey"];
            set => ViewState["OriginalEditKey"] = value;
        }

        #endregion

        #region Controls

        private Grid _gIncludedAttributes;
        private RockRadioButtonList _rblFieldSource;
        private RockDropDownList _ddlProperties;
        private RockDropDownList _ddlAttributes;
        private RockRadioButtonList _rblAttributeFormatType;
        private RockTextBox _tbKey;
        private RockRadioButtonList _rblFieldFormat;
        private CodeEditor _ceLavaExpression;
        private LinkButton _lbApply;
        private LinkButton _lbCancel;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFieldsBuilder"/> class.
        /// </summary>
        public JsonFieldsBuilder()
            : base()
        {
            _fieldSettings = new List<FieldSetting>();
            IsEditMode = false;

            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            EnsureChildControls();

            base.OnInit( e );
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, <see langword="null" />.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["FieldSettings"] = FieldSettings?.ToJson();

            return base.SaveViewState();
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            FieldSettings = ( ( string ) ViewState["FieldSettings"] ).FromJsonOrNull<List<FieldSetting>>();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            //
            // Create the Grid.
            //
            var gEditField = new EditField();
            gEditField.Click += gIncludedAttributesEdit_Click;
            var gDeleteField = new DeleteField();
            gDeleteField.Click += gIncludedAttributesDelete_Click;
            _gIncludedAttributes = new Grid
            {
                ID = "gIncludedAttributes",
                DisplayType = GridDisplayType.Light,
                DataKeyNames = new string[] { "Key" },
                ShowHeaderWhenEmpty = true
            };
            _gIncludedAttributes.Actions.ShowAdd = true;
            _gIncludedAttributes.Columns.Add( new RockBoundField { DataField = "FieldSource", HeaderText = "Source" } );
            _gIncludedAttributes.Columns.Add( new RockBoundField { DataField = "Key", HeaderText = "Key" } );
            _gIncludedAttributes.Columns.Add( new RockBoundField { DataField = "Value", HeaderText = "Expression" } );
            _gIncludedAttributes.Columns.Add( new RockBoundField { DataField = "FieldFormat", HeaderText = "Format" } );
            _gIncludedAttributes.Columns.Add( gEditField );
            _gIncludedAttributes.Columns.Add( gDeleteField );
            _gIncludedAttributes.GridRebind += gIncludedAttributes_GridRebind;
            _gIncludedAttributes.Actions.AddClick += gIncludedAttributes_AddClick;
            Controls.Add( _gIncludedAttributes );

            _rblFieldSource = new RockRadioButtonList
            {
                ID = "rblFieldSource",
                Label = "Field Source",
                AutoPostBack = true,
                CausesValidation = false,
                RepeatDirection = RepeatDirection.Horizontal
            };
            _rblFieldSource.SelectedIndexChanged += rblFieldSource_SelectedIndexChanged;
            _rblFieldSource.BindToEnum<FieldSource>();
            Controls.Add( _rblFieldSource );

            //
            // Build the Properties panel.
            //
            _ddlProperties = new RockDropDownList
            {
                ID = "ddlProperties",
                Label = "Property"
            };
            Controls.Add( _ddlProperties );

            //
            // Build the Attributes panel.
            //
            _ddlAttributes = new RockDropDownList
            {
                ID = "ddlAttributes",
                Label = "Attribute"
            };
            BindAttributePicker();
            Controls.Add( _ddlAttributes );

            _rblAttributeFormatType = new RockRadioButtonList
            {
                ID = "rblAttributeFormatType",
                Label = "Format Type",
                RepeatDirection = RepeatDirection.Horizontal
            };
            _rblAttributeFormatType.BindToEnum<AttributeFormat>();
            Controls.Add( _rblAttributeFormatType );

            //
            // Build the Lava Expression panel.
            //
            _tbKey = new RockTextBox
            {
                ID = "tbKey",
                Label = "Key",
                Help = "This will become the property name in the returned JSON.",
                Required = true
            };
            Controls.Add( _tbKey );

            _rblFieldFormat = new RockRadioButtonList
            {
                ID = "rblFieldFormat",
                Label = "Field Format",
                RepeatDirection = RepeatDirection.Horizontal
            };
            _rblFieldFormat.BindToEnum<FieldFormat>();
            Controls.Add( _rblFieldFormat );

            _ceLavaExpression = new CodeEditor
            {
                ID = "ceLavaExpression",
                Label = "Lava Expression",
                EditorMode = CodeEditorMode.Lava,
                Help = "Lava expression to use to get the value for the field. Note: The use of entity commands, SQL commands, etc are not recommended for performance reasons."
            };
            Controls.Add( _ceLavaExpression );

            _lbApply = new LinkButton
            {
                ID = "lbApply",
                CssClass = "btn btn-primary btn-xs",
                Text = "Apply"
            };
            _lbApply.Click += lbApply_Click;
            Controls.Add( _lbApply );

            _lbCancel = new LinkButton
            {
                ID = "lbCancel",
                CssClass = "btn btn-link btn-xs",
                Text = "Cancel",
                CausesValidation = false
            };
            _lbCancel.Click += lbCancel_Click;
            Controls.Add( _lbCancel );

            //
            // Perform initial data binding.
            //
            BindFieldGrid();
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( Visible )
            {
                if ( !IsEditMode )
                {
                    _gIncludedAttributes.RenderControl( writer );
                }
                else
                {
                    _rblFieldSource.RenderControl( writer );

                    switch ( _rblFieldSource.SelectedIndex )
                    {
                        case 0:
                            _ddlProperties.RenderControl( writer );
                            break;

                        case 1:
                            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                            writer.RenderBeginTag( HtmlTextWriterTag.Div );
                            {
                                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                                {
                                    _ddlAttributes.RenderControl( writer );
                                }
                                writer.RenderEndTag();

                                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                                {
                                    _rblAttributeFormatType.RenderControl( writer );
                                }
                                writer.RenderEndTag();
                            }
                            writer.RenderEndTag();

                            break;

                        case 2:
                            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                            writer.RenderBeginTag( HtmlTextWriterTag.Div );
                            {
                                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                                {
                                    _tbKey.RenderControl( writer );
                                }
                                writer.RenderEndTag();

                                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                                {
                                    _rblFieldFormat.RenderControl( writer );
                                }
                                writer.RenderEndTag();
                            }
                            writer.RenderEndTag();

                            _ceLavaExpression.RenderControl( writer );

                            break;
                    }

                    _lbApply.RenderControl( writer );
                    _lbCancel.RenderControl( writer );
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the field grid.
        /// </summary>
        private void BindFieldGrid()
        {
            _gIncludedAttributes.DataSource = FieldSettings;
            _gIncludedAttributes.DataBind();
        }

        /// <summary>
        /// Binds the attribute picker.
        /// </summary>
        private void BindAttributePicker()
        {
            _ddlAttributes.DataSource = AvailableAttributes.Select( a => new
            {
                a.Key,
                a.Name
            } ).ToList();
            _ddlAttributes.DataValueField = "Key";
            _ddlAttributes.DataTextField = "Name";
            _ddlAttributes.DataBind();
        }

        #endregion

        #region Grid Event Handlers

        /// <summary>
        /// Handles the AddClick event of the gIncludedAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void gIncludedAttributes_AddClick( object sender, EventArgs e )
        {
            IsEditMode = true;

            //
            // Reset fields.
            //
            _rblFieldSource.SetValue( FieldSource.Property.ConvertToInt().ToString() );
            _rblAttributeFormatType.SetValue( AttributeFormat.FriendlyValue.ConvertToInt().ToString() );
            _tbKey.Text = string.Empty;
            OriginalEditKey = "new_key";
            _rblFieldFormat.SetValue( FieldFormat.String.ConvertToInt().ToString() );
            _ceLavaExpression.Text = "{{ item | Attribute:'AttributeKey' }}";

            _ddlProperties.DataSource = AvailableProperties;
            _ddlProperties.DataValueField = "Name";
            _ddlProperties.DataTextField = "Name";
            _ddlProperties.DataBind();
        }

        /// <summary>
        /// Handles the GridRebind event of the gIncludedAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gIncludedAttributes_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindFieldGrid();
        }

        /// <summary>
        /// Handles the Click event of the gIncludedAttributesDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void gIncludedAttributesDelete_Click( object sender, RowEventArgs e )
        {
            var settingKey = e.RowKeyValue.ToString();

            var setting = FieldSettings.Where( s => s.Key == settingKey ).FirstOrDefault();

            _fieldSettings.Remove( setting );

            BindFieldGrid();
        }

        /// <summary>
        /// Handles the Click event of the gIncludedAttributesEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        private void gIncludedAttributesEdit_Click( object sender, RowEventArgs e )
        {
            var settingKey = e.RowKeyValue.ToString();

            var setting = FieldSettings.Where( s => s.Key == settingKey ).FirstOrDefault();

            if ( setting == null )
            {
                return;
            }

            IsEditMode = true;

            //
            // Set edit values.
            //
            _rblFieldSource.SetValue( setting.FieldSource.ConvertToInt().ToString() );
            _tbKey.Text = setting.Key;
            OriginalEditKey = setting.Key;
            _ceLavaExpression.Text = setting.Value;

            _ddlProperties.DataSource = AvailableProperties;
            _ddlProperties.DataValueField = "Name";
            _ddlProperties.DataTextField = "Name";
            _ddlProperties.DataBind();

            if ( setting.FieldSource == FieldSource.Property )
            {
                _ddlProperties.SelectedValue = setting.FieldName;
            }
            else if ( setting.FieldSource == FieldSource.Attribute )
            {
                _rblAttributeFormatType.SetValue( setting.AttributeFormat.ConvertToInt().ToString() );
            }
            else
            {
                _ceLavaExpression.Text = setting.Value;
                _rblAttributeFormatType.SetValue( setting.AttributeFormat.ConvertToInt().ToString() );
                _rblFieldFormat.SetValue( setting.FieldFormat.ConvertToInt().ToString() );
            }
        }

        #endregion

        #region Data Edit Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblFieldSource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rblFieldSource_SelectedIndexChanged( object sender, EventArgs e )
        {
            /* Intentionally left blank, handled in RenderBaseControl(). */
        }

        /// <summary>
        /// Handles the Click event of the lbApply control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbApply_Click( object sender, EventArgs e )
        {
            IsEditMode = false;

            var settings = FieldSettings;
            var setting = settings.Where( s => s.Key == OriginalEditKey ).FirstOrDefault();

            if ( setting == null )
            {
                setting = new FieldSetting();
                settings.Add( setting );
            }

            setting.Key = _tbKey.Text;
            setting.FieldSource = _rblFieldSource.SelectedValueAsEnum<FieldSource>();

            if ( setting.FieldSource == FieldSource.Property )
            {
                var propertyName = _ddlProperties.SelectedValue;

                setting.Value = $"{{{{ item.{propertyName} }}}}";
                setting.FieldName = propertyName;
                setting.Key = propertyName;

                var property = AvailableProperties.Where( p => p.Name == propertyName ).FirstOrDefault();

                if ( property.Type.Contains( ".Int" ) )
                {
                    setting.FieldFormat = FieldFormat.Number;
                }
                else if ( property.Type.Contains( "DateTime" ) )
                {
                    setting.FieldFormat = FieldFormat.Date;
                }
                else
                {
                    setting.FieldFormat = FieldFormat.String;
                }
            }
            else if ( setting.FieldSource == FieldSource.Attribute )
            {
                var attributeKey = _ddlAttributes.SelectedValue;
                var attributeFormatType = _rblAttributeFormatType.SelectedValueAsEnum<AttributeFormat>();

                setting.Key = attributeKey;

                if ( attributeFormatType == AttributeFormat.FriendlyValue )
                {
                    setting.Value = $"{{{{ item | Attribute:'{attributeKey}' }}}}";
                }
                else
                {
                    setting.Value = $"{{{{ item | Attribute:'{attributeKey}','RawValue' }}}}";
                }

                setting.FieldFormat = FieldFormat.String;
                setting.FieldName = attributeKey;
                setting.AttributeFormat = attributeFormatType;
            }
            else
            {
                setting.FieldFormat = _rblFieldFormat.SelectedValueAsEnum<FieldFormat>();
                setting.FieldName = string.Empty;
                setting.Value = _ceLavaExpression.Text;
            }

            FieldSettings = settings;

            BindFieldGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void lbCancel_Click( object sender, EventArgs e )
        {
            IsEditMode = false;
        }

        #endregion

        #region POCOs

        /// <summary>
        /// POCO to model info about the properties of the content channel item model
        /// </summary>
        private class PropertyInfoSummary
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            /// <value>
            /// The type.
            /// </value>
            public string Type { get; set; }
        }

        #endregion
    }
}
