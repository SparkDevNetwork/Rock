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
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Utility
{
    [DisplayName( "Stark Dynamic Attributes" )]
    [Category( "Utility" )]
    [Description( "Template block for developers to use to start a new detail block that supports dynamic attributes." )]

    #region Block Attributes

    [CodeEditorField( "Lava Template",
        Key = AttributeKey.LavaTemplate,
        Description = "The Lava Template",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 200,
        IsRequired = true,
        DefaultValue = LavaTemplateDefaultValue,
        Order = 0 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "7C34A0FA-ED0D-4B8B-B458-6EC970711726" )]
    public partial class StarkDynamicAttributes : Rock.Web.UI.RockBlock, Rock.Web.UI.IDynamicAttributesBlock
    {
        #region Constants

        /// <summary>
        /// The Default Value for the LavaTemplate block attribute
        /// </summary>
        private const string LavaTemplateDefaultValue = @"
<strong>Additional Attributes can be defined for this block type. These can be configured in <code>Home / CMS Configuration / Block Types</code> and will show up in block settings</strong>

<ul>
{% for attribute in Block.AttributeValues %}
    {% if attribute.AttributeKey != 'LavaTemplate' %}
    <li>{{ attribute.AttributeKey }}</li>
    {% endif %}
{% endfor %}
</ul>";

        #endregion Constants

        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// AttributeKey for Lava Template
            /// </summary>
            public const string LavaTemplate = "LavaTemplate";
        }

        #endregion Attribute Keys

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
            if ( !Page.IsPostBack )
            {
                // added for your convenience

                // to show the created/modified by date time details in the PanelDrawer do something like this:
                // pdAuditDetails.SetEntity( <YOUROBJECT>, ResolveRockUrl( "~" ) );

                ShowView();
            }

            base.OnLoad( e );
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
            ShowView();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        private void ShowView()
        {
            string lavaTemplateHtml = this.GetAttributeValue( "LavaTemplate" );
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
            mergeFields.Add( "Block", this.BlockCache );

            lLavaTemplateHtml.Text = lavaTemplateHtml.ResolveMergeFields( mergeFields );
        }

        #endregion
    }
}