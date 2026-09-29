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
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Administration
{
    /// <summary>
    /// Displays a notice to administrators when the running version of Rock
    /// has known security vulnerabilities.
    /// </summary>
    /// <seealso cref="RockBlockType" />
    [DisplayName( "Security Version Notice" )]
    [Category( "Administration" )]
    [Description( "Displays a notice to administrators when the running version of Rock has known security vulnerabilities." )]
    [IconCssClass( "ti ti-shield-half" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "31922DA8-A172-4B85-8FC7-BB07BCBBCDE5" )]
    [SystemGuid.BlockTypeGuid( "82BA1CB3-7220-46D1-9B5B-BC07F6D9005A" )]
    public class SecurityVersionNotice : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override async Task<string> GetControlMarkupAsync()
        {
            var status = await RockSecurityVersionHelper.GetStatusAsync();

            if ( status.IsSecure )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.Append( "<div class=\"alert alert-danger\">" );
            sb.Append( $"You're currently on Rock v{status.CurrentVersion.EncodeHtml()}, which has known security vulnerabilities." );

            if ( status.SecureVersion.IsNotNullOrWhiteSpace() )
            {
                sb.Append( $" We'd strongly encourage updating to v{status.SecureVersion.EncodeHtml()} or later soon to keep your data safe." );
            }
            else
            {
                sb.Append( " We'd strongly encourage updating to the latest version soon to keep your data safe." );
            }

            if ( status.Message.IsNotNullOrWhiteSpace() )
            {
                sb.Append( $"<p class=\"margin-t-sm\">{status.Message.EncodeHtml().ConvertCrLfToHtmlBr()}</p>" );
            }

            var rockUpdatePage = PageCache.Get( SystemGuid.Page.ROCK_UPDATE.AsGuid() );

            if ( rockUpdatePage != null )
            {
                var pageRef = new Rock.Web.PageReference( rockUpdatePage.Id );
                var rockUpdateUrl = pageRef.BuildUrl();

                sb.Append( $"<div class=\"margin-t-sm\"><a href=\"{rockUpdateUrl.EncodeHtml()}\" class=\"btn btn-danger btn-sm\">Update Rock</a></div>" );
            }

            sb.Append( "</div>" );

            return sb.ToString();
        }

        #endregion
    }
}
