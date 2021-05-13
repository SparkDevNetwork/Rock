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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents an <see cref="Rock.Model.WorkflowActionForm"/> (action or task) that is performed as part of a <see cref="Rock.Model.WorkflowActionForm"/>.
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowActionForm" )]
    [DataContract]
    public partial class WorkflowActionForm : Model<WorkflowActionForm>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the notification system communication identifier.
        /// </summary>
        /// <value>
        /// The notification system communication identifier.
        /// </value>
        [DataMember]
        public int? NotificationSystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the notification system email identifier.
        /// </summary>
        /// <value>
        /// The notification system email identifier.
        /// </value>
        [DataMember]
        [Obsolete( "Use NotificationSystemCommunicationId instead." )]
        [RockObsolete( "1.10" )]
        public int? NotificationSystemEmailId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include actions in notification].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include actions in notification]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IncludeActionsInNotification { get; set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        [DataMember]
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        /// <value>
        /// The footer.
        /// </value>
        [DataMember]
        public string Footer { get; set; }

        /// <summary>
        /// Gets or sets the delimited list of action buttons and actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        [MaxLength( 2000 )]
        [DataMember]
        public string Actions { get; set; }

        /// <summary>
        /// An optional text attribute that will be updated with the action that was selected
        /// </summary>
        /// <value>
        /// The action attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? ActionAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets whether Notes can be entered
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow notes entry]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool? AllowNotes { get; set; }

        #endregion Entity Properties

        #region Person entry related Entity Properties

        /// <summary>
        /// Gets or sets a value indicating whether a new person (and spouse) can be added
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow person entry]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowPersonEntry { get; set; } = false;

        /// <summary>
        /// Gets or sets the person entry preHTML.
        /// </summary>
        /// <value>
        /// The person entry preHTML.
        /// </value>
        [DataMember]
        public string PersonEntryPreHtml { get; set; }

        /// <summary>
        /// Gets or sets the person entry post HTML.
        /// </summary>
        /// <value>
        /// The person entry post HTML.
        /// </value>
        [DataMember]
        public string PersonEntryPostHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [person entry show campus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry show campus]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryCampusIsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Person Entry should auto-fill with the CurrentPerson
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry auto-fill current person]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryAutofillCurrentPerson { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether Person Entry should be hidden if the CurrentPerson is known
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry hide if current person known]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryHideIfCurrentPersonKnown { get; set; } = false;

        /// <summary>
        /// Gets or sets the person entry spouse entry option.
        /// </summary>
        /// <value>
        /// The person entry spouse entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntrySpouseEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry gender entry option.
        /// </summary>
        /// <value>
        /// The person entry gender entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryGenderEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Required;

        /// <summary>
        /// Gets or sets the person entry email entry option.
        /// </summary>
        /// <value>
        /// The person entry email entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryEmailEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Required;

        /// <summary>
        /// Gets or sets the person entry mobile phone entry option.
        /// </summary>
        /// <value>
        /// The person entry mobile phone entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryMobilePhoneEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry birthdate entry option.
        /// </summary>
        /// <value>
        /// The person entry birthdate entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryBirthdateEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry address entry option.
        /// </summary>
        /// <value>
        /// The person entry address entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryAddressEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry marital status entry option.
        /// </summary>
        /// <value>
        /// The person entry marital entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryMaritalStatusEntryOption { get; set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry spouse label.
        /// </summary>
        /// <value>
        /// The person entry spouse label.
        /// </value>
        [MaxLength( 50 )]
        [DataMember( IsRequired = false )]
        public string PersonEntrySpouseLabel { get; set; } = "Spouse";

        /// <summary>
        /// Gets or sets the person entry connection status value identifier.
        /// </summary>
        /// <value>
        /// The person entry connection status value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS )]
        public int? PersonEntryConnectionStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the person entry record status value identifier.
        /// </summary>
        /// <value>
        /// The person entry record status value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS )]
        public int? PersonEntryRecordStatusValueId { get; set; }

        /// <summary>
        /// Gets or sets the person entry address type value identifier.
        /// </summary>
        /// <value>
        /// The person entry address type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( Rock.SystemGuid.DefinedType.GROUP_LOCATION_TYPE )]
        public int? PersonEntryGroupLocationTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the person entry person workflow attribute unique identifier. (The one used to set the Added/Edited Person to)
        /// </summary>
        /// <value>
        /// The person entry person attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? PersonEntryPersonAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the person entry spouse workflow attribute unique identifier.  (The one used to set the Added/Edited Person's Spouse to)
        /// </summary>
        /// <value>
        /// The person entry spouse attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? PersonEntrySpouseAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the person entry family attribute unique identifier. (The one used to set the Added/Edited Person's Family to)
        /// </summary>
        /// <value>
        /// The person entry family attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? PersonEntryFamilyAttributeGuid { get; set; }

        #endregion Person entry related Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the form attributes.
        /// </summary>
        /// <value>
        /// The form attributes.
        /// </value>
        [DataMember]
        public virtual ICollection<WorkflowActionFormAttribute> FormAttributes
        {
            get { return _formAttributes ?? ( _formAttributes = new Collection<WorkflowActionFormAttribute>() ); }
            set { _formAttributes = value; }
        }

        private ICollection<WorkflowActionFormAttribute> _formAttributes;

        /// <summary>
        /// Gets or sets the notification system email.
        /// </summary>
        /// <value>
        /// The notification system email.
        /// </value>
        [LavaVisible]
        [Obsolete( "Use NotificationSystemCommunication instead." )]
        [RockObsolete( "1.10" )]
        public virtual SystemEmail NotificationSystemEmail { get; set; }

        /// <summary>
        /// Gets or sets the notification system communication.
        /// </summary>
        /// <value>
        /// The notification system communication.
        /// </value>
        [LavaVisible]
        public virtual SystemCommunication NotificationSystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the person entry connection status value
        /// </summary>
        /// <value>
        /// The person entry connection status value
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonEntryConnectionStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the person entry record status value identifier.
        /// </summary>
        /// <value>
        /// The person entry record status value identifier.
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonEntryRecordStatusValue { get; set; }

        /// <summary>
        /// Gets or sets the person entry address type value identifier.
        /// </summary>
        /// <value>
        /// The person entry address type value identifier.
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonEntryGroupLocationTypeValue { get; set; }

        #endregion Virtual Properties

        #region Other properties

        /// <summary>
        /// Gets or sets the buttons.
        /// </summary>
        /// <value>
        /// The buttons.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual List<LiquidButton> Buttons
        {
            get
            {
                return GetActionButtons( Actions );
            }
        }

        #endregion Other properties

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionForm"/> class.
        /// </summary>
        public WorkflowActionForm()
        {
            IncludeActionsInNotification = true;
        }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return WorkflowActionFormCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            WorkflowActionFormCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion

        #region Action Buttons

        /// <summary>
        /// Special class for adding a button field to liquid properties
        /// </summary>
        [DotLiquid.LiquidType( "Name", "Html", "EmailHtml" )]
        [LavaType( "Name", "Html", "EmailHtml" )]        
        public class LiquidButton
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the HTML.
            /// </summary>
            /// <value>
            /// The HTML.
            /// </value>
            public string Html { get; set; }

            /// <summary>
            /// Gets or sets the Email HTML.
            /// </summary>
            /// <value>
            /// The Email HTML.
            /// </value>
            public string EmailHtml { get; set; }
        }

        /// <summary>
        /// Gets the action buttons.
        /// </summary>
        /// <param name="actions">The actions.</param>
        /// <returns></returns>
        public static List<LiquidButton> GetActionButtons( string actions )
        {
            var buttonList = new List<LiquidButton>();

            foreach ( var actionButton in actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var button = new LiquidButton();
                var details = actionButton.Split( new char[] { '^' } );
                if ( details.Length > 0 )
                {
                    button.Name = details[0];

                    if ( details.Length > 1 )
                    {
                        var definedValue = DefinedValueCache.Get( details[1].AsGuid() );
                        if ( definedValue != null )
                        {
                            button.Html = definedValue.GetAttributeValue( "ButtonHTML" );
                            button.EmailHtml = definedValue.GetAttributeValue( "ButtonEmailHTML" );
                        }
                    }
                }

                buttonList.Add( button );
            }

            return buttonList;
        }

        #endregion
    }

    /// <summary>
    /// Represents an action that can be selected by the user on a workflow form.
    /// </summary>
    public class WorkflowActionFormUserAction
    {
        /// <summary>
        /// The name of the action.
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// The unique identifier of the workflow activity that will be activated if this action is selected.
        /// </summary>
        public string ActivateActivityTypeGuid { get; set; }

        /// <summary>
        /// The type of action.
        /// </summary>
        public string ButtonTypeGuid { get; set; }

        /// <summary>
        /// The response message displayed to the user if the action is performed.
        /// </summary>
        public string ResponseText { get; set; }

        /// <summary>
        /// A flag indicating if this form action triggers validation of the form fields.
        /// </summary>
        public bool CausesValidation { get; set; }

        /// <summary>
        /// Returns a collection of Workflow Form Actions from a Uri Encoded string.
        /// </summary>
        /// <param name="encodedString"></param>
        /// <returns></returns>
        /// <remarks>
        /// The Uri Encoded string is used to serialize a collection of actions for portability and storage.
        /// </remarks>
        public static List<WorkflowActionFormUserAction> FromUriEncodedString( string encodedString )
        {
            Guid buttonCancelGuid = Rock.SystemGuid.DefinedValue.BUTTON_HTML_CANCEL.AsGuid();

            var buttons = new List<WorkflowActionFormUserAction>();

            var buttonList = Rock.Utility.RockSerializableList.FromUriEncodedString( encodedString, StringSplitOptions.RemoveEmptyEntries );

            // Without any other way of determining this, assume that the built-in Cancel button is the only action that does not cause validation.
            var nonValidationButtonList = new List<Guid> { buttonCancelGuid };

            foreach ( var buttonDefinitionText in buttonList.List )
            {
                var button = new WorkflowActionFormUserAction();

                string[] nameValueResponse = buttonDefinitionText.Split( new char[] { '^' } );

                // Button Name
                button.ActionName = nameValueResponse.Length > 0 ? nameValueResponse[0] : string.Empty;

                // Button Type
                if ( nameValueResponse.Length > 1 )
                {
                    button.ButtonTypeGuid = nameValueResponse[1];
                }

                if ( button.ButtonTypeGuid.IsNullOrWhiteSpace() )
                {
                    button.ButtonTypeGuid = Rock.SystemGuid.DefinedValue.BUTTON_HTML_PRIMARY;
                }

                // Determine if the button causes form validation.                    
                button.CausesValidation = !nonValidationButtonList.Contains( button.ButtonTypeGuid.AsGuid() );

                if ( button.ButtonTypeGuid.AsGuid() == buttonCancelGuid )
                {
                    button.CausesValidation = false;
                }
                else
                {
                    // By default, assume that an action button triggers validation of the form.
                    button.CausesValidation = true;
                }

                // Button Activity
                button.ActivateActivityTypeGuid = nameValueResponse.Length > 2 ? nameValueResponse[2] : string.Empty;

                // Response Text
                button.ResponseText = nameValueResponse.Length > 3 ? nameValueResponse[3] : string.Empty;

                buttons.Add( button );
            }

            return buttons;
        }
    }

    #region Enums

    /// <summary>
    /// 
    /// </summary>
    public enum WorkflowActionFormPersonEntryOption
    {
        /// <summary>
        /// Don't show the control
        /// </summary>
        [Description( "Hide" )]
        Hidden = 0,

        /// <summary>
        /// Control is visible, but a value is not required
        /// </summary>
        [Description( "Optional" )]
        Optional = 1,

        /// <summary>
        /// Control is visible, and a value is required
        /// </summary>
        [Description( "Required" )]
        Required = 2,
    }

    #endregion Enums

    #region Entity Configuration

    /// <summary>
    /// Workflow Form Configuration class.
    /// </summary>
    public partial class WorkflowActionFormConfiguration : EntityTypeConfiguration<WorkflowActionForm>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionFormConfiguration"/> class.
        /// </summary>
        public WorkflowActionFormConfiguration()
        {
            this.HasOptional( f => f.NotificationSystemCommunication ).WithMany().HasForeignKey( f => f.NotificationSystemCommunicationId ).WillCascadeOnDelete( false );

#pragma warning disable CS0618 // Type or member is obsolete
            this.HasOptional( f => f.NotificationSystemEmail ).WithMany().HasForeignKey( f => f.NotificationSystemEmailId ).WillCascadeOnDelete( false );
#pragma warning restore CS0618 // Type or member is obsolete

            this.HasOptional( f => f.PersonEntryConnectionStatusValue ).WithMany().HasForeignKey( f => f.PersonEntryConnectionStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( f => f.PersonEntryRecordStatusValue ).WithMany().HasForeignKey( f => f.PersonEntryRecordStatusValueId ).WillCascadeOnDelete( false );
            this.HasOptional( f => f.PersonEntryGroupLocationTypeValue ).WithMany().HasForeignKey( f => f.PersonEntryGroupLocationTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}