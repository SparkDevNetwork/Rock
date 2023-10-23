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

using Rock.SystemGuid;

namespace Rock.Model
{
    public partial class BlockType
    {
        #region Methods

        /// <summary>
        /// Gets if the block exists.
        /// </summary>
        /// <returns></returns>
        public bool IsBlockExists()
        {
            if ( Path.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var blockPath = System.Web.HttpContext.Current.Request.MapPath( Path );
            return System.IO.File.Exists( blockPath );
        }

        #endregion
    }
}
