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

namespace Rock.Model
{
    public partial class PersonViewed
    {
        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} viewed ",
                ( ( ViewerPersonAlias != null && ViewerPersonAlias.Person != null ) ? ViewerPersonAlias.Person.ToStringSafe() : ViewerPersonAliasId.ToString() ),
                ( ( TargetPersonAlias != null && TargetPersonAlias.Person != null ) ? TargetPersonAlias.Person.ToStringSafe() : TargetPersonAliasId.ToString() ) );
        }

        #endregion
    }
}
