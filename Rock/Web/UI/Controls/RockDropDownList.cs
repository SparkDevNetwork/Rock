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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A <see cref="T:System.Web.UI.WebControls.DropDownList"/> control with an associated label.
    /// </summary>
    [ToolboxData( "<{0}:RockDropDownList runat=server></{0}:RockDropDownList>" )]
    public class RockDropDownList : DropDownList, IRockControl, IDisplayRequiredIndicator
    {
        #region IRockControl implementation (NOTE: uses a different Required property than other IRockControl controls)

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
            get
            {
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the Required indicator when Required=true
        /// </summary>
        /// <value>
        /// <c>true</c> if [display required indicator]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayRequiredIndicator
        {
            get { return ViewState["DisplayRequiredIndicator"] as bool? ?? true; }
            set { ViewState["DisplayRequiredIndicator"] = value; }
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
        /// Gets or sets the group of controls for which the control that is derived from the <see cref="T:System.Web.UI.WebControls.ListControl" /> class causes validation when it posts back to the server.
        /// </summary>
        /// <returns>The group of controls for which the derived <see cref="T:System.Web.UI.WebControls.ListControl" /> causes validation when it posts back to the server. The default is an empty string ("").</returns>
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

        /// <summary>
        /// Gets or sets a value indicating whether the dropdownlist should allow a searc when used for single select
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enhance for long list]; otherwise, <c>false</c>.
        /// </value>
        public bool EnhanceForLongLists
        {
            get { return ViewState["EnhanceForLongLists"] as bool? ?? false; }
            set { ViewState["EnhanceForLongLists"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [display drop as absolute].
        /// </summary>
        /// <value>
        /// <c>true</c> if [display drop as absolute]; otherwise, <c>false</c>.
        /// </value>
        public bool DisplayEnhancedAsAbsolute
        {
            get { return ViewState["DisplayEnhancedAsAbsolute"] as bool? ?? false; }
            set { ViewState["DisplayEnhancedAsAbsolute"] = value; }
        }

        /// <summary>
        /// Gets or sets the prepend text.
        /// </summary>
        /// <value>
        /// The prepend text.
        /// </value>
        [
        Bindable( false ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "Text that appears prepended to the front of the text box." )
        ]
        public string PrependText
        {
            get { return ViewState["PrependText"] as string ?? string.Empty; }
            set { ViewState["PrependText"] = value; }
        }

        /// <summary>
        /// Gets or sets the append text.
        /// </summary>
        /// <value>
        /// The append text.
        /// </value>
        [
        Bindable( false ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "Text that appears appended to the end of the text box." )
        ]
        public string AppendText
        {
            get { return ViewState["AppendText"] as string ?? string.Empty; }
            set { ViewState["AppendText"] = value; }
        }

        /// <summary>
        /// Gets the last selected value stored by the control state.
        /// </summary>
        /// <value>The last selected value stored by the control state.</value>
        public string LastSelectedValue
        {
            get { return ViewState["LastSelectedValue"] as string; }
            private set { ViewState["LastSelectedValue"] = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockDropDownList" /> class.
        /// </summary>
        public RockDropDownList()
        {
            RequiredFieldValidator = new RequiredFieldValidator();
            RequiredFieldValidator.ValidationGroup = this.ValidationGroup;
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init">Init</see> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs">EventArgs</see> object that contains the event data.</param>
        protected override void OnInit( System.EventArgs e )
        {
            Page.RegisterRequiresControlState( this );
            base.OnInit( e );
        }

        /// <summary>
        /// Saves any server control state changes that have occurred since the time the page was posted back to the server.
        /// </summary>
        /// <returns>
        /// Returns the server control's current state. If there is no state associated with the control, this method returns <span class="keyword"><span class="languageSpecificText"><span class="cs">null</span><span class="vb">Nothing</span><span class="cpp">nullptr</span></span></span><span class="nu">a null reference (<span class="keyword">Nothing</span> in Visual Basic)</span>.
        /// </returns>
        protected override object SaveControlState()
        {
            LastSelectedValue = SelectedValue;
            return SelectedValue;
        }

        /// <summary>
        /// Restores control-state information from a previous page request that was saved by the <see cref="M:System.Web.UI.Control.SaveControlState">SaveControlState</see> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object">Object</see> that represents the control state to be restored.</param>
        protected override void LoadControlState( object savedState )
        {
            if ( null != savedState )
            {
                LastSelectedValue = savedState.ToString();
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
            var renderInputGroup = PrependText.IsNotNullOrWhiteSpace() || AppendText.IsNotNullOrWhiteSpace();
            string cssClass = this.CssClass;

            if ( renderInputGroup )
            {
                writer.AddAttribute( "class", "input-group " + cssClass );
                if ( this.Style[HtmlTextWriterStyle.Display] == "none" )
                {
                    // render the display:none in the inputgroup div instead of the control itself
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                    this.Style[HtmlTextWriterStyle.Display] = string.Empty;
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                this.CssClass = string.Empty;
            }

            if ( !string.IsNullOrWhiteSpace( PrependText ) )
            {
                writer.AddAttribute( "class", "input-group-addon" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( PrependText );
                writer.RenderEndTag();
            }

            ( ( WebControl ) this ).AddCssClass( "form-control" );

            if ( EnhanceForLongLists )
            {
                ( ( WebControl ) this ).AddCssClass( "chosen-select" );

                if ( DisplayEnhancedAsAbsolute )
                {
                    ( ( WebControl ) this ).AddCssClass( "chosen-select-absolute" );
                }
            }

            base.RenderControl( writer );

            if ( !string.IsNullOrWhiteSpace( AppendText ) )
            {
                writer.AddAttribute( "class", "input-group-addon" );
                writer.RenderBeginTag( HtmlTextWriterTag.Span );
                writer.Write( AppendText );
                writer.RenderEndTag();
            }

            if ( renderInputGroup )
            {
                writer.RenderEndTag();  // input-group
                this.CssClass = cssClass;
            }

            RenderDataValidator( writer );
        }

        /// <summary>
        /// Renders any data validator.
        /// </summary>
        /// <param name="writer">The writer.</param>
        protected virtual void RenderDataValidator( HtmlTextWriter writer )
        {
        }

        /// <summary>
        /// Creates a collection to store child controls.
        /// </summary>
        /// <returns>
        /// Always returns an <see cref="T:System.Web.UI.EmptyControlCollection"/>.
        /// </returns>
        protected override ControlCollection CreateControlCollection()
        {
            // By default a DropDownList control does not allow adding of child controls.
            // This method needs to be overridden to allow this
            return new ControlCollection( this );
        }

        /// <summary>
        /// Loads the previously saved view state of the <see cref="T:System.Web.UI.WebControls.DetailsView" /> control.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the state of the <see cref="T:System.Web.UI.WebControls.ListControl" /> -derived control.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );
            var savedAttributes = ( ViewState["ItemAttributes"] as string ).FromJsonOrNull<List<Dictionary<string, string>>>();

            if ( savedAttributes == null )
            {
                return;
            }

            int itemPosition = 0;

            // make sure the list has the same number of items as it did when ViewState was saved
            if ( savedAttributes.Count == this.Items.Count )
            {
                // don't bother doing anything if nothing has any attributes
                if ( savedAttributes.Any( a => a.Count > 0 ) )
                {
                    foreach ( var item in this.Items.OfType<ListItem>() )
                    {
                        var itemAttributes = savedAttributes[itemPosition++];
                        foreach ( var a in itemAttributes )
                        {
                            item.Attributes.Add( a.Key, a.Value );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the current view state of the <see cref="T:System.Web.UI.WebControls.ListControl" /> -derived control and the items it contains.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Object" /> that contains the saved state of the <see cref="T:System.Web.UI.WebControls.ListControl" /> control.
        /// </returns>
        protected override object SaveViewState()
        {
            var itemAttributes = this.Items.OfType<ListItem>().Select( a => a.Attributes.Keys.OfType<string>().ToDictionary( k => k, v => a.Attributes[v] ) ).ToList();
            if ( itemAttributes.Any( a => a.Any() ) )
            {
                ViewState["ItemAttributes"] = itemAttributes.ToJson();
            }
            else
            {
                ViewState["ItemAttributes"] = null;
            }

            return base.SaveViewState();
        }

    }
}