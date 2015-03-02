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
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Report Filter control
    /// </summary>
    [ToolboxData( "<{0}:NewFamilyMembers runat=server></{0}:NewFamilyMembers>" )]
    public class NewFamilyMembers : CompositeControl, INamingContainer
    {
        private LinkButton _lbAddFamilyMember;

        /// <summary>
        /// Gets or sets a value indicating whether [require gender].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require gender]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGender
        {
            get { return ViewState["RequireGender"] as bool? ?? false; }
            set { ViewState["RequireGender"] = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require grade].
        /// </summary>
        /// <value>
        /// <c>true</c> if [require grade]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGrade
        {
            get { return ViewState["RequireGrade"] as bool? ?? false; }
            set { ViewState["RequireGrade"] = value; }
        }

        /// <summary>
        /// Gets the family member rows.
        /// </summary>
        /// <value>
        /// The family member rows.
        /// </value>
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

            _lbAddFamilyMember = new LinkButton();
            Controls.Add( _lbAddFamilyMember );
            _lbAddFamilyMember.ID = this.ID + "_btnAddFamilyMember";
            _lbAddFamilyMember.Click += lbAddFamilyMember_Click;
            _lbAddFamilyMember.AddCssClass( "add btn btn-action" );
            _lbAddFamilyMember.CausesValidation = false;

            var iAddFilter = new HtmlGenericControl( "i" );
            iAddFilter.AddCssClass("fa fa-plus-circle");
            _lbAddFamilyMember.Controls.Add( iAddFilter );
        }

        /// <summary>
        /// Handles the Click event of the lbAddFamilyMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
                writer.AddAttribute( HtmlTextWriterAttribute.Class, "table table-familymembers" );
                writer.RenderBeginTag( HtmlTextWriterTag.Table );

                writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "required" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Role" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Title" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "required" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Name" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Suffix" );
                writer.RenderEndTag();

                writer.AddAttribute( HtmlTextWriterAttribute.Class, "required" );
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Connection Status" );
                writer.RenderEndTag();

                if ( RequireGender )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "required" );
                }
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Gender" );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( "Birthdate" );
                writer.RenderEndTag();

                if ( RequireGrade )
                {
                    writer.AddAttribute( HtmlTextWriterAttribute.Class, "required" );
                }
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.Write( GlobalAttributesCache.Read().GetValue( "core.GradeLabel" ) );
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

                writer.AddAttribute( HtmlTextWriterAttribute.Colspan, "8" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.RenderEndTag();

                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                _lbAddFamilyMember.RenderControl( writer );
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

        /// <summary>
        /// Occurs when [add family member click].
        /// </summary>
        public event EventHandler AddFamilyMemberClick;
        
    }
}