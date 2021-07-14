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

using Rock.Model;
using Rock.Net;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// The most basic block type that all other blocks should inherit from. Provides
    /// default implementations of the <seealso cref="IRockBlockType"/> interface as
    /// well as a number of helper methods and properties to subclasses.
    /// </summary>
    /// <seealso cref="Rock.Blocks.IRockBlockType" />
    public abstract class RockBlockType : IRockBlockType
    {
        #region Properties

        /// <summary>
        /// Gets the block identifier.
        /// </summary>
        /// <value>
        /// The block identifier.
        /// </value>
        public int BlockId => BlockCache.Id;

        /// <summary>
        /// Gets or sets the block cache.
        /// </summary>
        /// <value>
        /// The block cache.
        /// </value>
        public BlockCache BlockCache { get; set ; }

        /// <summary>
        /// Gets or sets the page cache.
        /// </summary>
        /// <value>
        /// The page cache.
        /// </value>
        public PageCache PageCache { get; set; }

        /// <summary>
        /// Gets or sets the request context.
        /// </summary>
        /// <value>
        /// The request context.
        /// </value>
        public RockRequestContext RequestContext { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public abstract object GetBlockInitialization( RockClientType clientType );


        /// <summary>
        /// Gets the control markup.
        /// </summary>
        /// <returns></returns>
        public virtual string GetControlMarkup()
        {
            return string.Empty;
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public string GetAttributeValue( string key )
        {
            return BlockCache.GetAttributeValue( key );
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public object GetAttributeValueAsFieldType( string key )
        {
            var stringValue = GetAttributeValue( key );
            var attribute = BlockCache.Attributes.GetValueOrNull( key );
            var field = attribute?.FieldType?.Field;

            if ( field == null )
            {
                return stringValue;
            }

            return field.ValueAsFieldType( stringValue, attribute.QualifierValues );
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public T GetAttributeValueAsFieldType<T>( string key ) where T : class
        {
            var asObject = GetAttributeValueAsFieldType( key );
            return asObject as T;
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <returns></returns>
        protected Person GetCurrentPerson()
        {
            return RequestContext.CurrentPerson;
        }

        #endregion

        #region Action Response Methods

        /// <summary>
        /// Creates a 200-OK response with no content.
        /// </summary>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionOk()
        {
            return new BlockActionResult( System.Net.HttpStatusCode.OK );
        }

        /// <summary>
        /// Create a 200-OK response with the given content value.
        /// </summary>
        /// <typeparam name="T">The type of the content being returned.</typeparam>
        /// <param name="value">The value to be sent to the client.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionOk<T>( T value )
        {
            return new BlockActionResult( System.Net.HttpStatusCode.OK, value, typeof( T ) );
        }

        /// <summary>
        /// Create a response with the given status code.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionStatusCode( System.Net.HttpStatusCode statusCode )
        {
            return new BlockActionResult( statusCode );
        }

        /// <summary>
        /// Creates a generic response of the specified status code for the content value.
        /// </summary>
        /// <typeparam name="T">The type of the content being returned.</typeparam>
        /// <param name="statusCode">The status code.</param>
        /// <param name="value">The value to be sent to the client.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionContent<T>( System.Net.HttpStatusCode statusCode, T value )
        {
            return new BlockActionResult( statusCode, value, typeof( T ) );
        }

        /// <summary>
        /// Creates a 400-Bad Request response with an optional error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected BlockActionResult ActionBadRequest( string message = null )
        {
            if ( message == null )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.BadRequest );
            }
            else
            {
                return new BlockActionResult( System.Net.HttpStatusCode.BadRequest )
                {
                    Error = message
                };
            }
        }

        /// <summary>
        /// Creates a 403-Forbidden response with an optional error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected BlockActionResult ActionForbidden( string message = null )
        {
            if ( message == null )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.Forbidden );
            }
            else
            {
                return new BlockActionResult( System.Net.HttpStatusCode.Forbidden )
                {
                    Error = message
                };
            }
        }

        /// <summary>
        /// Creates a 404-Not Found response with no content.
        /// </summary>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionNotFound()
        {
            return new BlockActionResult( System.Net.HttpStatusCode.NotFound );
        }

        /// <summary>
        /// Creats a 500-Internal Server Error response with an optional error message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>A BlockActionResult instance.</returns>
        protected virtual BlockActionResult ActionInternalServerError( string message = null)
        {
            return new BlockActionResult( System.Net.HttpStatusCode.InternalServerError )
            {
                Error = message
            };
        }

        #endregion
    }
}
