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

using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Event.InteractiveExperiences;
using Rock.Web.Cache;

namespace Rock.Event.InteractiveExperiences
{
    /// <summary>
    /// Extension methods used by the Interactive Experiences framework.
    /// </summary>
    internal static class ExtensionMethods
    {
        public static ExperienceAnswerBag ToExperienceAnswerBag( this InteractiveExperienceAnswer answer )
        {
            var campusId = answer.CampusId;
            var campus = campusId.HasValue ? CampusCache.Get( campusId.Value ) : null;
            var personName = string.Empty;

            if ( answer.PersonAlias?.Person != null )
            {
                // Intentionally not using FullName since that isn't available
                // to pure SQL which is used when getting answers in bulk.
                personName = $"{answer.PersonAlias.Person.NickName} {answer.PersonAlias.Person.LastName}";
            }

            return new ExperienceAnswerBag
            {
                IdKey = answer.IdKey,
                ActionIdKey = IdHasher.Instance.GetHash( answer.InteractiveExperienceActionId ),
                CampusGuid = campus?.Guid,
                CampusName = campus?.Name,
                Response = answer.Response,
                Status = answer.ApprovalStatus,
                SubmitterName = personName
            };
        }
    }
}
