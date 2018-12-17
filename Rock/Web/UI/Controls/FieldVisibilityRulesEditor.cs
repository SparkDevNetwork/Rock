﻿// <copyright>
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Field;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Editor for <see cref="FieldVisibilityRules"/>
    /// </summary>
    public class FieldVisibilityRulesEditor : CompositeControl, IHasValidationGroup, INamingContainer
    {
        #region Controls

        private Panel _pnlFilterType;
        private RockDropDownList _ddlFilterShowHide;
        private Label _lblFieldName;
        private Label _lblIfSpan;
        private RockDropDownList _ddlFilterAllAny;
        private Label _lblOfTheFollowingMatchSpan;

        private Panel _pnlFilterActions;
        private LinkButton _btnAddFilterFieldCriteria;

        private NotificationBox _nbNoFieldsAvailable;

        private DynamicPlaceholder _phFilterFieldRuleControls;

        #endregion Controls

        #region Private fields

        // Keeps track of FieldVisibilityRules that we created filter rule controls for, so we can re-create them on postback
        private FieldVisibilityRules _fieldVisibilityRulesState = new FieldVisibilityRules();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the validation group. (Default is RockBlock's BlockValidationGroup)
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get => ViewState["ValidationGroup"] as string ?? this.RockBlock()?.BlockValidationGroup;
            set => ViewState["ValidationGroup"] = value;
        }

        /// <summary>
        /// Gets or sets the fields that will be available to compare to
        /// NOTE: Use Rock.Model.Attribute instead of AttributeCache since we might be using Attributes that haven't been saved to the database yet
        /// </summary>
        /// <value>
        /// The comparable attributes ids.
        /// </value>
        public Dictionary<Guid, Rock.Model.Attribute> ComparableAttributes { get; set; }

        /// <summary>
        /// The Name that should be displayed for the Field that the rules are for
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName
        {
            get
            {
                EnsureChildControls();
                return _lblFieldName.Text;
            }

            set
            {
                EnsureChildControls();
                _lblFieldName.Text = value;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            Panel pnlContainer = new Panel { CssClass = "filtervisibilityrules-container" };
            this.Controls.Add( pnlContainer );

            _nbNoFieldsAvailable = new NotificationBox
            {
                ID = this.ID + "_nbNoFieldsAvailable",
                NotificationBoxType = NotificationBoxType.Warning,
                Text = "At least one other supported attribute field is required to add criteria to this field.",
                Visible = false
            };

            pnlContainer.Controls.Add( _nbNoFieldsAvailable );

            Panel pnlRulesHeaderRow = new Panel { CssClass = "filtervisibilityrules-rulesheader row" };
            pnlContainer.Controls.Add( pnlRulesHeaderRow );
            Panel pnlRulesList = new Panel { CssClass = "filtervisibilityrules-ruleslist " };
            pnlContainer.Controls.Add( pnlRulesList );

            // Filter Type controls
            _pnlFilterType = new Panel { CssClass = "filtervisibilityrules-type col-md-10" };
            pnlRulesHeaderRow.Controls.Add( _pnlFilterType );

            _ddlFilterShowHide = new RockDropDownList();
            _ddlFilterShowHide.CssClass = "input-width-sm pull-left";
            _ddlFilterShowHide.ID = this.ID + "_ddlFilterShowHide";
            _ddlFilterShowHide.Items.Add( new ListItem( "Show", "Show" ) );
            _ddlFilterShowHide.Items.Add( new ListItem( "Hide", "Hide" ) );
            _pnlFilterType.Controls.Add( _ddlFilterShowHide );

            Panel pnlFieldNameIf = new Panel { CssClass = "pull-left margin-all-sm" };
            _pnlFilterType.Controls.Add( pnlFieldNameIf );
            _lblFieldName = new Label { CssClass = "filtervisibilityrules-fieldname" };
            pnlFieldNameIf.Controls.Add( _lblFieldName );
            _lblIfSpan = new Label { Text = "if", CssClass = "filtervisibilityrules-if margin-l-sm" };
            pnlFieldNameIf.Controls.Add( _lblIfSpan );

            _ddlFilterAllAny = new RockDropDownList();
            _ddlFilterAllAny.CssClass = "input-width-sm pull-left";
            _ddlFilterAllAny.ID = this.ID + "_ddlFilterAllAny";
            _ddlFilterAllAny.Items.Add( new ListItem( "All", "All" ) );
            _ddlFilterAllAny.Items.Add( new ListItem( "Any", "Any" ) );
            _pnlFilterType.Controls.Add( _ddlFilterAllAny );

            _lblOfTheFollowingMatchSpan = new Label { Text = "of the following match:", CssClass = "pull-left margin-all-sm" };
            _pnlFilterType.Controls.Add( _lblOfTheFollowingMatchSpan );

            // Filter Actions
            _pnlFilterActions = new Panel { CssClass = "filter-actions col-md-2" };
            pnlRulesHeaderRow.Controls.Add( _pnlFilterActions );

            _btnAddFilterFieldCriteria = new LinkButton();
            _btnAddFilterFieldCriteria.ID = this.ID + "_btnAddFilterFieldCriteria";
            _btnAddFilterFieldCriteria.CssClass = "btn btn-xs btn-action add-action pull-right margin-all-sm";
            _btnAddFilterFieldCriteria.Text = "<i class='fa fa-filter'></i> Add Criteria";
            _btnAddFilterFieldCriteria.Click += _btnAddFilterFieldCriteria_Click;
            _pnlFilterActions.Controls.Add( _btnAddFilterFieldCriteria );

            _phFilterFieldRuleControls = new DynamicPlaceholder();
            _phFilterFieldRuleControls.ID = this.ID + "_phFilterFieldRuleControls";
            pnlRulesList.Controls.Add( _phFilterFieldRuleControls );
        }

        /// <summary>
        /// Handles the Click event of the _btnAddFilterFieldCriteria control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _btnAddFilterFieldCriteria_Click( object sender, EventArgs e )
        {
            AddFilterRule( new Rock.Field.FieldVisibilityRule() );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            // show a warning if there aren't any fields to choose from
            _nbNoFieldsAvailable.Visible = !GetSupportedComparableAttributes().Any();

            ScriptManager.RegisterClientScriptInclude( this, this.GetType(), "reporting-include", this.RockBlock().RockPage.ResolveRockUrl( "~/Scripts/Rock/reportingInclude.js", true ) );
            base.RenderControl( writer );
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            this._fieldVisibilityRulesState = ( ViewState["_fieldVisibilityRulesStateJSON"] as string ).FromJsonOrNull<FieldVisibilityRules>();
            this.ComparableAttributes = ( ViewState["ComparableAttributesJSON"] as string ).FromJsonOrNull<Dictionary<Guid, Rock.Model.Attribute>>();

            EnsureChildControls();
            _phFilterFieldRuleControls.Controls.Clear();

            if ( _fieldVisibilityRulesState?.Any() == true )
            {
                foreach ( var fieldVisibilityRule in _fieldVisibilityRulesState )
                {
                    this.AddFilterRuleControl( fieldVisibilityRule, false );
                }
            }
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["_fieldVisibilityRulesStateJSON"] = this._fieldVisibilityRulesState.ToJson();
            ViewState["ComparableAttributesJSON"] = this.ComparableAttributes.ToJson();

            return base.SaveViewState();
        }

        #endregion

        #region Private Methods

        #endregion Private Methods

        #region Methods

        /// <summary>
        /// Sets the field visibility rules.
        /// </summary>
        /// <param name="fieldVisibilityRules">The field visibility rules.</param>
        public void SetFieldVisibilityRules( FieldVisibilityRules fieldVisibilityRules )
        {
            EnsureChildControls();

            switch ( fieldVisibilityRules.FilterExpressionType )
            {
                case FilterExpressionType.GroupAllFalse:
                    {
                        _ddlFilterShowHide.SetValue( "Hide" );
                        _ddlFilterAllAny.SetValue( "All" );
                        break;
                    }

                case FilterExpressionType.GroupAny:
                    {
                        _ddlFilterShowHide.SetValue( "Show" );
                        _ddlFilterAllAny.SetValue( "Any" );
                        break;
                    }

                case FilterExpressionType.GroupAnyFalse:
                    {
                        _ddlFilterShowHide.SetValue( "Hide" );
                        _ddlFilterAllAny.SetValue( "Any" );
                        break;
                    }

                default:
                    {
                        _ddlFilterShowHide.SetValue( "Show" );
                        _ddlFilterAllAny.SetValue( "All" );
                        break;
                    }
            }

            this._fieldVisibilityRulesState = new FieldVisibilityRules();
            _phFilterFieldRuleControls.Controls.Clear();
            foreach ( var fieldVisibilityRule in fieldVisibilityRules )
            {
                AddFilterRule( fieldVisibilityRule );
            }
        }

        /// <summary>
        /// Gets the field visibility rules.
        /// </summary>
        /// <returns></returns>
        public FieldVisibilityRules GetFieldVisibilityRules()
        {
            // get a new copy of the rules then get the filter rule settings from the controls
            var fieldVisibilityRules = _fieldVisibilityRulesState.Clone();

            if ( _ddlFilterShowHide.SelectedValue.Equals( "Show", StringComparison.OrdinalIgnoreCase ) )
            {
                if ( _ddlFilterAllAny.SelectedValue.Equals( "any", StringComparison.OrdinalIgnoreCase ) )
                {
                    fieldVisibilityRules.FilterExpressionType = FilterExpressionType.GroupAny;
                }
                else
                {
                    fieldVisibilityRules.FilterExpressionType = FilterExpressionType.GroupAll;
                }
            }
            else
            {
                if ( _ddlFilterAllAny.SelectedValue.Equals( "any", StringComparison.OrdinalIgnoreCase ) )
                {
                    fieldVisibilityRules.FilterExpressionType = FilterExpressionType.GroupAnyFalse;
                }
                else
                {
                    fieldVisibilityRules.FilterExpressionType = FilterExpressionType.GroupAllFalse;
                }
            }

            foreach ( var fieldVisibilityRule in fieldVisibilityRules.ToList() )
            {
                var rockControlWrapper = _phFilterFieldRuleControls.FindControl( $"_rockControlWrapper_{fieldVisibilityRule.Guid.ToString( "N" )}" ) as RockControlWrapper;
                if ( rockControlWrapper == null )
                {
                    continue;
                }

                var ddlCompareField = rockControlWrapper.FindControl( $"_ddlCompareField_{fieldVisibilityRule.Guid.ToString( "N" )}" ) as RockDropDownList;
                var selectedAttribute = this.ComparableAttributes.GetValueOrNull( ddlCompareField.SelectedValue.AsGuid() );
                if ( selectedAttribute != null )
                {
                    fieldVisibilityRule.ComparedToAttributeGuid = selectedAttribute?.Guid;
                    var fieldType = FieldTypeCache.Get( selectedAttribute.FieldTypeId );
                    var filterControl = rockControlWrapper.FindControl( $"_filterControl_{fieldVisibilityRule.Guid.ToString( "N" )}" );
                    var qualifiers = selectedAttribute.AttributeQualifiers.ToDictionary( k => k.Key, v => new ConfigurationValue( v.Value ) );
                    var filterValues = fieldType.Field.GetFilterValues( filterControl, qualifiers, Reporting.FilterMode.AdvancedFilter );

                    // NOTE: If filterValues.Count >= 2, then filterValues[0] is ComparisonType, and filterValues[1] is a CompareToValue. Otherwise, filterValues[0] is a CompareToValue (for example, a SingleSelect attribute)
                    if ( filterValues.Count >= 2 )
                    {
                        fieldVisibilityRule.ComparisonType = filterValues[0].ConvertToEnumOrNull<ComparisonType>() ?? ComparisonType.EqualTo;
                        fieldVisibilityRule.ComparedToValue = filterValues[1];
                    }
                    else if ( filterValues.Count == 1 )
                    {
                        fieldVisibilityRule.ComparedToValue = filterValues[0];
                    }
                }
                else
                {
                    // no attribute selected, so delete the rule
                    fieldVisibilityRules.Remove( fieldVisibilityRule );
                }
            }

            return fieldVisibilityRules;
        }

        /// <summary>
        /// Adds the filter rule.
        /// </summary>
        /// <param name="fieldVisibilityRule">The field visibility rule.</param>
        public void AddFilterRule( FieldVisibilityRule fieldVisibilityRule )
        {
            AddFilterRuleControl( fieldVisibilityRule, true );

            this._fieldVisibilityRulesState.Add( fieldVisibilityRule );
        }

        /// <summary>
        /// Adds the filter control.
        /// </summary>
        /// <param name="fieldVisibilityRule">The field visibility rule.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void AddFilterRuleControl( FieldVisibilityRule fieldVisibilityRule, bool setValues )
        {
            _phFilterFieldRuleControls.Controls.Add( new Literal { Text = "<hr>" } );

            RockControlWrapper rockControlWrapper = new RockControlWrapper
            {
                ID = $"_rockControlWrapper_{fieldVisibilityRule.Guid.ToString( "N" )}",
                Help = "Pick a field to use a filter condition. Note that some fields might not be listed if they don't support filtering or if they don't support change notifications."
            };

            _phFilterFieldRuleControls.Controls.Add( rockControlWrapper );

            Panel pnlFilterRuleRow = new Panel { CssClass = "filter-rule row" };
            rockControlWrapper.Controls.Add( pnlFilterRuleRow );

            Panel pnlFilterRuleCol1 = new Panel { CssClass = "col-md-10" };
            Panel pnlFilterRuleCol2 = new Panel { CssClass = "col-md-2" };
            pnlFilterRuleRow.Controls.Add( pnlFilterRuleCol1 );
            pnlFilterRuleRow.Controls.Add( pnlFilterRuleCol2 );

            Panel pnlFilterRuleCompareField = new Panel { CssClass = "pull-left margin-r-md filter-rule-comparefield" };
            Panel pnlFilterRuleFieldFilter = new Panel { CssClass = "filter-rule-fieldfilter" };

            pnlFilterRuleCol1.Controls.Add( pnlFilterRuleCompareField );
            pnlFilterRuleCol1.Controls.Add( pnlFilterRuleFieldFilter );

            HiddenFieldWithClass hiddenFieldRuleGuid = new HiddenFieldWithClass
            {
                ID = $"_hiddenFieldRuleGuid_{fieldVisibilityRule.Guid.ToString( "N" )}",
                CssClass = "js-rule-guid"
            };

            hiddenFieldRuleGuid.Value = fieldVisibilityRule.Guid.ToString();

            rockControlWrapper.Controls.Add( hiddenFieldRuleGuid );

            LinkButton btnDeleteRule = new LinkButton
            {
                ID = $"_btnDeleteRule_{fieldVisibilityRule.Guid.ToString( "N" )}",
                CssClass = "btn btn-danger btn-sm pull-right",
                Text = "<i class='fa fa-times'></i>"
            };

            btnDeleteRule.Click += btnDeleteRule_Click;
            pnlFilterRuleCol2.Controls.Add( btnDeleteRule );

            RockDropDownList ddlCompareField = new RockDropDownList()
            {
                ID = $"_ddlCompareField_{fieldVisibilityRule.Guid.ToString( "N" )}",
                CssClass = "input-width-xl",
                Required = false,
                ValidationGroup = this.ValidationGroup
            };

            ddlCompareField.Items.Add( new ListItem() );
            foreach ( var attribute in GetSupportedComparableAttributes() )
            {
                var listItem = new ListItem( attribute.Name, attribute.Guid.ToString() );
                ddlCompareField.Items.Add( listItem );
                if ( setValues && attribute.Guid == fieldVisibilityRule.ComparedToAttributeGuid )
                {
                    listItem.Selected = true;
                }
            }

            ddlCompareField.AutoPostBack = true;
            ddlCompareField.SelectedIndexChanged += ddlCompareField_SelectedIndexChanged;

            pnlFilterRuleCompareField.Controls.Add( ddlCompareField );

            DynamicPlaceholder filterControlPlaceholder = new DynamicPlaceholder()
            {
                ID = $"_filterControlPlaceholder_{fieldVisibilityRule.Guid.ToString( "N" )}"
            };

            pnlFilterRuleFieldFilter.Controls.Add( filterControlPlaceholder );

            CreateFilterControl( fieldVisibilityRule, setValues );
        }

        /// <summary>
        /// Gets the supported comparable attributes.
        /// </summary>
        /// <returns></returns>
        private List<Model.Attribute> GetSupportedComparableAttributes()
        {
            var supportedComparableAttributes = new List<Rock.Model.Attribute>();
            foreach ( var attribute in this.ComparableAttributes.Select( a => a.Value ) )
            {
                var fieldType = FieldTypeCache.Get( attribute.FieldTypeId );
                if ( fieldType.Field.HasFilterControl() )
                {
                    var qualifiers = attribute.AttributeQualifiers.ToDictionary( k => k.Key, v => new ConfigurationValue( v.Value ) );

                    // get the editControl to see if the FieldType supports a ChangeHandler for it (but don't actually use the control)
                    var editControl = fieldType.Field.EditControl( qualifiers, $"temp_editcontrol_attribute_{attribute.Id}" );

                    if ( fieldType.Field.HasChangeHandler( editControl ) )
                    {
                        supportedComparableAttributes.Add( attribute );
                    }
                }
            }

            return supportedComparableAttributes;
        }

        /// <summary>
        /// Handles the Click event of the BtnDeleteRule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnDeleteRule_Click( object sender, EventArgs e )
        {
            var btnDeleteRule = sender as LinkButton;
            var rockControlWrapper = btnDeleteRule.FirstParentControlOfType<RockControlWrapper>();
            var hiddenFieldRuleGuid = rockControlWrapper.ControlsOfTypeRecursive<HiddenFieldWithClass>().FirstOrDefault( a => a.CssClass == "js-rule-guid" );
            Guid fieldVisibilityRuleGuid = hiddenFieldRuleGuid.Value.AsGuid();
            var fieldVisibilityRule = this._fieldVisibilityRulesState.FirstOrDefault( a => a.Guid == fieldVisibilityRuleGuid );
            var updatedRules = this._fieldVisibilityRulesState.Clone();
            updatedRules.Remove( fieldVisibilityRule );

            SetFieldVisibilityRules( updatedRules );

            this._fieldVisibilityRulesState.Remove( fieldVisibilityRule );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the DdlCompareField control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ddlCompareField_SelectedIndexChanged( object sender, System.EventArgs e )
        {
            var ddlCompareField = sender as RockDropDownList;
            var rockControlWrapper = ddlCompareField.FirstParentControlOfType<RockControlWrapper>();
            var hiddenFieldRuleGuid = rockControlWrapper.ControlsOfTypeRecursive<HiddenFieldWithClass>().FirstOrDefault( a => a.CssClass == "js-rule-guid" );
            Guid fieldVisibilityRuleGuid = hiddenFieldRuleGuid.Value.AsGuid();

            var fieldVisibilityRule = this._fieldVisibilityRulesState.FirstOrDefault( a => a.Guid == fieldVisibilityRuleGuid );

            var selectedAttributeGuid = ddlCompareField.SelectedValue.AsGuidOrNull();
            fieldVisibilityRule.ComparedToAttributeGuid = selectedAttributeGuid;

            CreateFilterControl( fieldVisibilityRule, false );
        }

        /// <summary>
        /// Creates the filter control.
        /// </summary>
        /// <param name="fieldVisibilityRule">The field visibility rule.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateFilterControl( FieldVisibilityRule fieldVisibilityRule, bool setValues )
        {
            RockControlWrapper rockControlWrapper = this.FindControl( $"_rockControlWrapper_{fieldVisibilityRule.Guid.ToString( "N" )}" ) as RockControlWrapper;

            var filterControlPlaceholder = rockControlWrapper.FindControl( $"_filterControlPlaceholder_{fieldVisibilityRule.Guid.ToString( "N" )}" ) as DynamicPlaceholder;
            filterControlPlaceholder.Controls.Clear();

            var selectedAttribute = fieldVisibilityRule.ComparedToAttributeGuid.HasValue ? this.ComparableAttributes.GetValueOrNull( fieldVisibilityRule.ComparedToAttributeGuid.Value ) : null;

            if ( selectedAttribute != null )
            {
                var fieldType = FieldTypeCache.Get( selectedAttribute.FieldTypeId );
                var qualifiers = selectedAttribute.AttributeQualifiers.ToDictionary( k => k.Key, v => new ConfigurationValue( v.Value ) );
                rockControlWrapper.Label = selectedAttribute.Name;
                var filterControl = fieldType.Field.FilterControl( qualifiers, $"_filterControl_{fieldVisibilityRule.Guid.ToString( "N" )}", true, Rock.Reporting.FilterMode.AdvancedFilter );
                if ( filterControl != null )
                {
                    filterControlPlaceholder.Controls.Add( filterControl );
                    this.RockBlock()?.SetValidationGroup( filterControl.Controls, this.ValidationGroup );
                    if ( setValues )
                    {
                        List<string> filterValues = new List<string>();
                        filterValues.Add( fieldVisibilityRule.ComparisonType.ConvertToString( false ) );
                        filterValues.Add( fieldVisibilityRule.ComparedToValue );
                        fieldType.Field.SetFilterValues( filterControl, qualifiers, filterValues );
                    }
                }
            }
            else
            {
                rockControlWrapper.Label = "New Rule";
            }
        }

        #endregion Methods
    }
}
