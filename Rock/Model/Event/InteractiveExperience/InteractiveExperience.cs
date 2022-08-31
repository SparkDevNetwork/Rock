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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Enums.Event;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Interactive Experience.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "InteractiveExperience" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "3D90E693-476E-4DFC-B958-A28D1DD370BF" )]
    public partial class InteractiveExperience : Model<InteractiveExperience>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Name of the InteractiveExperience. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the InteractiveExperience.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the IsActive flag for the <see cref="Rock.Model.InteractiveExperience"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> for the IsActive flag.
        /// </value>
        [Required]
        [DataMember]
        public Boolean IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Description of the <see cref="Rock.Model.InteractiveExperience"/>
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> for the Description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Public Label of the InteractiveExperience.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Public Label of the InteractiveExperience.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string PublicLabel { get; set; }

        /// <summary>
        /// Gets or sets the photo binary file identifier.
        /// </summary>
        /// <value>
        /// The photo binary file identifier.
        /// </value>
        [DataMember]
        public int? PhotoBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the push notification type.
        /// </summary>
        /// <value>
        /// The push notification type.
        /// </value>
        [DataMember]
        public InteractiveExperiencePushNotificationType PushNotificationType { get; set; }

        /// <summary>
        /// Gets or sets the welcome title.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the welcome title.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string WelcomeTitle { get; set; }

        /// <summary>
        /// Gets or sets the welcome message.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the welcome message.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
        public string WelcomeMessage { get; set; }

        /// <summary>
        /// Gets or sets the welcome header image binary file identifier.
        /// </summary>
        /// <value>
        /// The welcome header image binary file identifier.
        /// </value>
        [DataMember]
        public int? WelcomeHeaderImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the no action title.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the no action title.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string NoActionTitle { get; set; }

        /// <summary>
        /// Gets or sets the no action message.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the no action message.
        /// </value>
        [MaxLength( 1000 )]
        [DataMember]
        public string NoActionMessage { get; set; }

        /// <summary>
        /// Gets or sets the no action header image binary file identifier.
        /// </summary>
        /// <value>
        /// The no action header image binary file identifier.
        /// </value>
        [DataMember]
        public int? NoActionHeaderImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the background color of the action.
        /// </summary>
        /// <value>
        /// The background color of the action.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string ActionBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the text color of the action.
        /// </summary>
        /// <value>
        /// The text color of the action.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string ActionTextColor { get; set; }

        /// <summary>
        /// Gets or sets the primary button color of the action.
        /// </summary>
        /// <value>
        /// The primary button color of the action.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string ActionPrimaryButtonColor { get; set; }

        /// <summary>
        /// Gets or sets the primary button text color of the action.
        /// </summary>
        /// <value>
        /// The primary button text color of the action.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string ActionPrimaryButtonTextColor { get; set; }

        /// <summary>
        /// Gets or sets the secondary button color of the action.
        /// </summary>
        /// <value>
        /// The secondary button color of the action.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string ActionSecondaryButtonColor { get; set; }

        /// <summary>
        /// Gets or sets the secondary button text color of the action.
        /// </summary>
        /// <value>
        /// The secondary button text color of the action.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string ActionSecondaryButtonTextColor { get; set; }

        /// <summary>
        /// Gets or sets the action background image binary file identifier.
        /// </summary>
        /// <value>
        /// The action background image binary file identifier.
        /// </value>
        [DataMember]
        public int? ActionBackgroundImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the custom css for the action.
        /// </summary>
        /// <value>
        /// The custom css for the action.
        /// </value>
        [DataMember]
        public string ActionCustomCss { get; set; }

        /// <summary>
        /// Gets or sets the background color for the audience.
        /// </summary>
        /// <value>
        /// The background color for the audience.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string AudienceBackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the text color for the audience.
        /// </summary>
        /// <value>
        /// The text color for the audience.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string AudienceTextColor { get; set; }

        /// <summary>
        /// Gets or sets the primary color for the audience.
        /// </summary>
        /// <value>
        /// The primary color for the audience.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string AudiencePrimaryColor { get; set; }

        /// <summary>
        /// Gets or sets the secondary color for the audience.
        /// </summary>
        /// <value>
        /// The secondary color for the audience.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string AudienceSecondaryColor { get; set; }

        /// <summary>
        /// Gets or sets the accent color for the audience.
        /// </summary>
        /// <value>
        /// The accent color for the audience.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string AudienceAccentColor { get; set; }

        /// <summary>
        /// Gets or sets the audience background image binary file identifier.
        /// </summary>
        /// <value>
        /// The audience background image binary file identifier.
        /// </value>
        [DataMember]
        public int? AudienceBackgroundImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the custom css for the audience.
        /// </summary>
        /// <value>
        /// The custom css for the audience.
        /// </value>
        [DataMember]
        public string AudienceCustomCss { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the photo binary file.
        /// </summary>
        /// <value>
        /// The photo binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile PhotoBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the welcome header image binary file.
        /// </summary>
        /// <value>
        /// The welcome header image binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile WelcomeHeaderImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the no action header image binary file.
        /// </summary>
        /// <value>
        /// The no action header image binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile NoActionHeaderImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the action background image binary file.
        /// </summary>
        /// <value>
        /// The action background image binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile ActionBackgroundImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the audience background image binary file.
        /// </summary>
        /// <value>
        /// The audience background image binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile AudienceBackgroundImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperienceSchedule">InteractiveExperienceSchedules</see>  for this Interactive Experience.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.InteractiveExperienceSchedule" /> InteractiveExperienceSchedules for this Interactive Experience.
        /// </value>
        [DataMember]
        public virtual ICollection<InteractiveExperienceSchedule> InteractiveExperienceSchedules
        {
            get { return _interactiveExperienceSchedules ?? ( _interactiveExperienceSchedules = new Collection<InteractiveExperienceSchedule>() ); }
            set { _interactiveExperienceSchedules = value; }
        }
        private ICollection<InteractiveExperienceSchedule> _interactiveExperienceSchedules;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperienceAction">InteractiveExperienceActions</see>  for this Interactive Experience.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.InteractiveExperienceAction" /> InteractiveExperienceActions for this Interactive Experience.
        /// </value>
        [DataMember]
        public virtual ICollection<InteractiveExperienceAction> InteractiveExperienceActions
        {
            get { return _interactiveExperienceActions ?? ( _interactiveExperienceActions = new Collection<InteractiveExperienceAction>() ); }
            set { _interactiveExperienceActions = value; }
        }
        private ICollection<InteractiveExperienceAction> _interactiveExperienceActions;

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class InteractiveExperienceConfiguration : EntityTypeConfiguration<InteractiveExperience>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveExperienceConfiguration"/> class.
        /// </summary>
        public InteractiveExperienceConfiguration()
        {
            HasOptional( ie => ie.PhotoBinaryFile ).WithMany().HasForeignKey( ie => ie.PhotoBinaryFileId ).WillCascadeOnDelete( false );
            HasOptional( ie => ie.WelcomeHeaderImageBinaryFile ).WithMany().HasForeignKey( ie => ie.WelcomeHeaderImageBinaryFileId ).WillCascadeOnDelete( false );
            HasOptional( ie => ie.NoActionHeaderImageBinaryFile ).WithMany().HasForeignKey( ie => ie.NoActionHeaderImageBinaryFileId ).WillCascadeOnDelete( false );
            HasOptional( ie => ie.ActionBackgroundImageBinaryFile ).WithMany().HasForeignKey( ie => ie.ActionBackgroundImageBinaryFileId ).WillCascadeOnDelete( false );
            HasOptional( ie => ie.AudienceBackgroundImageBinaryFile ).WithMany().HasForeignKey( ie => ie.AudienceBackgroundImageBinaryFileId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}