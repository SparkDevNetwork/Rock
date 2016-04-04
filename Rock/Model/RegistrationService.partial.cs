﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RegistrationService
    {
        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns></returns>
        public IQueryable<FinancialTransactionDetail> GetPayments( int registrationId )
        {
            int registrationEntityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Registration ) ).Id;
            return new FinancialTransactionDetailService( (RockContext)this.Context )
                .Queryable( "Transaction" )
                .Where( t =>
                    t.EntityTypeId == registrationEntityTypeId &&
                    t.EntityId == registrationId );
        }

        /// <summary>
        /// Gets the total payments.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns></returns>
        public decimal GetTotalPayments( int registrationId )
        {
            return GetPayments( registrationId )
                .Select( p => p.Amount ).DefaultIfEmpty()
                .Sum();
        }

    }
}