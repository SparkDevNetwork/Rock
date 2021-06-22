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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.ViewModel
{
    /// <summary>
    /// View Model Helper
    /// </summary>
    public static class ViewModelHelper
    {
        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="loadAttributes">if set to <c>true</c> [load attributes].</param>
        /// <returns></returns>
        public static IViewModel GetViewModel( object model, Person currentPerson = null, bool loadAttributes = true )
        {
            if ( model is IViewModel viewModel )
            {
                return viewModel;
            }

            if ( model is Person person )
            {
                return person.ToViewModel( currentPerson, loadAttributes );
            }

            if ( model is Group group )
            {
                return group.ToViewModel( currentPerson, loadAttributes );
            }

            var type = model.GetType();

            if ( type.IsDynamicProxyType() )
            {
                type = type.BaseType;
            }

            var isModel = type.BaseType.GetGenericTypeDefinition() == typeof( Rock.Data.Model<> );

            if ( isModel )
            {
                var viewModelTypes = Reflection.SearchAssembly( typeof( PersonViewModel ).Assembly, typeof( ViewModelBase ) );
                var viewModelType = viewModelTypes.GetValueOrNull( $"Rock.ViewModel.{type.Name}ViewModel" );

                if ( viewModelType != null )
                {
                    var helperType = typeof( ViewModelHelper<,> ).MakeGenericType( type, viewModelType );
                    var helper = Activator.CreateInstance( helperType );
                    var method = helperType.GetMethod( "CreateViewModel" );
                    return method.Invoke( helper, new object[] { model, currentPerson, loadAttributes } ) as IViewModel;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// View Model Helper
    /// </summary>
    public class ViewModelHelper<TModel, TViewModel>
        where TViewModel : IViewModel, new()
    {
        /// <summary>
        /// Converts to viewmodel.
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

                viewModelWithAttributes.Attributes = modelWithAttributes.AttributeValues.Where( av =>
                {
                    var attribute = AttributeCache.Get( av.Value.AttributeId );
                    return attribute?.IsAuthorized( Authorization.VIEW, currentPerson ) ?? false;
                } ).ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToViewModel() );
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
        /// <param name="source">The source.</param>
        /// <param name="destination">The dest.</param>
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
