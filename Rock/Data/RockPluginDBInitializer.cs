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
using System.Data.Entity;

namespace Rock.Data
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RockPluginDBInitializer<T> : IDatabaseInitializer<T> where T : DbContext
    {
        /// <summary>
        /// Initializes the database.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void InitializeDatabase( T context )
        {
            throw new NotImplementedException();
        }
    }
}