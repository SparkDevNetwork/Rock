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

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.ContentChannel"/> objects.
    /// </summary>
    public partial class ContentChannelService
    {
        /// <summary>
        /// Gets the child content channel types.
        /// </summary>
        /// <param name="contentChannelId">The content channel type identifier.</param>
        /// <returns></returns>
        public IQueryable<ContentChannel> GetChildContentChannels( int contentChannelId )
        {
            return Queryable().Where( t => t.ParentContentChannels.Select( p => p.Id ).Contains( contentChannelId ) );
        }

        /// <summary>
        /// Gets the child content channel types.
        /// </summary>
        /// <param name="contentChannelGuid">The content channel type unique identifier.</param>
        /// <returns></returns>
        public IQueryable<ContentChannel> GetChildContentChannels( Guid contentChannelGuid )
        {
            return Queryable().Where( t => t.ParentContentChannels.Select( p => p.Guid ).Contains( contentChannelGuid ) );
        }

        /// <summary>
        /// Gets the parent content channel types.
        /// </summary>
        /// <param name="contentChannelId">The content channel type identifier.</param>
        /// <returns></returns>
        public IQueryable<ContentChannel> GetParentContentChannels( int contentChannelId )
        {
            return Queryable().Where( t => t.ChildContentChannels.Select( p => p.Id ).Contains( contentChannelId ) );
        }

        /// <summary>
        /// Determines whether the specified content channel is manually sorted
        /// </summary>
        /// <param name="contentChannelId">The content channel ID.</param>
        /// <returns>
        ///   <c>true</c> if manually sorted otherwise, <c>false</c>.
        /// </returns>
        public bool IsManuallySorted( int contentChannelId )
        {
            return Queryable()
                    .Where( c => c.Id == contentChannelId )
                    .Select( c => c.ItemsManuallyOrdered )
                    .FirstOrDefault();
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public override bool Delete( ContentChannel item )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            return base.Delete( item );
        }
    }
}
