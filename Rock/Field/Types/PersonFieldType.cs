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
#if WEBFORMS
using System.Web.UI;
using OpenXmlPowerTools;
#endif
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field used to save and display a person. Stored as PersonAlias.Guid
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms, Utility.RockPlatform.Obsidian )]
    [Rock.SystemGuid.FieldTypeGuid( Rock.SystemGuid.FieldType.PERSON )]
    public class PersonFieldType : FieldType, IEntityFieldType, ILinkableFieldType, IEntityReferenceFieldType
    {
        #region Configuration

        private const string ENABLE_SELF_SELECTION_KEY = "EnableSelfSelection";
        private const string INCLUDE_BUSINESSES = "includeBusinesses";

        #endregion

        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( privateValue ) )
            {
                Guid guid = privateValue.AsGuid();

                using ( var rockContext = new RockContext() )
                {
                    formattedValue = new PersonAliasService( rockContext ).Queryable()
                    .AsNoTracking()
                    .Where( a => a.Guid.Equals( guid ) )
                    .Select( a => a.Person.NickName + " " + a.Person.LastName )
                    .FirstOrDefault();
                }
            }
            return formattedValue;
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

        /// <inheritdoc/>
        public override string GetPublicValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            if ( Guid.TryParse( privateValue, out Guid guid ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var personAlias = new PersonAliasService( rockContext )
                        .GetNoTracking( guid );
                    if ( personAlias != null )
                    {
                        return new ListItemBag()
                        {
                            Value = personAlias.Guid.ToString(),
                            Text = personAlias.Person.NickName + " " + personAlias.Person.LastName,
                        }.ToCamelCaseJson( false, true );
                    }
                }
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override string GetPrivateEditValue( string publicValue, Dictionary<string, string> privateConfigurationValues )
        {
            var personValue = publicValue.FromJsonOrNull<ListItemBag>();

            if ( personValue != null )
            {
                return personValue.Value;
            }

            return string.Empty;
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
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
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

        #region Persistence

        /// <inheritdoc/>
        public override PersistedValues GetPersistedValues( string privateValue, Dictionary<string, string> privateConfigurationValues, IDictionary<string, object> cache )
        {
            if ( string.IsNullOrWhiteSpace( privateValue ) )
            {
                return new PersistedValues
                {
                    TextValue = string.Empty,
                    CondensedTextValue = string.Empty,
                    HtmlValue = string.Empty,
                    CondensedHtmlValue = string.Empty
                };
            }

            var textValue = GetTextValue( privateValue, privateConfigurationValues );
            var condensedTextValue = textValue.Truncate( CondensedTruncateLength );

            return new PersistedValues
            {
                TextValue = textValue,
                CondensedTextValue = condensedTextValue,
                HtmlValue = textValue,
                CondensedHtmlValue = condensedTextValue
            };
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            var guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var personAliasValues = new PersonAliasService( rockContext ).Queryable()
                    .Where( pa => pa.Guid == guid.Value )
                    .Select( pa => new
                    {
                        pa.Id,
                        pa.PersonId
                    } )
                    .FirstOrDefault();

                if ( personAliasValues == null )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<PersonAlias>().Value, personAliasValues.Id ),
                    new ReferencedEntity( EntityTypeCache.GetId<Person>().Value, personAliasValues.PersonId )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            // This field type references the FirstName and LastName properties
            // of a Person and the PersonId property of a PersonAlias. It
            // should have its persisted values updated when changed.
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.NickName ) ),
                new ReferencedProperty( EntityTypeCache.GetId<Person>().Value, nameof( Person.LastName ) ),
                new ReferencedProperty( EntityTypeCache.GetId<PersonAlias>().Value, nameof( PersonAlias.PersonId ) ),
            };
        }

        #endregion

        #region WebForms
#if WEBFORMS

        /// <summary>
        /// Returns a list of the configuration keys
        /// </summary>
        /// <returns></returns>
        public override List<string> ConfigurationKeys()
        {
            var configKeys = base.ConfigurationKeys();
            configKeys.Add( ENABLE_SELF_SELECTION_KEY );
            configKeys.Add( INCLUDE_BUSINESSES );
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
            cbEnableSelfSelection.AutoPostBack = true;
            cbEnableSelfSelection.CheckedChanged += OnQualifierUpdated;
            cbEnableSelfSelection.Label = "Enable Self Selection";
            cbEnableSelfSelection.Help = "When using Person Picker, show the self selection option";

            var cbIncludeBusinesses = new RockCheckBox();
            controls.Add( cbIncludeBusinesses );
            cbIncludeBusinesses.AutoPostBack = true;
            cbIncludeBusinesses.CheckedChanged += OnQualifierUpdated;
            cbIncludeBusinesses.Label = "Include Businesses";
            cbIncludeBusinesses.Help = "When using Person Picker, include businesses in the search results";

            return controls;
        }

        /// <summary>
        /// Gets the configuration value.
        /// </summary>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override Dictionary<string, ConfigurationValue> ConfigurationValues( List<Control> controls )
        {
            Dictionary<string, ConfigurationValue> configurationValues = new Dictionary<string, ConfigurationValue>
            {
                { ENABLE_SELF_SELECTION_KEY, new ConfigurationValue( "Enable Self Selection", "When using Person Picker, show the self selection option", string.Empty ) },
                { INCLUDE_BUSINESSES, new ConfigurationValue( "Include Businesses", "When using Person Picker, include businesses in the search results", string.Empty ) }
            };

            if ( controls != null )
            {
                if ( controls.Count > 0 && controls[0] is RockCheckBox cbEnableSelfSelection )
                {
                    configurationValues[ENABLE_SELF_SELECTION_KEY].Value = cbEnableSelfSelection.Checked.ToString();
                }
                if ( controls.Count > 1 && controls[1] is RockCheckBox cbIncludeBusinesses )
                {
                    configurationValues[INCLUDE_BUSINESSES].Value = cbIncludeBusinesses.Checked.ToString();
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
            if ( controls != null && configurationValues != null )
            {
                if ( controls.Count > 0 && controls[0] is RockCheckBox cbEnableSelfSelection && configurationValues.ContainsKey( ENABLE_SELF_SELECTION_KEY ) )
                {
                    cbEnableSelfSelection.Checked = configurationValues[ENABLE_SELF_SELECTION_KEY].Value.AsBoolean();
                }
                if ( controls.Count > 1 && controls[1] is RockCheckBox cbIncludeBusinesses && configurationValues.ContainsKey( INCLUDE_BUSINESSES ) )
                {
                    cbIncludeBusinesses.Checked = configurationValues[INCLUDE_BUSINESSES].Value.AsBoolean();
                }
            }
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
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var personPicker = new PersonPicker { ID = id };
            if ( configurationValues.ContainsKey( ENABLE_SELF_SELECTION_KEY ) )
            {
                personPicker.EnableSelfSelection = configurationValues[ENABLE_SELF_SELECTION_KEY].Value.AsBoolean();
            }

            if ( configurationValues.ContainsKey( INCLUDE_BUSINESSES ) )
            {
                personPicker.IncludeBusinesses = configurationValues[INCLUDE_BUSINESSES].Value.AsBoolean();
            }

            return personPicker;
        }

        /// <summary>
        /// Reads new values entered by the user for the field (as PersonAlias.Guid)
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
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
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
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
            return item != null ? item.PersonId : ( int? ) null;
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

#endif
        #endregion

    }
}