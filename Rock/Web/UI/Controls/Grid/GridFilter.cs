//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [ToolboxData( "<{0}:GridFilter runat=server></{0}:GridFilter>" )]
    public class GridFilter : PlaceHolder, INamingContainer
    {
        private HiddenField hfVisible;
        private LinkButton lbFilter;
        private Dictionary<string, string> _userPreferences;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Get User Values
            _userPreferences = new Dictionary<string, string>();

            RockBlock rockBlock = this.RockBlock();
            if ( rockBlock != null )
            {
                string keyPrefix = string.Format( "grid-filter-{0}-", rockBlock.CurrentBlock.Id );

                foreach ( var userPreference in rockBlock.GetUserPreferences( keyPrefix ) )
                {
                    _userPreferences.Add( userPreference.Key.Replace( keyPrefix, string.Empty ), userPreference.Value );
                }
            }

            string scriptKey = "grid-filter-script";
            string script = @"
Sys.Application.add_load(function () {

    $('div.grid-filter header').click(function () {
        $('i.toggle-filter', this).toggleClass('icon-chevron-down icon-chevron-up');
        var $hf = $('input', this).first();
        if($hf.val() != 'true') {
            $hf.val('true');
        } else {
            $hf.val('false');
        }
        $(this).siblings('div').slideToggle();
    });

});";
            if ( !this.Page.ClientScript.IsStartupScriptRegistered( scriptKey ) )
            {
                this.Page.ClientScript.RegisterStartupScript( this.Page.GetType(), scriptKey, script, true );
            }
        }

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            hfVisible = new HiddenField();
            Controls.Add( hfVisible );
            hfVisible.ID = "hfVisible";

            lbFilter = new LinkButton();
            Controls.Add( lbFilter );
            lbFilter.ID = "lbFilter";
            lbFilter.CssClass = "filter btn btn-primary btn-xs";
            lbFilter.ToolTip = "Apply Filter";
            lbFilter.Text = "Apply Filter";
            lbFilter.CausesValidation = false;
            lbFilter.Click += lbFilter_Click;
        }

        /// <summary>
        /// Handles the Click event of the lbFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        protected void lbFilter_Click( object sender, System.EventArgs e )
        {
            hfVisible.Value = "false";

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
            bool visible = hfVisible.Value == "true";

            writer.AddAttribute( "class", "grid-filter" );
            writer.RenderBeginTag( HtmlTextWriterTag.Div );

            writer.Write( "<header>" );

            writer.RenderBeginTag( HtmlTextWriterTag.H3 );
            writer.Write( "Filter Options" );
            writer.RenderEndTag();

            hfVisible.RenderControl( writer );

            writer.AddAttribute( "class", visible ? "icon-chevron-up toggle-filter" : "icon-chevron-down toggle-filter" );
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

            var nonEmptyValues = _userPreferences.Where( v => !string.IsNullOrEmpty( v.Value ) ).ToList();
            if ( nonEmptyValues.Count > 0 )
            {
                writer.RenderBeginTag( HtmlTextWriterTag.Fieldset );

                writer.Write( "<h4>Enabled Filters</h4>" );

                foreach ( var userPreference in nonEmptyValues )
                {
                    DisplayFilterValueArgs args = new DisplayFilterValueArgs( userPreference.Key, userPreference.Value );
                    if ( DisplayFilterValue != null )
                    {
                        DisplayFilterValue( this, args );
                    }

                    if ( !string.IsNullOrWhiteSpace( args.Value ) )
                    {
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        writer.Write( string.Format( "{0}: {1}", args.Key, args.Value ) );
                        writer.RenderEndTag();
                    }
                }

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

            lbFilter.RenderControl( writer );

            writer.RenderEndTag();

            writer.RenderEndTag();

            SaveUserPreferences();
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
                int cellsPerRow = 3;

                // write first row
                writer.AddAttribute("class", "row");
                writer.RenderBeginTag( HtmlTextWriterTag.Div );

                foreach ( Control child in Controls )
                {
                    // write new row
                    if (cellCount >= cellsPerRow)
                    {
                        writer.RenderEndTag();
                        writer.AddAttribute("class", "row");
                        writer.RenderBeginTag( HtmlTextWriterTag.Div );
                        cellCount = 0;
                    }

                    if ( child != lbFilter && child != hfVisible )
                    {
                        // add row column
                        writer.AddAttribute("class", "col-md-4");
                        writer.RenderBeginTag(HtmlTextWriterTag.Div);

                        child.RenderControl( writer );

                        writer.RenderEndTag();

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
            if ( _userPreferences.ContainsKey( key ) )
            {
                return _userPreferences[key];
            }
            return string.Empty;
        }

        /// <summary>
        /// Adds or updates an item in the User Preferences dictionary
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SaveUserPreference( string key, string value )
        {
            if ( _userPreferences.ContainsKey( key ) )
            {
                _userPreferences[key] = value;
            }
            else
            {
                _userPreferences.Add( key, value );
            }
        }

        private void SaveUserPreferences()
        {
            RockBlock rockBlock = this.RockBlock();
            if ( rockBlock != null )
            {
                string keyPrefix = string.Format( "grid-filter-{0}-", rockBlock.CurrentBlock.Id );

                foreach ( var userPreference in _userPreferences )
                {
                    rockBlock.SetUserPreference( keyPrefix + userPreference.Key, userPreference.Value );
                }
            }
        }

        /// <summary>
        /// Occurs when user applies a filter.
        /// </summary>
        public event EventHandler ApplyFilterClick;

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
            public string Key
            {
                get { return _key; }
                set { _key = value; }
            }
            private string _key = string.Empty;
            
            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            /// <value>
            /// The value.
            /// </value>
            public string Value
            {
                get { return _value; }
                set { _value = value; }
            }
            private string _value = string.Empty;

            /// <summary>
            /// Initializes a new instance of the <see cref="DisplayFilterValueArgs" /> class.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="value">The value.</param>
            public DisplayFilterValueArgs( string key, string value )
            {
                _key = key;
                _value = value;
            }
        }
    }
}