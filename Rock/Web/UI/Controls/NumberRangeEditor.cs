//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control for selecting a number range
    /// </summary>
    [ToolboxData( "<{0}:NumberRangeEditor runat=server></{0}:NumberRangeEditor>" )]
    public class NumberRangeEditor : CompositeControl, ILabeledControl, IRequiredControl
    {
        /// <summary>
        /// The label
        /// </summary>
        private Literal _label;

        /// <summary>
        /// The lower value edit box
        /// </summary>
        private NumberBox _tbLowerValue;

        /// <summary>
        /// The upper value edit box
        /// </summary>
        private NumberBox _tbUpperValue;

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
                EnsureChildControls();
                return _label.Text;
            }

            set
            {
                EnsureChildControls();
                _label.Text = value;
            }
        }


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

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IRequiredControl" /> is required.
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
                EnsureChildControls();
                return _tbLowerValue.Required;
            }

            set
            {
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
        /// <exception cref="System.NotImplementedException">
        /// </exception>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The required error message.  If blank, the LabelName name will be used" )
        ]
        public string RequiredErrorMessage
        {
            get
            {
                EnsureChildControls();
                return _tbLowerValue.RequiredErrorMessage;
            }

            set
            {
                _tbLowerValue.RequiredErrorMessage = value;
                _tbUpperValue.RequiredErrorMessage = value;
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
                return _tbLowerValue.IsValid && _tbUpperValue.IsValid;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            _label = new Literal();
            Controls.Add( _label );

            _tbLowerValue = new NumberBox();
            _tbLowerValue.ID = this.ID + "_lower";
            _tbLowerValue.CssClass = "input-small";

            Controls.Add( _tbLowerValue );

            _tbUpperValue = new NumberBox();
            _tbUpperValue.ID = this.ID + "_upper";
            _tbUpperValue.CssClass = "input-small";
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
                bool renderControlGroupDiv = !string.IsNullOrWhiteSpace( Label );

                if ( renderControlGroupDiv )
                {
                    writer.AddAttribute( "class", "control-group" + ( IsValid ? "" : " error" ) + ( Required ? " required" : "" ) );

                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( "class", "control-label" );

                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    _label.RenderControl( writer );
                    writer.RenderEndTag();
                }

                // mark as input-xxlarge since we want the 2 inputs to stay on the same line
                writer.AddAttribute( "class", "controls input-xxlarge" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                _tbLowerValue.RenderControl( writer );
                writer.Write( "<span> to </span>" );
                _tbUpperValue.RenderControl( writer );

                writer.RenderEndTag();

                if ( renderControlGroupDiv )
                {
                    writer.RenderEndTag();
                }
            }
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
    }
}
