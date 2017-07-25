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

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached WorkflowActionForm
    /// </summary>
    [Serializable]
    public class WorkflowActionFormCache : CachedModel<WorkflowActionForm>
    {
        #region Constructors

        private WorkflowActionFormCache( WorkflowActionForm model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        private object _obj = new object();

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
                    if ( formAttributeIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            formAttributeIds = new Model.WorkflowActionFormAttributeService( rockContext )
                                .Queryable().AsNoTracking()
                                .Where( a => a.WorkflowActionFormId == this.Id )
                                .Select( a => a.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( int id in formAttributeIds )
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
        private List<int> formAttributeIds = null;

        /// <summary>
        /// Gets the buttons.
        /// </summary>
        /// <value>
        /// The buttons.
        /// </value>
        public List<WorkflowActionForm.LiquidButton> Buttons
        {
            get
            {
                return WorkflowActionForm.GetActionButtons( Actions );
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is WorkflowActionForm )
            {
                var workflowActionForm = (WorkflowActionForm)model;
                this.NotificationSystemEmailId = workflowActionForm.NotificationSystemEmailId;
                this.IncludeActionsInNotification = workflowActionForm.IncludeActionsInNotification;
                this.Header = workflowActionForm.Header;
                this.Footer = workflowActionForm.Footer;
                this.Actions = workflowActionForm.Actions;
                this.ActionAttributeGuid = workflowActionForm.ActionAttributeGuid;
                this.AllowNotes = workflowActionForm.AllowNotes;

                // set formAttributeIds to null so it load them all at once on demand
                this.formAttributeIds = null;
            }
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:WorkflowActionForm:{0}", id );
        }

        /// <summary>
        /// Returns WorkflowActionForm object from cache.  If workflowActionForm does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowActionFormCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static WorkflowActionFormCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static WorkflowActionFormCache LoadById2( int id, RockContext rockContext )
        {
            var workflowActionFormService = new WorkflowActionFormService( rockContext );
            var workflowActionFormModel = workflowActionFormService
                .Queryable()
                .Where( t => t.Id == id )
                .FirstOrDefault();

            if ( workflowActionFormModel != null )
            {
                return new WorkflowActionFormCache( workflowActionFormModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }

        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var workflowActionFormService = new WorkflowActionFormService( rockContext );
            return workflowActionFormService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Reads the specified defined type model.
        /// </summary>
        /// <param name="workflowActionFormModel">The defined type model.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowActionFormCache Read( WorkflowActionForm workflowActionFormModel, RockContext rockContext = null )
        {
            return GetOrAddExisting( WorkflowActionFormCache.CacheKey( workflowActionFormModel.Id ),
                () => LoadByModel( workflowActionFormModel ) );
        }

        private static WorkflowActionFormCache LoadByModel( WorkflowActionForm workflowActionFormModel )
        {
            if ( workflowActionFormModel != null )
            {
                return new WorkflowActionFormCache( workflowActionFormModel );
            }
            return null;
        }

        /// <summary>
        /// Removes workflowActionForm from cache
        /// </summary>
        /// <param name="id"></param>
        public static void Flush( int id )
        {
            var actionForm = WorkflowActionFormCache.Read( id );
            if ( actionForm != null  )
            {
                foreach ( var formAttribute in actionForm.FormAttributes )
                {
                    WorkflowActionFormAttributeCache.Flush( formAttribute.Id );
                }
            }
            FlushCache( WorkflowActionFormCache.CacheKey( id ) );
        }

        #endregion
    }

}