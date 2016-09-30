// <copyright>
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
using System.Collections.Generic;
using System.Linq;

using Rock.Attribute;
using Rock.Extension;
using Rock.UniversalSearch.IndexModels;

namespace Rock.UniversalSearch
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class IndexComponent : Component
    {
        public abstract bool IsConnected { get; }

        public abstract string IndexLocation { get; }

        public abstract void IndexDocument<T>( T document, string indexName = null, string mappingType = null ) where T : class, new();

        public abstract void DeleteDocumentsByType<T>( string indexName = null ) where T : class, new();

        public abstract void CreateIndex( Type documentType, bool deleteIfExists = true );

        public abstract void DeleteIndex( Type documentType );

        public abstract void DeleteDocument<T>( T document, string indexName = null ) where T : class, new();

        public abstract void DeleteDocumentById( Type documentType, int id );

        public abstract void DeleteDocumentByProperty( Type documentType, string propertyName, object propertyValue );

        public abstract IEnumerable<SearchResultModel> Search( string query, SearchType searchType = SearchType.ExactMatch, List<int> entities = null );
    }

    public enum SearchType
    {
        ExactMatch = 0,
        Fuzzy = 1
    }
}