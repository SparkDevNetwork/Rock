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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.TextBox"/> control for editing a phone number
    /// </summary>
    [ToolboxData( "<{0}:PhoneNumberBox runat=server></{0}:PhoneNumberBox>" )]
    public class PhoneNumberBox : TextBox, IRockControl
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
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
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
            set { ViewState["Required"] = value; }
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
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
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

        /// <summary>
        /// Gets or sets the group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the <see cref="T:System.Web.UI.WebControls.TextBox" /> control causes validation when it posts back to the server. The default value is an empty string ("").</returns>
        public override string ValidationGroup
        {
            get
            {
                return base.ValidationGroup;
            }
            set
            {
                base.ValidationGroup = value;
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ValidationGroup = value;
                }
            }
        }

        #endregion

        #region Controls

        private HiddenField _hfCountryCode;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        /// <value>
        /// The country code.
        /// </value>
        public string CountryCode
        {
            get 
            {
                EnsureChildControls();
                return _hfCountryCode.Value;
            }
            set 
            {
                EnsureChildControls();
                if ( !string.IsNullOrWhiteSpace( value ) )
                {
                    _hfCountryCode.Value = value;
                }
                else
                {
                    _hfCountryCode.Value = Rock.Model.PhoneNumber.DefaultCountryCode();
                }
            }
        }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        public string Number
        {
            get { return this.Text; }
            set { this.Text = value; }
        }

        /// <summary>
        /// Gets or sets the placeholder text to display inside textbox when it is empty
        /// </summary>
        /// <value>
        /// The placeholder text
        /// </value>
        public string Placeholder
        {
            get { return ViewState["Placeholder"] as string ?? string.Empty; }
            set { ViewState["Placeholder"] = value; }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneNumberBox"/> class.
        /// </summary>
        public PhoneNumberBox()
            : base()
        {
            RockControlHelper.Init( this );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            var rockPage = Page as RockPage;
            if ( rockPage != null )
            {
                string script = rockPage.GetSharedItem( "org.RockRMS.PhoneNumberBox.script" ) as string;

                if ( script == null )
                {
                    StringBuilder sbScript = new StringBuilder();
                    sbScript.Append( "\tvar phoneNumberFormats = {\n" );

                    var definedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() );
                    if ( definedType != null )
                    {
                        var definedValues = definedType.DefinedValues;

                        foreach ( var countryCode in definedValues.OrderBy( v => v.Order ).Select( v => v.Value).Distinct() )
                        {
                            sbScript.AppendFormat( "\t\t'{0}' : [\n", countryCode );

                            foreach( var definedValue in definedValues.Where( v => v.Value == countryCode).OrderBy( v => v.Order))
                            {
                                string match = definedValue.GetAttributeValue( "MatchRegEx" );
                                string replace = definedValue.GetAttributeValue( "FormatRegEx" );
                                if ( !string.IsNullOrWhiteSpace( match ) && !string.IsNullOrWhiteSpace( replace ) )
                                {
                                    sbScript.AppendFormat( "\t\t\t{{ 'match' : '{0}', 'replace' : '{1}' }},\n", match.Replace( @"\", @"\\" ), replace.Replace( @"\", @"\\" ) );
                                }
                            }

                            sbScript.Append( "\t\t],\n" );
                        }
                    }

                    sbScript.Append( "\t};\n" );

                    sbScript.Append( @"

    function phoneNumberBoxFormatNumber( tb ) {
        var countryCode = tb.closest('div.input-group').find('input:hidden').val();
        var origValue = tb.val();
        var number = tb.val().replace(/\D/g,'');
        var formats = phoneNumberFormats[countryCode];
        for ( var i = 0; i < formats.length; i++) {
            var matchRegex = new RegExp(formats[i].match);
            number = number.replace(matchRegex, formats[i].replace);
        }
        if (number != origValue) {
            tb.val(number);
        }
    }

    $('div.phone-number-box input:text').on('change', function(e) {
        phoneNumberBoxFormatNumber($(this));
    });

    $('div.phone-number-box ul.dropdown-menu a').click( function(e) {
        e.preventDefault();
        $(this).closest('div.input-group').find('input:hidden').val($(this).html());
        $(this).closest('div.input-group-btn').find('button').html($(this).html() + ' <span class=""caret""></span>');
        phoneNumberBoxFormatNumber($(this).closest('div.input-group').find('input:text').first());
    });
" );

                    script = sbScript.ToString();
                    rockPage.SaveSharedItem( "org.RockRMS.PhoneNumberBox.script", script );
                }

                ScriptManager.RegisterStartupScript( this, this.GetType(), "phone-number-box", script, true );
            }
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
                EnsureChildControls();
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _hfCountryCode = new HiddenField();
            _hfCountryCode.ID = this.ID + "_hfCountryCode";
            Controls.Add( _hfCountryCode );
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
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            string cssClass = this.CssClass;

            writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group phone-number-box" + cssClass );
            if ( this.Style[HtmlTextWriterStyle.Display] == "none" )
            {
                // render the display:none in the inputgroup div instead of the control itself
                writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                this.Style[HtmlTextWriterStyle.Display] = string.Empty;
            }
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            this.CssClass = string.Empty;

            bool renderCountryCodeButton = false;

            var definedType = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() );
            if ( definedType != null )
            {
                var countryCodes = definedType.DefinedValues.OrderBy( v => v.Order ).Select( v => v.Value ).Distinct();

                if ( countryCodes != null && countryCodes.Any() )
                {
                    if ( string.IsNullOrWhiteSpace( CountryCode ) )
                    {
                        CountryCode = countryCodes.FirstOrDefault();
                    }

                    if ( countryCodes.Count() > 1 )
                    {
                        renderCountryCodeButton = true;

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group-btn" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "btn btn-default dropdown-toggle" );
                        writer.AddAttribute( HtmlTextWriterAttribute.Type, "button" );
                        writer.AddAttribute( "data-toggle", "dropdown" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Button );

                        writer.Write( CountryCode + " " );

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "caret" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Span );
                        writer.RenderEndTag();

                        writer.RenderEndTag();  // Button

                        writer.AddAttribute( HtmlTextWriterAttribute.Class, "dropdown-menu" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Ul );

                        foreach ( string countryCode in countryCodes )
                        {
                            writer.RenderBeginTag( HtmlTextWriterTag.Li );

                            writer.AddAttribute( HtmlTextWriterAttribute.Href, "#" );
                            writer.RenderBeginTag( HtmlTextWriterTag.A );
                            writer.Write( countryCode );
                            writer.RenderEndTag();

                            writer.RenderEndTag();  // Li
                        }

                        writer.RenderEndTag();      // Ul

                        writer.RenderEndTag();      // div.input-group-btn
                    }
                }
            }

            if ( !renderCountryCodeButton )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "input-group-addon" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( "<i class='fa fa-phone-square'></i>" );
                writer.RenderEndTag();
            }

            _hfCountryCode.RenderControl( writer );

            ( (WebControl)this ).AddCssClass( "form-control" );
            if ( !string.IsNullOrWhiteSpace( Placeholder ) )
            {
                this.Attributes["placeholder"] = Placeholder;
            }

            base.RenderControl( writer );

            writer.RenderEndTag();              // div.input-group

            this.CssClass = cssClass;
        }   
    }
}