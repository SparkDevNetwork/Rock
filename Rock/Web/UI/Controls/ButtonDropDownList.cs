//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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
    public class ButtonDropDownList : CompositeControl
    {
        private HtmlGenericControl divControl;
        private HtmlGenericControl btnSelect;
        private HiddenField hfSelectedItemId;
        private HtmlGenericControl listControl;
        private ListItemCollection items = new ListItemCollection();

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
                        hfSelectedItemId.Value = eventArgs[1];
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
                postbackControlId = updatePanel.ID;
            }
            else
            {
                postbackControlId = this.ID;
            }

            string scriptFormat = @"
$('#ButtonDropDown_{0} .dropdown-menu a').click(function () {{
    {{
        var text = $(this).html() + "" <span class='caret'></span>"";
        var idvalue = $(this).attr('idvalue');
        $('#ButtonDropDown_btn_{0}').html(text);
        debugger
        $('#hfSelectedItemId_{0}').val(idvalue);
        {1}
    }}
}});";
            
            string postbackScript = string.Empty;
            if ( SelectionChanged != null )
            {
                postbackScript = string.Format("__doPostBack('{1}', '{0}=' + idvalue);", this.ID, postbackControlId);        
            }

            string script = string.Format( scriptFormat, this.ID, postbackScript );

            ScriptManager.RegisterStartupScript( this, this.GetType(), "buttondropdownlist-script-" + this.ID.ToString(), script, true );
        }

        /// <summary>
        /// Saves any state that was modified after the <see cref="M:System.Web.UI.WebControls.Style.TrackViewState" /> method was invoked.
        /// </summary>
        /// <returns>
        /// An object that contains the current view state of the control; otherwise, if there is no view state associated with the control, null.
        /// </returns>
        protected override object SaveViewState()
        {
            EnsureChildControls();
            this.ViewState["SelectedItemId"] = hfSelectedItemId.Value;
            return base.SaveViewState();
        }

        /// <summary>
        /// Restores view-state information from a previous request that was saved with the <see cref="M:System.Web.UI.WebControls.WebControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An object that represents the control state to restore.</param>
        protected override void LoadViewState( object savedState )
        {
            EnsureChildControls();
            base.LoadViewState( savedState );
            hfSelectedItemId.Value = this.ViewState["SelectedItemId"] as string;
        }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public ListItemCollection Items
        {
            get
            {
                return items;
            }

            set
            {
                items = value;
            }
        }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>
        /// The selected item.
        /// </value>
        public string SelectedItemId
        {
            get
            {
                EnsureChildControls();
                return hfSelectedItemId.Value;
            }

            set
            {
                EnsureChildControls();
                hfSelectedItemId.Value = value;
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

            btnSelect = new HtmlGenericControl( "button" );
            btnSelect.ClientIDMode = ClientIDMode.Static;
            btnSelect.ID = string.Format( "ButtonDropDown_btn_{0}", this.ID );
            btnSelect.Attributes["class"] = "btn dropdown-toggle";
            btnSelect.Attributes["data-toggle"] = "dropdown";
            string selectedText = "TODO";
            btnSelect.Controls.Add( new LiteralControl { Text = string.Format( "{0} <span class='caret'></span>", selectedText ) } );

            divControl.Controls.Add( btnSelect );

            listControl = new HtmlGenericControl( "ul" );
            listControl.Attributes["class"] = "dropdown-menu";
            
            Controls.Add( divControl );
            Controls.Add( hfSelectedItemId );
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            foreach ( var item in this.Items.OfType<ListItem>())
            {
                string controlHtmlFormat = "<li><a href='#' idvalue='{0}'>{1}</a></li>";
                listControl.Controls.Add( new LiteralControl { Text = string.Format( controlHtmlFormat, item.Value, item.Text ) } );
            }

            divControl.Controls.Add( listControl );

            divControl.RenderControl( writer );

            hfSelectedItemId.RenderControl( writer );
        }

        /// <summary>
        /// Occurs when [selection changed].
        /// </summary>
        public event EventHandler SelectionChanged;
    }
}
