//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.Runtime.Serialization;
using Rock.Security;

namespace Rock.Data
{
    /// <summary>
    /// Base class that all models need to inherit from
    /// </summary>
    [DataServiceKey("Id")]
    [IgnoreProperties(new[] { "ParentAuthority", "SupportedActions", "AuthEntity" })]
    [DataContract]
    public abstract class Model<T> : IModel
    {
        // Note: The DataServiceKey attribute is part of the magic behind WCF Data Services. This allows
        // the service to interface with EF and fetch data.

        /// <summary>
        /// The Id
        /// </summary>
        [Key]
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the GUID.
        /// </summary>
        /// <value>
        /// The GUID.
        /// </value>
        [DataMember]
        public Guid Guid { 
            get { return _guid; }
            set { _guid = value; }
        }
        private Guid _guid = Guid.NewGuid();

        /// <summary>
        /// Gets a publicly viewable unique key for the model.
        /// </summary>
        [NotMapped]
        public string EncryptedKey
        {
            get
            {
                string identifier = this.Id.ToString() + ">" + this.Guid.ToString();
                return Rock.Security.Encryption.EncryptString( identifier );
            }
        }

        [NotMapped]
        public string ContextKey
        {
            get
            {
                string identifier = 
                    typeof(T).FullName + "|" +
                    this.Id.ToString() + ">" + 
                    this.Guid.ToString();
                return System.Web.HttpUtility.UrlEncode(Rock.Security.Encryption.EncryptString( identifier ));
            }
        }

        /// <summary>
        /// Gets the validation results.
        /// </summary>
        [NotMapped]
        public List<ValidationResult> ValidationResults
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
        public bool IsValid
        {
            get
            {
                var valContext = new ValidationContext( this, serviceProvider: null, items: null );
                _validationResults = new List<ValidationResult>();
                return Validator.TryValidateObject( this, valContext, _validationResults );
            }
        }

        #region ISecured implementation

        /// <summary>
        /// The auth entity. Classes that implement the <see cref="Rock.Security.ISecured"/> interface should return
        /// a value that is unique across all <see cref="Rock.Security.ISecured"/> classes.  Typically this is the
        /// qualified name of the class.
        /// </summary>
        [NotMapped]
        public virtual string AuthEntity { get { return string.Empty; } }

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        [NotMapped]
        public virtual Security.ISecured ParentAuthority { get { return null; } }

        /// <summary>
        /// A list of actions that this class supports.
        /// </summary>
        [NotMapped]
        public virtual List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Edit"  }; }
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public virtual bool Authorized( string action, Rock.CMS.User user )
        {
            return Security.Authorization.Authorized( this, action, user );
        }

        /// <summary>
        /// Return <c>true</c> if the user is authorized to perform the selected action on this object.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="username">The user name.</param>
        /// <returns></returns>
        //public virtual bool Authorized( string action, string username )
        //{
        //    return Security.Authorization.Authorized( this, action, username );
        //}

        /// <summary>
        /// If a user or role is not specifically allowed or denied to perform the selected action,
        /// return <c>true</c> if they should be allowed anyway or <c>false</c> if not.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public virtual bool DefaultAuthorization (string action)
        {
            return action == "View";
        }

        public IQueryable<AuthRule> FindAuthRules()
        {
            return ( from action in SupportedActions
                     from rule in Authorization.AuthRules( this.AuthEntity, this.Id, action )
                     select rule ).AsQueryable();
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
        public virtual void RaiseDeletedEvent(int? personId)
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

    /// <summary>
    /// Event argument used when model was added, updated, or deleted
    /// </summary>
    public class ModelUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// The affected model
        /// </summary>
        public readonly IModel Model;

        /// <summary>
        /// The id of the person making the update
        /// </summary>
        public readonly int? PersonId;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpdatedEventArgs"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="personId">The person id.</param>
        public ModelUpdatedEventArgs( IModel model, int? personId )
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
                if (value == true)
                    cancel = true;
            }

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelUpdatingEventArgs"/> class.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="personId">The person id.</param>
        public ModelUpdatingEventArgs( IModel model, int? personId )
            : base( model, personId )
        {
        }
    }

    /// <summary>
    /// Object used for current model (context) implementation 
    /// </summary>
    internal class KeyModel
    {
        public string Key { get; set; }
        public IModel Model { get; set; }

        public KeyModel (string key)
        {
            Key = key;
        }
    }

}