﻿// <copyright>
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
using Rock.Lava;

namespace Rock.Store
{
    /// <summary>
    /// Base model class for the store 
    /// </summary>
    public class StoreModel : ILavaDataDictionarySource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoreModel"/> class.
        /// </summary>
        public StoreModel() { }

        ILavaDataDictionary ILavaDataDictionarySource.GetLavaDataDictionary()
        {
            var dictionary = this.ToLiquid( false ) as Dictionary<string, object>;

            return new LavaDataDictionary( dictionary );
        }

        /// <summary>
        /// Creates a DotLiquid compatible dictionary that represents the current entity object. 
        /// </summary>
        /// <returns>DotLiquid compatible dictionary.</returns>
        [Obsolete( "DotLiquid is not supported and will be fully removed in the future." )]
        [Rock.RockObsolete( "18.0" )]
        public object ToLiquid()
        {
            return this.ToLiquid( false );
        }

        /// <summary>
        /// Creates a Lava compatible dictionary that represents the current store model.
        /// </summary>
        /// <param name="debug">if set to <c>true</c> the entire object tree will be parsed immediately.</param>
        /// <returns>
        /// A Lava compatible dictionary.
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
                    propValue = ( (Guid)propValue ).ToString();
                }

                if ( debug && propValue is ILavaDataDictionarySource )
                {
                    dictionary.Add( propInfo.Name, ( (ILavaDataDictionarySource)propValue ).GetLavaDataDictionary() );
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
