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
using System.Data.Entity;
using System.Linq;

using Rock.Data;
using Rock.Model;
using System;
using System.Web.UI.WebControls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of step types and allow a single selection.
    /// </summary>
    public class StepTypeFieldType : EntitySingleSelectionListFieldTypeBase<Rock.Model.StepType>
    {
        /// <summary>
        /// Returns a user-friendly description of the entity.
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        protected override string OnFormatValue( Guid entityGuid )
        {
            var entity = GetEntity( entityGuid.ToString() ) as StepType;
            return entity.Name;
        }

        /// <summary>
        /// Returns a dictionary of the items available for selection.
        /// </summary>
        /// <returns></returns>
        protected override Dictionary<Guid, string> OnGetItemList()
        {
            var service = new StepTypeService( new RockContext() );

            var items = service
                .Queryable()
                .AsNoTracking()
                .OrderBy( o => o.StepProgram.Order )
                .ThenBy( o => o.StepProgram.Name )
                .ThenBy( o => o.Order )
                .ThenBy( o => o.Name )
                .Select( o => new
                {
                    o.Guid,
                    Name = o.StepProgram.Name + " - " + o.Name,
                } )
                .ToDictionary( o => o.Guid, o => o.Name );

            return items;
        }
    }
}
