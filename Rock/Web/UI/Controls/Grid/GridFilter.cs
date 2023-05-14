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
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Options for the layout of controls in the Grid Filter  Enum that defines when a column should be included in an Excel export ( when in ColumnOutput ExportSource )
    /// </summary>
    public enum GridFilterLayoutSpecifier
    {
        /// <summary>
        /// The default
        /// </summary>
        Default = 0,
        /// <summary>
        /// Layout the filter field controls as defined in the control markup.
        /// </summary>
        Custom = 1,
        /// <summary>
        /// Layout the filter field controls using a 2-column bootstrap layout.
        /// </summary>
        TwoColumnLayout = 2,
        /// <summary>
        /// Layout the filter field controls using a 3-column bootstrap layout.
        /// </summary>
        ThreeColumnLayout = 3
    }

    /// <summary>
    ///
    /// </summary>
    [ToolboxData( "<{0}:GridFilter runat=server></{0}:GridFilter>" )]
    public class GridFilter : PlaceHolder, INamingContainer
    {
        private HiddenField _hfVisible;
        private LinkButton _lbFilter;
        private LinkButton _lbClearFilter;
        private PersonPreferenceCollection _preferences;

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

            RockBlock rockBlock = this.RockBlock();
            if ( rockBlock != null )
            {
                _preferences = rockBlock.GetBlockPersonPreferences().WithPrefix( "grid-filter-" );
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
        [Obsolete( "Use PreferenceKeyPrefix instead." )]
        [RockObsolete( "1.16" )]
        public string UserPreferenceKeyPrefix
        {
            get => PreferenceKeyPrefix;
            set => PreferenceKeyPrefix = value;
        }

        /// <summary>
        /// <para>
        /// Gets or sets the optional preference key prefix. This will be
        /// included as part of the standard grid filter prefix when accessing
        /// the person preferences.
        /// </para>
        /// <para>
        /// For example, if this is a filter for a GroupMemberList, you might want
        /// the value be `<c>{{ GroupId }}-</c>` so that user preferences for this grid
        /// filter are per group instead of just per block.
        /// </para>
        /// </summary>
        /// <value>
        /// The user preference key prefix.
        /// </value>
        public string PreferenceKeyPrefix
        {
            get => ViewState["PreferenceKeyPrefix"] as string ?? string.Empty;
            set => ViewState["PreferenceKeyPrefix"] = value;
        }

        /// <summary>
        /// Gets or sets the layout format for the filter fields displayed by the control.
        /// </summary>
        public GridFilterLayoutSpecifier FieldLayout
        {
            get
            {
                var stateValue = ViewState["FieldLayout"];

                return ( stateValue == null ) ? GridFilterLayoutSpecifier.Default : ( GridFilterLayoutSpecifier ) stateValue;
            }

            set
            {
                ViewState["FieldLayout"] = value;
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
    $('div.grid-filter header').on('click', function () {
        $('.btn-filter-toggle', this).toggleClass('is-open').find('i.toggle-filter').toggleClass('fa-chevron-down fa-chevron-up');
        var $hf = $('input', this).first();
        if($hf.val() != 'true') {
            $hf.val('true');
        } else {
            $hf.val('false');
        }
        $(this).siblings('div').slideToggle();
        return false;
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
                
                var filterDisplay = new Dictionary<string, string>();
                AdditionalFilterDisplay.ToList().ForEach( d => filterDisplay.Add( d.Key, d.Value ) );
                PersonPreferenceCollection preferences;

                if ( PreferenceKeyPrefix.IsNotNullOrWhiteSpace() )
                {
                    preferences = _preferences.WithPrefix( PreferenceKeyPrefix );
                }
                else
                {
                    preferences = _preferences;
                }

                foreach ( var key in preferences.GetKeys() )
                {
                    var keyName = key.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries );

                    if ( keyName.Length < 2 )
                    {
                        continue;
                    }

                    var value = preferences.GetValue( key );

                    if ( value.IsNullOrWhiteSpace() )
                    {
                        continue;
                    }

                    var args = new DisplayFilterValueArgs( keyName[0], keyName[1], value );
                    if ( DisplayFilterValue != null )
                    {
                        DisplayFilterValue( this, args );
                    }

                    if ( !string.IsNullOrWhiteSpace( args.Value ) )
                    {
                        filterDisplay.AddOrReplace( args.Name, args.Value );
                    }
                }

                if ( filterDisplay.Any() )
                {
                    writer.AddAttribute( "class", "grid-filter has-criteria" );
                }
                else
                {
                    writer.AddAttribute( "class", "grid-filter" );
                }

                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                writer.Write( "<header>" );

                writer.AddAttribute( HtmlTextWriterAttribute.Class, visible ? "btn btn-link btn-xs btn-filter-toggle is-open" : "btn btn-link btn-xs btn-filter-toggle" );
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



                if ( filterDisplay.Any() )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Fieldset );
                    writer.WriteLine( "<h4>Enabled Filters</h4>" );
                    writer.WriteLine( "<div class='row'>" );

                    // Calculate the filter column size by dividing the Bootstrap 12-column layout into equal widths.
                    int columnSize = ( this.FieldLayout == GridFilterLayoutSpecifier.TwoColumnLayout ) ? 6 : 4;

                    foreach ( var filterNameValue in filterDisplay.OrderBy( f => f.Key ) )
                    {
                        writer.WriteLine( "<div class='col-md-{0}'>", columnSize );
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

                _preferences.Save();

                RegisterJavaScript();
            }
        }

        /// <summary>
        /// Outputs the content of a server control's children to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the rendered content.</param>
        protected override void RenderChildren( HtmlTextWriter writer )
        {
            if ( this.Controls == null )
            {
                return;
            }

            if ( this.FieldLayout == GridFilterLayoutSpecifier.Custom )
            {
                // If custom layout specified, do not reformat the child controls.
                foreach ( Control child in this.Controls )
                {
                    if ( child == _lbFilter || child == _lbClearFilter || child == _hfVisible )
                    {
                        continue;
                    }

                    child.RenderControl( writer );
                }
            }
            else
            {
                // wrap filter items in bootstrap responsive grid
                int cellCount = 0;

                // Calculate the Bootstrap column size by dividing the 12-column layout into equal partitions.
                int cellsPerRow = ( this.FieldLayout == GridFilterLayoutSpecifier.TwoColumnLayout ) ? 2 : 3;

                int bootstrapColumnSize = 12 / cellsPerRow;

                // write first row
                writer.AddAttribute( "class", "row" );
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
                            writer.AddAttribute( "class", string.Format( "col-lg-{0}", bootstrapColumnSize ) );
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
        /// Gets the Key after the <see cref="PreferenceKeyPrefix"></see> has been applied
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetPrefixedKey( string key )
        {
            return $"{PreferenceKeyPrefix}{key}";
        }

        /// <summary>
        /// <para>
        /// Gets the person preference filter value for a given key.
        /// </para>
        /// <para>
        /// Note: Key is internally stored with a custom prefix and optionally
        /// the <see cref="PreferenceKeyPrefix"/> prefix to make it unique.
        /// </para>
        /// </summary>
        /// <param name="key">The key to be retrieved.</param>
        /// <returns>The value of the <paramref name="key"/> or an empty string if no value was found.</returns>
        public string GetFilterPreference( string key )
        {
            var keyPrefixKey = $"{GetPrefixedKey( key )}|";

            var finalKey = _preferences.GetKeys()
                .Where( k => k.StartsWith( keyPrefixKey ) )
                .FirstOrDefault();

            if ( finalKey.IsNullOrWhiteSpace() )
            {
                return string.Empty;
            }

            return _preferences.GetValue( finalKey );
        }

        /// <summary>
        /// <para>
        /// Sets a person preference filter value for the Grid filter.
        /// </para>
        /// <para>
        /// Note: Key is internally stored with a custom prefix and optionally
        /// the <see cref="PreferenceKeyPrefix"/> prefix to make it unique.
        /// </para>
        /// </summary>
        /// <param name="key">The key whose value should be set.</param>
        /// <param name="name">The name of the filter field.</param>
        /// <param name="value">The filter value.</param>
        public void SetFilterPreference( string key, string name, string value )
        {
            var keyPrefixKey = $"{GetPrefixedKey( key )}|";
            var finalKey = $"{keyPrefixKey}{name}";

            // No duplicate user preference key values before the '|' are allowed.
            // This search for any keys that match before the '|' but mismatch after '|' and delete it before writing the user preference.
            var duplicateKeys = _preferences.GetKeys()
                .Where( k => k.StartsWith( keyPrefixKey ) && k != finalKey )
                .ToList();

            foreach ( var duplicateKey in duplicateKeys )
            {
                _preferences.SetValue( duplicateKey, string.Empty );
            }

            _preferences.SetValue( finalKey, value );
        }

        /// <summary>
        /// <para>
        /// Sets a person preference filter value for the Grid filters. The field name
        /// will be the same as the key.
        /// </para>
        /// <para>
        /// Preferences are automatically saved during the page rendering stage.
        /// </para>
        /// <para>
        /// Note: Key is internally stored with a custom prefix and optionally
        /// the <see cref="PreferenceKeyPrefix"/> prefix to make it unique.
        /// </para>
        /// </summary>
        /// <param name="key">The key whose value should be set.</param>
        /// <param name="value">The filter value.</param>
        public void SetFilterPreference( string key, string value )
        {
            SetFilterPreference( key, key, value );
        }

        /// <summary>
        /// Deletes all the grid person preferences for all filters values on the
        /// block. If <see cref="PreferenceKeyPrefix"/> is set, it will only
        /// delete preferences for the grid filter(s) on the block that match
        /// the prefix.
        /// </summary>
        public void DeleteFilterPreferences()
        {
            foreach ( var key in _preferences.GetKeys() )
            {
                if ( PreferenceKeyPrefix.IsNullOrWhiteSpace() || key.StartsWith( PreferenceKeyPrefix ) )
                {
                    _preferences.SetValue( key, string.Empty );
                }
            }

            _preferences.Save();
        }

        /// <summary>
        /// Gets the user preference for a given key if it exists.
        /// Note: Key is internally stored with the Block.Id plus any <see cref="PreferenceKeyPrefix"/> to make it unique per block and <see cref="PreferenceKeyPrefix"/>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [Obsolete( "Use GetFilterPreference() instead." )]
        [RockObsolete( "1.16" )]
        public string GetUserPreference( string key )
        {
            return GetFilterPreference( key );
        }

        /// <summary>
        /// Adds or updates an item in the User Preferences dictionary.
        /// Note: Key is internally stored with the Block.Id plus any <see cref="PreferenceKeyPrefix"/> to make it unique per block and <see cref="PreferenceKeyPrefix"/>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        [Obsolete( "Use SetFilterPreference() instead." )]
        [RockObsolete( "1.16" )]
        public void SaveUserPreference( string key, string name, string value )
        {
            SetFilterPreference( key, name, value );
        }

        /// <summary>
        /// Adds or updates an item in the User Preferences dictionary.
        /// Note: Key is internally stored with the Block.Id plus any <see cref="PreferenceKeyPrefix"/> to make it unique per block and <see cref="PreferenceKeyPrefix"/>
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        [Obsolete( "Use SetFilterPreference() instead." )]
        [RockObsolete( "1.16" )]
        public void SaveUserPreference( string key, string value )
        {
            SaveUserPreference( key, key, value );
        }

        /// <summary>
        /// Deletes all the grid user preferences for all grid filters on the block. If <see cref="PreferenceKeyPrefix"/> is set, it will only delete user preferences for the grid filter(s) on the block that has the PreferenceKeyPrefix.
        /// </summary>
        [Obsolete( "Use DeleteFilterPreferences() instead." )]
        [RockObsolete( "1.16" )]
        public void DeleteUserPreferences()
        {
            DeleteFilterPreferences();
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
            /// Note: This is the Key without the internally stored Block.Id plus any <see cref="PreferenceKeyPrefix"/> prefix
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
            /// Initializes a new instance of the <see cref="DisplayFilterValueArgs"/> class.
            /// </summary>
            /// <param name="userPreference">The user preference.</param>
            /// <param name="prefix">The prefix.</param>
            [Obsolete( "This constructor is not used anymore." )]
            [RockObsolete( "1.16" )]
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

            internal DisplayFilterValueArgs( string key, string name, string value )
            {
                Key = key;
                Name = name;
                Value = value;
            }
        }

        /// <summary>
        /// Helper class for user preferences
        /// </summary>
        [Obsolete( "This is not used anymore." )]
        [RockObsolete( "1.16" )]
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