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
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control that can be used to select a <see cref="MediaAccount"/>, and
    /// then a <see cref="MediaFolder"/>, and then a <see cref="MediaElement"/>.
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.CompositeControl" />
    /// <seealso cref="Rock.Web.UI.Controls.IRockControl" />
    public class MediaElementPicker : CompositeControl, IRockControl, IPostBackEventHandler
    {
        #region IRockControl implementation

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
            get => ViewState["Label"] as string ?? string.Empty;
            set => ViewState["Label"] = value;
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
            get => ViewState["FormGroupCssClass"] as string ?? string.Empty;
            set => ViewState["FormGroupCssClass"] = value;
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
        /// Gets or sets a value indicating whether this <see cref="MediaElementPicker"/>
        /// is required.
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
                return _ddlMediaElement.Required;
            }
            set
            {
                EnsureChildControls();
                _ddlMediaElement.Required = value;
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
            get => _customValidator.ErrorMessage;
            set => _customValidator.ErrorMessage = value;
        }

        /// <summary>
        /// Gets or sets an optional validation group to use.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup
        {
            get => _customValidator.ValidationGroup;
            set => _customValidator.ValidationGroup = value;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            // This method is primarily called when rendering after postback
            // which means the control would be marked as invalid while they
            // are still filling out the fields. NumberRangeEditor treats this
            // purely as a "is the value valid", not if the required condition
            // has been met. Do the same.
            get => true;
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

        /// <summary>
        /// The picker for the <see cref="MediaAccount"/>.
        /// </summary>
        private RockDropDownList _ddlMediaAccount;

        /// <summary>
        /// The picker for the <see cref="MediaFolder"/>.
        /// </summary>
        private RockDropDownList _ddlMediaFolder;

        /// <summary>
        /// The picker for the <see cref="MediaElement"/>.
        /// </summary>
        private RockDropDownList _ddlMediaElement;

        /// <summary>
        /// Gets or sets the custom validator.
        /// </summary>
        protected CustomValidator _customValidator;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to show the account picker.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the account picker should be shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowAccountPicker
        {
            get => ViewState["ShowAccountPicker"] as bool? ?? true;
            set => ViewState["ShowAccountPicker"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the folder picker.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the folder picker should be shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowFolderPicker
        {
            get => ViewState["ShowFolderPicker"] as bool? ?? true; 
            set => ViewState["ShowFolderPicker"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the media picker.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the media picker should be shown; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This is primarily used by <see cref="Field.Types.MediaElementFieldType"/>
        /// to select the locked in account and folder values. But feel free to
        /// use it elsewhere if you want.
        /// </remarks>
        public bool ShowMediaPicker
        {
            get => ViewState["ShowMediaPicker"] as bool? ?? true;
            set => ViewState["ShowMediaPicker"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user is allowed to
        /// refresh the items in the drop down lists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance allows refreshing; otherwise, <c>false</c>.
        /// </value>
        public bool IsRefreshAllowed
        {
            get => ViewState["IsRefreshAllowed"] as bool? ?? true;
            set =>ViewState["IsRefreshAllowed"] = value;
        }

        /// <summary>
        /// Gets or sets the item count threshold before the drop downs will have
        /// <see cref="RockDropDownList.EnhanceForLongLists"/> enabled.
        /// </summary>
        /// <value>The item count threshold before enhanced long lists is enabled, default is 20.</value>
        public int EnhanceForLongListsThreshold
        {
            get => ViewState["EnhanceForLongListsThreshold"] as int? ?? 20;
            set => ViewState["EnhanceForLongListsThreshold"] = value;
        }

        /// <summary>
        /// Gets or sets the media element label.
        /// </summary>
        /// <value>
        /// The media element label.
        /// </value>
        public string MediaElementLabel
        {
            get => ViewState["MediaElementLabel"] as string ?? "Media";
            set => ViewState["MediaElementLabel"] = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="MediaAccount"/> identifier.
        /// </summary>
        /// <value>
        /// The <see cref="MediaAccount"/> identifier.
        /// </value>
        public int? MediaAccountId
        {
            get
            {
                EnsureChildControls();
                return _ddlMediaAccount.SelectedValue.AsIntegerOrNull();
            }

            set => SetMediaAccountIdInternal( value, new RockContext() );
        }

        /// <summary>
        /// Gets or sets the <see cref="MediaFolder"/> identifier.
        /// </summary>
        /// <value>
        /// The <see cref="MediaFolder"/> identifier.
        /// </value>
        public int? MediaFolderId
        {
            get
            {
                EnsureChildControls();
                return _ddlMediaFolder.SelectedValue.AsIntegerOrNull();
            }

            set => SetMediaFolderIdInternal( value, new RockContext() );
        }

        /// <summary>
        /// Gets or sets the <see cref="MediaElement"/> identifier.
        /// </summary>
        /// <value>
        /// The <see cref="MediaElement"/> identifier.
        /// </value>
        public int? MediaElementId
        {
            get
            {
                EnsureChildControls();
                return _ddlMediaElement.SelectedValue.AsIntegerOrNull();
            }

            set => SetMediaElementIdInternal( value, new RockContext() );
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaElementPicker"/> class.
        /// </summary>
        public MediaElementPicker()
        {
            HelpBlock = new HelpBlock();
            WarningBlock = new WarningBlock();
            _customValidator = new CustomValidator();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls
        /// that use composition-based implementation to create any child
        /// controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            Controls.Clear();
            RockControlHelper.CreateChildControls( this, Controls );

            _ddlMediaAccount = new RockDropDownList();
            _ddlMediaAccount.ID = ID + "_ddlMediaAccount";
            _ddlMediaAccount.AutoPostBack = true;
            _ddlMediaAccount.SelectedIndexChanged += MediaAccount_SelectedIndexChanged;
            _ddlMediaAccount.Label = "Account";
            Controls.Add( _ddlMediaAccount );

            _ddlMediaFolder = new RockDropDownList();
            _ddlMediaFolder.ID = ID + "_ddlMediaFolder";
            _ddlMediaFolder.AutoPostBack = true;
            _ddlMediaFolder.SelectedIndexChanged += MediaFolder_SelectedIndexChanged;
            _ddlMediaFolder.Label = "Folder";
            Controls.Add( _ddlMediaFolder );

            _ddlMediaElement = new RockDropDownList();
            _ddlMediaElement.ID = ID + "_ddlMediaElement";
            _ddlMediaElement.CssClass = "js-media-element-value";
            Controls.Add( _ddlMediaElement );

            _customValidator.ID = ID + "_cfv";
            _customValidator.CssClass = "validation-error help-inline js-media-element-validator";
            _customValidator.ClientValidationFunction = "Rock.controls.mediaElementPicker.clientValidate";
            _customValidator.ErrorMessage = RequiredErrorMessage;
            _customValidator.Enabled = true;
            _customValidator.Display = ValidatorDisplay.Dynamic;
            _customValidator.ValidationGroup = ValidationGroup;
            Controls.Add( _customValidator );

            LoadMediaAccounts();
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" />
        /// object and stores tracing information about the control if tracing
        /// is enabled.
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
            // Update visibility of all the individual pickers.
            _ddlMediaAccount.Visible = ShowAccountPicker;
            _ddlMediaFolder.Visible = MediaAccountId.IsNotNullOrZero() && ShowFolderPicker;
            _ddlMediaElement.Visible = MediaFolderId.IsNotNullOrZero() && ShowMediaPicker;

            // Use > instead of >= to account for the blank entry.
            _ddlMediaAccount.EnhanceForLongLists = _ddlMediaAccount.Items.Count > EnhanceForLongListsThreshold;
            _ddlMediaFolder.EnhanceForLongLists = _ddlMediaFolder.Items.Count > EnhanceForLongListsThreshold;
            _ddlMediaElement.EnhanceForLongLists = _ddlMediaElement.Items.Count > EnhanceForLongListsThreshold;

            _ddlMediaElement.Label = MediaElementLabel;

            _ddlMediaFolder.AppendText = IsRefreshAllowed ? GetRefreshButtonHtml() : null;
            _ddlMediaElement.AppendText = IsRefreshAllowed ? GetRefreshButtonHtml() : null;

            writer.AddAttribute( "class", $"js-media-element-picker {CssClass}" );
            writer.AddAttribute( "id", ClientID );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            _ddlMediaAccount.RenderControl( writer );
            _ddlMediaFolder.RenderControl( writer );
            _ddlMediaElement.RenderControl( writer );

            RegisterStartupScript();

            _customValidator.ErrorMessage = RequiredErrorMessage.IsNotNullOrWhiteSpace()
                ? RequiredErrorMessage : $"{Label} is required.";

            _customValidator.RenderControl( writer );

            writer.RenderEndTag();
        }

        /// <summary>
        /// Registers the startup script.
        /// </summary>
        private void RegisterStartupScript()
        {
            var refreshScript = Page.ClientScript.GetPostBackEventReference( new PostBackOptions( this, "Refresh" ), true );
            refreshScript = refreshScript.Replace( '\'', '"' );

            var script = string.Format(
@"Rock.controls.mediaElementPicker.initialize({{
    controlId: '{0}',
    refreshScript: '{1}',
    required: {2}
}});",
                ClientID, // 0
                refreshScript, // 1
                Required.ToString().ToLowerInvariant() // 2
                );

            ScriptManager.RegisterStartupScript( this, GetType(), "MediaElementPickerScript_" + ClientID, script, true );
        }

        /// <summary>
        /// Gets the refresh button HTML content to be rendered.
        /// </summary>
        /// <returns>A string of HTML text.</returns>
        private string GetRefreshButtonHtml()
        {
            return $"<a href=\"#\" class=\"js-media-element-picker-refresh\"><i class=\"fa fa-refresh\"></i></a>";
        }

        /// <summary>
        /// Sets the media account identifier.
        /// </summary>
        /// <param name="mediaAccountId">The media account identifier.</param>
        /// <param name="rockContext">The rock context to use for database access.</param>
        private void SetMediaAccountIdInternal( int? mediaAccountId, RockContext rockContext )
        {
            EnsureChildControls();

            int accountId = mediaAccountId ?? 0;

            if ( _ddlMediaAccount.SelectedValue == accountId.ToString() )
            {
                return;
            }

            if ( accountId != 0 )
            {
                LoadMediaFolders( mediaAccountId.Value, rockContext );
            }

            _ddlMediaAccount.SetValue( accountId );
        }

        /// <summary>
        /// Sets the media folder identifier.
        /// </summary>
        /// <param name="mediaFolderId">The media folder identifier.</param>
        /// <param name="rockContext">The rock context to use for database access.</param>
        private void SetMediaFolderIdInternal( int? mediaFolderId, RockContext rockContext )
        {
            EnsureChildControls();

            int folderId = mediaFolderId ?? 0;

            if ( _ddlMediaFolder.SelectedValue == folderId.ToString() )
            {
                return;
            }

            if ( folderId != 0 )
            {
                var mediaAccountId = new MediaFolderService( rockContext ).Queryable()
                    .Where( f => f.Id == folderId )
                    .SingleOrDefault()?.MediaAccountId;

                SetMediaAccountIdInternal( mediaAccountId, rockContext );

                LoadMediaElements( folderId, rockContext );
            }

            _ddlMediaFolder.SelectedValue = folderId.ToString();
        }

        /// <summary>
        /// Sets the media element identifier.
        /// </summary>
        /// <param name="mediaElementId">The media element identifier.</param>
        /// <param name="rockContext">The rock context to use for database access.</param>
        private void SetMediaElementIdInternal( int? mediaElementId, RockContext rockContext )
        {
            EnsureChildControls();

            int mediaId = mediaElementId ?? 0;

            if ( _ddlMediaElement.SelectedValue == mediaId.ToString() )
            {
                return;
            }

            if ( mediaId != 0 )
            {
                var mediaFolderId = new MediaElementService( rockContext ).Queryable()
                    .Where( f => f.Id == mediaId )
                    .SingleOrDefault()?.MediaFolderId;

                SetMediaFolderIdInternal( mediaFolderId, rockContext );
            }

            _ddlMediaElement.SelectedValue = mediaId.ToString();
        }

        /// <summary>
        /// Loads the media accounts into the picker.
        /// </summary>
        private void LoadMediaAccounts()
        {
            _ddlMediaAccount.Items.Clear();
            _ddlMediaAccount.Items.Add( Rock.Constants.None.ListItem );

            var mediaAccountService = new Rock.Model.MediaAccountService( new RockContext() );

            // Get all media accounts that are active.
            var mediaAccounts = mediaAccountService.Queryable()
                .Where( a => a.IsActive )
                .OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } )
                .ToList();

            foreach ( var account in mediaAccounts )
            {
                _ddlMediaAccount.Items.Add( new ListItem( account.Name, account.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Loads the media folders into the picker.
        /// </summary>
        private void LoadMediaFolders( int mediaAccountId, RockContext rockContext )
        {
            _ddlMediaFolder.Items.Clear();
            _ddlMediaFolder.Items.Add( Rock.Constants.None.ListItem );

            var mediaFolderService = new Rock.Model.MediaFolderService( rockContext );

            // Get all media folders.
            var mediaFolders = mediaFolderService.Queryable()
                .Where( a => a.MediaAccountId == mediaAccountId )
                .OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } )
                .ToList();

            foreach ( var folder in mediaFolders )
            {
                _ddlMediaFolder.Items.Add( new ListItem( folder.Name, folder.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Loads the media elements into the picker.
        /// </summary>
        private void LoadMediaElements( int mediaFolderId, RockContext rockContext )
        {
            _ddlMediaElement.Items.Clear();
            _ddlMediaElement.Items.Add( Rock.Constants.None.ListItem );

            var mediaElementService = new Rock.Model.MediaElementService( rockContext );

            // Get all media elements.
            var mediaElements = mediaElementService.Queryable()
                .Where( a => a.MediaFolderId == mediaFolderId )
                .OrderBy( a => a.Name )
                .Select( a => new
                {
                    a.Id,
                    a.Name
                } )
                .ToList();

            foreach ( var media in mediaElements )
            {
                _ddlMediaElement.Items.Add( new ListItem( media.Name, media.Id.ToString() ) );
            }
        }

        /// <summary>
        /// When implemented by a class, enables a server control to process
        /// an event raised when a form is posted to the server.
        /// </summary>
        /// <param name="eventArgument">A <see cref="T:System.String" /> that represents an optional event argument to be passed to the event handler.</param>
        void IPostBackEventHandler.RaisePostBackEvent( string eventArgument )
        {
            if ( eventArgument == "Refresh" && MediaAccountId.IsNotNullOrZero() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var account = new MediaAccountService( rockContext ).GetNoTracking( MediaAccountId.Value );

                    if ( account == null )
                    {
                        return;
                    }

                    var task = Task.Run( async () =>
                    {
                        await MediaAccountService.RefreshMediaInAccountAsync( account );
                    } );

                    try
                    {
                        Task.WhenAny( task, Task.Delay( 7500 ) ).GetAwaiter().GetResult();
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex, Context );
                    }
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event of the MediaAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void MediaAccount_SelectedIndexChanged( object sender, EventArgs e )
        {
            int accountId = _ddlMediaAccount.SelectedValue.AsInteger();

            LoadMediaFolders( accountId, new RockContext() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the MediaAccount control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void MediaFolder_SelectedIndexChanged( object sender, EventArgs e )
        {
            int folderId = _ddlMediaFolder.SelectedValue.AsInteger();

            LoadMediaElements( folderId, new RockContext() );
        }

        #endregion
    }
}
