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
using Rock.Web.Cache;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a person's ethnicity
    /// </summary>
    public class EthnicityPicker : CompositeControl, IRockControl
    {
        #region Controls

        private RockDropDownList _ddlEthnicity;
        private RockRadioButtonList _rblEthnicity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the current picker mode.
        /// </summary>
        public RacePickerMode CurrentPickerMode
        {
            get
            {
                var currentPickerMode = ViewState["CurrentPickerMode"] as RacePickerMode?;
                if ( !currentPickerMode.HasValue )
                {
                    currentPickerMode = RacePickerMode.DropdownList;
                    CurrentPickerMode = currentPickerMode.Value;
                }

                return currentPickerMode.Value;
            }
            set
            {
                ViewState["CurrentPickerMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the Selected index
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                EnsureChildControls();
                switch ( CurrentPickerMode )
                {
                    case RacePickerMode.DropdownList:
                        return _ddlEthnicity.SelectedIndex;
                    case RacePickerMode.RadioButtonList:
                        return _rblEthnicity.SelectedIndex;
                    default:
                        return _ddlEthnicity.SelectedIndex;
                }
            }
            set
            {
                EnsureChildControls();
                switch ( CurrentPickerMode )
                {
                    case RacePickerMode.DropdownList:
                        _ddlEthnicity.SelectedIndex = value;
                        break;
                    case RacePickerMode.RadioButtonList:
                        _rblEthnicity.SelectedIndex = value;
                        break;
                    default:
                        _ddlEthnicity.SelectedIndex = value;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Selected Value
        /// </summary>
        public string SelectedValue
        {
            get
            {
                EnsureChildControls();
                switch ( CurrentPickerMode )
                {
                    case RacePickerMode.DropdownList:
                        return _ddlEthnicity.SelectedValue;
                    case RacePickerMode.RadioButtonList:
                        return _rblEthnicity.SelectedValue;
                    default:
                        return _ddlEthnicity.SelectedValue;
                }
            }
            set
            {
                EnsureChildControls();
                switch ( CurrentPickerMode )
                {
                    case RacePickerMode.DropdownList:
                        _ddlEthnicity.SetValue( value );
                        break;
                    case RacePickerMode.RadioButtonList:
                        _rblEthnicity.SetValue( value );
                        break;
                    default:
                        _ddlEthnicity.SetValue( value );
                        break;
                }
            }
        }

        #endregion

        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label text
        /// </value>
        public string Label
        {
            get
            {
                EnsureChildControls();
                return _ddlEthnicity.Label;
            }
            set
            {
                EnsureChildControls();
                _ddlEthnicity.Label = value;
                _rblEthnicity.Label = value;
            }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
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
        public string Help
        {
            get
            {
                EnsureChildControls();
                return _ddlEthnicity.Help;
            }
            set
            {
                EnsureChildControls();
                _ddlEthnicity.Help = value;
                _rblEthnicity.Help = value;
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        public string Warning
        {
            get
            {
                EnsureChildControls();
                return _ddlEthnicity.Warning;
            }
            set
            {
                EnsureChildControls();
                _ddlEthnicity.Warning = value;
                _rblEthnicity.Warning = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRockControl" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required
        {
            get
            {
                EnsureChildControls();
                return _ddlEthnicity.Required;
            }
            set
            {
                EnsureChildControls();
                _ddlEthnicity.Required = value;
                _rblEthnicity.Required = value;
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
                EnsureChildControls();
                return _ddlEthnicity.RequiredErrorMessage;
            }
            set
            {
                EnsureChildControls();
                _ddlEthnicity.RequiredErrorMessage = value;
                _rblEthnicity.RequiredErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return _ddlEthnicity.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _ddlEthnicity.ValidationGroup = value;
                _rblEthnicity.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                EnsureChildControls();
                switch ( CurrentPickerMode )
                {
                    case RacePickerMode.DropdownList:
                        return _ddlEthnicity.IsValid;
                    case RacePickerMode.RadioButtonList:
                        return _rblEthnicity.IsValid;
                    default:
                        return _ddlEthnicity.IsValid;
                }
            }
        }

        /// <summary>
        /// Gets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock
        {
            get
            {
                EnsureChildControls();
                return _ddlEthnicity.HelpBlock;
            }
            set
            {
                EnsureChildControls();
                _ddlEthnicity.HelpBlock = value;
                _rblEthnicity.HelpBlock = value;
            }
        }

        /// <summary>
        /// Gets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock
        {
            get
            {
                EnsureChildControls();
                return _ddlEthnicity.WarningBlock;
            }
            set
            {
                EnsureChildControls();
                _ddlEthnicity.WarningBlock = value;
                _rblEthnicity.WarningBlock = value;
            }
        }

        /// <summary>
        /// Gets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator
        {
            get
            {
                EnsureChildControls();
                return _ddlEthnicity.RequiredFieldValidator;
            }
            set
            {
                EnsureChildControls();
                _ddlEthnicity.RequiredFieldValidator = value;
                _rblEthnicity.RequiredFieldValidator = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to set the selected value. If the value does not exist, will set the first item in the list.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue( int? value )
        {
            EnsureChildControls();

            switch ( CurrentPickerMode )
            {
                case RacePickerMode.DropdownList:
                    _ddlEthnicity.SetValue( value );
                    break;
                case RacePickerMode.RadioButtonList:
                    _rblEthnicity.SetValue( value );
                    break;
                default:
                    _ddlEthnicity.SetValue( value );
                    break;
            }
        }

        /// <summary>
        /// Tries to set the selected value. If the value does not exist, will set the first item in the list.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetValue( string value )
        {
            EnsureChildControls();

            int? intValue = value.AsIntegerOrNull();
            SetValue( intValue );
        }

        /// <summary>
        /// Returns the value of the currently selected item.
        /// It will return NULL if either <see cref="T:Rock.Constants.None"/> or <see cref="T:Rock.Constants.All"/> is selected.
        /// </summary>
        /// <returns></returns>
        public int? SelectedValueAsId()
        {
            EnsureChildControls();

            switch ( CurrentPickerMode )
            {
                case RacePickerMode.DropdownList:
                    return _ddlEthnicity.SelectedValueAsId();
                case RacePickerMode.RadioButtonList:
                    return _rblEthnicity.SelectedValueAsId();
                default:
                    return _ddlEthnicity.SelectedValueAsId();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _ddlEthnicity = new RockDropDownList()
            {
                ID = "_ddlEthnicity",
                Label = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_ETHNICITY_LABEL )
            };

            _rblEthnicity = new RockRadioButtonList()
            {
                ID = "_rblEthnicity",
                RepeatDirection = RepeatDirection.Horizontal,
                Label = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_ETHNICITY_LABEL )
            };

            switch ( CurrentPickerMode )
            {
                case RacePickerMode.DropdownList:
                    Controls.Add( _ddlEthnicity );
                    break;
                case RacePickerMode.RadioButtonList:
                    Controls.Add( _rblEthnicity );
                    break;
                default:
                    Controls.Add( _ddlEthnicity );
                    break;
            }

            LoadItems( true );
        }

        /// <summary>
        /// Renders the <see cref="T:System.Web.UI.WebControls.TextBox" /> control to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> that receives the rendered output.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            //
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var currentPickerMode = ViewState["CurrentPickerMode"] as RacePickerMode?;
            if ( currentPickerMode.HasValue )
            {
                this.CurrentPickerMode = currentPickerMode.Value;
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
            ViewState["CurrentPickerMode"] = this.CurrentPickerMode;

            return base.SaveViewState();
        }

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        /// <param name="includeEmptyOption">if set to <c>true</c> [include empty option].</param>
        private void LoadItems( bool includeEmptyOption )
        {
            var ethnicities = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_ETHNICITY ).DefinedValues;

            switch ( CurrentPickerMode )
            {
                case RacePickerMode.RadioButtonList:
                    var selectedRblItems = _rblEthnicity.Items.Cast<ListItem>()
                        .Where( i => i.Selected )
                        .Select( i => i.Value ).AsIntegerList();

                    _rblEthnicity.Items.Clear();

                    foreach ( var ethnicity in ethnicities )
                    {
                        var li = new ListItem( ethnicity.Value, ethnicity.Id.ToString() )
                        {
                            Selected = selectedRblItems.Contains( ethnicity.Id )
                        };

                        _rblEthnicity.Items.Add( li );
                    }
                    break;
                default:
                    var selectedDdlItems = _ddlEthnicity.Items.Cast<ListItem>()
                        .Where( i => i.Selected )
                        .Select( i => i.Value ).AsIntegerList();

                    _ddlEthnicity.Items.Clear();

                    if ( includeEmptyOption )
                    {
                        // add Empty option first
                        _ddlEthnicity.Items.Add( new ListItem() );
                    }

                    foreach ( var ethnicity in ethnicities )
                    {
                        var li = new ListItem( ethnicity.Value, ethnicity.Id.ToString() )
                        {
                            Selected = selectedDdlItems.Contains( ethnicity.Id )
                        };

                        _ddlEthnicity.Items.Add( li );
                    }
                    break;
            }
        }

        #endregion
    }
}
