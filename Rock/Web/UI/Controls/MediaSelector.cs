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
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Enums.Controls;
using Rock.Media;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:MediaSelector runat=server></{0}:MediaSelector>" )]
    public class MediaSelector : CompositeControl, IRockControl
    {
        #region Constants

        /// <summary>
        /// The player controls.
        /// </summary>
        private static readonly string PlayerControls = string.Join( ",",
            MediaPlayerControls.PlayLarge,
            MediaPlayerControls.Play,
            MediaPlayerControls.Progress,
            MediaPlayerControls.CurrentTime,
            MediaPlayerControls.Captions,
            MediaPlayerControls.PictureInPicture,
            MediaPlayerControls.Airplay,
            MediaPlayerControls.Fullscreen
        );

        #endregion

        private List<MediaPlayer> _mediaPlayerControls = new List<MediaPlayer>();
        private List<RockCheckBox> _checkBoxControls = new List<RockCheckBox>();
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
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }
            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
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
                return CustomValidator != null ? CustomValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( CustomValidator != null )
                {
                    CustomValidator.ErrorMessage = value;
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
                if ( this.Required )
                {
                    // The control must contain a valid address.
                    return CustomValidator == null || CustomValidator.IsValid;
                }

                return true;
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
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

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
                return CustomValidator.ValidationGroup;
            }

            set
            {
                EnsureChildControls();

                CustomValidator.ValidationGroup = value;

                if ( this.RequiredFieldValidator != null )
                {
                    this.RequiredFieldValidator.ValidationGroup = value;
                }

                foreach ( var checkBox in _checkBoxControls )
                {
                    checkBox.ValidationGroup = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get
            {
                EnsureChildControls();
                return _checkBoxControls.Where( a => a.Checked ).Select( a => a.Text ).FirstOrDefault();
            }
            set
            {
                EnsureChildControls();
                var commaSeperativeValue = value.SplitDelimitedValues( "," );
                foreach ( var item in _checkBoxControls )
                {
                    if ( commaSeperativeValue.Contains( item.Text ) )
                    {
                        item.Checked = true;
                    }
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the custom keys.
        /// </summary>
        /// <value>
        /// The custom keys.
        /// </value>
        public Dictionary<string, string> MediaItems
        {
            get { return ViewState["MediaItems"] as Dictionary<string, string>; }
            set { ViewState["MediaItems"] = value; RecreateChildControls(); }
        }

        /// <summary>
        /// Gets or sets the custom validator.
        /// </summary>
        /// <value>
        /// The custom validator.
        /// </value>
        public CustomValidator CustomValidator { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>
        /// The mode.
        /// </value>
        public MediaSelectorMode Mode
        {
            get
            {
                return ViewState["MediaSelectorMode"] as MediaSelectorMode? ?? MediaSelectorMode.Image;
            }

            set
            {
                ViewState["MediaSelectorMode"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public string ItemWidth
        {
            get
            {
                return ViewState["ItemWidth"].ToStringSafe().IsNullOrWhiteSpace() ? "50px" : ViewState["ItemWidth"].ToString();
            }

            set
            {
                ViewState["ItemWidth"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSelector"/> class.
        /// </summary>
        public MediaSelector() : base()
        {
            this.HelpBlock = new HelpBlock();
            this.WarningBlock = new WarningBlock();
            this.RequiredFieldValidator = new HiddenFieldValidator();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            _mediaPlayerControls.Clear();
            _checkBoxControls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            if ( MediaItems != null )
            {
                foreach ( var mediaItem in this.MediaItems.Where( a => a.Key.IsNotNullOrWhiteSpace() && a.Value.IsNotNullOrWhiteSpace() ) )
                {
                    if ( Mode == MediaSelectorMode.Audio )
                    {
                        var mediaPlayer = new MediaPlayer();
                        mediaPlayer.ID = $"mp_{MakeKeyValid( mediaItem.Key )}";
                        mediaPlayer.MediaUrl = mediaItem.Value;
                        mediaPlayer.PlayerControls = PlayerControls;
                        Controls.Add( mediaPlayer );
                        _mediaPlayerControls.Add( mediaPlayer );
                    }

                    var checkBox = new RockCheckBox();
                    checkBox.ID = $"cb_{MakeKeyValid( mediaItem.Key )}";
                    checkBox.Text = mediaItem.Key;
                    Controls.Add( checkBox );
                    _checkBoxControls.Add( checkBox );
                }
            }

            // Add custom validator
            CustomValidator = new CustomValidator();
            CustomValidator.ID = this.ID + "_cfv";
            CustomValidator.ClientValidationFunction = "Rock.controls.mediaSelector.clientValidate";
            CustomValidator.CssClass = "validation-error help-inline";
            CustomValidator.Enabled = true;
            CustomValidator.Display = ValidatorDisplay.Dynamic;

            CustomValidator.ServerValidate += _CustomValidator_ServerValidate;
            Controls.Add( CustomValidator );
        }

        private void _CustomValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            if ( !this.Required )
            {
                return;
            }

            var isValid = Value.IsNotNullOrWhiteSpace();

            if ( !isValid )
            {
                args.IsValid = false;
                return;
            }

            if ( _checkBoxControls.Where( a => a.Checked ).Count() > 1 )
            {
                args.IsValid = false;
                return;
            }
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
        public virtual void RenderBaseControl( HtmlTextWriter writer )
        {
            writer.AddAttribute( HtmlTextWriterAttribute.Class, "d-flex flex-nowrap js-mediaselector" );
            writer.AddAttribute( HtmlTextWriterAttribute.Id, this.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            if ( this.MediaItems != null && this.MediaItems.Any() )
            {
                foreach ( var item in this.MediaItems.Where( a => a.Key.IsNotNullOrWhiteSpace() && a.Value.IsNotNullOrWhiteSpace() ) )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "well well-message flex-eq mr-2 p-2 js-media-selector-item" );
                    writer.AddAttribute( HtmlTextWriterAttribute.Style, "display: grid; text-align: left;" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    var itemWidth = Unit.Parse( ItemWidth );

                    if ( Mode == MediaSelectorMode.Audio )
                    {
                        foreach ( MediaPlayer mediaPlayer in _mediaPlayerControls.Where( a => a.ID == $"mp_{MakeKeyValid( item.Key )}" ) )
                        {
                            mediaPlayer.Width = itemWidth;
                            mediaPlayer.RenderControl( writer );
                        }
                    }
                    else
                    {
                        writer.Write( $"<img src=\"{item.Value}\" alt=\"#\" class=\"img-fluid\" style=\"width:{itemWidth.ToString()};\">" );
                    }

                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "custom-control-label align-self-end" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                    foreach ( RockCheckBox rockCheckBox in _checkBoxControls.Where( a => a.ID == $"cb_{MakeKeyValid( item.Key )}" ) )
                    {
                        rockCheckBox.RenderControl( writer );
                    }

                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
            }

            if ( this.Required )
            {
                this.CustomValidator.Enabled = true;
                if ( string.IsNullOrWhiteSpace( this.CustomValidator.ErrorMessage ) )
                {
                    this.CustomValidator.ErrorMessage = this.Label + " is required.";
                }
            }
            else
            {
                this.CustomValidator.Enabled = false;
            }
            CustomValidator.RenderControl( writer );

            writer.RenderEndTag();
            string script = string.Format( @"
    function setBackgroundColor(checkboxControl){{
        if (checkboxControl.prop('checked')) {{
            checkboxControl.closest('.js-media-selector-item').addClass( 'well-message-danger' );
        }}
        else {{
            checkboxControl.closest('.js-media-selector-item').removeClass( 'well-message-danger' );
        }}
    }}

    $( "".js-media-selector-item input[type=checkbox]"").on('click', function() {{
            onlyOne($(this));
    }})

    function onlyOne(checkbox) {{
       $(""input[id^='{0}']:checked"").each(function() {{
            $(this).prop(""checked"", false);
            setBackgroundColor($(this));
        }});

        $(checkbox).prop(""checked"", true);
        setBackgroundColor(checkbox);
    }}

    var checkboxes = $(""input[id^='{0}']:checked"");

    for (var i = 0; i < checkboxes.length; i++)
    {{
        setBackgroundColor($(checkboxes[i]));
    }}
    ", this.ClientID );

            ScriptManager.RegisterStartupScript( this, typeof( MediaSelector ), "MediaSelectorScript_" + this.ClientID, script, true );
        }

        /// <summary>
        /// Replace space with underscore
        /// Remove unsafe and reserved characters ; / ? : @ = &amp; &gt; &lt; # % " { } | \ ^ [ ] `
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        internal static string MakeKeyValid( string key )
        {
            key = key
                .Trim()
                .ToLower()
                .Replace( "&nbsp;", "_" )
                .Replace( "&#160;", "_" )
                .Replace( "&ndash;", "_" )
                .Replace( "&#8211;", "_" )
                .Replace( "&mdash;", "_" )
                .Replace( "&#8212;", "_" )
                .Replace( "-", "_" )
                .Replace( " ", "_" );

            // Remove multiple -- in a row from the slug.
            key = Regex.Replace( key, @"-+", "-" );

            // Remove any none alphanumeric characters, this negates the need
            // for .RemoveInvalidReservedUrlChars()
            key = Regex.Replace( key, @"[^a-zA-Z0-9 -]", string.Empty );

            return key
                    .TrimEnd( '_' );
        }
    }
}