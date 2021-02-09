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

namespace Rock.Store
{
    /// <summary>
    /// Base model class for the store 
    /// </summary>
    public class StoreModel : DotLiquid.ILiquidizable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreModel"/> class.
        /// </summary>
        public StoreModel() { }

        /// <summary>
        /// Creates a DotLiquid compatible dictionary that represents the current entity object. 
        /// </summary>
        /// <returns>DotLiquid compatible dictionary.</returns>
        public object ToLiquid()
        {
            return this.ToLiquid( false );
        }
        
        /// <summary>
        /// Creates a DotLiquid compatible dictionary that represents the current store model.
        /// </summary>
        /// <param name="debug">if set to <c>true</c> the entire object tree will be parsed immediately.</param>
        /// <returns>
        /// DotLiquid compatible dictionary.
        /// </returns>
        public virtual object ToLiquid( bool debug )
        {
            var dictionary = new Dictionary<string, object>();

            Type entityType = this.GetType();

            foreach ( var propInfo in entityType.GetProperties() )
            {
                
                object propValue = propInfo.GetValue( this, null );

                if ( propValue is Guid )
                {
                    propValue = ((Guid)propValue).ToString();
                }

                if ( debug && propValue is DotLiquid.ILiquidizable )
                {
                    dictionary.Add( propInfo.Name, ((DotLiquid.ILiquidizable)propValue).ToLiquid() );
                }
                else
                {
                    dictionary.Add( propInfo.Name, propValue );
                }
                
            }

            return dictionary;
        }
    }
}
