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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

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
                string keyPrefix = string.Format( "grid-filter-{0}-", rockBlock.BlockId );

                foreach ( var userPreference in rockBlock.GetUserPreferences( keyPrefix ) )
                {
                    var blockKey = userPreference.Key.Replace( keyPrefix, string.Empty );
                    var keyName = blockKey.Split( new char[] {'|'}, StringSplitOptions.RemoveEmptyEntries );
                    if ( blockKey.Contains("|") && keyName.Length == 2 )
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

                writer.RenderBeginTag( HtmlTextWriterTag.H3 );
                writer.Write( "Filter Options" );
                writer.RenderEndTag();

                _hfVisible.RenderControl( writer );

                writer.AddAttribute( "class", visible ? "fa fa-chevron-up toggle-filter" : "fa fa-chevron-down toggle-filter" );
                writer.RenderBeginTag( HtmlTextWriterTag.I );
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

                var nonEmptyValues = _userPreferences.Where( v => !string.IsNullOrEmpty( v.Value ) ).ToList();
                if ( nonEmptyValues.Count > 0 )
                {
                    foreach ( var userPreference in nonEmptyValues )
                    {
                        DisplayFilterValueArgs args = new DisplayFilterValueArgs( userPreference );
                        if ( DisplayFilterValue != null )
                        {
                            DisplayFilterValue( this, args );
                        }

                        if ( !string.IsNullOrWhiteSpace( args.Value ) )
                        {
                            filterDisplay.AddOrIgnore( args.Name, args.Value );
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

                foreach ( Control child in Controls )
                {
                    // write new row
                    if ( cellCount >= cellsPerRow )
                    {
                        writer.RenderEndTag();
                        writer.AddAttribute("class", "row");
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        cellCount = 0;
                    }

                    if ( child != _lbFilter && child != _lbClearFilter && child != _hfVisible )
                    {
                        // add column
                        if (child.Visible)
                        {
                            writer.AddAttribute("class", "col-lg-4");
                            writer.RenderBeginTag(HtmlTextWriterTag.Div);
                        }

                        child.RenderControl( writer );

                        if (child.Visible)
                        {
                            writer.RenderEndTag();
                        }

                        cellCount++;
                    }
                }

                // write end row div
                writer.RenderEndTag();
            }
        }

        /// <summary>
        /// Gets the user preference for a given key if it exists
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetUserPreference( string key )
        {
            return _userPreferences.Where( p => p.Key == key ).Select( p => p.Value ).FirstOrDefault() ?? string.Empty;
        }

        /// <summary>
        /// Adds or updates an item in the User Preferences dictionary
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void SaveUserPreference( string key, string name, string value )
        {
            var userPreference = _userPreferences.FirstOrDefault( p => p.Key == key );
            if ( userPreference != null  )
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
                _userPreferences.Add( new UserPreference( key, name, value ) );
            }
        }

        /// <summary>
        /// Adds or updates an item in the User Preferences dictionary
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
                    rockBlock.SetUserPreference( string.Format( "{0}{1}|{2}", keyPrefix, userPreference.Key, userPreference.Name ), userPreference.Value );
                }
            }
        }

        /// <summary>
        /// Deletes the user preferences.
        /// </summary>
        public void DeleteUserPreferences()
        {
            RockBlock rockBlock = this.RockBlock();
            if ( rockBlock != null && _userPreferences != null )
            {
                foreach ( var userPreference in _userPreferences )
                {
                    rockBlock.DeleteUserPreference( userPreference.Key );
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
            public DisplayFilterValueArgs( UserPreference userPreference )
            {
                Key = userPreference.Key;
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