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
using System.Linq;
using System.Web.UI;
using System.ComponentModel;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:ButtonDropDownList runat=server></{0}:ButtonDropDownList>" )]
    public class ButtonDropDownList : ListControl, IRockControl
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

        #region Controls

        private HtmlGenericControl _divControl;
        private HtmlGenericControl _btnSelect;
        private HiddenField _hfSelectedItemId;
        private HiddenField _hfSelectedItemText;
        private HtmlGenericControl _listControl;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get { return ViewState["Title"] as string ?? string.Empty; }
            set { ViewState["Title"] = value; }
        }

        /// <summary>
        /// Gets the selected item with the lowest index in the list control.
        /// </summary>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.ListItem" /> that represents the lowest indexed item selected from the list control. The default is null.</returns>
        public override ListItem SelectedItem
        {
            get
            {
                ListItem result = Items.FindByValue( _hfSelectedItemId.Value );
                return result;
            }
        }

        /// <summary>
        /// Gets the value of the selected item in the list control, or selects the item in the list control that contains the specified value.
        /// </summary>
        /// <returns>The value of the selected item in the list control. The default is an empty string ("").</returns>
        public override string SelectedValue
        {
            get
            {
                return _hfSelectedItemId.Value;
            }

            set
            {
                _hfSelectedItemId.Value = value;
                base.SelectedValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the lowest ordinal index of the selected items in the list.
        /// </summary>
        /// <returns>The lowest ordinal index of the selected items in the list. The default is -1, which indicates that nothing is selected.</returns>
        public override int SelectedIndex
        {
            get
            {
                return Items.IndexOf( SelectedItem );
            }

            set
            {
                if ( value >= 0 && value < Items.Count )
                {
                    SelectedValue = Items[value].Value;
                }
                else
                {
                    SelectedValue = string.Empty;
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ButtonDropDownList"/> class.
        /// </summary>
        public ButtonDropDownList()
            : base()
        {
            RequiredFieldValidator = new HiddenFieldValidator();
            RequiredFieldValidator.ValidationGroup = this.ValidationGroup;
            HelpBlock = new HelpBlock();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( this.Page.IsPostBack )
            {
                string[] eventArgs = ( this.Page.Request.Form["__EVENTARGUMENT"] ?? string.Empty ).Split( new[] { "=" }, StringSplitOptions.RemoveEmptyEntries );

                if ( eventArgs.Length == 2 )
                {
                    if ( eventArgs[0] == this.ID )
                    {
                        if ( SelectionChanged != null )
                        {
                            SelectionChanged( this, new EventArgs() );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            EnsureChildControls();

            var updatePanel = this.ParentUpdatePanel();
            string postbackControlId;
            if ( updatePanel != null )
            {
                postbackControlId = updatePanel.ClientID;
            }
            else
            {
                postbackControlId = this.ID;
            }

            // Caching $(this) into $el for efficiency purposes, and supressing the default
            // <a> click event to prevent the browser from appending '#' to the URL and 
            // causing the window to jump to the top of the.
            const string scriptFormat = @"
            $('#ButtonDropDown_{0} .dropdown-menu a').click(function (e) {{
                e.preventDefault();
                var $el = $(this);
                var text =  $el.html();
                var textHtml = $el.html() + "" <span class='fa fa-caret-down'></span>"";
                var idvalue = $el.attr('data-id');
                $('#ButtonDropDown_btn_{0}').html(textHtml);
                $('#hfSelectedItemId_{0}').val(idvalue);
                $('#hfSelectedItemText_{0}').val(text);
                {1}
            }});";

            string postbackScript = string.Empty;
            if ( SelectionChanged != null )
            {
                postbackScript = string.Format( "__doPostBack('{1}', '{0}=' + idvalue);", this.ID, postbackControlId );
            }

            string script = string.Format( scriptFormat, this.ID, postbackScript );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "buttondropdownlist-script-" + this.ID, script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _divControl = new HtmlGenericControl( "div" );
            _divControl.Attributes["class"] = "btn-group";
            _divControl.ClientIDMode = ClientIDMode.Static;
            _divControl.ID = string.Format( "ButtonDropDown_{0}", this.ID );

            _hfSelectedItemId = new HiddenField();
            _hfSelectedItemId.ClientIDMode = ClientIDMode.Static;
            _hfSelectedItemId.ID = string.Format( "hfSelectedItemId_{0}", this.ID );

            _hfSelectedItemText = new HiddenField();
            _hfSelectedItemText.ClientIDMode = ClientIDMode.Static;
            _hfSelectedItemText.ID = string.Format( "hfSelectedItemText_{0}", this.ID );

            _btnSelect = new HtmlGenericControl( "button" );
            _divControl.Controls.Add( _btnSelect );
            _btnSelect.ClientIDMode = ClientIDMode.Static;
            _btnSelect.ID = string.Format( "ButtonDropDown_btn_{0}", this.ID );
            _btnSelect.Attributes["type"] = "button";
            _btnSelect.Attributes["class"] = "btn btn-default dropdown-toggle";
            _btnSelect.Attributes["data-toggle"] = "dropdown";

            _listControl = new HtmlGenericControl( "ul" );
            _divControl.Controls.Add( _listControl );
            _listControl.Attributes["class"] = "dropdown-menu";

            Controls.Add( _divControl );
            Controls.Add( _hfSelectedItemId );
            Controls.Add( _hfSelectedItemText );

            RequiredFieldValidator.InitialValue = string.Empty;
            RequiredFieldValidator.ControlToValidate = _hfSelectedItemId.ID;
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
            writer.AddAttribute( "class", "controls" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            string selectedText = SelectedItem != null ? SelectedItem.Text : Title;
            _btnSelect.Controls.Clear();
            _btnSelect.Controls.Add( new LiteralControl { Text = string.Format( "{0} <span class='fa fa-caret-down'></span>", selectedText ) } );

            foreach ( var item in this.Items.OfType<ListItem>() )
            {
                string controlHtmlFormat = "<li><a href='#' data-id='{0}'>{1}</a></li>";
                _listControl.Controls.Add( new LiteralControl { Text = string.Format( controlHtmlFormat, item.Value, item.Text ) } );
            }
            _divControl.RenderControl( writer );

            _hfSelectedItemId.RenderControl( writer );
            _hfSelectedItemText.RenderControl( writer );

            writer.RenderEndTag();
        }

        /// <summary>
        /// Occurs when [selection changed].
        /// </summary>
        public event EventHandler SelectionChanged;
    }
}
