//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:ButtonDropDownList runat=server></{0}:ButtonDropDownList>" )]
    public class ButtonDropDownList : ListControl
    {
        protected String btnTitle = "";
        private HtmlGenericControl divControl;
        private HtmlGenericControl btnSelect;
        private HiddenField hfSelectedItemId;
        private HiddenField hfSelectedItemText;
        private HtmlGenericControl listControl;
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( this.Page.IsPostBack )
            {
                string[] eventArgs = ( this.Page.Request.Form["__EVENTARGUMENT"] ?? string.Empty ).Split( new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries );

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

            string scriptFormat = @"
            $('#ButtonDropDown_{0} .dropdown-menu a').click(function () {{
                {{
                    var text =  $(this).html();
                    var textHtml = $(this).html() + "" <span class='caret'></span>"";
                    var idvalue = $(this).attr('idvalue');
                    $('#ButtonDropDown_btn_{0}').html(textHtml);
                    $('#hfSelectedItemId_{0}').val(idvalue);
                    $('#hfSelectedItemText_{0}').val(text);
                    {1}
                    return false;
                }}
            }});";

            string postbackScript = string.Empty;
            if ( SelectionChanged != null )
            {
                postbackScript = string.Format( "__doPostBack('{1}', '{0}=' + idvalue);", this.ID, postbackControlId );
            }

            string script = string.Format( scriptFormat, this.ID, postbackScript );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "buttondropdownlist-script-" + this.ID.ToString(), script, true );
        }

        /// <summary>
        /// Gets the selected item with the lowest index in the list control.
        /// </summary>
        /// <returns>A <see cref="T:System.Web.UI.WebControls.ListItem" /> that represents the lowest indexed item selected from the list control. The default is null.</returns>
        public override ListItem SelectedItem
        {
            get
            {
                ListItem result = Items.FindByValue( hfSelectedItemId.Value );
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
                return hfSelectedItemId.Value;
            }

            set
            {
                hfSelectedItemId.Value = value;
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

        public string Title
        {
            get
            {
                return btnTitle;
            }
            set
            {
                btnTitle = value;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();

            divControl = new HtmlGenericControl( "div" );
            divControl.Attributes["class"] = "btn-group";
            divControl.ClientIDMode = ClientIDMode.Static;
            divControl.ID = string.Format( "ButtonDropDown_{0}", this.ID );

            hfSelectedItemId = new HiddenField();
            hfSelectedItemId.ClientIDMode = ClientIDMode.Static;
            hfSelectedItemId.ID = string.Format( "hfSelectedItemId_{0}", this.ID );

            hfSelectedItemText = new HiddenField();
            hfSelectedItemText.ClientIDMode = ClientIDMode.Static;
            hfSelectedItemText.ID = string.Format( "hfSelectedItemText_{0}", this.ID );

            btnSelect = new HtmlGenericControl( "button" );
            btnSelect.ClientIDMode = ClientIDMode.Static;
            btnSelect.ID = string.Format( "ButtonDropDown_btn_{0}", this.ID );
            btnSelect.Attributes["class"] = "btn dropdown-toggle";
            btnSelect.Attributes["data-toggle"] = "dropdown";
            
            divControl.Controls.Add( btnSelect );

            listControl = new HtmlGenericControl( "ul" );
            listControl.Attributes["class"] = "dropdown-menu";

            Controls.Add( divControl );
            Controls.Add( hfSelectedItemId );
            Controls.Add( hfSelectedItemText );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            foreach ( var item in this.Items.OfType<ListItem>() )
            {
                string controlHtmlFormat = "<li><a href='#' idvalue='{0}'>{1}</a></li>";
                listControl.Controls.Add( new LiteralControl { Text = string.Format( controlHtmlFormat, item.Value, item.Text ) } );
            }

            divControl.Controls.Add( listControl );

            string selectedText = SelectedItem != null ? SelectedItem.Text : btnTitle;
            btnSelect.Controls.Clear();
            btnSelect.Controls.Add( new LiteralControl { Text = string.Format( "{0} <span class='caret'></span>", selectedText ) } );

            divControl.RenderControl( writer );

            hfSelectedItemId.RenderControl( writer );
            hfSelectedItemText.RenderControl( writer );
        }

        /// <summary>
        /// Occurs when [selection changed].
        /// </summary>
        public event EventHandler SelectionChanged;
    }
}
