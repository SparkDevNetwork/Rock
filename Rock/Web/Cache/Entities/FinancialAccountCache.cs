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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached <see cref="Rock.Model.FinancialAccount"/>.
    /// </summary>
    [Serializable]
    [DataContract]
    public class FinancialAccountCache : ModelCache<FinancialAccountCache, Rock.Model.FinancialAccount>
    {
        #region Properties

        /// <inheritdoc cref="Rock.Model.FinancialAccount.Name" />
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.PublicName" />
        [DataMember]
        public string PublicName { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.ParentAccountId" />
        [DataMember]
        public int? ParentAccountId { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.GlCode" />
        [DataMember]
        public string GlCode { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.StartDate" />
        [DataMember]
        public DateTime? StartDate { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.EndDate" />
        [DataMember]
        public DateTime? EndDate { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.IsPublic" />
        [DataMember]
        public bool? IsPublic { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.CampusId" />
        [DataMember]
        public int? CampusId { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.Order" />
        [DataMember]
        public int Order { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.IsActive" />
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.Campus" />
        public CampusCache Campus
        {
            get
            {
                if ( CampusId.HasValue )
                {
                    return CampusCache.Get( CampusId.Value );
                }
                else
                {
                    return null;
                }
            }
        }

        /// <inheritdoc cref="Rock.Model.FinancialAccount.ParentAccount" />
        public FinancialAccountCache ParentAccount
        {
            get
            {
                if ( ParentAccountId.HasValue )
                {
                    return FinancialAccountCache.Get( this.ParentAccountId.Value );
                }
                else
                {
                    return null;
                }
            }
        }

        private readonly object _lockObject = new object();



        /// <summary>
        /// Gets the immediate-level child accounts.
        /// Use <see cref="GetDescendentFinancialAccounts()"/> to get child accounts recursive.
        /// </summary>
        /// <value>
        /// The child accounts.
        /// </value>
        public FinancialAccountCache[] ChildAccounts
        {
            get
            {
                var childAccounts = new List<FinancialAccountCache>();

                if ( ChildAccountIds == null )
                {
                    lock ( _lockObject )
                    {
                        if ( ChildAccountIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                ChildAccountIds = new FinancialAccountService( rockContext )
                                    .Queryable().Where( a => a.ParentAccountId.HasValue && a.ParentAccountId.Value == this.Id )
                                    .Select( g => g.Id )
                                    .ToArray();
                            }
                        }
                    }
                }


                if ( ChildAccountIds == null )
                {
                    return childAccounts.ToArray();
                }

                foreach ( var id in ChildAccountIds )
                {
                    var financialAccount = Get( id );
                    if ( financialAccount != null )
                    {
                        childAccounts.Add( financialAccount );
                    }
                }

                return childAccounts.OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToArray();
            }
        }

        [DataMember]
        private int[] ChildAccountIds { get; set; }

        #endregion Properties

        #region Methods

        /// <inheritdoc cref="FinancialAccount.ParentAccountIds"/>
        public FinancialAccountCache[] GetAncestorFinancialAccounts()
        {
            List<FinancialAccountCache> parentAccounts = new List<FinancialAccountCache>();
            var parentAccount = this.ParentAccount;
            while ( parentAccount != null )
            {
                parentAccounts.Add( parentAccount );
                parentAccount = parentAccount.ParentAccount;
            }

            return parentAccounts.ToArray();
        }

        /// <inheritdoc cref="FinancialAccount.ParentAccountIds"/>
        public int[] GetAncestorFinancialAccountIds()
        {
            return GetAncestorFinancialAccounts().Select( a => a.Id ).ToArray();
        }

        /// <summary>
        /// Gets FinancialAccountCaches by guids.
        /// </summary>
        /// <param name="accountGuids">The account guids.</param>
        /// <returns></returns>
        public static FinancialAccountCache[] GetByGuids( IEnumerable<Guid> accountGuids )
        {
            return accountGuids.Select( a => FinancialAccountCache.Get( a ) ).Where( a => a != null ).ToArray();
        }

        /// <summary>
        /// Gets FinancialAccountCaches by Ids
        /// </summary>
        /// <param name="accountIds">The account ids.</param>
        /// <returns></returns>
        public static FinancialAccountCache[] GetByIds( IEnumerable<int> accountIds )
        {
            return accountIds.Select( a => FinancialAccountCache.Get( a ) ).Where( a => a != null ).ToArray();
        }

        /// <summary>
        /// Gets the descendent financial accounts (all children recursively)
        /// </summary>
        /// <returns></returns>
        public FinancialAccountCache[] GetDescendentFinancialAccounts()
        {
            List<int> recursionControl = new List<int>();
            var results = GetDescendentFinancialAccountsWithRecursionCheck( this, recursionControl );

            return results;
        }

        /// <summary>
        /// Gets the descendent financial account ids (all children recursively)
        /// </summary>
        /// <returns></returns>
        public int[] GetDescendentFinancialAccountIds()
        {
            return GetDescendentFinancialAccounts().Select( a => a.Id ).ToArray();
        }

        /// <summary>
        /// Gets the descendent financial accounts.
        /// </summary>
        /// <param name="financialAccount">The financial account.</param>
        /// <param name="recursionControl">The recursion control.</param>
        /// <returns></returns>
        private FinancialAccountCache[] GetDescendentFinancialAccountsWithRecursionCheck( FinancialAccountCache financialAccount, List<int> recursionControl = null )
        {
            var results = new List<FinancialAccountCache>();

            if ( financialAccount == null )
            {
                return results.ToArray();
            }

            recursionControl = recursionControl ?? new List<int>();
            if ( !recursionControl.Contains( financialAccount.Id ) )
            {
                recursionControl.Add( financialAccount.Id );

                // don't include the current account, we only want the children.
                if ( financialAccount != this )
                {
                    results.Add( financialAccount );
                }

                foreach ( var childGroupType in financialAccount.ChildAccounts )
                {
                    var childResults = GetDescendentFinancialAccountsWithRecursionCheck( childGroupType, recursionControl );
                    foreach ( var childAccount in childResults )
                    {
                        results.Add( childAccount );
                    }
                }
            }

            return results.ToArray();
        }

        #endregion Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var financialAccount = entity as Rock.Model.FinancialAccount;
            if ( financialAccount == null )
            {
                return;
            }

            this.Name = financialAccount.Name;
            this.PublicName = financialAccount.PublicName;
            this.ParentAccountId = financialAccount.ParentAccountId;
            this.GlCode = financialAccount.GlCode;
            this.StartDate = financialAccount.StartDate;
            this.EndDate = financialAccount.EndDate;
            this.IsPublic = financialAccount.IsPublic;
            this.CampusId = financialAccount.CampusId;
            this.IsActive = financialAccount.IsActive;
            this.Order = financialAccount.Order;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
