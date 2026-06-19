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
using System.Collections.Generic;

using Rock.Data;
using Rock.ViewModels.Core;

#if WEBFORMS
using IActionResult = System.Web.Http.IHttpActionResult;
#endif

namespace Rock.Rest;

/// <summary>
/// Helper class for providing standard CRUD API actions to various
/// entity controllers.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TService">The type of the service class.</typeparam>
public abstract class CrudEndpoint<TEntity, TService>
    where TEntity : class, IEntity, new()
    where TService : Service<TEntity>
{
    /// <summary>
    /// A value indicating whether security is ignored. When security is
    /// not ignored the entity will be checked for either VIEW or EDIT
    /// permissions depending on the operation.
    /// </summary>
    public bool IsSecurityIgnored { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CrudEndpoint{TEntity, TService}"/> class.
    /// </summary>
    internal CrudEndpoint()
    {
    }

    #region API Methods

    /// <summary>
    /// POST endpoint. Use this to INSERT a new <typeparamref name="TEntity"/> entity.
    /// </summary>
    /// <param name="entity">The entity to be created.</param>
    /// <returns>The response that should be sent back.</returns>
    public abstract IActionResult Create( TEntity entity );

    /// <summary>
    /// GET endpoint. Use this to get an existing <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
    /// <returns>The response that should be sent back.</returns>
    public abstract IActionResult Get( string id );

    /// <summary>
    /// PUT endpoint. Use this to update an existing <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
    /// <param name="entity">The entity data to update the existing entity with.</param>
    /// <returns>The response that should be sent back.</returns>
    public abstract IActionResult Update( string id, TEntity entity );

    /// <summary>
    /// DELETE endpoint. Use this to delete an existing <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
    /// <returns>The response that should be sent back.</returns>
    public abstract IActionResult Delete( string id );

    /// <summary>
    /// PATCH endpoint. Use this to perform a partial update to an
    /// existing <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
    /// <param name="values">The new values to be set on the entity.</param>
    /// <returns>The response that should be sent back.</returns>
    public abstract IActionResult Patch( string id, Dictionary<string, object> values );

    /// <summary>
    /// Get all the attribute values for the <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
    /// <returns>The response that should be sent back.</returns>
    public abstract IActionResult GetAttributeValues( string id );

    /// <summary>
    /// PATCH endpoint. Use this to perform a partial update of attribute
    /// values to an existing <typeparamref name="TEntity"/>.
    /// </summary>
    /// <param name="id">The identifier, unique identifier or IdKey of the item.</param>
    /// <param name="values">The new values to be set on the entity.</param>
    /// <returns>The response that should be sent back.</returns>
    public abstract IActionResult PatchAttributeValues( string id, Dictionary<string, string> values );

    /// <summary>
    /// POST endpoint. Use this to perform a query via a user supplied
    /// entity search query. This should be considered an administrative
    /// level search since no security is checked and no limitations are
    /// set by the system.
    /// </summary>
    /// <param name="query">The custom user query options.</param>
    /// <returns>The response that should be sent back.</returns>
    public abstract IActionResult Search( EntitySearchQueryBag query );

    /// <summary>
    /// GET and POST endpoint. Use this to perform a query via a defined
    /// Entity Search.
    /// </summary>
    /// <param name="searchKey">The search key to use for the query.</param>
    /// <param name="query">The custom user query options.</param>
    /// <returns>The response that should be sent back.</returns>
    public abstract IActionResult Search( string searchKey, EntitySearchQueryBag query );

    #endregion
}
