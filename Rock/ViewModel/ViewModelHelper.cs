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
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.ViewModel
{
    /// <summary>
    /// View Model Helper
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal]
    public static class ViewModelHelper
    {
        /// <summary>
        /// The cached default helpers for converting models into view models.
        /// </summary>
        internal static ConcurrentDictionary<Type, Type> _defaultHelperTypes = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public static IViewModel GetDefaultViewModel( object model, Person currentPerson = null, bool loadAttributes = true )
        {
            // Check if it is already a view model.
            if ( model is IViewModel viewModel )
            {
                return viewModel;
            }

            var type = model.GetType();

            if ( type.IsDynamicProxyType() )
            {
                type = type.BaseType;
            }

            var helperType = _defaultHelperTypes.GetOrAdd( type, GetDefaultViewModelHelper );

            if ( helperType == null )
            {
                return null;
            }

            var helper = ( IViewModelHelper ) Activator.CreateInstance( helperType );

            return helper.CreateViewModel( model, currentPerson, loadAttributes );
        }

        /// <summary>
        /// Gets the default view model helper.
        /// </summary>
        /// <param name="type">The model type to be converted into a view model.</param>
        /// <returns>The <see cref="IViewModelHelper"/> type that will handle the conversion.</returns>
        private static Type GetDefaultViewModelHelper( Type type )
        {
            var helperType = Reflection.FindTypes( typeof( IViewModelHelper ) )
                .Select( a => a.Value )
                .Where( a => a.GetCustomAttribute<DefaultViewModelHelperAttribute>()?.Type == type )
                .OrderBy( a => a.Namespace.StartsWith( "Rock." ) )
                .FirstOrDefault();

            if ( helperType != null )
            {
                return helperType;
            }

            var viewModelType = Reflection.FindTypes( typeof( IViewModel ) )
                .Select( a => a.Value )
                .Where( a => a.GetCustomAttribute<DefaultViewModelHelperAttribute>()?.Type == type )
                .OrderBy( a => a.Namespace.StartsWith( "Rock." ) )
                .FirstOrDefault();

            if ( viewModelType == null )
            {
                return null;
            }

            return typeof( ViewModelHelper<,> ).MakeGenericType( type, viewModelType );
        }
    }

    /// <summary>
    /// View Model Helper
    /// </summary>
    public class ViewModelHelper<TModel, TViewModel> : IViewModelHelper
        where TViewModel : IViewModel, new()
    {
        /// <inheritdoc/>
        IViewModel IViewModelHelper.CreateViewModel( object model, Person currentPerson, bool loadAttributes )
        {
            if ( model is TModel tModel )
            {
                return CreateViewModel( tModel, currentPerson, loadAttributes );
            }

            return null;
        }

        /// <summary>
        /// Converts the model to the view model.
        /// </summary>
        /// <param name="model">The entity.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public virtual TViewModel CreateViewModel( TModel model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model == null )
            {
                return default;
            }

            var viewModel = Activator.CreateInstance<TViewModel>();
            CopyProperties( model, viewModel );

            AddAttributesToViewModel( model, viewModel, currentPerson, loadAttributes );
            ApplyAdditionalPropertiesAndSecurityToViewModel( model, viewModel, currentPerson, loadAttributes );

            return viewModel;
        }

        /// <summary>
        /// Adds the attributes to view model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="viewModel">The view model.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        public virtual void AddAttributesToViewModel( TModel model, TViewModel viewModel, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( loadAttributes && model is IHasAttributes modelWithAttributes && viewModel is IViewModelWithAttributes viewModelWithAttributes )
            {
                if ( modelWithAttributes.Attributes == null )
                {
                    modelWithAttributes.LoadAttributes();
                }

                viewModelWithAttributes.Attributes = modelWithAttributes.AttributeValues
                    .Where( av =>
                    {
                        var attribute = AttributeCache.Get( av.Value.AttributeId );
                        return attribute?.IsAuthorized( Authorization.VIEW, currentPerson ) ?? false;
                    } )
                    .ToDictionary( kvp => kvp.Key, kvp => ClientAttributeHelper.ToClientAttributeValue( kvp.Value ) );
            }
        }

        /// <summary>
        /// Applies the additional properties and security to view model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="viewModel">The view model.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        public virtual void ApplyAdditionalPropertiesAndSecurityToViewModel( TModel model, TViewModel viewModel, Person currentPerson = null, bool loadAttributes = true )
        {
            return;
        }

        /// <summary>
        /// Copies the properties to the destination.
        /// https://stackoverflow.com/a/28814556/13215483
        /// </summary>
        /// <param name="source">The source object to copy properties from.</param>
        /// <param name="destination">The destination object to copy properties to.</param>
        public static void CopyProperties( object source, object destination )
        {
            if ( source == null || destination == null )
            {
                return;
            }

            var sourceProps = source.GetType().GetProperties().Where( x => x.CanRead ).ToList();
            var destProps = destination.GetType().GetProperties().Where( x => x.CanWrite ).ToList();

            foreach ( var sourceProp in sourceProps )
            {
                if ( destProps.Any( x => x.Name == sourceProp.Name ) )
                {
                    var destType = destProps.First( x => x.Name == sourceProp.Name );

                    if ( destType.CanWrite && destType.PropertyType.IsAssignableFrom( sourceProp.PropertyType ) )
                    {
                        // check if the property can be set or no.
                        destType.SetValue( destination, sourceProp.GetValue( source, null ), null );
                    }
                }
            }
        }
    }
}
