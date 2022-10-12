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
using System.Text;
using System.Threading.Tasks;

using Rock.Data;

namespace Rock.Model
{
    public partial class FinancialAccount
    {
        private int? _originalParentAccountId;

        /// <summary>
        /// Save hook implementation for <see cref="FinancialAccount"/>
        /// </summary>
        internal class SaveHook: EntitySaveHook<FinancialAccount>
        {
            /// <inheritdoc cref="IEntitySaveHook.PreSave(IEntitySaveEntry)" />
            protected override void PreSave()
            {
                if ( State == EntityContextState.Modified || State == EntityContextState.Deleted )
                {
                    Entity._originalParentAccountId = Entry.OriginalValues[nameof( FinancialAccount.ParentAccountId )]?.ToString().AsIntegerOrNull();
                }

                base.PreSave();
            }
        }
    }
}
