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

using Rock.Cache;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached WorkflowActionForm
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheWorkflowActionForm instead" )]
    public class WorkflowActionFormCache : CachedModel<WorkflowActionForm>
    {
        #region Constructors

        private WorkflowActionFormCache( CacheWorkflowActionForm cacheItem )
        {
            CopyFromNewCache( cacheItem );
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the notification system email identifier.
        /// </summary>
        /// <value>
        /// The notification system email identifier.
        /// </value>
        public int? NotificationSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include actions in notification].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include actions in notification]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeActionsInNotification { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        /// <value>
        /// The footer.
        /// </value>
        public string Footer { get; set; }

        /// <summary>
        /// Gets or sets the delimited list of action buttons and actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        public string Actions { get; set; }

        /// <summary>
        /// An optional text attribute that will be updated with the action that was selected
        /// </summary>
        /// <value>
        /// The action attribute unique identifier.
        /// </value>
        public Guid? ActionAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the allow notes.
        /// </summary>
        /// <value>
        /// The allow notes.
        /// </value>
        public bool? AllowNotes { get; set; }

        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <value>
        /// The defined values.
        /// </value>
        public List<WorkflowActionFormAttributeCache> FormAttributes
        {
            get
            {
                var formAttributes = new List<WorkflowActionFormAttributeCache>();

                lock ( _obj )
                {
                    if ( _formAttributeIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            _formAttributeIds = new WorkflowActionFormAttributeService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( a => a.WorkflowActionFormId == Id )
                                .Select( a => a.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( var id in _formAttributeIds )
                {
                    var formAttribute = WorkflowActionFormAttributeCache.Read( id );
                    if ( formAttribute != null )
                    {
                        formAttributes.Add( formAttribute );
                    }
                }

                return formAttributes;
            }
        }
        private List<int> _formAttributeIds;

        /// <summary>
        /// Gets the buttons.
        /// </summary>
        /// <value>
        /// The buttons.
        /// </value>
        public List<WorkflowActionForm.LiquidButton> Buttons => WorkflowActionForm.GetActionButtons( Actions );

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is WorkflowActionForm ) ) return;

            var workflowActionForm = (WorkflowActionForm)model;
            NotificationSystemEmailId = workflowActionForm.NotificationSystemEmailId;
            IncludeActionsInNotification = workflowActionForm.IncludeActionsInNotification;
            Header = workflowActionForm.Header;
            Footer = workflowActionForm.Footer;
            Actions = workflowActionForm.Actions;
            ActionAttributeGuid = workflowActionForm.ActionAttributeGuid;
            AllowNotes = workflowActionForm.AllowNotes;

            // set _formAttributeIds to null so it load them all at once on demand
            _formAttributeIds = null;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheWorkflowActionForm ) ) return;

            var workflowActionForm = (CacheWorkflowActionForm)cacheEntity;
            NotificationSystemEmailId = workflowActionForm.NotificationSystemEmailId;
            IncludeActionsInNotification = workflowActionForm.IncludeActionsInNotification;
            Header = workflowActionForm.Header;
            Footer = workflowActionForm.Footer;
            Actions = workflowActionForm.Actions;
            ActionAttributeGuid = workflowActionForm.ActionAttributeGuid;
            AllowNotes = workflowActionForm.AllowNotes;

            _formAttributeIds = workflowActionForm.FormAttributes.Select( a => a.AttributeId ).ToList();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns WorkflowActionForm object from cache.  If workflowActionForm does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormCache Read( int id, RockContext rockContext = null )
        {
            return new WorkflowActionFormCache( CacheWorkflowActionForm.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormCache Read( Guid guid, RockContext rockContext = null )
        {
            return new WorkflowActionFormCache( CacheWorkflowActionForm.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="workflowActionFormModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormCache Read( WorkflowActionForm workflowActionFormModel, RockContext rockContext = null )
        {
            return new WorkflowActionFormCache( CacheWorkflowActionForm.Get( workflowActionFormModel ) );
        }

        /// <summary>
        /// Removes workflowActionForm from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            CacheWorkflowActionForm.Remove( id );
        }

        #endregion
    }

}