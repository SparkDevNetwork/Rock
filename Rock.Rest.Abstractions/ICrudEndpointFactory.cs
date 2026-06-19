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
using Rock.Data;

namespace Rock.Rest;

/// <summary>
/// Factory for creating instances of the <see cref="CrudEndpoint{TEntity, TService}"/>
/// helper.
/// </summary>
public interface ICrudEndpointFactory
{
    /// <summary>
    /// Creates a new instance of the <see cref="CrudEndpoint{TEntity, TService}"/>
    /// class for the specified entity and service types.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TService">The type of the service class.</typeparam>
    /// <param name="controller">The controller for which to create the helper.</param>
    /// <returns>A new instance of the <see cref="CrudEndpoint{TEntity, TService}"/> class.</returns>
    CrudEndpoint<TEntity, TService> Create<TEntity, TService>( ApiControllerBase controller )
        where TEntity : class, IEntity, new()
        where TService : Service<TEntity>;
}
