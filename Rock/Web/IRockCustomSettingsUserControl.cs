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
using Rock.Attribute;
using Rock.Data;

namespace Rock.Web
{

    /// <summary>
    /// Defines the methods that an ASCX based user control which provides custom
    /// UI for settings must implement.
    /// </summary>
    public interface IRockCustomSettingsUserControl
    {
        /// <summary>
        /// Update the custom UI to reflect the current settings found in the entity.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        void ReadSettingsFromEntity( IHasAttributes attributeEntity );

        /// <summary>
        /// Update the entity with values from the custom UI.
        /// </summary>
        /// <param name="attributeEntity">The attribute entity.</param>
        /// <param name="rockContext">The rock context to use when accessing the database.</param>
        /// <remarks>
        /// Do not save the entity, it will be automatically saved later. This call will be made inside
        /// a SQL transaction for the passed rockContext. If you need to make changes to the database
        /// do so on this context so they can be rolled back if something fails during the final save.
        /// </remarks>
        void WriteSettingsToEntity( IHasAttributes attributeEntity, RockContext rockContext );
    }
}
