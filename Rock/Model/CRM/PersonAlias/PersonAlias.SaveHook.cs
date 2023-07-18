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
    /*
         3/20/2023 - DSH

         The following areas of Rock bypass the pre and post save actions
         for PersonAlias. Be aware that any code you put in here might not
         execute under these situations:

            - The Stale Anonymous Visitor stage of the RockCleanup job.

         Reason: Performance
    */

    public partial class PersonAlias
    {
        /// <summary>
        /// Save hook implementation for <see cref="PersonAlias"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<PersonAlias>
        {
            protected override void PreSave()
            {
                base.PreSave();

                if ( Entry.State == EntityContextState.Added )
                {
                    Entity.AliasedDateTime = RockDateTime.Now;
                }
            }
        }
    }
}
