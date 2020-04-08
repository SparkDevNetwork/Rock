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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a content channel and then a item of that content channel 
    /// </summary>
    public class ContentChannelItemPicker : CompositeControl, IRockControl
    {
        #region IRockControl implementation (Custom implementation)

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get { return ViewState["Label"] as string ?? string.Empty; }
            set { ViewState["Label"] = value; }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                EnsureChildControls();
                return _ddlContentChannelItem.Required;
            }
            set
            {
                EnsureChildControls();
                _ddlContentChannelItem.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] as string; }
            set { ViewState["ValidationGroup"] = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        #region Controls

        private RockDropDownList _ddlContentChannel;
        private RockDropDownList _ddlContentChannelItem;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the content channel id.
        /// </summary>
        /// <value>
        /// The content channel id.
        /// </value>
        public int? ContentChannelId
        {
            get
            {
                return ViewState["ContentChannelId"] as int?;
            }

            set
            {
                ViewState["ContentChannelId"] = value;
                if ( value.HasValue )
                {
                    LoadContentChannelItems( value.Value );
                }
            }
        }

        /// <summary>
        /// Gets or sets the content channel item identifier.
        /// </summary>
        /// <value>
        /// The content channel item identifier.
        /// </value>
        public int? ContentChannelItemId
        {
            get
            {
                EnsureChildControls();
                return _ddlContentChannelItem.SelectedValue.AsIntegerOrNull();
            }

            set
            {
                EnsureChildControls();
                int contentChannelItemId = value ?? 0;
                if ( _ddlContentChannelItem.SelectedValue != contentChannelItemId.ToString() )
                {
                    if ( !ContentChannelId.HasValue )
                    {
                        var contentChannelItem = new Rock.Model.ContentChannelItemService( new RockContext() ).Get( contentChannelItemId );
                        if ( contentChannelItem != null &&
                            _ddlContentChannel.SelectedValue != contentChannelItem.ContentChannelId.ToString() )
                        {
                            _ddlContentChannel.SelectedValue = contentChannelItem.ContentChannelId.ToString();

                            LoadContentChannelItems( contentChannelItem.ContentChannelId );
                        }
                    }

                    _ddlContentChannelItem.SelectedValue = contentChannelItemId.ToString();
                }
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChannelItemPicker"/> class.
        /// </summary>
        public ContentChannelItemPicker()
            : base()
        {
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _ddlContentChannel = new RockDropDownList();
            _ddlContentChannel.ID = this.ID + "_ddlContentChannel";
            _ddlContentChannel.AutoPostBack = true;
            _ddlContentChannel.EnhanceForLongLists = true;
            _ddlContentChannel.Label = "Content Channel";
            _ddlContentChannel.SelectedIndexChanged += _ddlContentChannel_SelectedIndexChanged;
            Controls.Add( _ddlContentChannel );

            _ddlContentChannelItem = new RockDropDownList();
            _ddlContentChannelItem.EnhanceForLongLists = true;
            _ddlContentChannelItem.Label = "Content Channel Item";
            _ddlContentChannelItem.ID = this.ID + "_ddlContentChannelItem";
            Controls.Add( _ddlContentChannelItem );

            LoadContentChannels();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _ddlContentChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void _ddlContentChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            int contentChannelId = _ddlContentChannel.SelectedValue.AsInteger();
            LoadContentChannelItems( contentChannelId );
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        /// <summary>
        /// Renders the base control.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void RenderBaseControl( HtmlTextWriter writer )
        {
            if ( !ContentChannelId.HasValue )
            {
                _ddlContentChannel.RenderControl( writer );
            }

            _ddlContentChannelItem.RenderControl( writer );
        }

        /// <summary>
        /// Loads the content channels.
        /// </summary>
        private void LoadContentChannels()
        {
            _ddlContentChannel.Items.Clear();
            _ddlContentChannel.Items.Add( new ListItem() );

            var contentChannels = ContentChannelCache.All().OrderBy( a => a.Name ).ToList();

            foreach ( var contentChannel in contentChannels )
            {
                _ddlContentChannel.Items.Add( new ListItem( contentChannel.Name, contentChannel.Id.ToString().ToUpper() ) );
            }
        }

        /// <summary>
        /// Loads the content channel items.
        /// </summary>
        /// <param name="contentChannelId">The content channel identifier.</param>
        private void LoadContentChannelItems( int? contentChannelId )
        {
            int? currentContentChannelItemId = this.ContentChannelItemId;
            _ddlContentChannelItem.SelectedValue = null;
            _ddlContentChannelItem.Items.Clear();
            if ( contentChannelId.HasValue )
            {
                _ddlContentChannelItem.Items.Add( Rock.Constants.None.ListItem );

                var contentChannelItemService = new Rock.Model.ContentChannelItemService( new RockContext() );
                var contentChannelitems = contentChannelItemService.Queryable().Where( r => r.ContentChannelId == contentChannelId.Value ).OrderBy( a => a.Title ).ToList();

                foreach ( var r in contentChannelitems )
                {
                    var item = new ListItem( r.Title, r.Id.ToString().ToUpper() );
                    item.Selected = r.Id == currentContentChannelItemId;
                    _ddlContentChannelItem.Items.Add( item );
                }
            }
        }
    }
}