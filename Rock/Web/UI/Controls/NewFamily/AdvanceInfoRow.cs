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
using System.Web.UI.WebControls;

using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Displays a advance info row
    /// </summary>
    public class NewGroupAdvanceInfoRow : CompositeControl
    {
        private RockTextBox _tbAlternateId;
        private LinkButton _lbGenerate;

        /// <summary>
        /// Gets or sets the person GUID.
        /// </summary>
        /// <value>
        /// The person GUID.
        /// </value>
        public Guid? PersonGuid
        {
            get
            {
                if ( ViewState["PersonGuid"] != null )
                {
                    return ( Guid ) ViewState["PersonGuid"];
                }
                else
                {
                    return Guid.Empty;
                }
            }
            set { ViewState["PersonGuid"] = value; }
        }

        /// <summary>
        /// Gets or sets the name of the person.
        /// </summary>
        /// <value>
        /// The name of the person.
        /// </value>
        public string PersonName
        {
            get { return ViewState["PersonName"] as string ?? string.Empty; }
            set { ViewState["PersonName"] = value; }
        }

        /// <summary>
        /// Gets or sets the alternate id.
        /// </summary>
        /// <value>
        /// The alternate id.
        /// </value>
        public string AlternateId
        {
            get
            {
                EnsureChildControls();
                return _tbAlternateId.Text;
            }
            set
            {
                EnsureChildControls();
                _tbAlternateId.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get
            {
                EnsureChildControls();
                return _tbAlternateId.ValidationGroup;
            }
            set
            {
                EnsureChildControls();
                _tbAlternateId.ValidationGroup = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NewGroupAdvanceInfoRow" /> class.
        /// </summary>
        public NewGroupAdvanceInfoRow()
            : base()
        {
            _tbAlternateId = new RockTextBox();
            _lbGenerate = new LinkButton();
            _lbGenerate.Text = "<i class='fas fa-barcode text-color'></i>";
            _lbGenerate.Click += lbGenerate_Click;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Handles the Click event of the lbGenerate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbGenerate_Click( object sender, EventArgs e )
        {
            AlternateId = PersonSearchKeyService.GenerateRandomAlternateId( true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _tbAlternateId.ID = "_tbAlternateId";
            Controls.Add( _tbAlternateId );

            _lbGenerate.ID = "_btnGenerate";
            Controls.Add( _lbGenerate );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                writer.AddAttribute( "rowid", ID );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                writer.AddAttribute( "class", "person-name" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );

                writer.Write( PersonName );
                writer.RenderEndTag();

                writer.AddAttribute( "class", "person-alternateId" );
                writer.RenderBeginTag( HtmlTextWriterTag.Td );
                writer.AddAttribute( "class", "form-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                writer.AddAttribute( "class", "input-group" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _tbAlternateId.RenderControl( writer );
                writer.AddAttribute( "class", "input-group-addon" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );
                _lbGenerate.RenderControl( writer );
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.RenderEndTag();  // Tr
            }
        }
    }

}