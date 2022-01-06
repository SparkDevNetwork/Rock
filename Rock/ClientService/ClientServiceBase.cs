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

using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.ClientService
{
    /// <summary>
    /// A base class for all client service classes to inherit from. Provides
    /// the base for a standard feature set that child implementations should
    /// make use of, such as enforcing security checks by default.
    /// </summary>
    public abstract class ClientServiceBase
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether security checks are enabled.
        /// When enabled, any entity that implements <see cref="ISecured"/> will
        /// be checked to see if the person has view access to it.
        /// </summary>
        /// <value>
        ///   <c>true</c> if security checks are enabled; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSecurity { get; set; } = true;

        /// <summary>
        /// Gets the rock context to use when accessing the database.
        /// </summary>
        /// <value>The rock context to use when accessing the database.</value>
        protected RockContext RockContext { get; }

        /// <summary>
        /// Gets or sets the person to use for security checks.
        /// </summary>
        /// <value>The person to use for security checks.</value>
        protected Person Person { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Rock.ViewModel.Client.ClientHelper" /> class.
        /// </summary>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <param name="person">The person to use for security checks, generally the "current person".</param>
        public ClientServiceBase( RockContext rockContext, Person person )
        {
            RockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            Person = person;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Checks the security of the entities to ensure the person has access
        /// to view them. If <see cref="EnableSecurity"/> is <c>false</c> then
        /// this method simply returns the original enumerable. Any entities
        /// that the person does not have access to are filtered out.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="enumerable">The enumerable to be filtered.</param>
        /// <returns>An enumerable of entities which the person has view access to.</returns>
        protected IEnumerable<TEntity> CheckSecurity<TEntity>( IEnumerable<TEntity> enumerable )
            where TEntity : ISecured
        {
            if ( !EnableSecurity )
            {
                return enumerable;
            }

            // Only do the ToList() call if we were passed a queryable since
            // we can't do IsAuthorized() checks on database queries. This
            // provides a small performance boost.
            if ( enumerable is IQueryable )
            {
                return enumerable.ToList()
                    .Where( e => e.IsAuthorized( Authorization.VIEW, Person ) );
            }

            return enumerable.Where( e => e.IsAuthorized( Authorization.VIEW, Person ) );
        }

        #endregion
    }
}
