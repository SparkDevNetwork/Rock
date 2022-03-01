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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Workflow.FormBuilder;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached <seealso cref="WorkflowActionForm"/>
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowActionFormCache : ModelCache<WorkflowActionFormCache, WorkflowActionForm>
    {

        #region Properties

        private readonly object _obj = new object();

        /// <inheritdoc cref="WorkflowActionForm.NotificationSystemCommunicationId"/>
        [DataMember]
        public int? NotificationSystemCommunicationId { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.NotificationSystemEmailId"/>
        [DataMember]
        [Obsolete( "Use NotificationSystemCommunicationId instead." )]
        [RockObsolete( "1.10" )]
        public int? NotificationSystemEmailId { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.IncludeActionsInNotification"/>
        [DataMember]
        public bool IncludeActionsInNotification { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.Header"/>
        [DataMember]
        public string Header { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.Footer"/>
        [DataMember]
        public string Footer { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.Actions"/>
        [DataMember]
        public string Actions { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.ActionAttributeGuid"/>
        [DataMember]
        public Guid? ActionAttributeGuid { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.AllowNotes"/>
        [DataMember]
        public bool? AllowNotes { get; private set; }

        #region Person entry related Entity Properties

        /// <inheritdoc cref="WorkflowActionForm.AllowPersonEntry"/>
        [DataMember]
        public bool AllowPersonEntry { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryPreHtml"/>
        [DataMember]
        public string PersonEntryPreHtml { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryPostHtml"/>
        [DataMember]
        public string PersonEntryPostHtml { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryCampusIsVisible"/>
        [DataMember]
        public bool PersonEntryCampusIsVisible { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryAutofillCurrentPerson"/>
        [DataMember]
        public bool PersonEntryAutofillCurrentPerson { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryHideIfCurrentPersonKnown"/>
        [DataMember]
        public bool PersonEntryHideIfCurrentPersonKnown { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntrySpouseEntryOption"/>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntrySpouseEntryOption { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryGenderEntryOption"/>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryGenderEntryOption { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryEmailEntryOption"/>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryEmailEntryOption { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryMobilePhoneEntryOption"/>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryMobilePhoneEntryOption { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryBirthdateEntryOption"/>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryBirthdateEntryOption { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryAddressEntryOption"/>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryAddressEntryOption { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryMaritalStatusEntryOption"/>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryMaritalStatusEntryOption { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntrySpouseLabel"/>
        [DataMember]
        public string PersonEntrySpouseLabel { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryConnectionStatusValueId"/>
        [DataMember]
        public int? PersonEntryConnectionStatusValueId { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryRecordStatusValueId"/>
        [DataMember]
        public int? PersonEntryRecordStatusValueId { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryGroupLocationTypeValueId"/>
        [DataMember]
        public int? PersonEntryGroupLocationTypeValueId { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryCampusStatusValueId"/>
        public int? PersonEntryCampusStatusValueId { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryCampusTypeValueId"/>
        public int? PersonEntryCampusTypeValueId { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryFamilyAttributeGuid"/>
        [DataMember]
        public Guid? PersonEntryFamilyAttributeGuid { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntryPersonAttributeGuid"/>
        [DataMember]
        public Guid? PersonEntryPersonAttributeGuid { get; private set; }

        /// <inheritdoc cref="WorkflowActionForm.PersonEntrySpouseAttributeGuid"/>
        [DataMember]
        public Guid? PersonEntrySpouseAttributeGuid { get; private set; }

        #endregion Person entry related Entity Properties

        /// <summary>
        /// Gets the form attributes.
        /// </summary>
        /// <value>
        /// The form attributes.
        /// </value>
        public List<WorkflowActionFormAttributeCache> FormAttributes
        {
            get
            {
                var formAttributes = new List<WorkflowActionFormAttributeCache>();

                if ( _formAttributeIds == null )
                {
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
                }

                if ( _formAttributeIds == null )
                    return formAttributes;

                foreach ( var id in _formAttributeIds )
                {
                    var formAttribute = WorkflowActionFormAttributeCache.Get( id );
                    if ( formAttribute != null && formAttribute.Attribute != null )
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
        /// Gets the form person entry settings from either the WorkflowActionForm or WorkflowFormBuilderTemplate
        /// </summary>
        /// <param name="workflowFormBuilderTemplate">The workflow form builder template.</param>
        /// <returns></returns>
        public FormPersonEntrySettings GetFormPersonEntrySettings( WorkflowFormBuilderTemplateCache workflowFormBuilderTemplate )
        {
            var actionForm = this;

            FormPersonEntrySettings formPersonEntrySettings;
            if ( workflowFormBuilderTemplate != null )
            {
                formPersonEntrySettings = workflowFormBuilderTemplate.PersonEntrySettingsJson?.FromJsonOrNull<Rock.Workflow.FormBuilder.FormPersonEntrySettings>();
            }
            else
            {
                formPersonEntrySettings = new Rock.Workflow.FormBuilder.FormPersonEntrySettings
                {
                    Address = actionForm.PersonEntryAddressEntryOption,
                    AddressTypeValueId = actionForm.PersonEntryGroupLocationTypeValueId,
                    AutofillCurrentPerson = actionForm.PersonEntryAutofillCurrentPerson,
                    Birthdate = actionForm.PersonEntryBirthdateEntryOption,
                    CampusStatusValueId = actionForm.PersonEntryCampusStatusValueId,
                    CampusTypeValueId = actionForm.PersonEntryCampusTypeValueId,
                    ConnectionStatusValueId = actionForm.PersonEntryConnectionStatusValueId,
                    Email = actionForm.PersonEntryEmailEntryOption,
                    Gender = actionForm.PersonEntryGenderEntryOption,
                    HideIfCurrentPersonKnown = actionForm.PersonEntryHideIfCurrentPersonKnown,
                    MaritalStatus = actionForm.PersonEntryMaritalStatusEntryOption,
                    MobilePhone = actionForm.PersonEntryMobilePhoneEntryOption,
                    RecordStatusValueId = actionForm.PersonEntryRecordStatusValueId,
                    ShowCampus = actionForm.PersonEntryCampusIsVisible,
                    SpouseEntry = actionForm.PersonEntrySpouseEntryOption,
                    SpouseLabel = actionForm.PersonEntrySpouseLabel
                };
            }

            return formPersonEntrySettings;
        }

        /// <summary>
        /// Gets the AllowPersonEntry values form either the WorkflowActionForm or WorkflowFormBuilderTemplate
        /// </summary>
        /// <param name="workflowFormBuilderTemplate">The workflow form builder template.</param>
        /// <returns></returns>
        public bool GetAllowPersonEntry( WorkflowFormBuilderTemplateCache workflowFormBuilderTemplate )
        {
            if ( workflowFormBuilderTemplate != null )
            {
                return workflowFormBuilderTemplate.AllowPersonEntry;
            }
            else
            {
                return this.AllowPersonEntry;
            }
        }

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var workflowActionForm = entity as WorkflowActionForm;
            if ( workflowActionForm == null )
            {
                return;
            }

            this.Id = workflowActionForm.Id;
            this.ActionAttributeGuid = workflowActionForm.ActionAttributeGuid;
            this.Actions = workflowActionForm.Actions;
            this.AllowNotes = workflowActionForm.AllowNotes;
            this.AllowPersonEntry = workflowActionForm.AllowPersonEntry;
            this.Footer = workflowActionForm.Footer;
            this.ForeignGuid = workflowActionForm.ForeignGuid;
            this.ForeignKey = workflowActionForm.ForeignKey;
            this.Header = workflowActionForm.Header;
            this.IncludeActionsInNotification = workflowActionForm.IncludeActionsInNotification;
            this.NotificationSystemCommunicationId = workflowActionForm.NotificationSystemCommunicationId;
#pragma warning disable 612, 618
            this.NotificationSystemEmailId = workflowActionForm.NotificationSystemEmailId;
#pragma warning restore 612, 618
            this.PersonEntryAddressEntryOption = workflowActionForm.PersonEntryAddressEntryOption;
            this.PersonEntryGroupLocationTypeValueId = workflowActionForm.PersonEntryGroupLocationTypeValueId;

            this.PersonEntryCampusStatusValueId = workflowActionForm.PersonEntryCampusStatusValueId;
            this.PersonEntryCampusTypeValueId = workflowActionForm.PersonEntryCampusTypeValueId;

            this.PersonEntryAutofillCurrentPerson = workflowActionForm.PersonEntryAutofillCurrentPerson;
            this.PersonEntryBirthdateEntryOption = workflowActionForm.PersonEntryBirthdateEntryOption;
            this.PersonEntryCampusIsVisible = workflowActionForm.PersonEntryCampusIsVisible;
            this.PersonEntryConnectionStatusValueId = workflowActionForm.PersonEntryConnectionStatusValueId;
            this.PersonEntryGenderEntryOption = workflowActionForm.PersonEntryGenderEntryOption;
            this.PersonEntryEmailEntryOption = workflowActionForm.PersonEntryEmailEntryOption;
            this.PersonEntryFamilyAttributeGuid = workflowActionForm.PersonEntryFamilyAttributeGuid;
            this.PersonEntryHideIfCurrentPersonKnown = workflowActionForm.PersonEntryHideIfCurrentPersonKnown;
            this.PersonEntryMaritalStatusEntryOption = workflowActionForm.PersonEntryMaritalStatusEntryOption;
            this.PersonEntryMobilePhoneEntryOption = workflowActionForm.PersonEntryMobilePhoneEntryOption;
            this.PersonEntryPersonAttributeGuid = workflowActionForm.PersonEntryPersonAttributeGuid;
            this.PersonEntryPostHtml = workflowActionForm.PersonEntryPostHtml;
            this.PersonEntryPreHtml = workflowActionForm.PersonEntryPreHtml;
            this.PersonEntryRecordStatusValueId = workflowActionForm.PersonEntryRecordStatusValueId;
            this.PersonEntrySpouseAttributeGuid = workflowActionForm.PersonEntrySpouseAttributeGuid;
            this.PersonEntrySpouseEntryOption = workflowActionForm.PersonEntrySpouseEntryOption;
            this.PersonEntrySpouseLabel = workflowActionForm.PersonEntrySpouseLabel;
            this.Guid = workflowActionForm.Guid;
            this.ForeignId = workflowActionForm.ForeignId;

            // set formAttributeIds to null so it load them all at once on demand
            _formAttributeIds = null;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Removes a WorkflowActionForm from cache.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public new static void Remove( int id )
        {
            var actionForm = Get( id );
            if ( actionForm != null )
            {
                foreach ( var formAttribute in actionForm.FormAttributes )
                {
                    WorkflowActionFormAttributeCache.Remove( formAttribute.Id );
                }
            }

            Remove( id.ToString() );
        }

        #endregion

    }

}