//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:GroupLocations runat=server></{0}:GroupLocations>" )]
    public class GroupLocations : CompositeControl, INamingContainer
    {
        LinkButton lbAddGroupLocation;

        /// <summary>
        /// Gets the group location rows.
        /// </summary>
        /// <value>The group location rows.</value>
        public List<GroupLocationsRow> GroupLocationRows
        {
            get
            {
                var rows = new List<GroupLocationsRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is GroupLocationsRow )
                    {
                        var groupLocationRow = control as GroupLocationsRow;
                        if ( groupLocationRow != null )
                        {
                            rows.Add( groupLocationRow );
                        }
                    }
                }

                return rows;
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            Controls.Clear();

            lbAddGroupLocation = new LinkButton();
            Controls.Add( lbAddGroupLocation );
            lbAddGroupLocation.ID = this.ID + "_btnAddGroupLocation";
            lbAddGroupLocation.Click += lbAddGroupLocation_Click;
            lbAddGroupLocation.AddCssClass( "add btn" );
            lbAddGroupLocation.CausesValidation = false;

            var iAddFilter = new HtmlGenericControl( "i" );
            iAddFilter.AddCssClass( "icon-plus-sign" );
            lbAddGroupLocation.Controls.Add( iAddFilter );
        }

        /// <summary>
        /// Handles the Click event of the lbAddGroupLocation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddGroupLocation_Click( object sender, EventArgs e )
        {
            if ( AddGroupLocationClick != null )
            {
                AddGroupLocationClick( this, e );
            }
        }

        /// <summary>
        /// Writes the <see cref="T:System.Web.UI.WebControls.CompositeControl" /> content to the specified <see cref="T:System.Web.UI.HtmlTextWriter" /> object, for display on the client.
        /// </summary>
        /// <param name="writer">An <see cref="T:System.Web.UI.HtmlTextWriter" /> that represents the output stream to render HTML content on the client.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "table" );
                writer.RenderBeginTag( HtmlTextWriterTag.Table );

                writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                // Type
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                // Street 1
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Title" );
                writer.RenderEndTag();

                // Street 2
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                // City
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                // State
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                // Zip
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Mailing Address" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Location" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // thead

                writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                foreach ( Control control in Controls )
                {
                    if ( control is GroupLocationsRow )
                    {
                        control.RenderControl( writer );
                    }
                }

                writer.RenderEndTag();  // tbody

                writer.RenderBeginTag( HtmlTextWriterTag.Tfoot );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Colspan, "8" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                lbAddGroupLocation.RenderControl( writer );
                writer.RenderEndTag();

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // tfoot

                writer.RenderEndTag();  // table
            }
        }

        /// <summary>
        /// Clears the rows.
        /// </summary>
        public void ClearRows()
        {
            for (int i = Controls.Count - 1; i >= 0; i--)
            {
                if (Controls[i] is GroupLocationsRow )
                {
                    Controls.RemoveAt( i );
                }
            }
        }

        ///// <summary>
        ///// Occurs when [add filter click].
        ///// </summary>
        public event EventHandler AddGroupLocationClick;
        
    }
}