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
using System.Linq;
using Rock.Data;

namespace Rock.Obsidian.ViewModel
{
    /// <summary>
    /// View Arguments to edit a model
    /// </summary>
    public abstract class ViewArgs<T> where T : Entity<T>, new()
    {
        /// <summary>
        /// Applies the view arguments.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public virtual void ApplyViewArgs( T entity )
        {
            this.CopyPropertiesTo( entity );
        }
    }

    /// <summary>
    /// View Model Extensions
    /// </summary>
    public static class ViewModelExtensions
    {
        /// <summary>
        /// Converts to viewmodel.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public static T ToViewModel<T>( this IEntity entity )
        {
            var viewModel = Activator.CreateInstance<T>();
            entity.CopyPropertiesTo( viewModel );
            return viewModel;
        }

        /// <summary>
        /// Applies the view arguments.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="args">The arguments.</param>
        public static void ApplyViewArgs<T>( this T entity, ViewArgs<T> args ) where T : Entity<T>, new()
        {
            args.ApplyViewArgs( entity );
        }

        /// <summary>
        /// Copies the properties to.
        /// https://stackoverflow.com/a/28814556/13215483
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        public static void CopyPropertiesTo( this object source, object dest )
        {
            var sourceProps = source.GetType().GetProperties().Where( x => x.CanRead ).ToList();
            var destProps = dest.GetType().GetProperties().Where( x => x.CanWrite ).ToList();

            foreach ( var sourceProp in sourceProps )
            {
                if ( destProps.Any( x => x.Name == sourceProp.Name ) )
                {
                    var p = destProps.First( x => x.Name == sourceProp.Name );
                    if ( p.CanWrite )
                    {
                        // check if the property can be set or no.
                        p.SetValue( dest, sourceProp.GetValue( source, null ), null );
                    }
                }
            }
        }
    }
}
