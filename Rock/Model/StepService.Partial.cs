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
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.Step"/> entity objects.
    /// </summary>
    public partial class StepService
    {
        /// <summary>
        /// Determines whether this instance can add the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>
        ///   <c>true</c> if this instance can add the specified item; otherwise, <c>false</c>.
        /// </returns>
        public bool CanAdd( Step item, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( item == null )
            {
                errorMessage = "A null step cannot be added";
                return false;
            }

            if ( !item.IsValid )
            {
                errorMessage = item.ValidationResults.FirstOrDefault()?.ErrorMessage;
                return false;
            }

            var stepTypeService = new StepTypeService( Context as RockContext );
            var stepType = stepTypeService.Queryable( "StepTypePrerequisites.PrerequisiteStepType" ).AsNoTracking().FirstOrDefault( st => st.Id == item.StepTypeId );

            if ( stepType == null )
            {
                errorMessage = "The step type identifier is invalid and the step type could not be found";
                return false;
            }

            // If the step type doesn't allow multiple then the person cannot have two records of the same step type
            if ( !stepType.AllowMultiple )
            {
                var stepAlreadyExists = Queryable().AsNoTracking().Any( s =>
                    s.StepTypeId == stepType.Id &&
                    s.PersonAlias.Person.Aliases.Any( a => a.Id == item.PersonAliasId ) );

                if ( stepAlreadyExists )
                {
                    errorMessage = "A person cannot be added multiple times to this step type";
                    return false;
                }
            }

            // Make sure the person has completed all of the prerequisites (has a step record of that type that is a completed status) for the
            // step type before allowing a new step record
            var prerequisiteStepTypeIds = stepType.StepTypePrerequisites.Select( stp => stp.PrerequisiteStepTypeId );
            var completedStepTypeIds = Queryable().AsNoTracking()
                .Where( s =>
                    s.PersonAlias.Person.Aliases.Any( a => a.Id == item.PersonAliasId ) &&
                    s.StepStatus != null &&
                    s.StepStatus.IsCompleteStatus )
                .Select( s => s.StepTypeId );
            var hasUnmetPrereqs = prerequisiteStepTypeIds.Any( id => !completedStepTypeIds.Contains( id ) );

            if ( hasUnmetPrereqs )
            {
                errorMessage = "All of the prerequisite steps have not yet been completed";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Adds the specified item
        /// </summary>
        /// <param name="item">The item.</param>
        public override void Add( Step item )
        {
            var canAdd = CanAdd( item, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                throw new ArgumentException( errorMessage );
            }

            if ( !canAdd )
            {
                throw new ArgumentException( "The step cannot be added for an unspecified reason" );
            }

            base.Add( item );
        }
    }
}
