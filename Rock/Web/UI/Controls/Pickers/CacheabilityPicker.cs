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
    /// This control yields a Cacheability picker control.
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class CacheabilityPicker : CompositeControl, IRockControl
    {
        private const string CACHEABILITY_PICKER_NAME = "cacheabilityPicker";
        private const string MAX_AGE_UNIT_NAME = "maxAgeUnit";
        private const string MAX_AGE_VALUE_NAME = "maxAgeValue";
        private const string MAX_SHARED_AGE_UNIT_NAME = "maxSharedAgeUnit";
        private const string MAX_SHARED_AGE_VALUE_NAME = "maxSharedAgeValue";

        #region Internal Controls
        private RockRadioButtonList _cacheabilityType;
        private NumberBox _maxAgeValue;
        private RockDropDownList _maxAgeUnit;
        private Literal _maxAgeLabel;

        private NumberBox _maxSharedAgeValue;
        private RockDropDownList _maxSharedAgeUnit;
        private Literal _maxSharedAgeLabel;
        #endregion

        /// <summary>
        /// Gets or sets the current cacheablity.
        /// </summary>
        /// <value>
        /// The current cacheablity.
        /// </value>
        public RockCacheability CurrentCacheablity { get; set; } = new RockCacheability();

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheabilityPicker"/> class.
        /// </summary>
        public CacheabilityPicker() : base()
        {
            EnsureChildControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {

                if ( _maxAgeValue.Text.IsNullOrWhiteSpace() )
                {
                    CurrentCacheablity.MaxAge = null;
                }
                else
                {
                    CurrentCacheablity.MaxAge = new TimeInterval
                    {
                        Unit = _maxAgeUnit.SelectedValue.ConvertToEnum<TimeIntervalUnit>(),
                        Value = _maxAgeValue.Text.AsInteger()
                    };
                }

                if ( _maxSharedAgeValue.Text.IsNullOrWhiteSpace() )
                {
                    CurrentCacheablity.SharedMaxAge = null;
                }
                else
                {
                    CurrentCacheablity.SharedMaxAge = new TimeInterval
                    {
                        Unit = _maxSharedAgeUnit.SelectedValue.ConvertToEnum<TimeIntervalUnit>(),
                        Value = _maxSharedAgeValue.Text.AsInteger()
                    };
                }

                if ( _cacheabilityType.SelectedValue.IsNotNullOrWhiteSpace() )
                {
                    CurrentCacheablity.RockCacheablityType = _cacheabilityType.SelectedValue.ConvertToEnum<RockCacheablityType>();
                }
                else
                {
                    CurrentCacheablity.RockCacheablityType = RockCacheablityType.Public;
                }
            }
        }

        #region IRockControl Implementation
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label text
        /// </value>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        public string Help { get; set; }
        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        public string Warning { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRockControl" /> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; set; }
        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage { get; set; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool IsValid
        {
            get
            {
                return _cacheabilityType.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public string FormGroupCssClass { get; set; }
        /// <summary>
        /// Gets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public HelpBlock HelpBlock { get; set; }
        /// <summary>
        /// Gets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public WarningBlock WarningBlock { get; set; }
        /// <summary>
        /// Gets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }
        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        public string ValidationGroup { get; set; }

        /// <summary>
        /// This is where you implement the simple aspects of rendering your control.  The rest
        /// will be handled by calling RenderControlHelper's RenderControl() method.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            var showAges = false;

            _maxAgeUnit.SelectedValue = TimeIntervalUnit.Minutes.ConvertToInt().ToString();
            _maxSharedAgeUnit.SelectedValue = TimeIntervalUnit.Minutes.ConvertToInt().ToString();

            if ( CurrentCacheablity == null )
            {
                _cacheabilityType.SelectedValue = RockCacheablityType.Public.ConvertToInt().ToString();
                showAges = true;
            }
            else
            {
                showAges = CurrentCacheablity.OptionSupportsAge( CurrentCacheablity.RockCacheablityType );
                _cacheabilityType.SelectedValue = CurrentCacheablity.RockCacheablityType.ConvertToInt().ToString();

                _maxAgeValue.Text = CurrentCacheablity.MaxAge?.Value.ToStringSafe();
                if ( CurrentCacheablity.MaxAge != null )
                {
                    _maxAgeUnit.SelectedValue = CurrentCacheablity.MaxAge.Unit.ConvertToInt().ToStringSafe();
                }

                _maxSharedAgeValue.Text = CurrentCacheablity.SharedMaxAge?.Value.ToStringSafe();
                if ( CurrentCacheablity.SharedMaxAge != null )
                {
                    _maxSharedAgeUnit.SelectedValue = CurrentCacheablity.SharedMaxAge?.Unit.ConvertToInt().ToStringSafe();
                }
            }

            _cacheabilityType.Enabled = Enabled;
            _maxAgeUnit.Enabled = Enabled;
            _maxAgeValue.Enabled = Enabled;
            _maxSharedAgeUnit.Enabled = Enabled;
            _maxSharedAgeValue.Enabled = Enabled;

            writer.AddAttribute( HtmlTextWriterAttribute.Style, Style.Value );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _cacheabilityType.RenderControl( writer );

            if ( showAges )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "row" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _maxAgeLabel.RenderControl( writer );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group d-flex" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _maxAgeValue.RenderControl( writer );
                _maxAgeUnit.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "col-md-6" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _maxSharedAgeLabel.RenderControl( writer );
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group d-flex" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _maxSharedAgeValue.RenderControl( writer );
                _maxSharedAgeUnit.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            writer.RenderEndTag();
        }
        #endregion

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
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _cacheabilityType = new RockRadioButtonList
            {
                ID = $"{ID}_{CACHEABILITY_PICKER_NAME}",
                Label = "Cacheability Type",
                Help = @"This determines how the item will be treated in cache.<br />
                                        Public - This item can be cached on the browser or any other shared network cache like a CDN.<br />
                                        Private - This item can only be cached in the browser.<br />
                                        No-Cache - The item will be checked on every load, but if it is deemed to not have changed since the last load it will use a local copy.<br />
                                        No-Store - This item will never be stored by the local browser.This is used for sensitive files like check images."
            };
            _cacheabilityType.Items.AddRange( new ListItem[] {
                new ListItem{ Text = "Public", Value=RockCacheablityType.Public.ConvertToInt().ToString()},
                new ListItem{ Text = "Private", Value=RockCacheablityType.Private.ConvertToInt().ToString()},
                new ListItem{ Text = "No-Cache", Value=RockCacheablityType.NoCache.ConvertToInt().ToString()},
                new ListItem{ Text = "No-Store", Value=RockCacheablityType.NoStore.ConvertToInt().ToString()}
            } );
            _cacheabilityType.RepeatDirection = RepeatDirection.Horizontal;
            _cacheabilityType.AutoPostBack = true;

            _cacheabilityType.SelectedIndexChanged += CacheabilityType_SelectedIndexChanged;

            CreateMaxAgeControls();
            CreateMaxSharedAgeControls();

            Controls.Add( _cacheabilityType );

            Controls.Add( _maxAgeLabel );
            Controls.Add( _maxAgeValue );
            Controls.Add( _maxAgeUnit );

            Controls.Add( _maxSharedAgeLabel );
            Controls.Add( _maxSharedAgeValue );
            Controls.Add( _maxSharedAgeUnit );
        }

        private void CreateMaxAgeControls()
        {

            _maxAgeUnit = new RockDropDownList
            {
                ID = $"{ID}_{MAX_AGE_UNIT_NAME}",
                CssClass = "w-auto",
                EnableViewState = false
            };

            _maxAgeUnit.Items.AddRange( new ListItem[]
            {
                new ListItem("Secs", TimeIntervalUnit.Seconds.ConvertToInt().ToString()),
                new ListItem("Mins", TimeIntervalUnit.Minutes.ConvertToInt().ToString()),
                new ListItem("Hours", TimeIntervalUnit.Hours.ConvertToInt().ToString()),
                new ListItem("Days", TimeIntervalUnit.Days.ConvertToInt().ToString())
            } );

            _maxAgeValue = new NumberBox
            {
                ID = $"{ID}_{MAX_AGE_VALUE_NAME}",
                CssClass = "flex-shrink-1 input-width-md min-w-0 border-right-0",
                EnableViewState = false
            };

            _maxAgeLabel = new Literal
            {
                Text = @"<label class=""control-label"">Max Age<a class=""help""
                href=""#"" tabindex=""-1"" data-toggle=""tooltip"" data-placement=""auto""
                data-container=""body"" data-html=""true"" title=""The maximum amount of time that the item will be cached."">
                <i class=""fa fa-info-circle""></i></a></label>"
            };
        }

        private void CreateMaxSharedAgeControls()
        {
            _maxSharedAgeUnit = new RockDropDownList
            {
                ID = $"{ID}_{MAX_SHARED_AGE_UNIT_NAME}",
                CssClass = "w-auto",
                EnableViewState = false
            };
            _maxSharedAgeUnit.Items.AddRange( new ListItem[]
            {
                new ListItem("Secs", TimeIntervalUnit.Seconds.ConvertToInt().ToString()),
                new ListItem("Mins", TimeIntervalUnit.Minutes.ConvertToInt().ToString()),
                new ListItem("Hours", TimeIntervalUnit.Hours.ConvertToInt().ToString()),
                new ListItem("Days", TimeIntervalUnit.Days.ConvertToInt().ToString())
            } );

            _maxSharedAgeValue = new NumberBox
            {
                ID = $"{ID}_{MAX_SHARED_AGE_VALUE_NAME}",
                CssClass = "flex-shrink-1 input-width-md min-w-0 border-right-0",
                EnableViewState = false
            };

            _maxSharedAgeLabel = new Literal
            {
                Text = @"<label class=""control-label"">Max Shared Age<a class=""help""
                href=""#"" tabindex=""-1"" data-toggle=""tooltip"" data-placement=""auto""
                data-container=""body"" data-html=""true"" title=""The maximum amount of time the item will be cached in a shared cache (e.g. CDN). If not provided then the Max Age is typically used."">
                <i class=""fa fa-info-circle""></i></a></label>"
            };
        }

        private void CacheabilityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            CurrentCacheablity.RockCacheablityType = _cacheabilityType.SelectedValue.ConvertToEnum<RockCacheablityType>();
        }
    }
}
