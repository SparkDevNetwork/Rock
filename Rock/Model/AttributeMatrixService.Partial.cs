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

using Rock.Data;
using Rock.Field.Types;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public partial class AttributeMatrixService : Service<AttributeMatrix>
    {
        /// <summary>
        /// Gets the orphaned attribute matrices.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<AttributeMatrix> GetOrphanedAttributeMatrices()
        {
            string sql = @"
                DECLARE @MatrixFieldTypeId INT = (SELECT [Id] FROM [dbo].[FieldType] WHERE [Guid] = 'F16FC460-DC1E-4821-9012-5F21F974C677')

                SELECT am.*
                    FROM [dbo].[AttributeMatrix] AS am
                    WHERE (am.[CreatedDateTime] < GETDATE() -1 ) 
	                AND ( NOT (CONVERT(nvarchar(36), am.[Guid]) IN (
		                SELECT av.[Value] AS [Value]
		                FROM  [dbo].[AttributeValue] AS av
		                INNER JOIN [dbo].[Attribute] AS a ON av.[AttributeId] = a.[Id]
		                WHERE a.[FieldTypeId] = @MatrixFieldTypeId
		                AND av.[Value] IS NOT NULL
		                AND av.[Value] <> ''
	                )))";

            return this.ExecuteQuery( sql );
        }
    }
}
