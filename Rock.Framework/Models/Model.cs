using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services;
using System.Data.Services.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Rock.Models
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

        [Key]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public Guid Guid { get; set; }

        #region ISecured implementation

        [NotMapped]
        public virtual string AuthEntity { get { return string.Empty; } }

        [NotMapped]
        public virtual Rock.Cms.Security.ISecured ParentAuthority { get { return null; } }

        public virtual List<string> SupportedActions
        {
            get { return new List<string>() { "View", "Edit"  }; }
        }

        public virtual bool Authorized( string action, System.Web.Security.MembershipUser user )
        {
            return Rock.Cms.Security.Authorization.Authorized( this, action, user );
        }

        public virtual bool DefaultAuthorization (string action)
        {
            return action == "View";
        }

        #endregion

        #region Events

        // Adding
        public static event EventHandler<ModelUpdatingEventArgs> Adding;
        protected virtual void OnAdding( ModelUpdatingEventArgs e )
        {
            if ( Adding != null )
                Adding( this, e );
        }
        public virtual void RaiseAddingEvent( out bool cancel )
        {
            ModelUpdatingEventArgs e = new ModelUpdatingEventArgs( this );
            OnAdding( e );
            cancel = e.Cancel;
        }

        // Added
        public static event EventHandler<ModelUpdatedEventArgs> Added;
        protected virtual void OnAdded( ModelUpdatedEventArgs e )
        {
            if ( Added != null )
                Added( this, e );
        }
        public virtual void RaiseAddedEvent()
        {
            OnAdded( new ModelUpdatedEventArgs( this ) );
        }

        // Deleting
        public static event EventHandler<ModelUpdatingEventArgs> Deleting;
        protected virtual void OnDeleting( ModelUpdatingEventArgs e )
        {
            if ( Deleting != null )
                Deleting( this, e );
        }
        public virtual void RaiseDeletingEvent( out bool cancel )
        {
            ModelUpdatingEventArgs e = new ModelUpdatingEventArgs( this );
            OnDeleting( e );
            cancel = e.Cancel;
        }

        // Deleted
        public static event EventHandler<ModelUpdatedEventArgs> Deleted;
        protected virtual void OnDeleted( ModelUpdatedEventArgs e )
        {
            if ( Deleted != null )
                Deleted( this, e );
        }
        public virtual void RaiseDeletedEvent()
        {
            OnDeleted( new ModelUpdatedEventArgs( this ) );
        }

        // Updating
        public static event EventHandler<ModelUpdatingEventArgs> Updating;
        protected virtual void OnUpdating( ModelUpdatingEventArgs e )
        {
            if ( Updating != null )
                Updating( this, e );
        }
        public virtual void RaiseUpdatingEvent( out bool cancel )
        {
            ModelUpdatingEventArgs e = new ModelUpdatingEventArgs( this );
            OnUpdating( e );
            cancel = e.Cancel;
        }

        // Updated
        public static event EventHandler<ModelUpdatedEventArgs> Updated;
        protected virtual void OnUpdated( ModelUpdatedEventArgs e )
        {
            if ( Updated != null )
                Updated( this, e );
        }
        public virtual void RaiseUpdatedEvent()
        {
            OnUpdated( new ModelUpdatedEventArgs( this ) );
        }

        #endregion
    }

    public class ModelUpdatingEventArgs : EventArgs
    {
        public readonly IModel Model;

        private bool cancel = false;
        public bool Cancel 
        { 
            get { return cancel; }
            set 
            { 
                if (value == true)
                    cancel = true;
            }

        }

        public ModelUpdatingEventArgs( IModel model )
        {
            Model = model;
        }
    }

    public class ModelUpdatedEventArgs : EventArgs
    {
        public readonly IModel Model;

        public ModelUpdatedEventArgs( IModel model )
        {
            Model = model;
        }
    }
}