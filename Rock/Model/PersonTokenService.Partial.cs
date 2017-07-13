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
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Linq;

using Rock;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data Access/Service class for <see cref="Rock.Model.PersonToken"/> entity objects.
    /// </summary>
    public partial class PersonTokenService
    {
        /// <summary>
        /// Gets the PersonToken by impersonation token (rckipid)
        /// </summary>
        /// <param name="impersonationToken">The impersonation token.</param>
        /// <returns></returns>
        public PersonToken GetByImpersonationToken( string impersonationToken )
        {
            var decryptedToken = Rock.Security.Encryption.DecryptString( impersonationToken );
            return this.Queryable().FirstOrDefault( a => a.Token == decryptedToken );
        }
    }
}
