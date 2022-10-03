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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Web.UI.Controls;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Base class for a Field Type that allows a single entity of type TEntity to be selected from a dropdown list of entities.
    /// The selected value is stored using the Entity Guid.
    /// </summary>
    public abstract class EntitySingleSelectionListFieldTypeBase<TEntity> : FieldType, IEntityFieldType
        where TEntity : class, IEntity
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( guid.HasValue )
            {
                return OnFormatValue( guid.Value );
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
        }

        /// <summary>
        /// Returns a user-friendly description of the entity.
        /// </summary>
        /// <param name="entityGuid"></param>
        /// <returns></returns>
        protected abstract string OnFormatValue( Guid entityGuid );

        /// <summary>
        /// Returns a dictionary of the items available for selection.
        /// </summary>
        /// <returns></returns>
        protected abstract Dictionary<Guid, string> OnGetItemList();

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var editControl = new RockDropDownList { ID = id };
            editControl.Items.Add( new ListItem() );

            var items = this.OnGetItemList();

            if ( items != null
                 && items.Any() )
            {
                foreach ( var item in items )
                {
                    var listItem = new ListItem( item.Value, item.Key.ToString().ToUpper() );

                    editControl.Items.Add( listItem );
                }

                return editControl;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as DropDownList;
            if ( picker != null )
            {
                return picker.SelectedValue;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var editControl = control as ListControl;
            if ( editControl != null )
            {
                editControl.SetValue( value );
            }
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var guid = GetEditValue( control, configurationValues ).AsGuid();

            var item = this.GetEntityByGuid( guid, new RockContext() );

            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = this.GetEntityById( id, new RockContext() );

            var guidValue = item != null ? item.Guid.ToString() : string.Empty;

            SetEditValue( control, configurationValues, guidValue );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public TEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public TEntity GetEntity( string value, RockContext rockContext )
        {
            return GetEntityByGuid( value.AsGuidOrNull(), rockContext );
        }

        /// <summary>
        /// Gets the entity by unique identifier.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private TEntity GetEntityByGuid( Guid? guid, RockContext rockContext )
        {
            if ( !guid.HasValue )
            {
                return null;
            }

            var service = GetEntityService( rockContext );

            var getInfo = service.GetType().GetMethod( "GetNoTracking", new Type[] { typeof( Guid ) } );

            var entity = getInfo.Invoke( service, new object[] { guid.Value } );

            return entity as TEntity;
        }

        /// <summary>
        /// Gets the entity by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private TEntity GetEntityById( int? id, RockContext rockContext )
        {
            if ( !id.HasValue )
            {
                return null;
            }

            var service = GetEntityService( rockContext );

            var getInfo = service.GetType().GetMethod( "GetNoTracking", new Type[] { typeof( int ) } );

            var entity = getInfo.Invoke( service, new object[] { id.Value } );

            return entity as TEntity;
        }

        private IService GetEntityService( RockContext dataContext )
        {
            dataContext = dataContext ?? new RockContext();

            return Reflection.GetServiceForEntityType( typeof(TEntity), dataContext );
        }

        #endregion

        #region IEntityFieldType

        IEntity IEntityFieldType.GetEntity( string value )
        {
            return this.GetEntity( value );
        }

        IEntity IEntityFieldType.GetEntity( string value, RockContext rockContext )
        {
            return this.GetEntity( value, rockContext );
        }

        #endregion
    }
}
