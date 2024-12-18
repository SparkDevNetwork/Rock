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

using Rock.Data;

namespace Rock.Model
{
    public partial class Registration
    {
        /// <summary>
        /// Save hook implementation for <see cref="Registration"/>.
        /// </summary>
        internal class SaveHook : EntitySaveHook<Registration>
        {
            /// <summary>
            /// Method that will be called on an entity immediately before the item is saved by context.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Added )
                {
                    if ( Entity.RegistrationInstanceId != 0 )
                    {
                        Entity.RegistrationTemplateId = new RegistrationInstanceService( RockContext ).GetSelect( Entity.RegistrationInstanceId, am => am.RegistrationTemplateId );
                    }
                }

                base.PreSave();
            }
        }
    }
}
