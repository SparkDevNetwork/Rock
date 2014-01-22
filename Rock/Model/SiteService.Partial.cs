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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.Site"/> entity. This inherits from the Service class
    /// </summary>
    public partial class SiteService 
    {
        /// <summary>
        /// Returns a collection of <see cref="Rock.Model.Site"/> entities that by their Default <see cref="Rock.Model.Page">Page's</see> PageId.
        /// </summary>
        /// <param name="defaultPageId">An <see cref="System.Int32"/> containing the Id of the default <see cref="Rock.Model.Page"/> to search by. This
        /// value is nullable.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Site"/> entities that use reference the provided PageId.</returns>
        public IEnumerable<Site> GetByDefaultPageId( int? defaultPageId )
        {
            return Repository.Find( t => ( t.DefaultPageId == defaultPageId || ( defaultPageId == null && t.DefaultPageId == null ) ) );
        }
    }
}
