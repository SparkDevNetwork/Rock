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

using System;
using System.Collections.Generic;
using System.Linq;
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
        /// The abbreviated name control
        /// </summary>
        protected RockTextBox _tbAbbreviatedName;

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
        /// The IsActive checkbox
        /// </summary>
        protected RockCheckBox _cbIsActive;

        /// <summary>
        /// The Enable History checkbox
        /// </summary>
        protected RockCheckBox _cbEnableHistory;

        /// <summary>
        /// The Pre-HTML codeeditor
        /// </summary>
        protected CodeEditor _cePreHtml;

        /// <summary>
        /// The Post-HTML codeeditor
        /// </summary>
        protected CodeEditor _cePostHtml;

        /// <summary>
        /// The hidden field that stores the FieldTypeId when the FieldType DropDown is not visible
        /// </summary>
        protected HiddenField _hfReadOnlyFieldTypeId;

        /// <summary>
        /// Field type control (readonly)
        /// </summary>
        protected RockLiteral _lFieldType;

        /// <summary>
        /// Field type control
        /// </summary>
        protected RockDropDownList _ddlFieldType;

        /// <summary>
        /// Qualifiers control
        /// </summary>
        protected DynamicPlaceholder _phQualifiers;

        /// <summary>
        /// Default value control
        /// </summary>
        protected DynamicPlaceholder _phDefaultValue;

        /// <summary>
        /// The advanced panel widget
        /// </summary>
        protected PanelWidget _pwAdvanced;

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
        public string AttributeEntityTypeQualifierColumn
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
        public string AttributeEntityTypeQualifierValue
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
        /// Gets or sets a value indicating whether [show action title].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show action title]; otherwise, <c>false</c>.
        ///   defaulted True
        /// </value>
        public bool ShowActionTitle
        {
            get { return ViewState["ShowActionTitle"] as bool? ?? true; }
            set { ViewState["ShowActionTitle"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this Attribute is marked as IsSystem=true
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem
        {
            get
            {
                return ViewState["IsSystem"] as bool? ?? false;
            }

            private set
            {
                EnsureChildControls();
                ViewState["IsSystem"] = value;
                IsKeyEditable = !value;
            }
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
                return _tbKey.Visible;
            }
            set
            {
                EnsureChildControls();
                _lKey.Visible = !value;
                _tbKey.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to allow editing the FieldType field.
        /// </summary>
        /// <value>
        ///   <c>true</c> to see an editable FieldType field; otherwise, <c>false</c>.
        /// </value>
        public bool IsFieldTypeEditable
        {
            get
            {
                EnsureChildControls();
                return _ddlFieldType.Visible;
            }
            set
            {
                EnsureChildControls();
                _lFieldType.Visible = !value;
                _ddlFieldType.Visible = value;
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
        /// Gets or sets the abbreviated name.
        /// </summary>
        /// <value>
        /// The name of the abbreviated.
        /// </value>
        public string AbbreviatedName
        {
            get
            {
                EnsureChildControls();
                return _tbAbbreviatedName.Text;
            }
            set
            {
                EnsureChildControls();
                _tbAbbreviatedName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the label of the abbreviated name field.
        /// </summary>
        /// <value>
        /// The abbreviated name label.
        /// </value>
        public string AbbreviatedNameLabel
        {
            get
            {
                EnsureChildControls();
                return string.IsNullOrEmpty( _tbAbbreviatedName.Label ) ? "Abbreviated Name" : _tbAbbreviatedName.Label;
            }

            set
            {
                EnsureChildControls();
                _tbAbbreviatedName.Label = value;
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
        ///   <c>true</c> if the Analytics options are visible; otherwise, <c>false</c>.
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
        [RockObsolete( "1.7" )]
        [Obsolete( "Use IsShowInGridVisible instead.", true )]
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
                return excludedFieldTypeIds?.Select( a => FieldTypeCache.Get( a ) ).ToArray() ?? new FieldTypeCache[0];
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
                return includedFieldTypeIds?.Select( a => FieldTypeCache.Get( a ) ).ToArray() ?? new FieldTypeCache[0];
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
        /// Gets or sets a value indicating whether [enable history].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable history]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableHistory
        {
            get
            {
                EnsureChildControls();
                return _cbEnableHistory.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbEnableHistory.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        public string PreHtml
        {
            get
            {
                EnsureChildControls();
                return _cePreHtml.Text;
            }
            set
            {
                EnsureChildControls();
                _cePreHtml.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the post HTML.
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        public string PostHtml
        {
            get
            {
                EnsureChildControls();
                return _cePostHtml.Text;
            }
            set
            {
                EnsureChildControls();
                _cePostHtml.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get
            {
                EnsureChildControls();
                return _cbIsActive.Checked;
            }
            set
            {
                EnsureChildControls();
                _cbIsActive.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the field type id.
        /// </summary>
        /// <value>
        /// The field type id.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use AttributeFieldTypeId or SetAttributeFieldType instead" )]
        public int? FieldTypeId
        {
            get
            {
                return this.AttributeFieldTypeId;
            }

            set
            {
                var dummyQualifiers = FieldTypeCache.Get( value ?? 1 ).Field.ConfigurationKeys().ToDictionary( k => k, v => new ConfigurationValue() );
                SetAttributeFieldType( value ?? 1, dummyQualifiers );
            }
        }

        /// <summary>
        /// Gets the FieldTypeId of the Attribute
        /// </summary>
        /// <value>
        /// The attribute field typeid.
        /// </value>
        public int AttributeFieldTypeId
        {
            get
            {
                EnsureChildControls();
                return _ddlFieldType.SelectedValueAsInt() ?? FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT.AsGuid() )?.Id ?? 1;
            }
        }

        /// <summary>
        /// Gets or sets the qualifiers.
        /// </summary>
        /// <value>
        /// The qualifiers.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use AttributeQualifiers or SetAttributeFieldType instead" )]
        public Dictionary<string, ConfigurationValue> Qualifiers
        {
            get
            {
                return this.AttributeQualifiers;
            }

            set
            {
                SetAttributeFieldType( this.AttributeFieldTypeId, value );
            }
        }

        /// <summary>
        /// Gets the attribute qualifiers.
        /// </summary>
        /// <value>
        /// The attribute qualifiers.
        /// </value>
        public Dictionary<string, ConfigurationValue> AttributeQualifiers
        {
            get
            {
                EnsureChildControls();

                var field = FieldTypeCache.Get( AttributeFieldTypeId )?.Field;
                return field?.ConfigurationValues( _phQualifiers.Controls.OfType<Control>().ToList() ) ?? new Dictionary<string, ConfigurationValue>();
            }
        }

        /// <summary>
        /// Sets the FieldType and Qualifiers 
        /// </summary>
        /// <param name="fieldTypeId">The field type identifier.</param>
        /// <param name="qualifiers">The qualifiers.</param>
        public void SetAttributeFieldType( int fieldTypeId, Dictionary<string, ConfigurationValue> qualifiers )
        {
            EnsureChildControls();

            _ddlFieldType.SetValue( fieldTypeId );
            _hfReadOnlyFieldTypeId.Value = fieldTypeId.ToString();
            _lFieldType.Text = FieldTypeCache.Get( fieldTypeId )?.Name;

            _phDefaultValue.Controls.Clear();
            _phQualifiers.Controls.Clear();
            CreateFieldTypeQualifierControls( fieldTypeId );
            CreateFieldTypeDefaultControl( fieldTypeId, qualifiers );

            var field = FieldTypeCache.Get( this.AttributeFieldTypeId )?.Field;
            field?.SetConfigurationValues( _phQualifiers.Controls.OfType<Control>().ToList(), qualifiers );
            qualifiers = field.ConfigurationValues( _phQualifiers.Controls.OfType<Control>().ToList() );

            this.FieldTypeIdState = this.AttributeFieldTypeId;
            this.FieldTypeQualifierStateJSON = this.AttributeQualifiers.ToJson();
        }

        /// <summary>
        /// Gets or sets the ViewState of the FieldTypeId
        /// </summary>
        /// <value>
        /// The state of the field type qualifier.
        /// </value>
        private int? FieldTypeIdState
        {
            get
            {
                return ViewState["FieldTypeIdState"] as int?;
            }

            set
            {
                ViewState["FieldTypeIdState"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the ViewState of the field type qualifiers 
        /// </summary>
        /// <value>
        /// The state of the field type qualifier.
        /// </value>
        private string FieldTypeQualifierStateJSON
        {
            get
            {
                return ( ViewState["FieldTypeQualifierStateJSON"] as string );
            }

            set
            {
                ViewState["FieldTypeQualifierStateJSON"] = value;
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
                EnsureChildControls();
                var defaultValue = string.Empty;

                var field = FieldTypeCache.Get( this.AttributeFieldTypeId ).Field;
                var defaultControl = _phDefaultValue.Controls.OfType<Control>().FirstOrDefault();
                if ( defaultControl != null )
                {
                    defaultValue = field.GetEditValue( defaultControl, this.AttributeQualifiers );
                }


                return defaultValue;
            }
            set
            {
                EnsureChildControls();

                var field = FieldTypeCache.Get( this.AttributeFieldTypeId ).Field;
                var defaultControl = _phDefaultValue.Controls.OfType<Control>().FirstOrDefault();
                if ( defaultControl != null )
                {
                    field.SetEditValue( defaultControl, this.AttributeQualifiers, value );
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
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            this.FieldTypeIdState = this.AttributeFieldTypeId;
            this.FieldTypeQualifierStateJSON = this.AttributeQualifiers.ToJson();

            return base.SaveViewState();
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            // Get the FieldType that was selected in the postback 
            // This will either come from ddlFieldType of hfFieldTypeId depending if the FieldType is editable
            int? postBackFieldTypeId = this.Page.Request[_ddlFieldType.UniqueID].AsIntegerOrNull() ?? this.Page.Request[_hfReadOnlyFieldTypeId.UniqueID].AsIntegerOrNull();
            int? fieldTypeIdState = ViewState["FieldTypeIdState"] as int?;

            // NOTE: if the FieldTypeId has changed, we don't want to create the field controls prior to OnLoad since we'll be recreating them after OnLoad (and we don't want to use the postback values from the other fieldtype)
            if ( fieldTypeIdState.HasValue && postBackFieldTypeId == fieldTypeIdState.Value )
            {
                // if the FieldType hasn't changed, we need to recreate the FieldType Qualifier controls so they can receive their postback values (which won't happen until OnLoad)
                CreateFieldTypeQualifierControls( fieldTypeIdState.Value );

                // We won't know if the qualifiers change until after OnLoad, so use the Qualifiers from ViewState
                // so that we know how the fieldType default control was originally created
                Dictionary<string, ConfigurationValue> qualifiersState = ( ViewState["FieldTypeQualifierStateJSON"] as string ).FromJsonOrNull<Dictionary<string, ConfigurationValue>>();
                CreateFieldTypeDefaultControl( fieldTypeIdState.Value, qualifiersState );
            }
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
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
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
            _validationSummary.CssClass = "alert alert-validation";
            _validationSummary.HeaderText = "Please correct the following:";
            Controls.Add( _validationSummary );

            _tbName = new RockTextBox();
            _tbName.ID = "tbName";
            _tbName.Label = NameFieldLabel;
            _tbName.Required = true;
            Controls.Add( _tbName );

            _tbAbbreviatedName = new RockTextBox();
            _tbAbbreviatedName.ID = "tbAbbreviatedName";
            _tbAbbreviatedName.Label = AbbreviatedNameLabel;
            _tbAbbreviatedName.Required = false;
            Controls.Add( _tbAbbreviatedName );

            _cbIsActive = new RockCheckBox();
            _cbIsActive.ID = "_cbIsActive";
            _cbIsActive.Label = "Active";
            _cbIsActive.Text = "Yes";
            _cbIsActive.Help = "Set to Inactive to exclude this attribute from Edit and Display UIs";
            Controls.Add( _cbIsActive );

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
            _cpCategories.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Attribute ) ).Id;
            _cpCategories.EntityTypeQualifierColumn = "EntityTypeId";
            Controls.Add( _cpCategories );

            _lKey = new RockLiteral();
            _lKey.Label = "Key";
            _lKey.ID = "lKey";
            _lKey.Visible = false;
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

            _lFieldType = new RockLiteral();
            _lFieldType.Label = "Field Type";
            _lFieldType.ID = "_lFieldType";
            _lFieldType.Visible = false;
            Controls.Add( _lFieldType );

            _hfReadOnlyFieldTypeId = new HiddenField();
            _hfReadOnlyFieldTypeId.ID = "_hfReadOnlyFieldTypeId";
            Controls.Add( _hfReadOnlyFieldTypeId );

            _ddlFieldType = new RockDropDownList();
            _ddlFieldType.ID = "ddlFieldType";
            _ddlFieldType.Label = "Field Type";
            _ddlFieldType.AutoPostBack = true;
            _ddlFieldType.SelectedIndexChanged += _ddlFieldType_SelectedIndexChanged;
            _ddlFieldType.DataValueField = "Id";
            _ddlFieldType.DataTextField = "Name";
            _ddlFieldType.EnhanceForLongLists = true;
            Controls.Add( _ddlFieldType );

            _phQualifiers = new DynamicPlaceholder();
            _phQualifiers.ID = "phQualifiers";
            Controls.Add( _phQualifiers );

            _phDefaultValue = new DynamicPlaceholder();
            _phDefaultValue.ID = "phDefaultValue";
            Controls.Add( _phDefaultValue );

            _pwAdvanced = new PanelWidget();
            _pwAdvanced.ID = "pwAdvanced";
            _pwAdvanced.Title = "Advanced Settings";
            
            var pnlAdvancedTopRow = new Panel { CssClass = "row" };
            _pwAdvanced.Controls.Add( pnlAdvancedTopRow );
            var pnlAdvancedTopRowCol1 = new Panel { CssClass = "col-md-6" };
            pnlAdvancedTopRow.Controls.Add( pnlAdvancedTopRowCol1 );
            var pnlAdvancedTopRowCol2 = new Panel { CssClass = "col-md-6" };
            pnlAdvancedTopRow.Controls.Add( pnlAdvancedTopRowCol2 );
            Controls.Add( _pwAdvanced );

            _tbIconCssClass = new RockTextBox();
            _tbIconCssClass.ID = "_tbIconCssClass";
            _tbIconCssClass.Label = "Icon CSS Class";
            pnlAdvancedTopRowCol1.Controls.Add( _tbIconCssClass );

            _cbEnableHistory = new RockCheckBox();
            _cbEnableHistory.ID = "_cbEnableHistory";
            _cbEnableHistory.Label = "Enable History";
            _cbEnableHistory.Text = "Yes";
            _cbEnableHistory.Help = "If selected, changes to the value of this attribute will be stored in attribute value history";
            pnlAdvancedTopRowCol1.Controls.Add( _cbEnableHistory );

            _cbAllowSearch = new RockCheckBox();
            _cbAllowSearch.ID = "cbAllowSearch";
            _cbAllowSearch.Label = "Allow Search";
            _cbAllowSearch.Text = "Yes";
            _cbAllowSearch.Help = "If selected, this attribute can be searched on.";
            _cbAllowSearch.Visible = false;  // Default is to not show this option
            pnlAdvancedTopRowCol1.Controls.Add( _cbAllowSearch );

            _cbIsIndexingEnabled = new RockCheckBox();
            _cbIsIndexingEnabled.ID = "cbAllowIndexing";
            _cbIsIndexingEnabled.Label = "Indexing Enabled";
            _cbIsIndexingEnabled.Text = "Yes";
            _cbIsIndexingEnabled.Help = "If selected, this attribute can be used when indexing for universal search.";
            _cbIsIndexingEnabled.Visible = false;  // Default is to not show this option
            pnlAdvancedTopRowCol1.Controls.Add( _cbIsIndexingEnabled );

            _cbIsAnalytic = new RockCheckBox();
            _cbIsAnalytic.ID = "_cbIsAnalytic";
            _cbIsAnalytic.Label = "Analytics Enabled";
            _cbIsAnalytic.Text = "Yes";
            _cbIsAnalytic.Help = "If selected, this attribute will be made available as an Analytic";
            _cbIsAnalytic.Visible = false;  // Default is to not show this option
            pnlAdvancedTopRowCol2.Controls.Add( _cbIsAnalytic );

            _cbIsAnalyticHistory = new RockCheckBox();
            _cbIsAnalyticHistory.ID = "_cbIsAnalyticHistory";
            _cbIsAnalyticHistory.Label = "Analytics History Enabled";
            _cbIsAnalyticHistory.Text = "Yes";
            _cbIsAnalyticHistory.Help = "If selected, changes to the value of this attribute will cause Analytics to create a history record. Note that this requires that 'Analytics Enabled' is also enabled.";
            _cbIsAnalyticHistory.Visible = false;  // Default is to not show this option
            pnlAdvancedTopRowCol2.Controls.Add( _cbIsAnalyticHistory );

            var pnlAdvancedPrePostHTMLRow = new Panel { CssClass = "row" };
            _pwAdvanced.Controls.Add( pnlAdvancedPrePostHTMLRow );
            var pnlAdvancedPrePostHTMLCol = new Panel { CssClass = "col-md-12" };
            pnlAdvancedPrePostHTMLRow.Controls.Add( pnlAdvancedPrePostHTMLCol );

            _cePreHtml = new CodeEditor();
            _cePreHtml.ID = "_cePreHtml";
            _cePreHtml.Label = "Pre-HTML";
            _cePreHtml.Help = "HTML that should be rendered before the attribute's edit control";
            _cePreHtml.EditorMode = CodeEditorMode.Html;
            pnlAdvancedPrePostHTMLCol.Controls.Add( _cePreHtml );

            _cePostHtml = new CodeEditor();
            _cePostHtml.ID = "_cePostHtml";
            _cePostHtml.Label = "Post-HTML";
            _cePostHtml.Help = "HTML that should be rendered after the attribute's edit control";
            _cePostHtml.EditorMode = CodeEditorMode.Html;
            pnlAdvancedPrePostHTMLCol.Controls.Add( _cePostHtml );

            _btnSave = new LinkButton();
            _btnSave.ID = "btnSave";
            _btnSave.Text = "OK";
            _btnSave.CssClass = "btn btn-primary";
            _btnSave.Click += btnSave_Click;
            Controls.Add( _btnSave );

            _btnCancel = new LinkButton();
            _btnCancel.ID = "btnCancel";
            _btnCancel.Text = "Cancel";
            _btnCancel.CssClass = "btn btn-default";
            _btnCancel.CausesValidation = false;
            _btnCancel.Click += btnCancel_Click;
            Controls.Add( _btnCancel );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( this.Page.IsPostBack )
            {
                // if the fieldType and/or qualifiers have changed, then we need to recreate the default control
                if ( this.FieldTypeIdState != this.AttributeFieldTypeId || this.FieldTypeQualifierStateJSON != this.AttributeQualifiers.ToJson() )
                {
                    string defaultValue = null;
                    if ( _phDefaultValue.Controls.Count == 1 )
                    {
                        defaultValue = this.DefaultValue;
                        _phDefaultValue.Controls.RemoveAt( 0 );
                    }

                    CreateFieldTypeDefaultControl( this.AttributeFieldTypeId, this.AttributeQualifiers );

                    if ( defaultValue != null )
                    {
                        // if we had to recreate control, try to restore the default value that it had with the previous instance of the default control
                        this.DefaultValue = defaultValue;
                    }
                }
            }

            _cbIsAnalytic.Visible = false;
            _cbIsAnalyticHistory.Visible = false;

            // Only show certain options depending on the EntityType
            if ( this.AttributeEntityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( this.AttributeEntityTypeId.Value );
                if ( entityType != null )
                {
                    _cbIsAnalytic.Visible = entityType.IsAnalyticAttributesSupported( this.AttributeEntityTypeQualifierColumn, this.AttributeEntityTypeQualifierValue );
                    _cbIsAnalyticHistory.Visible = entityType.IsAnalyticsHistoricalSupported( this.AttributeEntityTypeQualifierColumn, this.AttributeEntityTypeQualifierValue );
                    _cePreHtml.Visible = entityType.AttributesSupportPrePostHtml;
                    _cePostHtml.Visible = entityType.AttributesSupportPrePostHtml;
                }
            }

            // Set the validation group for all controls
            string validationGroup = ValidationGroup;
            _validationSummary.ValidationGroup = validationGroup;
            _tbName.ValidationGroup = validationGroup;
            _tbAbbreviatedName.ValidationGroup = validationGroup;
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

            foreach ( var control in _pwAdvanced.ControlsOfTypeRecursive<Control>() )
            {
                if ( control is IRockControl )
                {
                    ( ( IRockControl ) control ).ValidationGroup = validationGroup;
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

            if ( ShowActionTitle )
            {
                writer.RenderBeginTag( HtmlTextWriterTag.Legend );
                _lAttributeActionTitle.RenderControl( writer );
                writer.RenderEndTag();
            }

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
            _cbIsActive.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            // row 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbAbbreviatedName.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            writer.RenderEndTag(); // empty column

            writer.RenderEndTag();  // row

            // row 3
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-12" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _tbDescription.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();  // row

            // row 4
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            // row 4 col 1
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cpCategories.RenderControl( writer );
            _lKey.RenderControl( writer );
            _tbKey.RenderControl( writer );
            _cvKey.RenderControl( writer );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbRequired.RenderControl( writer );
            writer.RenderEndTag();

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-sm-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _cbShowInGrid.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            writer.RenderEndTag();

            // row 4 col 2
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _hfReadOnlyFieldTypeId.RenderControl( writer );
            _ddlFieldType.RenderControl( writer );
            _lFieldType.RenderControl( writer );
            _phQualifiers.RenderControl( writer );
            _phDefaultValue.RenderControl( writer );
            writer.RenderEndTag();

            writer.RenderEndTag();

            _pwAdvanced.RenderControl( writer );

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
        protected void _ddlFieldType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // When someone changes the field type, we clear the default value since it's no longer relevant.
            DefaultValue = string.Empty;
            SetAttributeFieldType( _ddlFieldType.SelectedValueAsId() ?? 1, new Dictionary<string, ConfigurationValue>() );
        }

        /// <summary>
        /// Rebuilds the field type controls call this if FieldType or Qualifiers change
        /// </summary>
        private void RebuildFieldTypeControls( int fieldTypeId, Dictionary<string, ConfigurationValue> qualifiers = null )
        {
            _phQualifiers.Controls.Clear();
            _phDefaultValue.Controls.Clear();
            CreateFieldTypeQualifierControls( fieldTypeId );
            CreateFieldTypeDefaultControl( fieldTypeId, qualifiers ?? this.AttributeQualifiers );
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
                this.IsSystem = attribute.IsSystem;
                this.AttributeId = attribute.Id;
                this.AttributeGuid = attribute.Guid;
                this.AttributeEntityTypeQualifierColumn = attribute.EntityTypeQualifierColumn;
                this.AttributeEntityTypeQualifierValue = attribute.EntityTypeQualifierValue;
                this.Name = attribute.Name;
                this.Key = attribute.Key;
                this.IconCssClass = attribute.IconCssClass;
                this.CategoryIds = attribute.Categories.Select( c => c.Id ).ToList();
                this.Description = attribute.Description;
                this.Required = attribute.IsRequired;
                this.ShowInGrid = attribute.IsGridColumn;
                this.AllowSearch = attribute.AllowSearch;
                this.IsIndexingEnabled = attribute.IsIndexEnabled;
                this.IsAnalytic = attribute.IsAnalytic;
                this.IsAnalyticHistory = attribute.IsAnalyticHistory;
                this.IsActive = attribute.IsActive;
                this.EnableHistory = attribute.EnableHistory;
                this.PreHtml = attribute.PreHtml;
                this.PostHtml = attribute.PostHtml;
                this.AbbreviatedName = attribute.AbbreviatedName;

                // only allow the fieldtype to be set if this a new attribute
                this.IsFieldTypeEditable = attribute.Id == 0 || attribute.FieldTypeId == 0;

                var qualifiers = new Dictionary<string, ConfigurationValue>();
                
                var field = FieldTypeCache.Get( attribute.FieldTypeId )?.Field;
                if ( field != null )
                {
                    // initialize qualifiers from field's Configuration Keys in case the Attribute doesn't have AttributeQualifiers for all the config keys yet
                    qualifiers = field.ConfigurationKeys().ToDictionary( k => k, v => new ConfigurationValue() );
                }

                if ( attribute.AttributeQualifiers != null )
                {
                    // Update the Qualifiers with the values from AttributeQualifiers
                    foreach ( Rock.Model.AttributeQualifier qualifier in attribute.AttributeQualifiers )
                    {
                        qualifiers.AddOrReplace( qualifier.Key, new ConfigurationValue( qualifier.Value ) );
                    }
                }

                this.SetAttributeFieldType( attribute.FieldTypeId, qualifiers );

                this.DefaultValue = attribute.DefaultValue;

                SetSubTitleOnModal( attribute );
            }

            if ( objectType != null )
            {
                this.AttributeEntityTypeId = EntityTypeCache.Get( objectType ).Id;

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
                attribute.FieldTypeId = this.AttributeFieldTypeId;
                attribute.IsMultiValue = false;
                attribute.IsRequired = this.Required;
                attribute.IsGridColumn = this.ShowInGrid;
                attribute.AllowSearch = this.AllowSearch;
                attribute.IsIndexEnabled = this.IsIndexingEnabled;
                attribute.IsAnalytic = this.IsAnalytic;
                attribute.IsAnalyticHistory = this.IsAnalyticHistory;
                attribute.IsActive = this.IsActive;
                attribute.EnableHistory = this.EnableHistory;
                attribute.PreHtml = this.PreHtml;
                attribute.PostHtml = this.PostHtml;
                attribute.AbbreviatedName = this.AbbreviatedName;

                attribute.Categories.Clear();
                new CategoryService( new RockContext() ).Queryable().Where( c => this.CategoryIds.Contains( c.Id ) ).ToList().ForEach( c =>
                    attribute.Categories.Add( c ) );

                // Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime just in case Categories changed
                attribute.ModifiedDateTime = RockDateTime.Now;

                attribute.AttributeQualifiers.Clear();
                foreach ( var qualifier in AttributeQualifiers )
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
        /// Creates the field type qualifier controls.
        /// </summary>
        /// <param name="fieldTypeId">The field type identifier.</param>
        protected void CreateFieldTypeQualifierControls( int fieldTypeId )
        {
            EnsureChildControls();

            var field = FieldTypeCache.Get( fieldTypeId ).Field;

            var configControls = field.ConfigurationControls();
            int i = 0;
            foreach ( var control in configControls )
            {
                // make sure each qualifier control has a unique/predictable ID to help avoid viewstate issues
                var controlTypeName = control.GetType().Name;
                var oldControlId = control.ID ?? string.Empty;
                control.ID = $"qualifier_{fieldTypeId}_{controlTypeName}_{i++}";

                // if this is a RockControl with a required field validator, make sure RequiredFieldValidator.ControlToValidate gets updated with the new control id
                if ( control is IRockControl rockControl )
                {
                    if ( rockControl.RequiredFieldValidator != null)
                    {
                        if ( rockControl.RequiredFieldValidator.ControlToValidate == oldControlId )
                        {
                            rockControl.RequiredFieldValidator.ControlToValidate = control.ID;
                        }
                    }
                }
                _phQualifiers.Controls.Add( control );
            }
        }

        /// <summary>
        /// Creates the field type detail controls.
        /// </summary>
        /// <param name="fieldTypeId">The field type id.</param>
        /// <param name="qualifiers">The qualifiers or null to get it from controls</param>
        protected void CreateFieldTypeDefaultControl( int fieldTypeId, Dictionary<string, ConfigurationValue> qualifiers )
        {
            EnsureChildControls();

            var field = FieldTypeCache.Get( fieldTypeId ).Field;

            // default control id needs to be unique to field type because some field types will transform
            // field (i.e. htmleditor) and switching field types will not reset that
            if ( field.HasDefaultControl )
            {
                if ( qualifiers == null )
                {
                    qualifiers = this.AttributeQualifiers;
                }

                // make sure each default control has a unique/predictable ID to help avoid viewstate issues
                var defaultControl = field.EditControl( qualifiers, $"defaultValue_{fieldTypeId}_{this.AttributeGuid.ToString("N")}" );
                if ( defaultControl != null )
                {
                    _phDefaultValue.Controls.Add( defaultControl );

                    if ( defaultControl is IRockControl )
                    {
                        var rockControl = defaultControl as IRockControl;
                        rockControl.Required = false;
                        rockControl.Label = "Default Value";
                    }
                }
            }
        }

        /// <summary>
        /// Set the Subtitle of modal dialog
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        protected void SetSubTitleOnModal( Model.Attribute attribute )
        {
            ModalDialog modalDialog = this.FirstParentControlOfType<ModalDialog>();
            if ( modalDialog != null && ( string.IsNullOrEmpty( modalDialog.SubTitle ) || modalDialog.SubTitle.StartsWith( "Id: " ) ) )
            {
                modalDialog.SubTitle = string.Format( "Id: {0}", attribute.Id );
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
        var $literalKeyControl = $('#' + literalKeyControlId);
        var $keyControl = $('#' + keyControlId);
        var keyValue = $keyControl.val();

        var reservedKeyJson = $keyControl.closest('fieldset').find('.js-existing-key-names').val();
        var reservedKeyNames = eval('(' + reservedKeyJson + ')');

        if ($keyControl.length && (keyValue == '')) {

            keyValue = $('#' + nameControlId).val().replace(/[^a-zA-Z0-9_.\-]/g, '');
            var newKeyValue = keyValue;
        
            var i = 1;
            while ($.inArray(newKeyValue, reservedKeyNames) >= 0) {
                newKeyValue = keyValue + i++;
            }
            
            $keyControl.val(newKeyValue);
            $literalKeyControl.html(newKeyValue);
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