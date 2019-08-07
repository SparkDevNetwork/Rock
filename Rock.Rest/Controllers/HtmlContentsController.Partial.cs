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
using System.Web.Http;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// HtmlContents REST API
    /// </summary>
    public partial class HtmlContentsController
    {
        /// <summary>
        /// Updates the contents.
        /// </summary>
        /// <param name="blockId">The block identifier.</param>
        /// <param name="htmlContents">The HTML contents.</param>
        [Authenticate]
        [HttpPost]
        [System.Web.Http.Route( "api/HtmlContents/UpdateContents/{blockId}" )]
        public void UpdateContents( int blockId, [FromBody] HtmlContents htmlContents )
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

            var person = GetPerson();

            var block = new BlockService( (RockContext)Service.Context ).Get( blockId );
            if ( block != null && block.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
            {
                var htmlContentService = (HtmlContentService)Service;
                var htmlContent = htmlContentService.GetActiveContent( blockId, htmlContents.EntityValue );
                if ( htmlContent != null )
                {
                    htmlContent.Content = htmlContents.Content;
                    if ( !System.Web.HttpContext.Current.Items.Contains( "CurrentPerson" ) )
                    {
                        System.Web.HttpContext.Current.Items.Add( "CurrentPerson", person );
                    }

                    Service.Context.SaveChanges();

                    HtmlContentService.FlushCachedContent( blockId, htmlContents.EntityValue );
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class HtmlContents
        {
            /// <summary>
            /// Gets or sets the entity value.
            /// </summary>
            /// <value>
            /// The entity value.
            /// </value>
            public string EntityValue { get; set; }
            
            /// <summary>
            /// Gets or sets the content.
            /// </summary>
            /// <value>
            /// The content.
            /// </value>
            public string Content { get; set; }
        }
    }
}
