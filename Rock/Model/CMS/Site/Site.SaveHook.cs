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

using System.Threading.Tasks;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Site
    {
        /// <summary>
        /// Save hook implementation for <see cref="Site"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Site>
        {
            /// <summary>
            /// Called after the save operation has been executed.
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                base.PostSave();

                if ( Entity.SiteType == SiteType.Mobile )
                {
                    var transactionCompletedTask = DbContext.WrappedTransactionCompletedTask;

                    Task.Run( async () =>
                    {
                        // Wait for save task to complete, then flush the cache.
                        if ( await transactionCompletedTask )
                        {
                            DeepLinkCache.Flush();
                        }
                    } );
                }
            }
        }
    }
}
