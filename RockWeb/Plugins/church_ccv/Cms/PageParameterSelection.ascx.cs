using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;

namespace RockWeb.Plugins.church_ccv.Cms
{
    [DisplayName( "Page Parameter Selection" )]
    [Category( "CCV > Cms" )]
    [Description( "Displays a drop down menu that will update the page parameters when a selection is made." )]

    [TextField( "Label", "The label for the dropdown.", true, "Label" )]
    [TextField( "Page Parameter Name", "Name of the page parameter to be updated.", true, "Param" )]
    [KeyValueListField( "Selection", "A list of parameter names and values.", true, "", "Name", "Value", "", "" )]
    public partial class PageParameterSelection : Rock.Web.UI.RockBlock
    {
        private string pageParameterName;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            pageParameterName = GetAttributeValue( "PageParameterName" ).ToString();

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string label = GetAttributeValue( "Label" );
            ddlSelection.Label = !string.IsNullOrWhiteSpace( label ) ? label : "";

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the selection as a dictionary to be included in the Lava.
        /// </summary>
        /// <returns>A dictionary of Titles with their Links.</returns>
        private Dictionary<string, object> GetSelectionList()
        {
            var properties = new Dictionary<string, object>();

            var selectionString = GetAttributeValue( "Selection" );

            if ( !string.IsNullOrWhiteSpace( selectionString ) )
            {
                selectionString = selectionString.TrimEnd( '|' );
                var selections = selectionString.Split( '|' )
                                .Select( s => s.Split( '^' ) )
                                .Select( p => new { Name = p[0], Value = p[1] } );

                StringBuilder sbPageMarkup = new StringBuilder();
                foreach ( var selection in selections )
                {
                    properties.Add( selection.Name, selection.Value );
                }
            }
            return properties;
        }

        private void LoadDropDowns()
        {
            var selections = GetSelectionList();

            // First item should be blank
            ddlSelection.Items.Add( new ListItem( "", "" ) );
;
            foreach ( var selection in selections )
            {
                ddlSelection.Items.Add( new ListItem( selection.Key, selection.Value.ToString() ) );
            }

            var selectionString = Request.QueryString[pageParameterName];
            if ( selectionString != null )
            {
                ddlSelection.SelectedIndex = ddlSelection.Items.IndexOf( ddlSelection.Items.FindByValue( selectionString ) );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadDropDowns();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSelection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSelection_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( ddlSelection.SelectedValue ) )
            {
                var selection = GetSelectionList();

                var item = selection.Where( p => p.Value.ToString() == ddlSelection.SelectedValue ).FirstOrDefault();

                var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                queryString.Set( pageParameterName, item.Value.ToString() );
                Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
            }
        }

        #endregion
    }
}