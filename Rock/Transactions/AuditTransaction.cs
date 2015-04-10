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
using System.Collections.Generic;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Writes entity audits 
    /// </summary>
    public class AuditTransaction : ITransaction
    {
        /// <summary>
        /// Gets or sets the audits.
        /// </summary>
        /// <value>
        /// The audits.
        /// </value>
        public List<Audit> Audits { get; set; }

        /// <summary>
        /// Execute method to write transaction to the database.
        /// </summary>
        public void Execute()
        {
            if ( Audits != null && Audits.Count > 0 )
            {
                var auditsToAdd = Audits.Where( a => a.Details.Any() );
                if ( auditsToAdd.Any() )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var auditService = new AuditService( rockContext );

                        auditService.AddRange( auditsToAdd );

                        rockContext.SaveChanges();
                    }
                }
            }
        }
    }
}