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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:GridFilter runat=server></{0}:GridFilter>" )]
    public class GridFilter : PlaceHolder, INamingContainer
    {
        private HiddenField _hfVisible;
        private LinkButton _lbFilter;
        private LinkButton _lbClearFilter;
        private List<UserPreference> _userPreferences;
        private bool _isDirty = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridFilter"/> class.
        /// </summary>
        public GridFilter() : base()
        {
            AdditionalFilterDisplay = new Dictionary<string, string>();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Get User Values
            _userPreferences = new List<UserPreference>();

            RockBlock rockBlock = this.RockBlock();
            if ( rockBlock != null )
            {
                string blockKeyPrefix = string.Format( "grid-filter-{0}-", rockBlock.BlockId );

                foreach ( var userPreference in rockBlock.GetUserPreferences( blockKeyPrefix ) )
                {
                    var blockKey = userPreference.Key.Replace( blockKeyPrefix, string.Empty );
                    var keyName = blockKey.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );
                    if ( blockKey.Contains( "|" ) && keyName.Length == 2 )
                    {
                        // only load userPreferences that are stored in the {key}|{name} format
                        // and make sure there isn't more than one with the same key
                        if ( !_userPreferences.Any( a => a.Key == keyName[0] ) )
                        {
                            _userPreferences.Add( new UserPreference( keyName[0], keyName[1], userPreference.Value ) );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the additional filter display.
        /// </summary>
        /// <value>
        /// The additional filter display.
        /// </value>
        public Dictionary<string, string> AdditionalFilterDisplay
        {
            get
            {
                return ViewState["AdditionalFilterDisplay"] as Dictionary<string, string> ?? new Dictionary<string, string>();
            }
            set
            {
                ViewState["AdditionalFilterDisplay"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the user preference key prefix.
        /// Set this to add an additional prefix ( other than just block.Id ) on each UserPreference key for this filter. 
        /// For example, if this is a filter for a GroupMemberList, you might want the UserPreferenceKeyPrefix be `{{ GroupId }}-`
        /// so that user preferences for this grid filter are per group instead of just per block. 
        /// </summary>
        /// <value>
        /// The user preference key prefix.
        /// </value>
        public string UserPreferenceKeyPrefix
        {
            get
            {
                return ViewState["UserPreferenceKeyPrefix"] as string ?? string.Empty;
            }

            set
            {
                ViewState["UserPreferenceKeyPrefix"] = value;
            }
        }

        /// <summary>
        /// Shows the filter in expanded mode
        /// </summary>
        public void Show()
        {
            EnsureChildControls();
            _hfVisible.Value = "true";
        }

        /// <summary>
        /// Registers the java script.
        /// </summary>
        private void RegisterJavaScript()
        {
            const string scriptKey = "grid-filter-script";
            const string script = @"
    $('div.grid-filter header').click(function () {
        $('i.toggle-filter', this).toggleClass('fa-chevron-down fa-chevron-up');
        var $hf = $('input', this).first();
        if($hf.val() != 'true') {
            $hf.val('true');
        } else {
            $hf.val('false');
        }
        $(this).siblings('div').slideToggle();
    });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), scriptKey, script, true );
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _hfVisible = new HiddenField();
            Controls.Add( _hfVisible );
            _hfVisible.ID = "hfVisible";

            _lbFilter = new LinkButton();
            Controls.Add( _lbFilter );
            _lbFilter.ID = "lbFilter";
            _lbFilter.CssClass = "filter btn btn-action btn-xs";
            _lbFilter.ToolTip = "Apply Filter";
            _lbFilter.Text = "Apply Filter";
            _lbFilter.CausesValidation = false;
            _lbFilter.Click += lbFilter_Click;

            _lbClearFilter = new LinkButton();
            Controls.Add( _lbClearFilter );
            _lbClearFilter.ID = "_lbClearFilter";
            _lbClearFilter.CssClass = "filter-clear btn btn-default btn-xs";
            _lbClearFilter.ToolTip = "Clear Filter";
            _lbClearFilter.Text = "Clear Filter";
            _lbClearFilter.CausesValidation = false;
            _lbClearFilter.Click += lbClearFilter_Click;
        }

        /// <summary>
        /// Handles the Click event of the _lbClearFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbClearFilter_Click( object sender, EventArgs e )
        {
            if ( ClearFilterClick != null )
            {
                ClearFilterClick( sender, e );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void lbFilter_Click( object sender, EventArgs e )
        {
            _hfVisible.Value = "false";

            if ( ApplyFilterClick != null )
            {
                ApplyFilterClick( sender, e );
            }
        }

        /// <summary>
        /// Outputs server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object and stores tracing information about the control if tracing is enabled.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the control content.</param>
        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                bool visible = _hfVisible.Value == "true";

                writer.AddAttribute( "class", "grid-filter" );
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.Write( "<header>" );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, visible ? "btn btn-link btn-xs is-open" : "btn btn-link btn-xs" );
                writer.RenderBeginTag( HtmlTextWriterTag.Button );
                writer.Write( "Filter Options" );

                _hfVisible.RenderControl( writer );

                writer.AddAttribute( "class", visible ? "btn-icon fa fa-chevron-up toggle-filter" : "btn-icon fa fa-chevron-down toggle-filter" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
                writer.RenderEndTag();

                writer.RenderEndTag();

                writer.Write( "</header>" );

                // Filter Overview
                writer.AddAttribute( "class", "grid-filter-overview" );
                if ( visible )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                var filterDisplay = new Dictionary<string, string>();
                AdditionalFilterDisplay.ToList().ForEach( d => filterDisplay.Add( d.Key, d.Value ) );

                List<UserPreference> userPreferencesForFilter;
                if ( this.UserPreferenceKeyPrefix.IsNotNullOrWhiteSpace() )
                {
                    userPreferencesForFilter = _userPreferences.Where( a => a.Key.StartsWith( this.UserPreferenceKeyPrefix ) ).ToList();
                }
                else
                {
                    userPreferencesForFilter = _userPreferences;
                }

                var nonEmptyValues = userPreferencesForFilter.Where( v => !string.IsNullOrEmpty( v.Value ) ).ToList();
                if ( nonEmptyValues.Count > 0 )
                {
                    foreach ( var userPreference in nonEmptyValues )
                    {
                        DisplayFilterValueArgs args = new DisplayFilterValueArgs( userPreference, this.UserPreferenceKeyPrefix );
                        if ( DisplayFilterValue != null )
                        {
                            DisplayFilterValue( this, args );
                        }

                        if ( !string.IsNullOrWhiteSpace( args.Value ) )
                        {
                            filterDisplay.AddOrReplace( args.Name, args.Value );
                        }
                    }
                }

                if ( filterDisplay.Any() )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Fieldset );
                    writer.WriteLine( "<h4>Enabled Filters</h4>" );
                    writer.WriteLine( "<div class='row'>" );
                    foreach( var filterNameValue in filterDisplay.OrderBy( f => f.Key ) )
                    {
                        writer.WriteLine( "<div class='col-md-3'>" );
                        writer.WriteLine( string.Format( "<label>{0}:</label> {1}", filterNameValue.Key, filterNameValue.Value ) );
                        writer.WriteLine( "</div>" );
                    }
                    writer.WriteLine( "</div>" );
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();

                // Filter Entry
                writer.AddAttribute( "class", "grid-filter-entry" );
                if ( !visible )
                {
                    writer.AddStyleAttribute( HtmlTextWriterStyle.Display, "none" );
                }
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.RenderBeginTag( HtmlTextWriterTag.Fieldset );

                writer.Write( "<h4>Filter Options</h4>" );

                base.RenderControl( writer );

                writer.RenderEndTag();

                _lbFilter.RenderControl( writer );

                if ( ClearFilterClick != null )
                {
                    writer.WriteLine();
                    _lbClearFilter.RenderControl( writer );
                }

                writer.RenderEndTag();

                writer.RenderEndTag();

                if ( _isDirty )
                {
                    SaveUserPreferences();
                    _isDirty = false;
                }

                RegisterJavaScript();
            }
        }

        /// <summary>
        /// Outputs the content of a server control's children to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the rendered content.</param>
        protected override void RenderChildren( HtmlTextWriter writer )
        {
            if ( this.Controls != null )
            {
                // wrap filter items in bootstrap responsive grid
                int cellCount = 0;
                const int cellsPerRow = 3;

                // write first row
                writer.AddAttribute("class", "row");
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                var filterControls = new List<Control>();
                foreach ( Control child in Controls )
                {
                    if ( child is PlaceHolder )
                    {
                        filterControls.AddRange( ( child as PlaceHolder ).Controls.OfType<Control>().ToList() );
                    }
                    else
                    {
                        filterControls.Add( child );
                    }
                }

                foreach ( Control child in filterControls )
                {
                    // write new row
                    if ( cellCount >= cellsPerRow )
                    {
                        writer.RenderEndTag();
                        writer.AddAttribute( "class", "row" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        cellCount = 0;
                    }

                    if ( child != _lbFilter && child != _lbClearFilter && child != _hfVisible )
                    {
                        // add column
                        if ( child.Visible )
                        {
                            writer.AddAttribute( "class", "col-lg-4" );
                            writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        }

                        child.RenderControl( writer );

                        if ( child.Visible )
                        {
                            writer.RenderEndTag();
                            cellCount++;
                        }
                    }
                }

                // write end row div
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Gets the Key after the <see cref="UserPreferenceKeyPrefix"></see> has been applied
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetPrefixedKey( string key )
        {
            return $"{UserPreferenceKeyPrefix}{key}";
        }

        /// <summary>
        /// Gets the user preference for a given key if it exists.
        /// Note: Key is internally stored with the Block.Id plus any <see cref="UserPreferenceKeyPrefix"/> to make it unique per block and <see cref="UserPreferenceKeyPrefix"/>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetUserPreference( string key )
        {
            var prefixedKey = this.GetPrefixedKey( key );
            return _userPreferences.Where( p => p.Key == prefixedKey ).Select( p => p.Value ).FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Adds or updates an item in the User Preferences dictionary.
        /// Note: Key is internally stored with the Block.Id plus any <see cref="UserPreferenceKeyPrefix"/> to make it unique per block and <see cref="UserPreferenceKeyPrefix"/>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void SaveUserPreference( string key, string name, string value )
        {
            var prefixedKey = this.GetPrefixedKey( key );
            var userPreference = _userPreferences.FirstOrDefault( p => p.Key == prefixedKey );
            if ( userPreference != null )
            {
                if ( userPreference.Name != name || userPreference.Value != value )
                {
                    _isDirty = true;
                    userPreference.Name = name;
                    userPreference.Value = value;
                }
            }
            else
            {
                _isDirty = true;
                _userPreferences.Add( new UserPreference( prefixedKey, name, value ) );
            }
        }

        /// <summary>
        /// Adds or updates an item in the User Preferences dictionary.
        /// Note: Key is internally stored with the Block.Id plus any <see cref="UserPreferenceKeyPrefix"/> to make it unique per block and <see cref="UserPreferenceKeyPrefix"/>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SaveUserPreference( string key, string value )
        {
            SaveUserPreference( key, key, value );
        }

        /// <summary>
        /// Saves the user preferences.
        /// </summary>
        private void SaveUserPreferences()
        {
            RockBlock rockBlock = this.RockBlock();
            if ( rockBlock != null )
            {
                string keyPrefix = string.Format( "grid-filter-{0}-", rockBlock.BlockId );

                foreach ( var userPreference in _userPreferences )
                {
                    string keyPrefixUserPreferenceKey = string.Format( "{0}{1}|", keyPrefix, userPreference.Key );
                    string key = string.Format( "{0}{1}", keyPrefixUserPreferenceKey, userPreference.Name);

                    // No duplicate user preference key values before the '|' are allowed.
                    // This search for any keys that match before the '|' but mismatch after '|' and delete it before writing the user preference.
                    int? personEntityTypeId = EntityTypeCache.Get( Person.USER_VALUE_ENTITY ).Id;

                    using ( var rockContext = new Rock.Data.RockContext() )
                    {
                        var attributeService = new Model.AttributeService( rockContext );
                        var attributes = attributeService
                            .Queryable()
                            .Where( a => a.EntityTypeId == personEntityTypeId )
                            .Where( a => a.Key.StartsWith( keyPrefixUserPreferenceKey ) )
                            .Where( a => a.Key != key );

                        if ( attributes.Count() != 0 )
                        {
                            foreach ( var attribute in attributes )
                            {
                                rockBlock.DeleteUserPreference( attribute.Key );
                            }
                        }
                        rockContext.SaveChanges();
                    }

                    rockBlock.SetUserPreference( key, userPreference.Value );
                }
            }
        }

        /// <summary>
        /// Deletes all the grid user preferences for all grid filters on the block. If <see cref="UserPreferenceKeyPrefix"/> is set, it will only delete user preferences for the grid filter(s) on the block that has the UserPreferenceKeyPrefix.
        /// </summary>
        public void DeleteUserPreferences()
        {
            RockBlock rockBlock = this.RockBlock();
            if ( rockBlock != null && _userPreferences != null )
            {
                string keyPrefix = string.Format( "grid-filter-{0}-", rockBlock.BlockId ) + this.UserPreferenceKeyPrefix;

                foreach ( var userPreference in _userPreferences )
                {
                    rockBlock.DeleteUserPreference( string.Format( "{0}{1}|{2}", keyPrefix, userPreference.Key, userPreference.Name ) );
                }

                _userPreferences.Clear();
            }
        }

        /// <summary>
        /// Occurs when user applies a filter.
        /// </summary>
        public event EventHandler ApplyFilterClick;

        /// <summary>
        /// Occurs when user clears a filter.
        /// HINT: call gFilter.DeleteUserPreferences() then re-bind your filter controls 
        /// </summary>
        public event EventHandler ClearFilterClick;

        /// <summary>
        /// Occurs when grid filter displays an existing filter value.  Key and Value can be 
        /// updated to a more human-readable form if needed.
        /// </summary>
        public event EventHandler<DisplayFilterValueArgs> DisplayFilterValue;

        /// <summary>
        /// Argument for DisplayFilterValue event
        /// </summary>
        public class DisplayFilterValueArgs : EventArgs
        {
            /// <summary>
            /// Gets or sets the key.
            /// Note: This is the Key without the internally stored Block.Id plus any <see cref="UserPreferenceKeyPrefix"/> prefix
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="DisplayFilterValueArgs" /> class.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="name">The name.</param>
            /// <param name="value">The value.</param>
            [RockObsolete( "1.7.4" )]
            [Obsolete( "DisplayFilterValueArgs(userPreference, prefix) instead", true )]
            public DisplayFilterValueArgs( string key, string name, string value )
            {
                Key = key;
                Name = name;
                Value = value;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DisplayFilterValueArgs"/> class.
            /// </summary>
            /// <param name="userPreference">The user preference.</param>
            [RockObsolete( "1.7.4" )]
            [Obsolete( "DisplayFilterValueArgs(userPreference, prefix) instead", true )]
            public DisplayFilterValueArgs( UserPreference userPreference ) :
                this( userPreference, null )
            {
                //
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DisplayFilterValueArgs"/> class.
            /// </summary>
            /// <param name="userPreference">The user preference.</param>
            /// <param name="prefix">The prefix.</param>
            public DisplayFilterValueArgs( UserPreference userPreference, string prefix )
            {
                if ( !string.IsNullOrEmpty( prefix ) && userPreference.Key.StartsWith( prefix ) )
                {
                    Key = userPreference.Key.Substring( prefix.Length );
                }
                else
                {
                    Key = userPreference.Key;
                }

                Name = userPreference.Name;
                Value = userPreference.Value;
            }
        }

        /// <summary>
        /// Helper class for user preferences
        /// </summary>
        public class UserPreference
        {
            /// <summary>
            /// Gets or sets the key.
            /// </summary>
            /// <value>
            /// The key.
            /// </value>
            public string Key { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="UserPreference"/> class.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="name">The name.</param>
            /// <param name="value">The value.</param>
            public UserPreference( string key, string name, string value )
            {
                Key = key;
                Name = name;
                Value = value;
            }
        }
    }
}