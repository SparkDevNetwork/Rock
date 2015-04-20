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
using System;
using System.Linq;
using System.Collections.Generic;

using Rock.Data;
using Rock.Model;

namespace Rock.Transactions
{
    /// <summary>
    /// Deletes a binary file
    /// </summary>
    public class DeleteAttributeBinaryFile : ITransaction
    {

        /// <summary>
        /// Gets or sets the binary file unique identifier.
        /// </summary>
        /// <value>
        /// The binary file unique identifier.
        /// </value>
        public Guid BinaryFileGuid { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteAttributeBinaryFile"/> class.
        /// </summary>
        /// <param name="binaryFileGuid">The binary file unique identifier.</param>
        public DeleteAttributeBinaryFile( Guid binaryFileGuid )
        {
            BinaryFileGuid = binaryFileGuid;
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                var binaryFile = binaryFileService.Get( BinaryFileGuid );
                if ( binaryFile != null )
                {
                    string guidAsString = BinaryFileGuid.ToString();

                    // If any attribute still has this file as a default value, don't delete it
                    if ( new AttributeService( rockContext ).Queryable().Any( a => a.DefaultValue == guidAsString ) )
                    {
                        return;
                    }

                    // If any attribute value still has this file as a value, don't delete it
                    if ( new AttributeValueService( rockContext ).Queryable().Any( a => a.Value == guidAsString ) )
                    {
                        return;
                    }

                    binaryFileService.Delete( binaryFile );

                    rockContext.SaveChanges();
                }
            }
        }
    }
}