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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
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
        /// Gets or sets the notification system email identifier.
        /// </summary>
        /// <value>
        /// The notification system email identifier.
        /// </value>
        [DataMember]
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
        /// Gets or sets the allow notes.
        /// </summary>
        /// <value>
        /// The allow notes.
        /// </value>
        [DataMember]
        public bool? AllowNotes { get; set; }

        #endregion

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
        [LavaInclude]
        public virtual SystemEmail NotificationSystemEmail {get;set;}

        /// <summary>
        /// Gets or sets the buttons.
        /// </summary>
        /// <value>
        /// The buttons.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual List<LiquidButton> Buttons
        {
            get
            {
                return GetActionButtons( Actions );
            }
        }

        #endregion

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

        /// <summary>
        /// Special class for adding a button field to liquid properties
        /// </summary>
        [DotLiquid.LiquidType( "Name", "Html", "EmailHtml" )]
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
    }

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
            this.HasOptional( f => f.NotificationSystemEmail ).WithMany().HasForeignKey( f => f.NotificationSystemEmailId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}

