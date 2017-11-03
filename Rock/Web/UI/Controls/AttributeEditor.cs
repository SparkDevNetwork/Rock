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
using System.Reflection;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Custom attribute editor control
    /// </summary>
    public class AttributeEditor : CompositeControl, IHasValidationGroup
    {
        #region Private Variables

        private bool _controlsLoaded = false;

        #endregion

        #region Controls

        /// <summary>
        /// Existing key names control
        /// </summary>
        protected HtmlInputHidden _hfExistingKeyNames;

        /// <summary>
        /// Attribute action title control
        /// </summary>
        protected Literal _lAttributeActionTitle;

        /// <summary>
        /// Validation summary control
        /// </summary>
        protected ValidationSummary _validationSummary;

        /// <summary>
        /// Name control
        /// </summary>
        protected RockTextBox _tbName;

        /// <summary>
        /// Description control
        /// </summary>
        protected RockTextBox _tbDescription;

        /// <summary>
        /// Categories control
        /// </summary>
        protected CategoryPicker _cpCategories;

        /// <summary>
        /// Key control (readonly)
        /// </summary>
        protected RockLiteral _lKey;

        /// <summary>
        /// Key control (editable)
        /// </summary>
        protected RockTextBox _tbKey;

        /// <summary>
        /// Key validator
        /// </summary>
        protected CustomValidator _cvKey;

        /// <summary>
        /// Icon CSS class control
        /// </summary>
        protected RockTextBox _tbIconCssClass;

        /// <summary>
        /// Required control
        /// </summary>
        protected RockCheckBox _cbRequired;

        /// <summary>
        /// Show in grid control
        /// </summary>
        protected RockCheckBox _cbShowInGrid;

        /// <summary>
        /// Allow search control
        /// </summary>
        protected RockCheckBox _cbAllowSearch;

        /// <summary>
        /// Is indexing enabled control
        /// </summary>
        protected RockCheckBox _cbIsIndexingEnabled;

        /// <summary>
        /// Is analytic control
        /// </summary>
        protected RockCheckBox _cbIsAnalytic;

        /// <summary>
        /// Is analytic history control
        /// </summary>
        protected RockCheckBox _cbIsAnalyticHistory;

        /// <summary>
        /// Field type control
        /// </summary>
        protected RockDropDownList _ddlFieldType;

        /// <summary>
        /// Qualifiers control
        /// </summary>
        protected PlaceHolder _phQualifiers;

        /// <summary>
        /// Default value control
        /// </summary>
        protected PlaceHolder _phDefaultValue;

        /// <summary>
        /// Save control
        /// </summary>
        protected LinkButton _btnSave;

        /// <summary>
        /// Cancel control
        /// </summary>
        protected LinkButton _btnCancel;

        #endregion Controls

        #region Properties

        /// <summary>
        /// Gets or sets the attribute id.
        /// </summary>
        /// <value>
        /// The attribute id.
        /// </value>
        public int? AttributeId
        {
            get
            {
                return ViewState["AttributeId"] as int?;
            }
            set
            {
                ViewState["AttributeId"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute GUID.
        /// </summary>
        /// <value>
        /// The attribute GUID.
        /// </value>
        public Guid AttributeGuid
        {
            get
            {
                string guid = ViewState["AttributeGuid"] as string;
                if ( guid == null )
                {
                    return Guid.NewGuid();
                }
                else
                {
                    return new Guid( guid );
                }
            }
            set { ViewState["AttributeGuid"] = value.ToString(); }
        }

        /// <summary>
        /// Gets or sets the attribute entity type id.
        /// </summary>
        /// <value>
        /// The attribute entity type id.
        /// </value>
        public int? AttributeEntityTypeId
        {
            get
            {
                return ViewState["AttributeEntityTypeId"] as int?;
            }
            set
            {
                ViewState["AttributeEntityTypeId"] = value;
                EnsureChildControls();
                _cpCategories.EntityTypeQualifierValue = value.HasValue && value.Value != 0 ? value.ToString() : string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the attribute entity type qualifier column.
        /// </summary>
        /// <value>
        /// The attribute entity type qualifier column.
        /// </value>
        private string AttributeEntityTypeQualifierColumn
        {
            get
            {
                return ViewState["AttributeEntityTypeQualifierColumn"] as string;
            }
            set
            {
                ViewState["AttributeEntityTypeQualifierColumn"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute entity type qualifier value.
        /// </summary>
        /// <value>
        /// The attribute entity type qualifier value.
        /// </value>
        private string AttributeEntityTypeQualifierValue
        {
            get
            {
                return ViewState["AttributeEntityTypeQualifierValue"] as string;
            }
            set
            {
                ViewState["AttributeEntityTypeQualifierValue"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show actions].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show actions]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowActions
        {
            get { return ViewState["ShowActions"] as bool? ?? true; }
            set { ViewState["ShowActions"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow editing the Key field.
        /// </summary>
        /// <value>
        ///   <c>true</c> to see an editable Key field; otherwise, <c>false</c>.
        /// </value>
        public bool IsKeyEditable
        {
            get
            {
                EnsureChildControls();
                return _lKey.Visible;
            }
            set
            {
                EnsureChildControls();
                _lKey.Visible = !value;
                if ( !value )
                {
                    _tbKey.CssClass = "hidden";
                    _tbKey.FormGroupCssClass = "hidden";
                }
            }
        }

        /// <summary>
        /// Gets or sets the action title.
        /// </summary>
        /// <value>
        /// The action title.
        /// </value>
        public string ActionTitle
        {
            get
            {
                EnsureChildControls();
                return _lAttributeActionTitle.Text;
            }
            set
            {
                EnsureChildControls();
                _lAttributeActionTitle.Text = value;
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
                EnsureChildControls();
                return _tbName.Text;
            }
            set
            {
                EnsureChildControls();
                _tbName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the label of the Name field.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string NameFieldLabel
        {
            get
            {
                EnsureChildControls();
                return string.IsNullOrEmpty( _tbName.Label ) ? "Name" : _tbName.Label;
            }
            set
            {
                EnsureChildControls();
                _tbName.Label = value;
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
                EnsureChildControls();
                return _tbDescription.Text;
            }
            set
            {
                EnsureChildControls();
                _tbDescription.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key
        {
            get
            {
                EnsureChildControls();
                return _tbKey.Text;
            }
            set
            {
                EnsureChildControls();
                _tbKey.Text = value;
                _lKey.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass
        {
            get
            {
                EnsureChildControls();
                return _tbIconCssClass.Text;
            }
            set
            {
                EnsureChildControls();
                _tbIconCssClass.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the category ids.
        /// </summary>
        /// <value>
        /// The category ids.
        /// </value>
        public IEnumerable<int> CategoryIds
        {
            get
            {
                EnsureChildControls();
                return _cpCategories.SelectedValuesAsInt();
            }
            set
            {
                EnsureChildControls();
                _cpCategories.SetValues( value );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="AttributeEditor"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required
        {
            get
            {
                EnsureChildControls();
                return _cbRequired.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbRequired.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not to show the analytics options are displayed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the Analuytics options are visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnalyticsVisible
        {
            get
            {
                EnsureChildControls();
                return _cbIsAnalytic.Visible;
            }
            set
            {
                EnsureChildControls();
                _cbIsAnalytic.Visible = value;
                _cbIsAnalyticHistory.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Categories option is displayed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Categories option is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsCategoriesVisible
        {
            get
            {
                EnsureChildControls();
                return _cpCategories.Visible;
            }
            set
            {
                EnsureChildControls();
                _cpCategories.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Description option is displayed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Description option is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsDescriptionVisible
        {
            get
            {
                EnsureChildControls();
                return _tbDescription.Visible;
            }
            set
            {
                EnsureChildControls();
                _tbDescription.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Icon Css Class option is displayed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if Icon Css Class option is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsIconCssClassVisible
        {
            get
            {
                EnsureChildControls();
                return _tbIconCssClass.Visible;
            }
            set
            {
                EnsureChildControls();
                _tbIconCssClass.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Show in Grid option is displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if Show in Grid option is visible; otherwise, <c>false</c>.
        /// </value>
        [Obsolete( "Use IsShowInGridVisible instead." )]
        public bool ShowInGridVisible
        {
            get
            {
                EnsureChildControls();
                return _cbShowInGrid.Visible;
            }
            set
            {
                EnsureChildControls();
                _cbShowInGrid.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Show in Grid option is displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if Show in Grid option is visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsShowInGridVisible
        {
            get
            {
                EnsureChildControls();
                return _cbShowInGrid.Visible;
            }
            set
            {
                EnsureChildControls();
                _cbShowInGrid.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets the excluded field types.
        /// </summary>
        /// <value>
        /// The excluded field types.
        /// </value>
        public FieldTypeCache[] ExcludedFieldTypes
        {
            get
            {
                int[] excludedFieldTypeIds = this.ViewState["ExcludedFieldTypeIds"] as int[];
                return excludedFieldTypeIds?.Select( a => FieldTypeCache.Read( a ) ).ToArray() ?? new FieldTypeCache[0];
            }
            set
            {
                this.ViewState["ExcludedFieldTypeIds"] = value.Select( a => a.Id ).ToArray();

                EnsureChildControls();
                LoadFieldTypes();
            }
        }

        /// <summary>
        /// Gets or sets the included field types.
        /// </summary>
        /// <value>
        /// The included field types.
        /// </value>
        public FieldTypeCache[] IncludedFieldTypes
        {
            get
            {
                int[] includedFieldTypeIds = this.ViewState["IncludedFieldTypeIds"] as int[];
                return includedFieldTypeIds?.Select( a => FieldTypeCache.Read( a ) ).ToArray() ?? new FieldTypeCache[0];
            }
            set
            {
                this.ViewState["IncludedFieldTypeIds"] = value.Select( a => a.Id ).ToArray();

                EnsureChildControls();
                LoadFieldTypes();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show in grid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in grid]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInGrid
        {
            get
            {
                EnsureChildControls();
                return _cbShowInGrid.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbShowInGrid.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow search visible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow search visible]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowSearchVisible
        {
            get
            {
                EnsureChildControls();
                return _cbAllowSearch.Visible;
            }
            set
            {
                EnsureChildControls();
                _cbAllowSearch.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow indexing visible].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow indexing visible]; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexingEnabledVisible
        {
            get
            {
                EnsureChildControls();
                return _cbIsIndexingEnabled.Visible;
            }
            set
            {
                EnsureChildControls();
                _cbIsIndexingEnabled.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow search].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow search]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowSearch
        {
            get
            {
                EnsureChildControls();
                return _cbAllowSearch.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbAllowSearch.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [allow indexing].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow indexing]; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexingEnabled
        {
            get
            {
                EnsureChildControls();
                return _cbIsIndexingEnabled.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbIsIndexingEnabled.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is analytic; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnalytic
        {
            get
            {
                EnsureChildControls();
                return _cbIsAnalytic.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbIsAnalytic.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is analytic history.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is analytic history; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnalyticHistory
        {
            get
            {
                EnsureChildControls();
                return _cbIsAnalyticHistory.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbIsAnalyticHistory.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the field type id.
        /// </summary>
        /// <value>
        /// The field type id.
        /// </value>
        public int? FieldTypeId
        {
            get
            {
                EnsureChildControls();
                return _ddlFieldType.SelectedValueAsInt();
            }
            set
            {
                EnsureChildControls();
                if ( value != _ddlFieldType.SelectedValueAsInt() )
                {
                    _ddlFieldType.SetValue( value );
                    CreateFieldTypeDetailControls( value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the qualifiers.
        /// </summary>
        /// <value>
        /// The qualifiers.
        /// </value>
        public Dictionary<string, ConfigurationValue> Qualifiers
        {
            get
            {
                return ViewState["Qualifiers"] as Dictionary<string, ConfigurationValue> ??
                    new Dictionary<string, ConfigurationValue>();
            }
            set
            {
                ViewState["Qualifiers"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public string DefaultValue
        {
            get
            {
                return ViewState["DefaultValue"] as string;
            }
            set
            {
                ViewState["DefaultValue"] = value;
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
                return ViewState["ValidationGroup"] as string;
            }
            set
            {
                ViewState["ValidationGroup"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the reserved key names.
        /// </summary>
        /// <value>
        /// The reserved key names.
        /// </value>
        public List<string> ReservedKeyNames
        {
            get
            {
                return ViewState["ReservedKeyNames"] as List<string> ?? new List<string>();
            }
            set
            {
                ViewState["ReservedKeyNames"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the object property names.
        /// </summary>
        /// <value>
        /// The object property names.
        /// </value>
        private List<string> ObjectPropertyNames
        {
            get
            {
                return ViewState["ObjectPropertyNames"] as List<string> ?? new List<string>();
            }
            set
            {
                ViewState["ObjectPropertyNames"] = value;
            }
        }

        #endregion

        #region Overridden Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            EnsureChildControls();
            LoadFieldTypes();

            base.OnInit( e );
        }

        /// <summary>
        /// Loads the field types.
        /// </summary>
        protected void LoadFieldTypes()
        {
            if ( this.IncludedFieldTypes.Any() )
            {
                _ddlFieldType.DataSource = FieldTypeCache.All().Where( a => this.IncludedFieldTypes.Any( x => x.Id == a.Id ) ).ToList();
            }
            else
            {
                _ddlFieldType.DataSource = FieldTypeCache.All().Where( a => !this.ExcludedFieldTypes.Any( x => x.Id == a.Id ) ).ToList();
            }
            _ddlFieldType.DataBind();
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            CreateFieldTypeDetailControls( ViewState["FieldTypeId"] as int? );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            if ( !_controlsLoaded )
            {
                Controls.Clear();

                _hfExistingKeyNames = new HtmlInputHidden();
                _hfExistingKeyNames.AddCssClass( "js-existing-key-names" );
                _hfExistingKeyNames.ID = this.ID + "_hfExistingKeyNames";
                Controls.Add( _hfExistingKeyNames );

                _lAttributeActionTitle = new Literal();
                _lAttributeActionTitle.ID = "lAttributeActionTitle";
                Controls.Add( _lAttributeActionTitle );

                _validationSummary = new ValidationSummary();
                _validationSummary.ID = "valiationSummary";
                _validationSummary.CssClass = "alert alert-danger";
                _validationSummary.HeaderText = "Please correct the following:";
                Controls.Add( _validationSummary );

                _tbName = new RockTextBox();
                _tbName.ID = "tbName";
                _tbName.Label = NameFieldLabel;
                _tbName.Required = true;
                Controls.Add( _tbName );

                _tbDescription = new RockTextBox();
                _tbDescription.Label = "Description";
                _tbDescription.ID = "tbDescription";
                _tbDescription.TextMode = TextBoxMode.MultiLine;
                _tbDescription.Rows = 3;
                Controls.Add( _tbDescription );

                _cpCategories = new CategoryPicker();
                _cpCategories.ID = "cpCategories_" + this.ID.ToString();
                _cpCategories.Label = "Categories";
                _cpCategories.AllowMultiSelect = true;
                _cpCategories.EntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Attribute ) ).Id;
                _cpCategories.EntityTypeQualifierColumn = "EntityTypeId";
                Controls.Add( _cpCategories );

                _lKey = new RockLiteral();
                _lKey.Label = "Key";
                _lKey.ID = "lKey";
                _lKey.Visible = false;  // Default is to not show this option
                Controls.Add( _lKey );

                _tbKey = new RockTextBox();
                _tbKey.ID = "tbKey";
                _tbKey.Label = "Key";
                _tbKey.Required = true;
                Controls.Add( _tbKey );

                _cvKey = new CustomValidator();
                _cvKey.ID = "cvKey";
                _cvKey.ControlToValidate = _tbKey.ID;
                _cvKey.ClientValidationFunction = "validateKey";
                _cvKey.ServerValidate += cvKey_ServerValidate;
                _cvKey.Display = ValidatorDisplay.Dynamic;
                _cvKey.CssClass = "validation-error help-inline";
                _cvKey.ErrorMessage = "There is already an existing property with the key value you entered or the key has illegal characters. Please select a different key value and use only letters, numbers and underscores.";
                Controls.Add( _cvKey );

                _tbIconCssClass = new RockTextBox();
                _tbIconCssClass.ID = "_tbIconCssClass";
                _tbIconCssClass.Label = "Icon CSS Class";
                Controls.Add( _tbIconCssClass );

                _cbRequired = new RockCheckBox();
                _cbRequired.ID = "cbRequired";
                _cbRequired.Label = "Required";
                _cbRequired.Text = "Require a value";
                Controls.Add( _cbRequired );

                _cbShowInGrid = new RockCheckBox();
                _cbShowInGrid.ID = "cbShowInGrid";
                _cbShowInGrid.Label = "Show in Grid";
                _cbShowInGrid.Text = "Yes";
                _cbShowInGrid.Help = "If selected, this attribute will be included in a grid.";
                Controls.Add( _cbShowInGrid );

                _cbAllowSearch = new RockCheckBox();
                _cbAllowSearch.ID = "cbAllowSearch";
                _cbAllowSearch.Label = "Allow Search";
                _cbAllowSearch.Text = "Yes";
                _cbAllowSearch.Help = "If selected, this attribute can be search on.";
                _cbAllowSearch.Visible = false;  // Default is to not show this option
                Controls.Add( _cbAllowSearch );

                _cbIsIndexingEnabled = new RockCheckBox();
                _cbIsIndexingEnabled.ID = "cbAllowIndexing";
                _cbIsIndexingEnabled.Label = "Indexing Enabled";
                _cbIsIndexingEnabled.Text = "Yes";
                _cbIsIndexingEnabled.Help = "If selected, this attribute can be used when indexing for universal search.";
                _cbIsIndexingEnabled.Visible = false;  // Default is to not show this option
                Controls.Add( _cbIsIndexingEnabled );

                _cbIsAnalytic = new RockCheckBox();
                _cbIsAnalytic.ID = "_cbIsAnalytic";
                _cbIsAnalytic.Label = "Analytics Enabled";
                _cbIsAnalytic.Text = "Yes";
                _cbIsAnalytic.Help = "If selected, this attribute will be made available as an Analytic";
                _cbIsAnalytic.Visible = false;  // Default is to not show this option
                Controls.Add( _cbIsAnalytic );

                _cbIsAnalyticHistory = new RockCheckBox();
                _cbIsAnalyticHistory.ID = "_cbIsAnalyticHistory";
                _cbIsAnalyticHistory.Label = "Analytics History Enabled";
                _cbIsAnalyticHistory.Text = "Yes";
                _cbIsAnalyticHistory.Help = "If selected, changes to the value of this attribute will cause Analytics to create a history record. Note that this requires that 'Analytics Enabled' is also enabled.";
                _cbIsAnalyticHistory.Visible = false;  // Default is to not show this option
                Controls.Add( _cbIsAnalyticHistory );

                _ddlFieldType = new RockDropDownList();
                _ddlFieldType.ID = "ddlFieldType";
                _ddlFieldType.Label = "Field Type";
                _ddlFieldType.AutoPostBack = true;
                _ddlFieldType.SelectedIndexChanged += _ddlFieldType_SelectedIndexChanged;
                _ddlFieldType.DataValueField = "Id";
                _ddlFieldType.DataTextField = "Name";
                _ddlFieldType.EnhanceForLongLists = true;
                Controls.Add( _ddlFieldType );

                _phQualifiers = new PlaceHolder();
                _phQualifiers.ID = "phQualifiers";
                _phQualifiers.EnableViewState = false;
                Controls.Add( _phQualifiers );

                _phDefaultValue = new PlaceHolder();
                _phDefaultValue.ID = "phDefaultValue";
                _phDefaultValue.EnableViewState = false;
                Controls.Add( _phDefaultValue );

                _btnSave = new LinkButton();
                _btnSave.ID = "btnSave";
                _btnSave.Text = "OK";
                _btnSave.CssClass = "btn btn-primary";
                _btnSave.Click += btnSave_Click;
                Controls.Add( _btnSave );

                _btnCancel = new LinkButton();
                _btnCancel.ID = "btnCancel";
                _btnCancel.Text = "Cancel";
                _btnCancel.CssClass = "btn btn-link";
                _btnCancel.CausesValidation = false;
                _btnCancel.Click += btnCancel_Click;
                Controls.Add( _btnCancel );

                _controlsLoaded = true;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack && FieldTypeId.HasValue )
            {
                var field = Rock.Web.Cache.FieldTypeCache.Read( FieldTypeId.Value ).Field;
                var qualifierControls = new List<Control>();
                foreach ( Control control in _phQualifiers.Controls )
                {
                    qualifierControls.Add( control );
                }

                DefaultValue = _phDefaultValue.Controls.Count >= 1 ?
                    field.GetEditValue( _phDefaultValue.Controls[0], Qualifiers ) : string.Empty;

                Qualifiers = field.ConfigurationValues( qualifierControls );
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            // Recreate the qualifiers and default control in case they changed due to new field type or
            // new qualifier values
            CreateFieldTypeDetailControls( FieldTypeId, true );

            _cbIsAnalytic.Visible = false;
            _cbIsAnalyticHistory.Visible = false;

            // Only show the Analytic checkbox if the Entity is IAnalytic
            if ( this.AttributeEntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Read( this.AttributeEntityTypeId.Value );
                if ( entityType != null )
                {
                    _cbIsAnalytic.Visible = entityType.IsAnalyticAttributesSupported( this.AttributeEntityTypeQualifierColumn, this.AttributeEntityTypeQualifierValue );
                    _cbIsAnalyticHistory.Visible = entityType.IsAnalyticsHistoricalSupported( this.AttributeEntityTypeQualifierColumn, this.AttributeEntityTypeQualifierValue ) ;
                }
            }

            // Set the validation group for all controls
            string validationGroup = ValidationGroup;
            _validationSummary.ValidationGroup = validationGroup;
            _tbName.ValidationGroup = validationGroup;
            _tbDescription.ValidationGroup = validationGroup;
            _cpCategories.ValidationGroup = validationGroup;
            _tbKey.ValidationGroup = validationGroup;
            _cvKey.ValidationGroup = validationGroup;
            _tbIconCssClass.ValidationGroup = validationGroup;
            _cbRequired.ValidationGroup = validationGroup;
            _cbShowInGrid.ValidationGroup = validationGroup;
            _cbAllowSearch.ValidationGroup = validationGroup;
            _cbIsIndexingEnabled.ValidationGroup = validationGroup;
            _cbIsAnalytic.ValidationGroup = validationGroup;
            _cbIsAnalyticHistory.ValidationGroup = validationGroup;
            _ddlFieldType.ValidationGroup = validationGroup;
            foreach ( var control in _phQualifiers.Controls )
            {
                if ( control is IRockControl )
                {
                    ( ( IRockControl ) control ).ValidationGroup = validationGroup;
                }
            }
            foreach ( var control in _phDefaultValue.Controls )
            {
                var rockControl = control as IRockControl;
                if ( rockControl != null )
                {
                    rockControl.ValidationGroup = validationGroup;
                }
            }
            _btnSave.ValidationGroup = validationGroup;
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            _tbName.Attributes["onblur"] = string.Format( "populateAttributeKey('{0}','{1}','{2}' )", _tbName.ClientID, _tbKey.ClientID, _lKey.ClientID );

            writer.RenderBeginTag( HtmlTextWriterTag.Fieldset );

            writer.RenderBeginTag( HtmlTextWriterTag.Legend );
            _lAttributeActionTitle.RenderControl( writer );
            writer.RenderEndTag();

            var existingKeyNames = new List<string>();
            ReservedKeyNames.ForEach( n => existingKeyNames.Add( n ) );
            ObjectPropertyNames.ForEach( n => existingKeyNames.Add( n ) );
            _hfExistingKeyNames.Value = existingKeyNames.ToJson();
            _hfExistingKeyNames.RenderControl( writer );

            _validationSummary.RenderControl( writer );

            // row 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _lKey.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            // row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbDescription.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            // row 3
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // row 3 col 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cpCategories.RenderControl( writer );
            _tbKey.RenderControl( writer );
            _cvKey.RenderControl( writer );
            _tbIconCssClass.RenderControl( writer );


            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbRequired.RenderControl( writer );
            _cbIsIndexingEnabled.RenderControl( writer );
            _cbIsAnalytic.RenderControl( writer );
            _cbIsAnalyticHistory.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbShowInGrid.RenderControl( writer );
            _cbAllowSearch.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();
            
            writer.RenderEndTag();

            // row 3 col 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _ddlFieldType.RenderControl( writer );
            _phQualifiers.RenderControl( writer );
            _phDefaultValue.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            // </fieldset>
            writer.RenderEndTag();

            if ( ShowActions )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "actions" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _btnSave.RenderControl( writer );
                writer.Write( Environment.NewLine );
                _btnCancel.RenderControl( writer );
                writer.RenderEndTag();
            }

            RegisterClientScript();
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["FieldTypeId"] = FieldTypeId;

            return base.SaveViewState();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the ServerValidate event of the cvKey control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvKey_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = 
                !ReservedKeyNames.Contains( _tbKey.Text.Trim(), StringComparer.CurrentCultureIgnoreCase ) &&
                !ObjectPropertyNames.Contains( _tbKey.Text.Trim(), StringComparer.CurrentCultureIgnoreCase );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlFieldType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void _ddlFieldType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // When someone changes the field type, we clear the default value since it's no longer relevant.
            DefaultValue = string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( SaveClick != null )
            {
                SaveClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( CancelClick != null )
            {
                CancelClick( sender, e );
            }
        }
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the attribute properties.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="objectType">Type of the object.</param>
        public void SetAttributeProperties( Rock.Model.Attribute attribute, Type objectType = null )
        {
            if ( attribute != null )
            {
                this.AttributeId = attribute.Id;
                this.AttributeGuid = attribute.Guid;
                this.AttributeEntityTypeQualifierColumn = attribute.EntityTypeQualifierColumn;
                this.AttributeEntityTypeQualifierValue = attribute.EntityTypeQualifierValue;
                this.Name = attribute.Name;
                this.Key = attribute.Key;
                this.IconCssClass = attribute.IconCssClass;
                this.CategoryIds = attribute.Categories.Select( c => c.Id ).ToList();
                this.Description = attribute.Description;
                this.FieldTypeId = attribute.FieldTypeId;
                this.Required = attribute.IsRequired;
                this.ShowInGrid = attribute.IsGridColumn;
                this.AllowSearch = attribute.AllowSearch;
                this.IsIndexingEnabled = attribute.IsIndexEnabled;
                this.IsAnalytic = attribute.IsAnalytic;
                this.IsAnalyticHistory = attribute.IsAnalyticHistory;

                var qualifiers = new Dictionary<string, ConfigurationValue>();
                if ( attribute.AttributeQualifiers != null )
                {
                    foreach ( Rock.Model.AttributeQualifier qualifier in attribute.AttributeQualifiers )
                    {
                        qualifiers.Add( qualifier.Key, new ConfigurationValue( qualifier.Value ) );
                    }
                }

                this.Qualifiers = qualifiers;
                this.DefaultValue = attribute.DefaultValue;
            }

            if ( objectType != null )
            {
                this.AttributeEntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( objectType ).Id;

                ObjectPropertyNames = new List<string>();
                foreach ( var propInfo in objectType.GetProperties() )
                {
                    ObjectPropertyNames.Add( propInfo.Name );
                }
            }

        }

        /// <summary>
        /// Gets the attribute properties.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        public void GetAttributeProperties( Rock.Model.Attribute attribute )
        {
            if ( attribute != null )
            {
                attribute.Id = this.AttributeId ?? 0;
                attribute.Guid = this.AttributeGuid;
                attribute.Name = this.Name;
                attribute.Key = this.Key;
                attribute.IconCssClass = this.IconCssClass;
                attribute.Description = this.Description;
                attribute.FieldTypeId = this.FieldTypeId ?? 0;
                attribute.IsMultiValue = false;
                attribute.IsRequired = this.Required;
                attribute.IsGridColumn = this.ShowInGrid;
                attribute.AllowSearch = this.AllowSearch;
                attribute.IsIndexEnabled = this.IsIndexingEnabled;
                attribute.IsAnalytic = this.IsAnalytic;
                attribute.IsAnalyticHistory = this.IsAnalyticHistory;

                attribute.Categories.Clear();
                new CategoryService( new RockContext() ).Queryable().Where( c => this.CategoryIds.Contains( c.Id ) ).ToList().ForEach( c =>
                    attribute.Categories.Add( c ) );

                attribute.AttributeQualifiers.Clear();
                foreach ( var qualifier in Qualifiers )
                {
                    AttributeQualifier attributeQualifier = new AttributeQualifier();
                    attributeQualifier.IsSystem = false;
                    attributeQualifier.Key = qualifier.Key;
                    attributeQualifier.Value = qualifier.Value.Value ?? string.Empty;
                    attribute.AttributeQualifiers.Add( attributeQualifier );
                }

                attribute.DefaultValue = this.DefaultValue;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates the field type detail controls.
        /// </summary>
        /// <param name="fieldTypeId">The field type id.</param>
        /// <param name="recreate">if set to <c>true</c> [recreate].</param>
        protected void CreateFieldTypeDetailControls( int? fieldTypeId, bool recreate = false )
        {
            EnsureChildControls();

            if ( recreate )
            {
                _phQualifiers.Controls.Clear();
                _phDefaultValue.Controls.Clear();
            }

            if ( fieldTypeId.HasValue )
            {
                var field = Rock.Web.Cache.FieldTypeCache.Read( fieldTypeId.Value ).Field;

                var configControls = field.ConfigurationControls();
                if ( recreate )
                {
                    field.SetConfigurationValues( configControls, Qualifiers );
                }

                int i = 0;
                foreach ( var control in configControls )
                {
                    control.ID = string.Format( "qualifier_{0}", i++ );
                    _phQualifiers.Controls.Add( control );
                }

                // default control id needs to be unique to field type because some field types will transform
                // field (i.e. htmleditor) and switching field types will not reset that
                if ( field.HasDefaultControl )
                {
                    var defaultControl = field.EditControl( Qualifiers, string.Format( "defaultValue_{0}", fieldTypeId.Value ) );
                    if ( defaultControl != null )
                    {
                        _phDefaultValue.Controls.Add( defaultControl );

                        if ( recreate )
                        {
                            field.SetEditValue( defaultControl, Qualifiers, DefaultValue );
                        }

                        if ( defaultControl is IRockControl )
                        {
                            var rockControl = defaultControl as IRockControl;
                            rockControl.Required = false;
                            rockControl.Label = "Default Value";
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Registers the client script.
        /// </summary>
        protected void RegisterClientScript()
        {
            string script = @"
    function populateAttributeKey(nameControlId, keyControlId, literalKeyControlId ) {
        // if the attribute key hasn't been filled in yet, populate it with the attribute name minus whitespace
        var literalKeyControl = $('#' + literalKeyControlId);
        var keyControl = $('#' + keyControlId);
        var keyValue = keyControl.val();

        var reservedKeyJson = keyControl.closest('fieldset').find('.js-existing-key-names').val();
        var reservedKeyNames = eval('(' + reservedKeyJson + ')');

        if (keyValue == '') {

            keyValue = $('#' + nameControlId).val().replace(/[^a-zA-Z0-9_.\-]/g, '');
            var newKeyValue = keyValue;
        
            var i = 1;
            while ($.inArray(newKeyValue, reservedKeyNames) >= 0) {
                newKeyValue = keyValue + i++;
            }
            
            keyControl.val(newKeyValue);
            literalKeyControl.html(newKeyValue);
        }
    }

    function validateKey(sender, args) {
        var keyControl = $('#' + sender.controltovalidate);
        var reservedKeyJson = keyControl.closest('fieldset').find('.js-existing-key-names').val();
        var reservedKeyNames = eval('(' + reservedKeyJson + ')');
        args.IsValid = ( $.inArray( keyControl.val(), reservedKeyNames ) < 0 && ! keyControl.val().match(/[^a-zA-Z0-9_.\-]/g) );
    }
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "AttributeEditor", script, true );
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when Save is clicked
        /// </summary>
        public event EventHandler SaveClick;

        /// <summary>
        /// Occurs when Cancel is clicked.
        /// </summary>
        public event EventHandler CancelClick;

        #endregion

    }

}