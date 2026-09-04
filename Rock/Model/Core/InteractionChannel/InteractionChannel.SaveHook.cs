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
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class InteractionChannel
    {
        /// <summary>
        /// Save hook implementation for <see cref="InteractionChannel"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<InteractionChannel>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                /*
                    6/15/26 - NA

                    On Add only, copy the medium's "Default Component Daily Counts"
                    attribute value onto the new channel's EnableComponentDailyCounts
                    flag. The hook intentionally does NOT fire on Modified or Deleted:
                    once a channel exists, the operator owns the flag's value and
                    toggling the medium's default must not retroactively change it.

                    Reason: Spec "InteractionChannel pre-save hook (Add only)".
                */
                if ( State == EntityContextState.Added && Entity.ChannelTypeMediumValueId.HasValue )
                {
                    var medium = DefinedValueCache.Get( Entity.ChannelTypeMediumValueId.Value );
                    if ( medium != null )
                    {
                        var defaultEnabled = medium
                            .GetAttributeValue( "DefaultComponentDailyCounts" )
                            .AsBoolean();

                        if ( defaultEnabled )
                        {
                            Entity.EnableComponentDailyCounts = true;
                        }
                    }
                }

                base.PreSave();
            }
        }
    }
}
