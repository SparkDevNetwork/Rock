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
        /// Returns an enumerable collection of <see cref="Rock.Model.ContentChannel">ContentChannel</see> that are descendants of a specified content channel type.
        /// WARNING: This will fail (max recursion) if there is a circular reference in the ContentChannelAssociation table.
        /// </summary>
        /// <param name="parentContentChannelId">The parent content channel type identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.ContentChannel">ContentChannel</see>.
        /// </returns>
        public IEnumerable<ContentChannel> GetAllAssociatedDescendents( int parentContentChannelId )
        {
            return this.ExecuteQuery(
                @"
                WITH CTE AS (
		            SELECT [ContentChannelId],[ChildContentChannelId] FROM [ContentChannelAssociation] WHERE [ContentChannelId] = {0}
		            UNION ALL
		            SELECT [a].[ContentChannelId],[a].[ChildContentChannelId] FROM [ContentChannelAssociation] [a]
		            JOIN CTE acte ON acte.[ChildContentChannelId] = [a].[ContentChannelId]
                    WHERE acte.[ChildContentChannelId] <> acte.[ContentChannelId]
                 )
                SELECT *
                FROM [ContentChannel]
                WHERE [Id] IN ( SELECT [ChildContentChannelId] FROM CTE )
                ", parentContentChannelId );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.ContentChannelPath">ContentChannelPath</see> objects that are
        /// associated descendants of a specified content channel type.
        /// WARNING: This will fail if there is a circular reference in the ContentChannelAssociation table.
        /// </summary>
        /// <param name="parentContentChannelId">The parent content channel type identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.ContentChannelPath">ContentChannelPath</see> objects.
        /// </returns>
        public IEnumerable<ContentChannelPath> GetAllAssociatedDescendentsPath( int parentContentChannelId )
        {
            return this.Context.Database.SqlQuery<ContentChannelPath>(
                @"
                -- Get ContentChannel association hierarchy with ContentChannel ancestor path information
                WITH CTE (ChildContentChannelId,ContentChannelId, HierarchyPath) AS
                (
                      SELECT [ChildContentChannelId], [ContentChannelId], CONVERT(nvarchar(500),'')
                      FROM   [ContentChannelAssociation] GTA
		                INNER JOIN [ContentChannel] GT ON GT.[Id] = GTA.[ContentChannelId]
                      WHERE  [ContentChannelId] = {0}
                      UNION ALL 
                      SELECT
                            GTA.[ChildContentChannelId], GTA.[ContentChannelId], CONVERT(nvarchar(500), CTE.HierarchyPath + ' > ' + GT2.Name)
                      FROM
                            ContentChannelAssociation GTA
		                INNER JOIN CTE ON CTE.[ChildContentChannelId] = GTA.[ContentChannelId]
		                INNER JOIN [ContentChannel] GT2 ON GT2.[Id] = GTA.[ContentChannelId]
                      WHERE CTE.[ChildContentChannelId] <> CTE.[ContentChannelId]
                )
                SELECT GT3.Id as 'ContentChannelId', SUBSTRING( CONVERT(nvarchar(500), CTE.HierarchyPath + ' > ' + GT3.Name), 4, 500) AS 'Path'
                FROM CTE
                INNER JOIN [ContentChannel] GT3 ON GT3.[Id] = CTE.[ChildContentChannelId]
                ", parentContentChannelId );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.ContentChannel">ContentChannel</see> that are descendants of a specified content channel type.
        /// WARNING: This will fail if their is a circular reference in the ContentChannelAssociation table.
        /// </summary>
        /// <param name="parentContentChannelGuid">The parent content channel type unique identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.ContentChannel">ContentChannel</see>.
        /// </returns>
        public IEnumerable<ContentChannel> GetAllAssociatedDescendents( Guid parentContentChannelGuid )
        {
            return this.GetAllAssociatedDescendents( this.Get( parentContentChannelGuid ).Id );
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

    /// <summary>
    /// Represents a ContentChannelPath object in Rock.
    /// </summary>
    public class ContentChannelPath
    {
        /// <summary>
        /// Gets or sets the ID of the ContentChannel.
        /// </summary>
        /// <value>
        /// ID of the ContentChannel.
        /// </value>
        public int ContentChannelId { get; set; }

        /// <summary>
        /// Gets or sets the full associated ancestor path (of content channel type associations). 
        /// </summary>
        /// <value>
        /// Full path of the ancestor content channel type associations. 
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Returns the Path of the ContentChannelPath
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Path;
        }
    }
}
