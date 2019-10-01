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
using System.Web.UI;
using Rock;
using Rock.Web.UI;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A screen based keyboard control for either number pad, ten key pad (used in check-in)
    /// or QWERTY style keyboard.  Note: For it to work properly with decimal values, the
    /// ControlToTarget must be non-numeric TextBox.
    /// </summary>
    [ToolboxData( "<{0}:ScreenKeyboard runat=server></{0}:ScreenKeyboard>" )]
    public class ScreenKeyboard : Control, IPostBackEventHandler
    {
        #region Events

        /// <summary>
        /// Occurs when the enter key is pressed.
        /// </summary>
        public event EventHandler EnterPressed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the target control to receive the key presses.
        /// </summary>
        /// <value>
        /// The target control to receive the key presses.
        /// </value>
        public string ControlToTarget
        {
            get
            {
                return ( string ) ViewState["ControlToTarget"] ?? string.Empty;
            }
            set
            {
                ViewState["ControlToTarget"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the keyboard type to render.
        /// </summary>
        /// <value>
        /// The keyboard type to render.
        /// </value>
        public ScreenKeyboardType KeyboardType
        {
            get
            {
                return ( ScreenKeyboardType? ) ViewState["KeyboardType"] ?? ScreenKeyboardType.NumberPad;
            }
            set
            {
                ViewState["KeyboardType"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show number pad enter].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show number pad enter]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowNumberPadEnter
        {
            get
            {
                return ( bool? ) ViewState["ShowNumberPadEnter"] ?? false;
            }
            set
            {
                ViewState["ShowNumberPadEnter"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the HTML content to be used on the Enter button.
        /// </summary>
        /// <value>
        /// The HTML content to be used on the Enter button.
        /// </value>
        public string EnterHtmlContent
        {
            get
            {
                return ( string ) ViewState["EnterHtmlContent"] ?? string.Empty;
            }
            set
            {
                ViewState["EnterHtmlContent"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the text to be displayed on the enter button.
        /// </summary>
        /// <value>
        /// The text to be displayed on the enter button.
        /// </value>
        public string EnterText
        {
            get
            {
                return ( string ) ViewState["EnterText"] ?? "Enter";
            }
            set
            {
                ViewState["EnterText"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the key CSS class.
        /// </summary>
        /// <value>
        /// The key CSS class.
        /// </value>
        public string KeyCssClass
        {
            get
            {
                return ( string ) ViewState["KeyCssClass"] ?? "btn-default";
            }
            set
            {
                ViewState["KeyCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the enter key CSS class override.
        /// </summary>
        /// <value>
        /// The enter key CSS class override.
        /// </value>
        public string EnterKeyCssClass
        {
            get
            {
                return ( string ) ViewState["EnterKeyCssClass"] ?? "";
            }
            set
            {
                ViewState["EnterKeyCssClass"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the keyboard wrapper CSS class override.
        /// </summary>
        /// <value>
        /// The enter key CSS class override.
        /// </value>
        public string WrapperCssClass
        {
            get
            {
                return ( string ) ViewState["WrapperCssClass"] ?? "";
            }
            set
            {
                ViewState["WrapperCssClass"] = value;
            }
        }

        #endregion

        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( Page is RockPage rockPage )
            {
                rockPage.AddScriptLink( "~/Scripts/Rock/Controls/screen-keyboard.js" );
                rockPage.AddCSSLink( "~/Styles/screenkeyboard/screen-keyboard.css" );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            string script = $"Rock.controls.screenKeyboard.initialize({{ id: '{ ClientID }', postback: '{ UniqueID }' }});";

            ScriptManager.RegisterStartupScript( this, GetType(), $"screen-keyboard-init-{ ClientID }", script, true );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        /// <exception cref="System.InvalidOperationException">Could not find control '{ ControlToTarget }</exception>
        public override void RenderControl( HtmlTextWriter writer )
        {
            var target = FindControl( ControlToTarget );

            if ( target == null )
            {
                throw new InvalidOperationException( $"Could not find control '{ ControlToTarget }' to target for keyboard events." );
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Id, ClientID );
            writer.AddAttribute( HtmlTextWriterAttribute.Class, $"screen-keyboard { KeyboardType.ToString().ToLower() } {WrapperCssClass}" );
            writer.AddAttribute( "data-target", target.ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                if ( KeyboardType == ScreenKeyboardType.NumberPad )
                {
                    RenderNumberPadContent( writer );
                }
                else if (KeyboardType == ScreenKeyboardType.Qwerty )
                {
                    RenderQwertyContent( writer );
                }
                else if ( KeyboardType == ScreenKeyboardType.TenKey )
                {
                    RenderTenKeyContent( writer );
                }
            }
            writer.RenderEndTag();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Renders the content of a ten key (0-9) pad with a clear and backspace button.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected void RenderTenKeyContent( HtmlTextWriter writer )
        {
            RenderPadContent( writer, isTenKey: true );
        }

        /// <summary>
        /// Renders the content of the number pad.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected void RenderNumberPadContent( HtmlTextWriter writer )
        {
            RenderPadContent( writer, isTenKey: false );
        }

        /// <summary>
        /// Renders the content of the key pad (
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="isTenKey">if set to <c>true</c> [is ten key].</param>
        protected void RenderPadContent( HtmlTextWriter writer, bool isTenKey )
        {
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                RenderDigit( writer, "1", "1" );
                RenderDigit( writer, "2", "2" );
                RenderDigit( writer, "3", "3" );
            }
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                RenderDigit( writer, "4", "4" );
                RenderDigit( writer, "5", "5" );
                RenderDigit( writer, "6", "6" );
            }
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                RenderDigit( writer, "7", "7" );
                RenderDigit( writer, "8", "8" );
                RenderDigit( writer, "9", "9" );
            }
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                if ( isTenKey )
                {
                    RenderCommand( writer, "clear", "Clear" );
                }
                else
                {
                    RenderDigit( writer, ".", "." );

                }

                RenderDigit( writer, "0", "0" );                

                if ( isTenKey )
                {
                    RenderCommand( writer, "backspace", "<i class='fa fa-backspace'></i>" );
                }
                else
                {
                    RenderCommand( writer, "clear", "<i class='fa fa-times'></i>" );
                }
            }
            writer.RenderEndTag();

            if ( ShowNumberPadEnter )
            {
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                {
                    RenderCommand( writer, "enter", EnterHtmlContent != string.Empty ? EnterHtmlContent : EnterText.EncodeHtml(), "key-3x", EnterKeyCssClass );
                }
            }
        }

        /// <summary>
        /// Renders the content of the qwerty keyboard.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected void RenderQwertyContent( HtmlTextWriter writer )
        {
            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                RenderDigit( writer, "`", "~" );
                RenderDigit( writer, "1", "!" );
                RenderDigit( writer, "2", "@" );
                RenderDigit( writer, "3", "#" );
                RenderDigit( writer, "4", "$" );
                RenderDigit( writer, "5", "%" );
                RenderDigit( writer, "6", "^" );
                RenderDigit( writer, "7", "&" );
                RenderDigit( writer, "8", "*" );
                RenderDigit( writer, "9", "(" );
                RenderDigit( writer, "0", ")" );
                RenderDigit( writer, "-", "_" );
                RenderDigit( writer, "=", "+" );
                RenderCommand( writer, "backspace", "<i class='fa fa-arrow-left'></i>", "key-2x" );
            }
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                RenderCommand( writer, "tab", "Tab", "key-1halfx" );
                RenderDigit( writer, "q", "Q" );
                RenderDigit( writer, "w", "W" );
                RenderDigit( writer, "e", "E" );
                RenderDigit( writer, "r", "R" );
                RenderDigit( writer, "t", "T" );
                RenderDigit( writer, "y", "Y" );
                RenderDigit( writer, "u", "U" );
                RenderDigit( writer, "i", "I" );
                RenderDigit( writer, "o", "O" );
                RenderDigit( writer, "p", "P" );
                RenderDigit( writer, "[", "{" );
                RenderDigit( writer, "]", "}" );
                RenderDigit( writer, "\\", "|", "key-1halfx" );
            }
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                RenderCommand( writer, "caps", "Caps", "key-2x" );
                RenderDigit( writer, "a", "A" );
                RenderDigit( writer, "s", "S" );
                RenderDigit( writer, "d", "D" );
                RenderDigit( writer, "f", "F" );
                RenderDigit( writer, "g", "G" );
                RenderDigit( writer, "h", "H" );
                RenderDigit( writer, "j", "J" );
                RenderDigit( writer, "k", "K" );
                RenderDigit( writer, "l", "L" );
                RenderDigit( writer, ";", ":" );
                RenderDigit( writer, "'", "\"" );
                RenderCommand( writer, "enter", EnterHtmlContent != string.Empty ? EnterHtmlContent : EnterText.EncodeHtml(), "key-2x", EnterKeyCssClass );
            }
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                RenderCommand( writer, "shift", "Shift", "key-2halfx" );
                RenderDigit( writer, "z", "Z" );
                RenderDigit( writer, "x", "X" );
                RenderDigit( writer, "c", "C" );
                RenderDigit( writer, "v", "V" );
                RenderDigit( writer, "b", "B" );
                RenderDigit( writer, "n", "N" );
                RenderDigit( writer, "m", "M" );
                RenderDigit( writer, ",", "<" );
                RenderDigit( writer, ".", ">" );
                RenderDigit( writer, "/", "?" );
                RenderCommand( writer, "shift", "Shift", "key-2halfx" );
            }
            writer.RenderEndTag();

            writer.RenderBeginTag( HtmlTextWriterTag.Div );
            {
                RenderDigit( writer, " ", " ", "key-7x" );
            }
            writer.RenderEndTag();
        }

        /// <summary>
        /// Renders a digit key.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="value">The value.</param>
        /// <param name="altValue">The alt value.</param>
        /// <param name="sizeClass">The size class.</param>
        /// <param name="styleClass">The style class.</param>
        protected void RenderDigit( HtmlTextWriter writer, string value, string altValue, string sizeClass = "", string styleClass = null )
        {
            if ( string.IsNullOrWhiteSpace( styleClass ) )
            {
                styleClass = KeyCssClass;
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, $"key { sizeClass } { styleClass } digit" );
            writer.AddAttribute( "data-value", value );
            writer.AddAttribute( "data-alt-value", altValue );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            {
                writer.WriteEncodedText( value );
            }
            writer.RenderEndTag();
        }

        /// <summary>
        /// Renders a command key.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="command">The command.</param>
        /// <param name="content">The content.</param>
        /// <param name="sizeClass">The size class.</param>
        /// <param name="styleClass">The style class.</param>
        protected void RenderCommand( HtmlTextWriter writer, string command, string content, string sizeClass = "", string styleClass = null )
        {
            if ( string.IsNullOrWhiteSpace( styleClass ) )
            {
                styleClass = KeyCssClass;
            }

            writer.AddAttribute( HtmlTextWriterAttribute.Class, $"key { sizeClass } { styleClass } command" );
            writer.AddAttribute( "data-command", command );
            writer.RenderBeginTag( HtmlTextWriterTag.Span );
            {
                writer.Write( content );
            }
            writer.RenderEndTag();
        }

        #endregion

        #region IPostBackEventHandler

        /// <summary>
        /// When implemented by a class, enables a server control to process an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        public void RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "enter" )
            {
                EnterPressed?.Invoke( this, new EventArgs() );
            }
        }

        #endregion
    }

    /// <summary>
    /// The Enum representing all the types of keyboards this control can render.
    /// </summary>
    public enum ScreenKeyboardType
    {
        /// <summary>
        /// The number pad
        /// </summary>
        NumberPad,
        /// <summary>
        /// The qwerty keyboard
        /// </summary>
        Qwerty,
        /// <summary>
        /// The ten key (like a phone dialer) pad
        /// </summary>
        TenKey
    }
}
