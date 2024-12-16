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

using System.ComponentModel.DataAnnotations.Schema;

using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class LearningParticipant
    {
        /// <summary>
        /// Backing field for <see cref="TypeId"/>.
        /// </summary>
        private int? _typeId;

        /// <summary>
        /// Backing field for <see cref="TypeName"/>.
        /// </summary>
        private string _typeName = null;

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        [NotMapped]
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.LearningClass != null ? this.LearningClass : base.ParentAuthority;
            }
        }

        /// <inheritdoc/>
        public override bool IsAuthorized( string action, Rock.Model.Person person )
        {
            // Defer to the parent authority.
            // We don't add any logic to the authorization process
            // that's not already included in that logic.
            return ParentAuthority.IsAuthorized( action, person );
        }

        /// <inheritdoc/>
        [LavaVisible]
        public override int TypeId
        {
            get
            {
                if ( _typeId == null )
                {
                    // Once this instance is created, there is no need to set the _typeId more than once.
                    // Also, read should never return null since it will create entity type if it doesn't exist.
                    _typeId = EntityTypeCache.GetId<LearningParticipant>();
                }

                return _typeId.Value;

            }
        }

        /// <inheritdoc/>
        [NotMapped]
        [LavaVisible]
        public override string TypeName
        {
            get
            {
                if ( _typeName.IsNullOrWhiteSpace() )
                {
                    // Once this instance is created, there is no need to set the _typeName more than once.
                    // Also, read should never return null since it will create entity type if it doesn't exist.
                    _typeName = typeof( LearningParticipant ).FullName;
                }

                return _typeName;
            }
        }
    }
}
