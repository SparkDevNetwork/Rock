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
using System.Linq;
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
        [Rock.SystemGuid.RestActionGuid( "6236DEEB-8536-4485-890B-0FA4B0C86F81" )]
        public void UpdateContents( int blockId, [FromBody] HtmlContents htmlContents )
        {
            // Enable proxy creation since security is being checked and need to navigate parent authorities
            SetProxyCreation( true );

            var person = GetPerson();

            var block = new BlockService( ( RockContext ) Service.Context ).Get( blockId );
            if ( block != null && block.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
            {
                block.LoadAttributes( ( RockContext ) Service.Context );

                // Shared content may only be written through a block that uses the same context name.
                var contextName = block.GetAttributeValue( "ContextName" );
                var isSharedContent = htmlContents.EntityValue != null && htmlContents.EntityValue.Contains( "&ContextName=" );

                if ( isSharedContent && ( contextName.IsNullOrWhiteSpace() || !htmlContents.EntityValue.EndsWith( "&ContextName=" + contextName ) ) )
                {
                    throw new HttpResponseException( System.Net.HttpStatusCode.Unauthorized );
                }

                var isApprovalRequired = block.GetAttributeValue( "SupportVersions" ).AsBoolean()
                    && block.GetAttributeValue( "RequireApproval" ).AsBoolean();

                if ( isApprovalRequired && !block.IsAuthorized( Rock.Security.Authorization.APPROVE, person ) )
                {
                    throw new HttpResponseException( System.Net.HttpStatusCode.Unauthorized );
                }

                var htmlContentService = ( HtmlContentService ) Service;
                var htmlContent = htmlContentService.GetActiveContentQueryable( blockId, htmlContents.EntityValue ).FirstOrDefault();
                if ( htmlContent != null )
                {
                    htmlContent.Content = htmlContents.Content;
                    System.Web.HttpContext.Current.AddOrReplaceItem( "CurrentPerson", person );

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