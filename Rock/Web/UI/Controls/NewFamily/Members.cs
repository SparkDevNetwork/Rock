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
    [ToolboxData( "<{0}:NewFamilyMembers runat=server></{0}:NewFamilyMembers>" )]
    public class NewFamilyMembers : CompositeControl, INamingContainer
    {
        LinkButton lbAddFamilyMember;

        public List<NewFamilyMembersRow> FamilyMemberRows
        {
            get
            {
                var rows = new List<NewFamilyMembersRow>();

                foreach ( Control control in Controls )
                {
                    if ( control is NewFamilyMembersRow )
                    {
                        var newFamilyMemberRow = control as NewFamilyMembersRow;
                        if ( newFamilyMemberRow != null )
                        {
                            rows.Add( newFamilyMemberRow );
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

            lbAddFamilyMember = new LinkButton();
            Controls.Add( lbAddFamilyMember );
            lbAddFamilyMember.ID = this.ID + "_btnAddFamilyMember";
            lbAddFamilyMember.Click += lbAddFamilyMember_Click;
            lbAddFamilyMember.AddCssClass( "add btn" );
            lbAddFamilyMember.CausesValidation = false;

            var iAddFilter = new HtmlGenericControl( "i" );
            iAddFilter.AddCssClass( "icon-plus-sign" );
            lbAddFamilyMember.Controls.Add( iAddFilter );
        }

        protected void lbAddFamilyMember_Click( object sender, EventArgs e )
        {
            if ( AddFamilyMemberClick != null )
            {
                AddFamilyMemberClick( this, e );
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

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Role" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Title" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "First Name" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Nick Name" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Last Name" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Gender" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Birthdate" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Status" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Grade" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "" );
                writer.RenderEndTag();

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // thead

                writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                foreach ( Control control in Controls )
                {
                    if ( control is NewFamilyMembersRow )
                    {
                        control.RenderControl( writer );
                    }
                }

                writer.RenderEndTag();  // tbody

                writer.RenderBeginTag( HtmlTextWriterTag.Tfoot );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Colspan, "9" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                lbAddFamilyMember.RenderControl( writer );
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
                if (Controls[i] is NewFamilyMembersRow )
                {
                    Controls.RemoveAt( i );
                }
            }
        }

        ///// <summary>
        ///// Occurs when [add filter click].
        ///// </summary>
        public event EventHandler AddFamilyMemberClick;
        
    }
}