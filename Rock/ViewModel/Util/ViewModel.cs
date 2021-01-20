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
using System.Reflection;
using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.ViewModel
{
    /// <summary>
    /// View Model
    /// </summary>
    public interface IViewModel
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        Dictionary<string, AttributeValueViewModel> Attributes { get; set; }

        /// <summary>
        /// Sets the properties from entity.
        /// </summary>
        /// <param name="entity">The entity, cache, or other object to copy properties from.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        void SetPropertiesFrom( object entity, bool loadAttributes = true );
    }

    /// <summary>
    /// View Model Base
    /// </summary>
    /// <seealso cref="Rock.ViewModel.IViewModel" />
    public abstract class ViewModelBase : IViewModel
    {
        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <value>
        /// The Id.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Guid.
        /// </summary>
        /// <value>
        /// The Guid.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [TypeScriptType( "Record<string, AttributeValue> | null", "import AttributeValue from './AttributeValueViewModel.js';" )]
        public Dictionary<string, AttributeValueViewModel> Attributes { get; set; }

        /// <summary>
        /// Sets the properties from entity.
        /// </summary>
        /// <param name="entity">The entity, cache item, or some object.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        public virtual void SetPropertiesFrom( object entity, bool loadAttributes = true )
        {
            if ( entity == null )
            {
                return;
            }

            entity.CopyPropertiesTo( this );

            if ( loadAttributes && entity is IHasAttributes hasAttributes )
            {
                if ( hasAttributes.Attributes == null )
                {
                    hasAttributes.LoadAttributes();
                }

                Attributes = hasAttributes.AttributeValues.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToViewModel<AttributeValueViewModel>() );
            }
        }
    }

    /// <summary>
    /// View Model Extensions
    /// </summary>
    public static class ViewModelExtensions
    {
        /// <summary>
        /// The view model types mapped by entity type
        /// </summary>
        private static Dictionary<Type, Type> _viewModelTypeMap = null;

        /// <summary>
        /// The view model types mapped by entity type
        /// </summary>
        private static Dictionary<Type, MethodInfo> _toViewModelMap = null;

        /// <summary>
        /// Gets the type of the view model for the entity type.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        private static Type GetViewModelType( Type entityType )
        {
            if ( _viewModelTypeMap == null )
            {
                _viewModelTypeMap = new Dictionary<Type, Type>();
                var viewModelTypes = Reflection.GetTypesWithAttribute<ViewModelOfAttribute>( true );

                foreach ( var viewModelType in viewModelTypes )
                {
                    var props = viewModelType.GetCustomAttributes( typeof( ViewModelOfAttribute ), true ) as ViewModelOfAttribute[];

                    foreach ( var prop in props )
                    {
                        _viewModelTypeMap[prop.RelatedEntityType] = viewModelType;
                    }
                }
            }

            if ( entityType.Namespace == "System.Data.Entity.DynamicProxies" )
            {
                entityType = entityType.BaseType;
            }

            if ( entityType.BaseType.IsGenericType && entityType.BaseType.GetGenericTypeDefinition() == typeof( ModelCache<,> ) )
            {
                entityType = entityType.BaseType.GenericTypeArguments[1];
            }

            return _viewModelTypeMap.GetValueOrNull( entityType );
        }

        /// <summary>
        /// Gets to view model method.
        /// </summary>
        /// <param name="viewModelType">Type of the view model.</param>
        /// <returns></returns>
        private static MethodInfo GetToViewModelMethod( Type viewModelType )
        {
            if ( _toViewModelMap == null )
            {
                _toViewModelMap = new Dictionary<Type, MethodInfo>();
            }

            var methodInfo = _toViewModelMap.GetValueOrNull( viewModelType );

            if ( methodInfo == null )
            {
                methodInfo = typeof( ViewModelExtensions )
                    .GetMethods( BindingFlags.Public | BindingFlags.Static )
                    .First( m => m.Name == nameof( ToViewModel ) && m.IsGenericMethodDefinition )
                    .MakeGenericMethod( viewModelType );
                _toViewModelMap[viewModelType] = methodInfo;
            }

            return methodInfo;
        }

        /// <summary>
        /// Converts to viewmodel.
        /// </summary>
        /// <param name="entity">The entity, cache, or other item.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public static IViewModel ToViewModel( this object entity, bool loadAttributes = true )
        {
            if ( entity == null )
            {
                return null;
            }

            var entityType = entity.GetType();
            var viewModelType = GetViewModelType( entityType );

            if ( viewModelType == null )
            {
                return null;
            }

            var methodInfo = GetToViewModelMethod( viewModelType );
            var viewModel = methodInfo.Invoke( null, new[] { entity, loadAttributes } ) as IViewModel;

            return viewModel;
        }

        /// <summary>
        /// Converts to viewmodel.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public static TViewModel ToViewModel<TViewModel>( this object entity, bool loadAttributes = true ) where TViewModel : IViewModel
        {
            if ( entity == null )
            {
                return default;
            }

            var viewModel = Activator.CreateInstance<TViewModel>();
            viewModel.SetPropertiesFrom( entity, loadAttributes );
            return viewModel;
        }

        /// <summary>
        /// Copies the properties to.
        /// https://stackoverflow.com/a/28814556/13215483
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="dest">The dest.</param>
        public static void CopyPropertiesTo( this object source, object dest )
        {
            if ( source == null || dest == null )
            {
                return;
            }

            var sourceProps = source.GetType().GetProperties().Where( x => x.CanRead ).ToList();
            var destProps = dest.GetType().GetProperties().Where( x => x.CanWrite ).ToList();

            foreach ( var sourceProp in sourceProps )
            {
                if ( destProps.Any( x => x.Name == sourceProp.Name ) )
                {
                    var destType = destProps.First( x => x.Name == sourceProp.Name );

                    if ( destType.CanWrite && destType.PropertyType.IsAssignableFrom( sourceProp.PropertyType ) )
                    {
                        // check if the property can be set or no.
                        destType.SetValue( dest, sourceProp.GetValue( source, null ), null );
                    }
                }
            }
        }
    }
}
