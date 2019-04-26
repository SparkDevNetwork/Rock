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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    // Inspired by: https://weblog.west-wind.com/posts/2017/Sep/13/A-Literal-Markdown-Control-for-ASPNET-WebForms

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Web.UI.WebControls.Literal" />
    [DefaultProperty( "Text" )]
    [ToolboxData( "<{0}:Lava runat=server></{0}:Lava>" )]
    public class Lava : System.Web.UI.WebControls.Literal
    {
        /// <summary>
        /// Gets or sets a value indicating whether to try to strip whitespace before all lines based on the whitespace applied on the first line..
        /// </summary>
        /// <value>
        ///   <c>true</c> if white space should be normalized; otherwise, <c>false</c>.
        /// </value>
        [Description( "Tries to strip whitespace before all lines based on the whitespace applied on the first line." )]
        [Category( "Markdown" )]
        public bool NormalizeWhiteSpace { get; set; } = true;

        /// <summary>
        /// Sends server control content to a provided <see cref="T:System.Web.UI.HtmlTextWriter" /> object, which writes the content to be rendered on the client.
        /// </summary>
        /// <param name="writer">The <see cref="T:System.Web.UI.HtmlTextWriter" /> object that receives the server control content.</param>
        protected override void Render( HtmlTextWriter writer )
        {
            var html = ParseLava( Text, NormalizeWhiteSpace );
            writer.Write( html );
        }

        /// <summary>
        /// Parses the specified lava.
        /// </summary>
        /// <param name="lava">The lava.</param>
        /// <returns></returns>
        public static string Parse( string lava )
        {
            return ParseLava( lava );
        }

        #region Private Methods

        /// <summary>
        /// Parses the lava.
        /// </summary>
        /// <param name="lava">The lava.</param>
        /// <param name="normalizeWhiteSpace">if set to <c>true</c> [normalize white space].</param>
        /// <returns></returns>
        private static string ParseLava( string lava, bool normalizeWhiteSpace = true )
        {
            if ( string.IsNullOrEmpty( lava ) )
            {
                return string.Empty;
            }

            if ( normalizeWhiteSpace )
            {
                lava = NormalizeWhiteSpaceText( lava );
            }

            //var currentPerson = HttpContext.Current.Items["CurrentPerson"] as Person;
            //var currentPage = HttpContext.Current.Handler as RockPage;
            RockPage rockPage = null;
            if ( HttpContext.Current != null )
            {
                rockPage = HttpContext.Current.Handler as RockPage;
            }

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( rockPage );
            mergeFields.Add( "CurrentPage", PageCache.Get( rockPage.PageId ) );

            return lava.ResolveMergeFields( mergeFields, "All" );
        }

        /// <summary>
        /// Normalizes the white space text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        private static string NormalizeWhiteSpaceText( string text )
        {
            if ( string.IsNullOrEmpty( text ) )
            {
                return string.Empty;
            }

            var lines = GetLines( text );
            if ( lines.Length < 1 )
            {
                return text;
            }

            string line1 = null;

            // find first non-empty line
            for ( int i = 0; i < lines.Length; i++ )
            {
                line1 = lines[i];
                if ( !string.IsNullOrEmpty( line1 ) )
                    break;
            }

            if ( string.IsNullOrEmpty( line1 ) )
                return text;

            string trimLine = line1.TrimStart();
            int whitespaceCount = line1.Length - trimLine.Length;

            if ( whitespaceCount == 0 )
            {
                return text;
            }

            string whitespace = line1.Substring( 0, whitespaceCount );


            StringBuilder sb = new StringBuilder();
            for ( int i = 0; i < lines.Length; i++ )
            {
                sb.AppendLine( lines[i].Replace( whitespace, "" ) );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets the lines.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="maxLines">The maximum lines.</param>
        /// <returns></returns>
        private static string[] GetLines( string s, int maxLines = 0 )
        {
            if ( s == null )
            {
                return null;
            }

            s = s.Replace( "\r\n", "\n" );

            if ( maxLines < 1 )
            {
                return s.Split( new char[] { '\n' } );
            }

            return s.Split( new char[] { '\n' } ).Take( maxLines ).ToArray();
        }
        #endregion
    }
}
