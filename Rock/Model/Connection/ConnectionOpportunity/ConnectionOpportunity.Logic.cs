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
using System.Linq;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class ConnectionOpportunity
    {
        #region Properties

        /// <summary>
        /// Gets the URL of the Opportunity's photo.
        /// </summary>
        /// <value>
        /// URL of the photo
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual string PhotoUrl
        {
            get
            {
                return ConnectionOpportunity.GetPhotoUrl( this.PhotoId );
            }
            private set { }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the default connector person alias.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        private PersonAlias GetDefaultConnectorPersonAlias( int? campusId )
        {
            if ( ConnectionOpportunityCampuses == null )
            {
                return null;
            }

            if ( !campusId.HasValue && CampusCache.All().Count == 1 )
            {
                // Rock hides campus pickers if there is only one campus
                campusId = CampusCache.All().First().Id;
            }

            if ( campusId.HasValue )
            {
                var connectionOpportunityCampus = ConnectionOpportunityCampuses
                    .Where( c => c.CampusId == campusId.Value )
                    .FirstOrDefault();

                if ( connectionOpportunityCampus != null )
                {
                    return connectionOpportunityCampus.DefaultConnectorPersonAlias;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.ConnectionType != null ? this.ConnectionType : base.ParentAuthority;
            }
        }

        /// <summary>
        /// Gets the default connector person identifier.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        public int? GetDefaultConnectorPersonId( int? campusId )
        {
            var personAlias = GetDefaultConnectorPersonAlias( campusId );
            {
                if ( personAlias != null )
                {
                    return personAlias.PersonId;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the default connector.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <returns></returns>
        public int? GetDefaultConnectorPersonAliasId( int? campusId )
        {
            var personAlias = GetDefaultConnectorPersonAlias( campusId );
            {
                if ( personAlias != null )
                {
                    return personAlias.Id;
                }
            }

            return null;
        }

#if NET5_0_OR_GREATER

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
                string widthHeightParams = string.Empty;
                if ( maxWidth.HasValue )
                {
                    widthHeightParams += string.Format( "&maxwidth={0}", maxWidth.Value );
                }

                if ( maxHeight.HasValue )
                {
                    widthHeightParams += string.Format( "&maxheight={0}", maxHeight.Value );
                }

                virtualPath = string.Format( "/GetImage.ashx?id={0}" + widthHeightParams, photoId );
            }
            else
            {
                virtualPath = "/Assets/Images/no-picture.svg?";
            }

            return virtualPath;
        }
#endif


        #endregion
    }
}
