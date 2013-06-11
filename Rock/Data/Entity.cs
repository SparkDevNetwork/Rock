//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Rock.Data
{
    /// <summary>
    /// Base class that all models need to inherit from
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DataContract]
    public abstract class Entity<T> : IEntity, DotLiquid.ILiquidizable
        where T : Entity<T>, new()
    {
        #region Entity Properties

        /// <summary>
        /// The Id
        /// </summary>
        [Key]
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or 
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        [AlternateKey]
        [DataMember]
        public Guid Guid
        {
            get { return _guid; }
            set { _guid = value; }
        }
        private Guid _guid = Guid.NewGuid();

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets the type id.
        /// </summary>
        /// <value>
        /// The type id.
        /// </value>
        public virtual int TypeId
        {
            get
            {
                // Read should never return null since it will create entity type if it doesn't exist
                return Rock.Web.Cache.EntityTypeCache.Read( typeof( T ) ).Id;
            }
        }

        /// <summary>
        /// Gets the unique type name of the entity.  Typically this is the qualified name of the class
        /// </summary>
        /// <value>
        /// The name of the entity type.
        /// </value>
        [NotMapped]
        public virtual string TypeName 
        {
            get
            {
                return typeof( T ).FullName;
            }
        }

        /// <summary>
        /// Gets the context key.
        /// </summary>
        /// <value>
        /// The context key.
        /// </value>
        [NotMapped]
        public virtual string ContextKey
        {
            get
            {
                string identifier =
                    TypeName + "|" +
                    this.Id.ToString() + ">" +
                    this.Guid.ToString();
                return System.Web.HttpUtility.UrlEncode( Rock.Security.Encryption.EncryptString( identifier ) );
            }
        }

        /// <summary>
        /// Gets the validation results.
        /// </summary>
        [NotMapped]
        public virtual List<ValidationResult> ValidationResults
        {
            get { return _validationResults; }
        }
        private List<ValidationResult> _validationResults;

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        [NotMapped]
        public virtual bool IsValid
        {
            get
            {
                var valContext = new ValidationContext( this, serviceProvider: null, items: null );
                _validationResults = new List<ValidationResult>();
                return Validator.TryValidateObject( this, valContext, _validationResults );
            }
        }

        /// <summary>
        /// Gets a publicly viewable unique key for the model.
        /// </summary>
        [NotMapped]
        public virtual string EncryptedKey
        {
            get
            {
                string identifier = this.Id.ToString() + ">" + this.Guid.ToString();
                return Rock.Security.Encryption.EncryptString( identifier );
            }
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Gets the name of the entity type friendly.
        /// </summary>
        /// <value>
        /// The name of the entity type friendly.
        /// </value>
        [NotMapped]
        public static string FriendlyTypeName
        {
            get
            {
                var type = typeof( T );
                return type.GetFriendlyTypeName();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a deep copy of this instance
        /// </summary>
        /// <returns></returns>
        public virtual IEntity Clone()
        {
            var json = this.ToJson();
            return FromJson( json );
        }

        /// <summary>
        /// Converts object to dictionary.
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();

            foreach(var propInfo in this.GetType().GetProperties())
            {
                if ( !propInfo.GetGetMethod().IsVirtual || propInfo.Name == "Id" || propInfo.Name == "Guid" || propInfo.Name == "Order" )
                {
                    dictionary.Add( propInfo.Name, propInfo.GetValue( this, null ) );
                }
            }

            return dictionary;
        }

        /// <summary>
        /// Froms the dictionary.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public virtual void FromDictionary( Dictionary<string, object> properties )
        {
            Type type = this.GetType();

            foreach ( var property in properties )
            {
                var propInfo = type.GetProperty( property.Key );
                if ( propInfo != null )
                {
                    propInfo.SetValue( this, property.Value );
                }
            }
        }

        /// <summary>
        /// Converts object to dictionary for DotLiquid.
        /// </summary>
        /// <returns></returns>
        public virtual object ToLiquid()
        {
            var dictionary = new Dictionary<string, object>();

            foreach ( var propInfo in this.GetType().GetProperties() )
            {
                if ( propInfo.GetCustomAttributes( typeof( Rock.Data.MergeFieldAttribute ) ).Count() > 0 )
                    dictionary.Add( propInfo.Name, propInfo.GetValue( this, null ) );
            }

            return dictionary;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Static method to return an object from a json string.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static T FromJson(string json) 
        {
            return JsonConvert.DeserializeObject(json, typeof(T)) as T;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when model is being added.
        /// </summary>
        public static event EventHandler<ModelUpdatingEventArgs> Adding;

        /// <summary>
        /// Raises the <see cref="E:Adding"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ModelUpdatingEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAdding( ModelUpdatingEventArgs e )
        {
            if ( Adding != null )
                Adding( this, e );
        }

        /// <summary>
        /// Raises the adding event.
        /// </summary>
        /// <param name="cancel">if set to <c>true</c> [cancel].</param>
        /// <param name="personId">The person id.</param>
        public virtual void RaiseAddingEvent( out bool cancel, int? personId )
        {
            ModelUpdatingEventArgs e = new ModelUpdatingEventArgs( this, personId );
            OnAdding( e );
            cancel = e.Cancel;
        }

        /// <summary>
        /// Occurs when model was added.
        /// </summary>
        public static event EventHandler<ModelUpdatedEventArgs> Added;

        /// <summary>
        /// Raises the <see cref="E:Added"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ModelUpdatedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAdded( ModelUpdatedEventArgs e )
        {
            if ( Added != null )
                Added( this, e );
        }

        /// <summary>
        /// Raises the added event.
        /// </summary>
        /// <param name="personId">The person id.</param>
        public virtual void RaiseAddedEvent( int? personId )
        {
            OnAdded( new ModelUpdatedEventArgs( this, personId ) );
        }

        /// <summary>
        /// Occurs when model is being deleted.
        /// </summary>
        public static event EventHandler<ModelUpdatingEventArgs> Deleting;

        /// <summary>
        /// Raises the <see cref="E:Deleting"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ModelUpdatingEventArgs"/> instance containing the event data.</param>
        protected virtual void OnDeleting( ModelUpdatingEventArgs e )
        {
            if ( Deleting != null )
                Deleting( this, e );
        }

        /// <summary>
        /// Raises the deleting event.
        /// </summary>
        /// <param name="cancel">if set to <c>true</c> [cancel].</param>
        /// <param name="personId">The person id.</param>
        public virtual void RaiseDeletingEvent( out bool cancel, int? personId )
        {
            ModelUpdatingEventArgs e = new ModelUpdatingEventArgs( this, personId );
            OnDeleting( e );
            cancel = e.Cancel;
        }

        /// <summary>
        /// Occurs when model was deleted.
        /// </summary>
        public static event EventHandler<ModelUpdatedEventArgs> Deleted;

        /// <summary>
        /// Raises the <see cref="E:Deleted"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ModelUpdatedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnDeleted( ModelUpdatedEventArgs e )
        {
            if ( Deleted != null )
                Deleted( this, e );
        }

        /// <summary>
        /// Raises the deleted event.
        /// </summary>
        /// <param name="personId">The person id.</param>
        public virtual void RaiseDeletedEvent( int? personId )
        {
            OnDeleted( new ModelUpdatedEventArgs( this, personId ) );
        }

        /// <summary>
        /// Occurs when model is being updated.
        /// </summary>
        public static event EventHandler<ModelUpdatingEventArgs> Updating;

        /// <summary>
        /// Raises the <see cref="E:Updating"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ModelUpdatingEventArgs"/> instance containing the event data.</param>
        protected virtual void OnUpdating( ModelUpdatingEventArgs e )
        {
            if ( Updating != null )
                Updating( this, e );
        }

        /// <summary>
        /// Raises the updating event.
        /// </summary>
        /// <param name="cancel">if set to <c>true</c> [cancel].</param>
        /// <param name="personId">The person id.</param>
        public virtual void RaiseUpdatingEvent( out bool cancel, int? personId )
        {
            ModelUpdatingEventArgs e = new ModelUpdatingEventArgs( this, personId );
            OnUpdating( e );
            cancel = e.Cancel;
        }

        /// <summary>
        /// Occurs when model was updated
        /// </summary>
        public static event EventHandler<ModelUpdatedEventArgs> Updated;

        /// <summary>
        /// Raises the <see cref="E:Updated"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ModelUpdatedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnUpdated( ModelUpdatedEventArgs e )
        {
            if ( Updated != null )
                Updated( this, e );
        }

        /// <summary>
        /// Raises the updated event.
        /// </summary>
        /// <param name="personId">The person id.</param>
        public virtual void RaiseUpdatedEvent( int? personId )
        {
            OnUpdated( new ModelUpdatedEventArgs( this, personId ) );
        }

        #endregion

    }

    #region Event Arguments

    /// <summary>
    /// Event argument used when model was added, updated, or deleted
    /// </summary>
    public class ModelUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// The affected model
        /// </summary>
        public readonly IEntity Model;

        /// <summary>
        /// The id of the person making the update
        /// </summary>
        public readonly int? PersonId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpdatedEventArgs"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="personId">The person id.</param>
        public ModelUpdatedEventArgs( IEntity model, int? personId )
        {
            Model = model;
            PersonId = personId;
        }
    }

    /// <summary>
    /// Event argument used when model is being added, updated, or deleted
    /// </summary>
    public class ModelUpdatingEventArgs : ModelUpdatedEventArgs
    {
        private bool cancel = false;
        /// <summary>
        /// Gets or sets a value indicating whether event should be cancelled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if event should be canceled; otherwise, <c>false</c>.
        /// </value>
        public bool Cancel
        {
            get { return cancel; }
            set
            {
                if ( value == true )
                    cancel = true;
            }

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpdatingEventArgs"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="personId">The person id.</param>
        public ModelUpdatingEventArgs( IEntity model, int? personId )
            : base( model, personId )
        {
        }
    }

    #endregion

    #region KeyEntity

    /// <summary>
    /// Object used for current model (context) implementation 
    /// </summary>
    internal class KeyEntity
    {
        public string Key { get; set; }
        public int Id { get; set; }
        public IEntity Entity { get; set; }

        public KeyEntity( int id )
        {
            Id = id;
        }

        public KeyEntity( string key )
        {
            Key = key;
        }
    }

    #endregion


} 