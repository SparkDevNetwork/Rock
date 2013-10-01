//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    public class ButtonDropDownList : ListControl, ILabeledControl
    {
        /// <summary>
        /// The label
        /// </summary>
        protected Literal label;

        private String _btnTitle = string.Empty;
        private HtmlGenericControl _divControl;
        private HtmlGenericControl _btnSelect;
        private HiddenField _hfSelectedItemId;
        private HiddenField _hfSelectedItemText;
        private HtmlGenericControl _listControl;

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        public string Label
        {
            get
            {
                EnsureChildControls();
                return label.Text;
            }
            set
            {
                EnsureChildControls();
                label.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title
        {
            get
            {
                return _btnTitle;
            }
            set
            {
                _btnTitle = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        public string FieldName
        {
            get { return label.Text; }
            set { label.Text = value; }
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
                var textHtml = $el.html() + "" <span class='caret'></span>"";
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
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

			Controls.Clear();

            label = new Literal();
            Controls.Add( label );

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
            _btnSelect.ClientIDMode = ClientIDMode.Static;
            _btnSelect.ID = string.Format( "ButtonDropDown_btn_{0}", this.ID );
            _btnSelect.Attributes["class"] = "btn dropdown-toggle";
            _btnSelect.Attributes["data-toggle"] = "dropdown";

            _divControl.Controls.Add( _btnSelect );

            _listControl = new HtmlGenericControl( "ul" );
            _listControl.Attributes["class"] = "dropdown-menu";

            Controls.Add( _divControl );
            Controls.Add( _hfSelectedItemId );
            Controls.Add( _hfSelectedItemText );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                bool renderControlGroupDiv = !string.IsNullOrEmpty( Label );

                if ( renderControlGroupDiv )
                {
                    writer.AddAttribute( "class", "form-group" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Div );

                    writer.AddAttribute( HtmlTextWriterAttribute.For, this.ClientID );
                    writer.RenderBeginTag( HtmlTextWriterTag.Label );
                    label.RenderControl( writer );
                    writer.RenderEndTag();

                    writer.RenderBeginTag( HtmlTextWriterTag.Div );
                }

                foreach ( var item in this.Items.OfType<ListItem>() )
                {
                    string controlHtmlFormat = "<li><a href='#' data-id='{0}'>{1}</a></li>";
                    _listControl.Controls.Add( new LiteralControl { Text = string.Format( controlHtmlFormat, item.Value, item.Text ) } );
                }

                _divControl.Controls.Add( _listControl );

                string selectedText = SelectedItem != null ? SelectedItem.Text : _btnTitle;
                _btnSelect.Controls.Clear();
                _btnSelect.Controls.Add( new LiteralControl { Text = string.Format( "{0} <span class='caret'></span>", selectedText ) } );

                _divControl.RenderControl( writer );

                _hfSelectedItemId.RenderControl( writer );
                _hfSelectedItemText.RenderControl( writer );

                if ( renderControlGroupDiv )
                {
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                }
            }
        }

        /// <summary>
        /// Occurs when [selection changed].
        /// </summary>
        public event EventHandler SelectionChanged;
    }
}
