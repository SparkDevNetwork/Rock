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
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Lms
{
    /// <summary>
    /// MEF Container class for <see cref="LearningActivityComponent">LearningActivityComponents</see>.
    /// </summary>
    public class LearningActivityContainer : Container<LearningActivityComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<LearningActivityContainer> instance =
            new Lazy<LearningActivityContainer>( () => new LearningActivityContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static LearningActivityContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();

            // Check for other uses of this elsewhere...
            // Create any attributes that need to be created
            //int badgeEntityTypeId = EntityTypeCache.Get( typeof( Model.Badge ) ).Id;
            //using ( var rockContext = new RockContext() )
            //{
            //    foreach ( var badge in this.Components )
            //    {
            //        Type badgeType = badge.Value.Value.GetType();
            //        int badgeComponentEntityTypeId = EntityTypeCache.Get( badgeType ).Id;
            //        Rock.Attribute.Helper.UpdateAttributes( badgeType, badgeEntityTypeId, "BadgeComponentEntityTypeId", badgeComponentEntityTypeId.ToString(), rockContext );
            //    }
            //}
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static LearningActivityComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( LearningActivityComponent ) )]
        protected override IEnumerable<Lazy<LearningActivityComponent, IComponentData>> MEFComponents { get; set; }
    }
}
