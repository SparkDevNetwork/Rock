// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for selecting a number range
    /// </summary>
    [ToolboxData( "<{0}:NumberRangeEditor runat=server></{0}:NumberRangeEditor>" )]
    public class NumberRangeEditor : CompositeControl, IRockControl
    {
        #region IRockControl implementation (Uses non-standard logic for required fields)

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
            get { return ViewState["Required"] as bool? ?? false; }
            set 
            { 
                ViewState["Required"] = value;
                EnsureChildControls();
                _tbLowerValue.Required = value;
                _tbUpperValue.Required = value;
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
                return _tbLowerValue.RequiredErrorMessage;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.RequiredErrorMessage = value;
                _tbUpperValue.RequiredErrorMessage = value;
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
                return _tbLowerValue.ValidationGroup;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.ValidationGroup = value;
                _tbUpperValue.ValidationGroup = value;
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
                return _tbLowerValue.IsValid && _tbUpperValue.IsValid;
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
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        /// <summary>
        /// The lower value edit box
        /// </summary>
        private NumberBox _tbLowerValue;

        /// <summary>
        /// The upper value edit box
        /// </summary>
        private NumberBox _tbUpperValue;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the type of the number.
        /// </summary>
        /// <value>
        /// The type of the number.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The NumberType (Decimal or Integer) for the Controls" )
        ]
        public ValidationDataType NumberType
        {
            get
            {
                EnsureChildControls();
                return _tbLowerValue.NumberType;
            }

            set 
            {
                EnsureChildControls();
                _tbLowerValue.NumberType = value;
                _tbUpperValue.NumberType = value;
            }
        }

        /// <summary>
        /// Gets or sets the range label (the label between the two number boxes). Defaults to "to"
        /// </summary>
        /// <value>
        /// The range label.
        /// </value>
        public string RangeLabel
        {
            get
            {
                return (ViewState["RangeLabel"] as string ) ?? "to";
            }

            set
            {
                ViewState["RangeLabel"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the minimum value that either number can be
        /// </summary>
        /// <value>
        /// The minimum value.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The minimum value that either number can be" )
        ]
        public string MinimumValue
        {
            get {
                EnsureChildControls();
                return _tbLowerValue.MinimumValue; 
            }

            set {
                EnsureChildControls();
                _tbLowerValue.MinimumValue = value;
                _tbUpperValue.MinimumValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value that either number can be
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The maximum value that either number can be" )
        ]
        public string MaximumValue
        {
            get
            {
                EnsureChildControls();
                return _tbLowerValue.MaximumValue;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.MaximumValue = value;
                _tbUpperValue.MaximumValue = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NumberRangeEditor"/> class.
        /// </summary>
        public NumberRangeEditor()
            : base()
        {
            HelpBlock = new HelpBlock();
        }

        #endregion

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _tbLowerValue = new NumberBox();
            _tbLowerValue.ID = this.ID + "_lower";
            _tbLowerValue.CssClass = "input-width-md js-number-range-lower";
            Controls.Add( _tbLowerValue );

            _tbUpperValue = new NumberBox();
            _tbUpperValue.ID = this.ID + "_upper";
            _tbUpperValue.CssClass = "input-width-md js-number-range-upper";
            Controls.Add( _tbUpperValue );
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
        public void RenderBaseControl(HtmlTextWriter writer)
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "form-control-group " + this.CssClass );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _tbLowerValue.RenderControl( writer );
            writer.Write( "<span class='to'> " + this.RangeLabel + " </span>" );
            _tbUpperValue.RenderControl( writer );

            writer.RenderEndTag();
        }

        /// <summary>
        /// Gets or sets the lower value.
        /// </summary>
        /// <value>
        /// The lower value.
        /// </value>
        public decimal? LowerValue {
            get
            {
                EnsureChildControls();

                decimal result;
                if (decimal.TryParse(_tbLowerValue.Text, out result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                _tbLowerValue.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the upper value.
        /// </summary>
        /// <value>
        /// The upper value.
        /// </value>
        public decimal? UpperValue
        {
            get
            {
                EnsureChildControls();

                decimal result;
                if (decimal.TryParse(_tbUpperValue.Text, out result))
                {
                    return result;
                }

                return null;
            }

            set
            {
                EnsureChildControls();
                _tbUpperValue.Text = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the comma-delimited values.
        /// </summary>
        /// <value>
        /// The delimited values.
        /// </value>
        public string DelimitedValues
        {
            get
            {
                return string.Format( "{0},{1}", this.LowerValue, this.UpperValue );
            }
            set
            {
                if ( value != null )
                {
                    string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                    if ( valuePair.Length == 2 )
                    {
                        decimal result;

                        if ( decimal.TryParse( valuePair[0], out result ) )
                        {
                            this.LowerValue = result;
                        }
                        else
                        {
                            this.LowerValue = null;
                        }

                        if ( decimal.TryParse( valuePair[1], out result ) )
                        {
                            this.UpperValue = result;
                        }
                        else
                        {
                            this.UpperValue = null;
                        }
                    }
                    else
                    {
                        this.LowerValue = null;
                        this.UpperValue = null;
                    }
                }
                else
                {
                    this.LowerValue = null;
                    this.UpperValue = null;
                }
            }
        }
        /// <summary>
        /// Formats the delimited values.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static string FormatDelimitedValues(string value, string format = "N0")
        {
            try
            {
                if ( value != null )
                {
                    if ( value.StartsWith( "," ) )
                    {
                        string upperValue = decimal.Parse( value.Substring( 1 ) ).ToString(format);
                        return string.Format( "through {0}", upperValue );
                    }
                    else if ( value.EndsWith( "," ) )
                    {
                        string lowerValue = decimal.Parse( value.Substring( 0, value.Length - 1 ) ).ToString( format );
                        return string.Format( "from {0}", lowerValue );
                    }
                    else
                    {
                        string[] valuePair = value.Split( new char[] { ',' }, StringSplitOptions.None );
                        if ( valuePair.Length == 2 )
                        {
                            string lowerValue = decimal.Parse( valuePair[0] ).ToString( format );
                            string upperValue = decimal.Parse( valuePair[1] ).ToString( format );
                            return string.Format( "{0} to {1}", lowerValue, upperValue );
                        }
                    }
                }

                return null;
            }
            catch 
            {
                return null;  
            }
        }
    }
}
