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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Model;
using Rock.Web.UI;
using System.Xml.Linq;

namespace RockWeb.Blocks.Utility
{
    [DisplayName( "ViewState Viewer" )]
    [Category( "Utility" )]
    [Description( "Block allows you to see what's in the View State of a page." )]
    [Rock.SystemGuid.BlockTypeGuid( "268F9F11-BC74-4E86-A72D-6AA668BF470D" )]
    public partial class ViewStateViewer : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables

        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
            RockPage.EnableViewStateInspection = true;
            RockPage.ViewStatePersisted += RockPage_ViewStatePersisted;

            if ( !Page.IsPostBack )
            {
            }

            base.OnLoad( e );
        }

        private void RockPage_ViewStatePersisted( object sender, EventArgs e )
        {
            var page = ( RockPage ) sender;

            var viewStateSize = 0;

            try
            {
                TextReader tr = new StringReader( page.ViewStateValue );
                XDocument xdoc = XDocument.Load( tr );

                viewStateSize = xdoc.Descendants( "Pair" ).First().Attribute( "Size" ).Value.AsInteger();
            }
            catch ( Exception ) { }

            hlblViewStateSize.Text = string.Format( "ViewState Size: {0}", viewStateSize.FormatAsSpecificMemorySize( ExtensionMethods.MemorySizeUnit.Bytes ));//.FormatAsMemorySize() );
            ceViewState.Text = page.ViewStateValue;
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
    }
}