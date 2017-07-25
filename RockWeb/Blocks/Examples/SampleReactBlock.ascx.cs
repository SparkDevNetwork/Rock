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
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Examples
{
    /// <summary>
    /// A sample block that uses many of the Rock UI controls.
    /// </summary>
    [DisplayName("Sample React Block")]
    [Category("Examples")]
    [Description("Creates a generic counter to showcase React integration")]

    [IntegerField("StartingNumber", "Specify a value to start the counter with", required: false)]
    public partial class SampleReactBlock : ReactBlock
    {
        #region Properties

        class InitialProps : Props
        {
            public int? startingNumber { get; set; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Page.IsPostBack)
            {
  
                var initialProps = new InitialProps();
                initialProps.startingNumber = GetAttributeValue("StartingNumber").ToStringSafe().AsIntegerOrNull() ?? 0;
                PageContent.Text = Render(initialProps);
            }
        }

        #endregion
    }
}
