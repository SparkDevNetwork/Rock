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
    /// Control that can be used to select a person's race
    /// </summary>
    public class RacePicker : CompositeControl, IRockControl
    {
        #region Controls

        private RockRadioButtonList _rblRace;
        private RockDropDownList _ddlRace;

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
                        return _ddlRace.SelectedIndex;
                    case RacePickerMode.RadioButtonList:
                        return _rblRace.SelectedIndex;
                    default:
                        return _ddlRace.SelectedIndex;
                }
            }
            set
            {
                EnsureChildControls();
                switch ( CurrentPickerMode )
                {
                    case RacePickerMode.DropdownList:
                        _ddlRace.SelectedIndex = value;
                        break;
                    case RacePickerMode.RadioButtonList:
                        _rblRace.SelectedIndex = value;
                        break;
                    default:
                        _ddlRace.SelectedIndex = value;
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
                        return _ddlRace.SelectedValue;
                    case RacePickerMode.RadioButtonList:
                        return _rblRace.SelectedValue;
                    default:
                        return _ddlRace.SelectedValue;
                }
            }
            set
            {
                EnsureChildControls();
                switch ( CurrentPickerMode )
                {
                    case RacePickerMode.DropdownList:
                        _ddlRace.SetValue( value );
                        break;
                    case RacePickerMode.RadioButtonList:
                        _rblRace.SetValue( value );
                        break;
                    default:
                        _ddlRace.SetValue( value );
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
                return _ddlRace.Label;
            }
            set
            {
                EnsureChildControls();
                _ddlRace.Label = value;
                _rblRace.Label = value;
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
                return _ddlRace.Help;
            }
            set
            {
                EnsureChildControls();
                _ddlRace.Help = value;
                _rblRace.Help = value;
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
                return _ddlRace.Warning;
            }
            set
            {
                EnsureChildControls();
                _ddlRace.Warning = value;
                _rblRace.Warning = value;
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
                return _ddlRace.Required;
            }
            set
            {
                EnsureChildControls();
                _ddlRace.Required = value;
                _rblRace.Required = value;
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
                return _ddlRace.RequiredErrorMessage;
            }
            set
            {
                EnsureChildControls();
                _ddlRace.RequiredErrorMessage = value;
                _rblRace.RequiredErrorMessage = value;
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
                return _ddlRace.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _ddlRace.ValidationGroup = value;
                _rblRace.ValidationGroup = value;
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
                        return _ddlRace.IsValid;
                    case RacePickerMode.RadioButtonList:
                        return _rblRace.IsValid;
                    default:
                        return _ddlRace.IsValid;
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
                return _ddlRace.HelpBlock;
            }
            set
            {
                EnsureChildControls();
                _ddlRace.HelpBlock = value;
                _rblRace.HelpBlock = value;
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
                return _ddlRace.WarningBlock;
            }
            set
            {
                EnsureChildControls();
                _ddlRace.WarningBlock = value;
                _rblRace.WarningBlock = value;
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
                return _ddlRace.RequiredFieldValidator;
            }
            set
            {
                EnsureChildControls();
                _ddlRace.RequiredFieldValidator = value;
                _rblRace.RequiredFieldValidator = value;
            }
        }

        #endregion

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
                    _ddlRace.SetValue( value );
                    break;
                case RacePickerMode.RadioButtonList:
                    _rblRace.SetValue( value );
                    break;
                default:
                    _ddlRace.SetValue( value );
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
                    return _ddlRace.SelectedValueAsId();
                case RacePickerMode.RadioButtonList:
                    return _rblRace.SelectedValueAsId();
                default:
                    return _ddlRace.SelectedValueAsId();
            }
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
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _ddlRace = new RockDropDownList()
            {
                ID = "_ddlRace",
                Label = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_RACE_LABEL )
            };

            _rblRace = new RockRadioButtonList()
            {
                ID = "_rblRace",
                RepeatDirection = RepeatDirection.Horizontal,
                Label = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.PERSON_RACE_LABEL )
            };

            switch ( CurrentPickerMode )
            {
                case RacePickerMode.DropdownList:
                    Controls.Add( _ddlRace );
                    break;
                case RacePickerMode.RadioButtonList:
                    Controls.Add( _rblRace );
                    break;
                default:
                    Controls.Add( _ddlRace );
                    break;
            }

            LoadItems( true );
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
            var ethnicities = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_RACE ).DefinedValues;

            switch ( CurrentPickerMode )
            {
                case RacePickerMode.RadioButtonList:
                    var selectedRblItems = _rblRace.Items.Cast<ListItem>()
                        .Where( i => i.Selected )
                        .Select( i => i.Value ).AsIntegerList();

                    _rblRace.Items.Clear();

                    foreach ( var ethnicity in ethnicities )
                    {
                        var li = new ListItem( ethnicity.Value, ethnicity.Id.ToString() )
                        {
                            Selected = selectedRblItems.Contains( ethnicity.Id )
                        };

                        _rblRace.Items.Add( li );
                    }
                    break;
                default:
                    var selectedDdlItems = _ddlRace.Items.Cast<ListItem>()
                        .Where( i => i.Selected )
                        .Select( i => i.Value ).AsIntegerList();

                    _ddlRace.Items.Clear();

                    if ( includeEmptyOption )
                    {
                        // add Empty option first
                        _ddlRace.Items.Add( new ListItem() );
                    }

                    foreach ( var ethnicity in ethnicities )
                    {
                        var li = new ListItem( ethnicity.Value, ethnicity.Id.ToString() )
                        {
                            Selected = selectedDdlItems.Contains( ethnicity.Id )
                        };

                        _ddlRace.Items.Add( li );
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Interface used by race pickers
    /// </summary>
    public enum RacePickerMode
    {
        /// <summary>
        /// A DropdownList
        /// </summary>
        DropdownList = 1,
        /// <summary>
        /// A RadioButtonList
        /// </summary>
        RadioButtonList = 0,
    }
}
