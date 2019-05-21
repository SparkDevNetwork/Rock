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
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;

using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a person. Stored as PersonAlias.Guid
    /// </summary>
    [Serializable]
    public class PersonFieldType : FieldType, IEntityFieldType, ILinkableFieldType
    {
        #region configuration

        private const string ENABLE_SELF_SELECTION_KEY = "EnableSelfSelection";

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ENABLE_SELF_SELECTION_KEY );
            return configKeys;
        }

        /// <summary>
        /// Creates the HTML controls required to configure this type of field
        /// </summary>
        /// <returns></returns>
        public override List<Control> ConfigurationControls()
        {
            var controls = base.ConfigurationControls();

            var cbEnableSelfSelection = new RockCheckBox();
            controls.Add( cbEnableSelfSelection );
            cbEnableSelfSelection.Label = "Enable Self Selection";
            cbEnableSelfSelection.Text = "Yes";
            cbEnableSelfSelection.Help = "When using Person Picker, show the self selection option";
            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>();
            configurationValues.Add( ENABLE_SELF_SELECTION_KEY, new ConfigurationValue( "Enable Self Selection", "When using Person Picker, show the self selection option", string.Empty ) );

            if ( controls != null && controls.Count > 0 )
            {
                var cbEnableSelfSelection = controls[0] as RockCheckBox;
                if ( cbEnableSelfSelection != null )
                {
                    configurationValues[ENABLE_SELF_SELECTION_KEY].Value = cbEnableSelfSelection.Checked.ToString();
                }

            }

            return configurationValues;
        }

        /// <summary>
        /// Sets the configuration value.
        /// </summary>
        /// <param name="controls"></param>
        /// <param name="configurationValues"></param>
        public override void SetConfigurationValues( List<Control> controls, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( controls != null && configurationValues != null && controls.Count > 0 )
            {
                var cbEnableSelfSelection = controls[0] as RockCheckBox;

                if ( cbEnableSelfSelection != null && configurationValues.ContainsKey( ENABLE_SELF_SELECTION_KEY ) )
                {
                    cbEnableSelfSelection.Checked = configurationValues[ENABLE_SELF_SELECTION_KEY].Value.AsBoolean();
                }
            }
        }

        #endregion

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                Guid guid = value.AsGuid();
                using ( var rockContext = new RockContext() )
                {
                    formattedValue = new PersonAliasService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( a => a.Guid.Equals( guid ) )
                    .Select( a => a.Person.NickName + " " + a.Person.LastName )
                    .FirstOrDefault();
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        /// <summary>
        /// Formats the value extended.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public string UrlLink( string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                Guid guid = value.AsGuid();
                int personId = new PersonAliasService( new RockContext() ).Queryable()
                    .Where( a => a.Guid.Equals( guid ) )
                    .Select( a => a.PersonId )
                    .FirstOrDefault();
                return string.Format( "person/{0}", personId );
            }

            return value;
        }

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
        public override System.Web.UI.Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var personPicker = new PersonPicker { ID = id }; 
            if ( configurationValues.ContainsKey( ENABLE_SELF_SELECTION_KEY ) )
            {
                personPicker.EnableSelfSelection = configurationValues[ENABLE_SELF_SELECTION_KEY].Value.AsBoolean();
            }

            return personPicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field (as PersonAlias.Guid)
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            PersonPicker ppPerson = control as PersonPicker;
            string result = string.Empty;

            if ( ppPerson != null )
            {
                Guid personGuid = Guid.Empty;
                int? personId = ppPerson.PersonId;

                if ( personId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var personAlias = new PersonAliasService( rockContext ).GetByAliasId( personId.Value );
                        if ( personAlias != null )
                        {
                            result = personAlias.Guid.ToString();
                        }
                    }
                }

                return result;
            }

            return null;
        }

        /// <summary>
        /// Sets the value (as PersonAlias.Guid)
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            PersonPicker ppPerson = control as PersonPicker;
            if ( ppPerson != null )
            {
                Person person = null;
                Guid? personAliasGuid = value.AsGuidOrNull();
                if ( personAliasGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        person = new PersonAliasService( rockContext ).Queryable()
                            .Where( a => a.Guid == personAliasGuid.Value )
                            .Select( a => a.Person )
                            .FirstOrDefault();
                    }
                }

                ppPerson.SetValue( person );
            }
        }

        #endregion

        #region Filter Control

        /// <summary>
        /// Gets a filter expression for an attribute value.
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="filterValues">The filter values.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns></returns>
        public override System.Linq.Expressions.Expression AttributeFilterExpression( Dictionary<string, ConfigurationValue> configurationValues, List<string> filterValues, System.Linq.Expressions.ParameterExpression parameterExpression )
        {
            if ( filterValues.Count >= 2 )
            {
                string comparisonValue = filterValues[0];
                if ( comparisonValue != "0" )
                {
                    Guid guid = filterValues[1].AsGuid();
                    int personId = new PersonAliasService( new RockContext() ).Queryable()
                        .Where( a => a.Guid.Equals( guid ) )
                        .Select( a => a.PersonId )
                        .FirstOrDefault();

                    if ( personId > 0 )
                    {
                        ComparisonType comparisonType = comparisonValue.ConvertToEnum<ComparisonType>( ComparisonType.EqualTo );
                        MemberExpression propertyExpression = Expression.Property( parameterExpression, "ValueAsPersonId" );
                        ConstantExpression constantExpression = Expression.Constant( personId, typeof( int ) );
                        return ComparisonHelper.ComparisonExpression( comparisonType, propertyExpression, constantExpression );
                    }
                }
            }

            return new NoAttributeFilterExpression();
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            var item = new PersonAliasService( new RockContext() ).Get( guid );
            return item != null ? item.PersonId : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            var item = new PersonService( new RockContext() ).Get( id ?? 0 );
            string guidValue = item != null ? item.PrimaryAlias.Guid.ToString() : string.Empty;
            SetEditValue( control, configurationValues, guidValue );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity(string value)
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity(string value, RockContext rockContext)
        {
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                rockContext = rockContext ?? new RockContext();
                return new PersonAliasService( rockContext ).GetPerson( guid.Value );
            }

            return null;
        }

        #endregion


    }
}