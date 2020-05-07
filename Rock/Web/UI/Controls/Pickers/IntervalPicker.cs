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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Utility;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// This controls yields a Interval picker control.
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    public class IntervalPicker : CompositeControl, IRockControl
    {
        private TimeIntervalSetting _timeIntervalSetting = new TimeIntervalSetting( 0, IntervalTimeUnit.None );
        private TimeIntervalSetting _defaultIntervalSetting = new TimeIntervalSetting( 12, IntervalTimeUnit.Hour );

        #region Controls
        private RangeSlider _scheduleInterval;
        private ButtonGroup _scheduleUnit;
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public int DefaultValue
        {
            get => _defaultIntervalSetting.IntervalValue;
            set
            {
                _defaultIntervalSetting.IntervalValue = value;
                _defaultIntervalSetting.UpdateIntervalSettings( value, _defaultIntervalSetting.IntervalUnit, null );
            }
        }

        /// <summary>
        /// Gets or sets the default interval.
        /// </summary>
        /// <value>
        /// The default interval.
        /// </value>
        public IntervalTimeUnit DefaultInterval
        {
            get => _defaultIntervalSetting.IntervalUnit;
            set
            {
                _defaultIntervalSetting.UpdateIntervalSettings( _defaultIntervalSetting.IntervalValue, value, null );
            }
        }

        /// <summary>
        /// Gets or sets the interval in minutes.
        /// </summary>
        /// <value>
        /// The interval in minutes.
        /// </value>
        public int? IntervalInMinutes
        {
            get
            {
                return GetPersistedScheduleIntervalMinutes();
            }
            set
            {
                SetPersistenceScheduleFromInterval( value, null );
            }
        }
        #endregion

        #region IRockControl Implementation
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
                return _scheduleInterval.Label;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.Label = value;
            }
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
                return _scheduleInterval.Help;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.Help = value;
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
                return _scheduleInterval.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.ValidationGroup = value;
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
                return _scheduleInterval.Warning;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.Warning = value;
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
                return _scheduleInterval.Required;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.Required = value;
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
                return _scheduleInterval.RequiredErrorMessage;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.RequiredErrorMessage = value;
            }
        }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                EnsureChildControls();
                return _scheduleInterval.IsValid;
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
            get
            {
                EnsureChildControls();
                return _scheduleInterval.FormGroupCssClass;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.FormGroupCssClass = value;
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
                return _scheduleInterval.HelpBlock;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.HelpBlock = value;
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
                return _scheduleInterval.WarningBlock;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.WarningBlock = value;
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
                return _scheduleInterval.RequiredFieldValidator;
            }
            set
            {
                EnsureChildControls();
                _scheduleInterval.RequiredFieldValidator = value;
                _scheduleUnit.RequiredFieldValidator = value;
            }
        }
        #endregion

        #region Control Overrides
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                SetModelPropertiesFromView();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _scheduleInterval = new RangeSlider();
            _scheduleInterval.ID = this.ID + "_scheduleInterval";

            _scheduleUnit = new ButtonGroup()
            {
                CssClass = "text-right margin-b-md",
                UnselectedItemClass = "btn btn-xs btn-default",
                SelectedItemClass = "btn btn-xs btn-primary",
                AutoPostBack = true
            };

            _scheduleUnit.Items.AddRange( new ListItem[] {
                    new ListItem{ Text = "Mins", Value="1"},
                    new ListItem{ Text = "Hours", Value="2"},
                    new ListItem{ Text = "Days", Value="3"}
                } );
            _scheduleUnit.SelectedIndexChanged += ScheduleUnit_SelectedIndexChanged;
            _scheduleUnit.ID = this.ID + "_scheduleUnit";

            BindViewToModelProperties( _defaultIntervalSetting );

            Controls.Add( _scheduleInterval );
            Controls.Add( _scheduleUnit );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RenderBaseControl( writer );
            }
        }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Style, this.Style.Value );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            _scheduleInterval.RenderControl( writer );
            _scheduleUnit.RenderControl( writer );
            writer.RenderEndTag();
        }
        #endregion

        #region Private Helper Methods
        private void ScheduleUnit_SelectedIndexChanged( object sender, EventArgs e )
        {
            var unit = _scheduleUnit.SelectedValueAsEnum<IntervalTimeUnit>( IntervalTimeUnit.None );

            SetPersistedScheduleUnit( unit );
        }

        /// <summary>
        /// Set the unit in which the persisted schedule is measured.
        /// </summary>
        /// <param name="unit"></param>
        private void SetPersistedScheduleUnit( IntervalTimeUnit unit )
        {
            SetPersistenceScheduleFromInterval( _timeIntervalSetting.IntervalValue, unit );
        }

        /// <summary>
        /// Set and validate the persistence schedule settings.
        /// </summary>
        /// <param name="persistedScheduleIntervalMinutes">The persisted schedule interval minutes.</param>
        /// <param name="scheduleUnit">The schedule unit, or null if the unit should be determined by the interval.</param>
        private void SetPersistenceScheduleFromInterval( int? persistedScheduleIntervalMinutes, IntervalTimeUnit? scheduleUnit )
        {
            _timeIntervalSetting.UpdateIntervalSettings( persistedScheduleIntervalMinutes, scheduleUnit, _scheduleInterval.SelectedValue.GetValueOrDefault( 0 ) );

            BindViewToModelProperties(_timeIntervalSetting);
        }

        /// <summary>
        /// Update the view controls to synchronise with the model.
        /// </summary>
        private void BindViewToModelProperties(TimeIntervalSetting timeIntervalSetting)
        {
            // Bind the persistence view controls to the model.
            if ( timeIntervalSetting.IntervalUnit == IntervalTimeUnit.None )
            {
                _scheduleUnit.SelectedValue = null;
            }
            else
            {
                _scheduleUnit.SelectedValue = timeIntervalSetting.IntervalUnit.ConvertToInt().ToString();
            }

            _scheduleInterval.MinValue = 1;
            _scheduleInterval.MaxValue = timeIntervalSetting.MaxValue;
            _scheduleInterval.SelectedValue = timeIntervalSetting.IntervalValue;
        }

        /// <summary>
        /// Calculates the persistence schedule interval for the current settings.
        /// </summary>
        /// <returns></returns>
        private int? GetPersistedScheduleIntervalMinutes()
        {
            return _timeIntervalSetting.IntervalMinutes;
        }

        /// <summary>
        /// Set the fields and properties of the view model from the controls in the view.
        /// </summary>
        private void SetModelPropertiesFromView()
        {
            _timeIntervalSetting.IntervalUnit = _scheduleUnit.SelectedValueAsEnum<IntervalTimeUnit>( DefaultInterval );
            _timeIntervalSetting.IntervalValue = _scheduleInterval.SelectedValue.GetValueOrDefault( DefaultValue );
            _scheduleInterval.MaxValue = _timeIntervalSetting.MaxValue;
        }
        #endregion
    }
}
