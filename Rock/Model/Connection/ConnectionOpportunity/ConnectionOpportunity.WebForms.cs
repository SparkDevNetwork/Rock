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
using System.Web;
using Rock.Utility;

namespace Rock.Model
{
    public partial class ConnectionOpportunity
    {
        #region Methods

        /// <summary>
        /// Gets the photo URL.
        /// </summary>
        /// <param name="photoId">The photo identifier.</param>
        /// <param name="maxWidth">The maximum width.</param>
        /// <param name="maxHeight">The maximum height.</param>
        /// <returns></returns>
        public static string GetPhotoUrl( int? photoId, int? maxWidth = null, int? maxHeight = null )
        {
            string virtualPath = string.Empty;
            if ( photoId.HasValue )
            {
                virtualPath = FileUrlHelper.GetImageUrl( photoId.Value, new GetImageUrlOptions { MaxWidth = maxWidth, MaxHeight = maxHeight } );
            }
            else
            {
                virtualPath = "~/Assets/Images/no-picture.svg?";
            }

            if ( System.Web.HttpContext.Current == null )
            {
                return virtualPath;
            }
            else
            {
                return VirtualPathUtility.ToAbsolute( virtualPath );
            }

        }

        #endregion
    }
}
