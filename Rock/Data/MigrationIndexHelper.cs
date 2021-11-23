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

using System.Collections.Generic;
using System.Linq;

namespace Rock.Data
{
    /// <summary>
    /// Migration Index Helper
    /// </summary>
    public static class MigrationIndexHelper
    {
        /// <summary>
        /// Generates the name of the index based on the ordered keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public static string GenerateIndexName( IEnumerable<string> keys )
        {
            return $"IX_{keys.JoinStrings( "_" )}";
        }

        /// <summary>
        /// Creates the index if it doesn't exist. The index name is calculated from the keys. Uses a default fill factor of 90%.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="includes">The includes.</param>
        public static string GenerateCreateIndexIfNotExistsSql( string tableName, IEnumerable<string> keys, IEnumerable<string> includes )
        {
            var indexName = GenerateIndexName( keys );
            return GenerateCreateIndexIfNotExistsSql( tableName, indexName, keys, includes );
        }

        /// <summary>
        /// Creates the index if it doesn't exist. Uses a default fill factor of 90%.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="includes">The includes.</param>
        public static string GenerateCreateIndexIfNotExistsSql( string tableName, string indexName, IEnumerable<string> keys, IEnumerable<string> includes )
        {
            return GenerateCreateIndexIfNotExistsSql( tableName, indexName, keys, includes, 90 );
        }

        /// <summary>
        /// Creates the index if it doesn't exist.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="indexName">Name of the index.</param>
        /// <param name="keys">The keys.</param>
        /// <param name="includes">The includes.</param>
        /// <param name="fillFactor">The percent fullness of the leaf-level pages of the index. Lower values will result in more physical space used. Higher values may result in frequent page allocations and fragmentation. Use 0 or 100+ to turn off.</param>
        public static string GenerateCreateIndexIfNotExistsSql( string tableName, string indexName, IEnumerable<string> keys, IEnumerable<string> includes, byte fillFactor )
        {
            // 23-Nov-2021 DMV: Fill factor 0 and 100 are essentially the same thing.
            var isFillFactor0 = ( fillFactor >= 100 || fillFactor == 0 );
            return $@"

            IF NOT EXISTS( SELECT * FROM sys.indexes WHERE NAME = '{indexName}' AND object_id = OBJECT_ID( '{tableName}' ) )
            BEGIN
                CREATE INDEX [{indexName}]
                ON [{tableName}] ( {keys.JoinStrings( "," )} )
                { ( includes.Any() ? $"INCLUDE ( {includes.JoinStrings( "," )} )" : "" ) } WITH (FILLFACTOR={ (isFillFactor0 ? 0 : fillFactor) }, PAD_INDEX={ (isFillFactor0 ? "OFF" : "ON") });
            END";
        }

        /// <summary>
        /// Drops the index if it exists.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="indexName">Name of the index.</param>
        public static string GenerateDropIndexIfExistsSql( string tableName, string indexName )
        {
            return $@"

            IF EXISTS( SELECT * FROM sys.indexes WHERE NAME = '{indexName}' AND object_id = OBJECT_ID( '{tableName}' ) )
            BEGIN
                DROP INDEX [{indexName}]
                ON [{tableName}];
            END";
        }
    }
}
