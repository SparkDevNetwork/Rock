// <copyright>
// Copyright by BEMA Information Technologies
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
using Rock.Data;

namespace com.bemaservices.HrManagement.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class PtoBracketService : Service<PtoBracket>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoBracketService"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public PtoBracketService( RockContext context ) : base( context ) { }

    }

    public static partial class PtoBracketExtensionMethods
    {
        /// <summary>
        /// Clones this PtoBracketType object to a new PtoBracket object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns></returns>
        public static PtoBracket Clone( this PtoBracket source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as PtoBracket;
            }
            else
            {
                var target = new PtoBracket();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another PtoBracket object to this PtoBracketType object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this PtoBracket target, PtoBracket source )
        {
            target.Id = source.Id;
            target.PtoTierId = source.PtoTierId;
            target.IsActive = source.IsActive;
            target.MinimumYear = source.MinimumYear;
            target.MaximumYear = source.MaximumYear;
            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;

        }
    }
}
