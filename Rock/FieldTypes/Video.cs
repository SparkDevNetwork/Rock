//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.FieldTypes
{
    public class Video : Field
    {
        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, bool condensed )
        {
            if ( value.Trim() != string.Empty )
            {


                string poster = string.Empty;
                string video = value;

                if ( value.Contains( "|" ) )
                {
                    string[] values = value.Split( '|' );
                    if ( values.Length >= 2 )
                    {
                        poster = string.Format( "poster='{0}'", values[0] );
                        video = values[1];
                    }
                }

                return string.Format( @"
    <video id='{0}_video' class='video-js vjs-default-skin' controls width='640' height='264' {1} preload='auto'>
        <source type='video/mp4' src='{2}'/>
    </video>
    <script>
        var myPlayer = _V_('{0}_video');
    </script>
", parentControl.ID, poster, video );
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Adds any required CSS or Script Links to the current page
        /// </summary>
        /// <param name="page">The page.</param>
        public static void AddLinks( Page page )
        {
            Rock.Web.UI.Page.AddCSSLink( page, "http://vjs.zencdn.net/c/video-js.css" );
            Rock.Web.UI.Page.AddScriptLink( page, "http://vjs.zencdn.net/c/video.js" );
        }
    }

}