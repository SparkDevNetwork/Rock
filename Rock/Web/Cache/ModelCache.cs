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
using System.Runtime.Serialization;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Implementation of EntityCache for models
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TT">The type of the t.</typeparam>
    /// <seealso cref="EntityCache{T, TT}" />
    /// <seealso cref="Rock.Security.ISecured" />
    /// <seealso cref="Rock.Attribute.IHasAttributes" />
    /// <seealso cref="Rock.Lava.ILiquidizable" />
    [Serializable]
    [DataContract]
    public abstract class ModelCache<T, TT> : EntityCache<T, TT>, ISecured, IHasAttributes, Lava.ILiquidizable where T : IEntityCache, new()
        where TT : Model<TT>, new()
    {

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        [RockObsolete( "1.8" )]
        [Obsolete("Use SetFromEntity instead")]
        public virtual void CopyFromModel( Rock.Data.IEntity model )
        {
            this.SetFromEntity( model );
        }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            // If this model implements ISecured, set up those properties
            var secureModel = entity as ISecured;
            if ( secureModel != null )
            {
                TypeId = secureModel.TypeId;
                TypeName = secureModel.TypeName;
                SupportedActions = secureModel.SupportedActions;
            }

            // If this model implements IHasAttributes
            var attributeModel = entity as IHasAttributes;
            if ( attributeModel == null ) return;

            if ( attributeModel.Attributes == null )
            {
                attributeModel.LoadAttributes();
            }

            Attributes = attributeModel.Attributes;
            AttributeValues = attributeModel.AttributeValues;
        }

        #region ISecured Implementation

        /// <summary>
        /// Gets the Entity Type ID for this entity.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        [DataMember]
        [LavaIgnore]
        public virtual int TypeId { get; private set; }

        /// <summary>
        /// The auth entity. Classes that implement the <see cref="ISecured" /> interface should return
        /// a value that is unique across all <see cref="ISecured" /> classes.  Typically this is the
        /// qualified name of the class.
        /// </summary>
        [DataMember]
        [LavaIgnore]
        public virtual string TypeName { get; private set; }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        [LavaIgnore]
        public virtual ISecured ParentAuthority
        {
            get
            {
                if ( Id == 0 )
                {
                    return new GlobalDefault();
                }

                return new TT();
            }
        }

        /// <summary>
        /// An optional additional parent authority.  (i.e for Groups, the GroupType is main parent
        /// authority, but parent group is an additional parent authority )
        /// </summary>
        [LavaIgnore]
        public virtual ISecured ParentAuthorityPre => null;

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        [DataMember]
        [LavaIgnore]
        public virtual Dictionary<string, string> SupportedActions { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is authorized; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsAuthorized( string action, Person person )
        {
            return Authorization.Authorized( this, action, person );
        }

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public virtual bool IsAllowedByDefault( string action )
        {
            return action == Authorization.VIEW;
        }

        /// <summary>
        /// Determines whether the specified action is private (Only the current user has access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <returns>
        ///   <c>true</c> if the specified action is private; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsPrivate( string action, Person person )
        {
            return Authorization.IsPrivate( this, action, person );
        }

        /// <summary>
        /// Makes the action on the current entity private (Only the current user will have access).
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakePrivate( string action, Person person, RockContext rockContext = null )
        {
            Authorization.MakePrivate( this, action, person, rockContext );
        }

        /// <summary>
        /// If action on the current entity is private, removes security that made it private.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        public virtual void MakeUnPrivate( string action, Person person, RockContext rockContext )
        {
            Authorization.MakeUnPrivate( this, action, person, rockContext );
        }

        #endregion

        #region IHasAttribute Implementation

        /// <summary>
        /// List of attributes associated with the object.  This property will not include the attribute values.
        /// The <see cref="AttributeValues" /> property should be used to get attribute values.  Dictionary key
        /// is the attribute key, and value is the cached attribute
        /// </summary>
        /// <value>
        /// The attributes.
        /// </value>
        [LavaIgnore]
        public Dictionary<string, AttributeCache> Attributes
        {
            get
            {
                var attributes = new Dictionary<string, AttributeCache>();

                foreach ( var id in AttributeIds.ToList() )
                {
                    var attribute = AttributeCache.Get( id );
                    if ( attribute == null )
                    {
                        // this could happen if another thread deleted the attribute. If so, don't add it since it no longer exists
                        // NOTE:this should only happen if another thread deleted the attribute at the same time that this object was trying to fetch it, and should correct itself
                        System.Diagnostics.Debug.WriteLine( "Deleted AttributeCache detected. This is OK, but should be rare and only happen in a multi-threaded situation. If you see this message alot, something is wrong." );
                    }
                    else
                    {
                        attributes.Add( attribute.Key, attribute );
                    }
                }

                return attributes;
            }

            set
            {
                AttributeIds = new List<int>();
                if ( value == null ) return;

                foreach ( var attribute in value )
                {
                    AttributeIds.Add( attribute.Value.Id );
                }
            }
        }

        /// <summary>
        /// The attribute ids
        /// </summary>
        [DataMember]
        [LavaIgnore]
        protected List<int> AttributeIds = new List<int>();

        /// <summary>
        /// Dictionary of all attributes and their value.
        /// </summary>
        [DataMember]
        [LavaIgnore]
        public virtual Dictionary<string, AttributeValueCache> AttributeValues { get; set; }

        /// <summary>
        /// Gets the attribute value defaults.
        /// </summary>
        /// <value>
        /// The attribute defaults.
        /// </value>
        [LavaIgnore]
        public virtual Dictionary<string, string> AttributeValueDefaults => null;

        /// <summary>
        /// Saves the attribute values.
        /// </summary>
        public virtual void SaveAttributeValues()
        {
            var rockContext = new RockContext();
            var service = new Service<TT>( rockContext );
            var model = service.Get( Id );

            if ( model == null ) return;

            model.LoadAttributes();
            foreach ( var attribute in model.Attributes )
            {
                if ( AttributeValues.ContainsKey( attribute.Key ) )
                {
                    Attribute.Helper.SaveAttributeValue( model, attribute.Value, AttributeValues[attribute.Key].Value, rockContext );
                }
            }
        }

        /// <summary>
        /// Gets the value of an attribute key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The stored value as a string or null if none exists.</returns>
        public string GetAttributeValue( string key )
        {
            if ( AttributeValues != null &&
                AttributeValues.ContainsKey( key ) )
            {
                return AttributeValues[key].Value;
            }

            if ( Attributes != null &&
                Attributes.ContainsKey( key ) )
            {
                return Attributes[key].DefaultValue;
            }

            return null;
        }

        /// <summary>
        /// Gets the value of an attribute key - splitting that delimited value into a list of strings.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>A list of strings or an empty list if none exists.</returns>
        public List<string> GetAttributeValues( string key )
        {
            var value = GetAttributeValue( key );
            return !string.IsNullOrWhiteSpace( value ) ? value.SplitDelimitedValues().ToList() : new List<string>();
        }

        /// <summary>
        /// Sets the value of an attribute key in memory. Once values have been set, use the <see cref="SaveAttributeValues()" /> method to save all values to database
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void SetAttributeValue( string key, string value )
        {
            if ( AttributeValues == null ) return;

            if ( AttributeValues.ContainsKey( key ) )
            {
                AttributeValues[key].Value = value;
            }
            else if ( Attributes.ContainsKey( key ) )
            {
                var attributeValue = new AttributeValueCache
                {
                    AttributeId = Attributes[key].Id,
                    Value = value
                };
                AttributeValues.Add( key, attributeValue );
            }
        }

        /// <summary>
        /// Reloads the attribute values.
        /// </summary>
        [RockObsolete( "1.8" )]
        [Obsolete( "No longer needed. The Attributes will get reloaded automatically." )]
        public virtual void ReloadAttributeValues()
        {
            using ( var rockContext = new RockContext() )
            {
                var service = new Service<TT>( rockContext );
                var model = service.Get( Id );

                if ( model == null ) return;

                model.LoadAttributes( rockContext );

                AttributeValues = model.AttributeValues;
                Attributes = model.Attributes;
            }
        }

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( "No longer needed on Cached items. The Attributes will get loaded automatically.", true )]
        public void LoadAttributes( RockContext rockContext )
        {
            ReloadAttributeValues();
        }

        /// <summary>
        /// Loads the attributes.
        /// </summary>
        [RockObsolete( "1.8" )]
        [Obsolete( "No longer needed on Cached items. The Attributes will get loaded automatically." )]
        public void LoadAttributes()
        {
            ReloadAttributeValues();
        }

        #endregion

        #region ILiquidizable Implementation

        /// <summary>
        /// To the liquid.
        /// </summary>
        /// <returns></returns>
        public object ToLiquid()
        {
            return this;
        }

        /// <summary>
        /// Gets the available keys (for debugging info).
        /// </summary>
        /// <value>
        /// The available keys.
        /// </value>
        [LavaIgnore]
        public virtual List<string> AvailableKeys => ( from propInfo in GetType().GetProperties() where propInfo != null && !propInfo.GetCustomAttributes( typeof( LavaIgnoreAttribute ) ).Any() select propInfo.Name ).ToList();

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="System.Object"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        [LavaIgnore]
        public virtual object this[object key]
        {
            get
            {
                var keyString = key.ToStringSafe();

                if ( keyString == "AttributeValues" )
                {
                    return AttributeValues.Select( a => a.Value ).ToList();
                }

                var propInfo = GetType().GetProperty( key.ToStringSafe() );
                if ( propInfo != null && !propInfo.GetCustomAttributes( typeof( LavaIgnoreAttribute ) ).Any() )
                {
                    var propValue = propInfo.GetValue( this, null );
                    return ( propValue as Guid? )?.ToString() ?? propValue;
                }

                // The remainder of this method is only necessary to support the old way of getting attribute 
                // values in liquid templates (e.g. {{ Person.BaptismData }} ).  Once support for this method is 
                // deprecated ( in v4.0 ), and only the new method of using the Attribute filter is 
                // supported (e.g. {{ Person | Attribute:'BaptismDate' }} ), the remainder of this method 
                // can be removed

                if ( Attributes == null ) return null;

                var unformatted = false;
                var url = false;

                var attributeKey = key.ToStringSafe();
                if ( attributeKey.EndsWith( "_unformatted" ) )
                {
                    attributeKey = attributeKey.Replace( "_unformatted", string.Empty );
                    unformatted = true;
                }
                else if ( attributeKey.EndsWith( "_url" ) )
                {
                    attributeKey = attributeKey.Replace( "_url", string.Empty );
                    url = true;
                }

                if ( !Attributes.ContainsKey( attributeKey ) ) return null;

                var attribute = Attributes[attributeKey];
                if ( !attribute.IsAuthorized( Authorization.VIEW, null ) ) return null;

                var field = attribute.FieldType.Field;
                var value = GetAttributeValue( attribute.Key );

                if ( unformatted )
                {
                    return value;
                }

                if ( url && field is Field.ILinkableFieldType )
                {
                    return ( (Field.ILinkableFieldType)field ).UrlLink( value, attribute.QualifierValues );
                }

                return field.FormatValue( null, attribute.EntityTypeId, Id, value, attribute.QualifierValues, false );

            }
        }

        /// <summary>
        /// Determines whether the specified key contains key.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual bool ContainsKey( object key )
        {
            string attributeKey = key.ToStringSafe();

            if ( attributeKey == "AttributeValues" )
            {
                return true;
            }

            var propInfo = GetType().GetProperty( key.ToStringSafe() );
            if ( propInfo != null && !propInfo.GetCustomAttributes( typeof( LavaIgnoreAttribute ) ).Any() )
            {
                return true;
            }

            if ( Attributes == null )
            {
                // shouldn't happen
                return false;
            }

            if ( attributeKey.EndsWith( "_unformatted" ) )
            {
                attributeKey = attributeKey.Replace( "_unformatted", string.Empty );
            }
            else if ( attributeKey.EndsWith( "_url" ) )
            {
                attributeKey = attributeKey.Replace( "_url", string.Empty );
            }

            if ( Attributes == null || !Attributes.ContainsKey( attributeKey ) ) return false;

            var attribute = Attributes[attributeKey];

            return attribute.IsAuthorized( Authorization.VIEW, null );
        }

        #endregion
    }
}
