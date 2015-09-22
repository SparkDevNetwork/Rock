// <copyright>
// Copyright 2013 by the Spark Development Network
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
using System.Data.Entity;
using System.Net;
using System.Web.Http;
using System.Linq;

using Rock.Model;
using Rock.Rest.Filters;
using Rock.Security;
using Rock.Data;
using System.Collections.Generic;
using System.Data;
using System;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PhoneNumbersController
    {
        /// <summary>
        /// Puts the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="phoneNumber">The phone number.</param>
        public override void Put( int id, PhoneNumber phoneNumber )
        {
            SetProxyCreation( true );

            var rockContext = (RockContext)Service.Context;
            var existingPhone = Service.Get( id );
            if ( existingPhone != null )
            {
                var changes = new List<string>();

                History.EvaluateChange( changes, "Phone Type", DefinedValueCache.GetName( existingPhone.NumberTypeValueId ), DefinedValueCache.GetName( phoneNumber.NumberTypeValueId ) );

                string oldPhoneNumber = existingPhone.NumberFormattedWithCountryCode;
                string newPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                History.EvaluateChange(
                    changes,
                    string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumber.NumberTypeValueId ) ),
                    oldPhoneNumber,
                    newPhoneNumber );

                if ( changes.Any() )
                {
                    System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                        phoneNumber.PersonId,
                        changes );
                }
            }

            base.Put( id, phoneNumber );
        }

        /// <summary>
        /// Posts the specified phone number.
        /// </summary>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        public override System.Net.Http.HttpResponseMessage Post( PhoneNumber phoneNumber )
        {
            var changes = new List<string>();
            string newPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
            History.EvaluateChange(
                changes,
                string.Format( "{0} Phone", DefinedValueCache.GetName( phoneNumber.NumberTypeValueId ) ),
                string.Empty,
                newPhoneNumber );

            var rockContext = (RockContext)Service.Context;

            System.Web.HttpContext.Current.Items.Add( "CurrentPerson", GetPerson() );
            HistoryService.SaveChanges(
                rockContext,
                typeof( Person ),
                Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                phoneNumber.PersonId,
                changes );            
            
            return base.Post( phoneNumber );
        }
    }
}
