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
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of achievement types and allow a single selection.
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class AchievementTypeFieldType : EntitySingleSelectionListFieldTypeBase<AchievementType>
    {
        /// <summary>
        /// Returns a user-friendly description of the entity.
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        protected override string OnFormatValue( Guid entityGuid )
        {
            var entity = GetEntity( entityGuid.ToString() );
            return entity.Name;
        }

        /// <summary>
        /// Returns a dictionary of the items available for selection.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<Guid, string> OnGetItemList()
        {
            return AchievementTypeCache.All()
                .Where( s => s.IsActive )
                .OrderBy( s => s.Name )
                .Select( s => new
                {
                    s.Guid,
                    s.Name,
                } )
                .ToDictionary( s => s.Guid, s => s.Name );
        }
    }
}
