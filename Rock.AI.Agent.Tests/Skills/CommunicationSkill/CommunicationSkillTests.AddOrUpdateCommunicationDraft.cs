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

using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.AI.Agent.Utilities.CommunicationSkill;
using Rock.Configuration;
using Rock.Enums.AI.Agent;
using Rock.Model;
using Rock.Tests.Shared.TestAccess.AI.Agent;
using Rock.Tests.Shared.TestFramework;
using Rock.Utility;

namespace Rock.AI.Agent.Tests.Skills.CommunicationSkill;

public partial class CommunicationSkillTests
{
    #region AddOrUpdateCommunicationDraft

    [TestMethod]
    public void AddOrUpdateCommunicationDraft_ExistingDraftByOtherPerson_ReturnsError()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var currentPerson = MockData.CreatePerson( rockContext, "Current", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        var draft = SeedDraftCommunication( rockContext, 400, sender );

        var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, currentPerson ) );

        var result = skill.AddOrUpdateCommunicationDraft(
            recipientIdKey: IdHasher.Instance.GetHash( recipient.Id ),
            communicationType: AgentCommunicationType.Email,
            draftedSubject: "Updated subject",
            draftedBody: "Updated body",
            existingDraftIdKey: IdHasher.Instance.GetHash( draft.Id ) );

        Assert.AreEqual( ToolStatus.Error, result.GetStatus() );
        Assert.IsTrue( result.GetErrorMessages().Any( m => m.Contains( "not authorized to edit this draft" ) ) );
        Assert.AreEqual( "Draft", draft.Subject );
    }

    [TestMethod]
    public void AddOrUpdateCommunicationDraft_ExistingDraftBySender_UpdatesDraft()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        recipient.Email = "recipient@example.org";
        var draft = SeedDraftCommunication( rockContext, 400, sender );

        var restoreEmailMedium = ActivateEmailMedium( rockContext );

        try
        {
            var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, sender ) );

            var result = skill.AddOrUpdateCommunicationDraft(
                recipientIdKey: IdHasher.Instance.GetHash( recipient.Id ),
                communicationType: AgentCommunicationType.Email,
                draftedSubject: "Updated subject",
                draftedBody: "Updated body",
                existingDraftIdKey: IdHasher.Instance.GetHash( draft.Id ) );

            Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
            Assert.AreEqual( "Updated subject", draft.Subject );
            Assert.AreEqual( "Updated body", draft.Message );
        }
        finally
        {
            restoreEmailMedium();
        }
    }

    [TestMethod]
    public void AddOrUpdateCommunicationDraft_NewDraft_AddsTransientCommunication()
    {
        using var scope = TestHelper.CreateScopedRockApp();
        var rockContext = scope.App.CreateRockContext();

        var sender = MockData.CreatePerson( rockContext, "Sender", "Person" );
        var recipient = MockData.CreatePerson( rockContext, "Recipient", "Person" );
        recipient.Email = "recipient@example.org";

        var restoreEmailMedium = ActivateEmailMedium( rockContext );

        try
        {
            var skill = CreateSkill( scope.App, CreateRequestContext( rockContext, sender ) );

            var result = skill.AddOrUpdateCommunicationDraft(
                recipientIdKey: IdHasher.Instance.GetHash( recipient.Id ),
                communicationType: AgentCommunicationType.Email,
                draftedSubject: "New subject",
                draftedBody: "New body" );

            Assert.AreEqual( ToolStatus.Success, result.GetStatus() );
            Assert.IsTrue( rockContext.Set<Rock.Model.Communication>().Any( c => c.Subject == "New subject"
                && c.Status == CommunicationStatus.Transient
                && c.SenderPersonAliasId == sender.PrimaryAliasId ) );
        }
        finally
        {
            restoreEmailMedium();
        }
    }

    /// <summary>
    /// Makes the email medium, and an SMTP transport for it, report as active
    /// so the draft tool can build an email communication.
    /// </summary>
    /// <remarks>
    /// Extension components are discovered by their containers but are inactive
    /// in the mocked environment, because their attributes are never loaded from
    /// the database. This gives the components "Active" attributes that default
    /// to true, and points the medium at the transport, the same way the
    /// reporting skill tests activate a data select component. The components
    /// are shared singletons, so the returned action must be called to restore
    /// their original attributes.
    /// </remarks>
    /// <param name="rockContext">The mocked context to seed.</param>
    /// <returns>An action that restores the components' original attributes.</returns>
    private static System.Action ActivateEmailMedium( Data.RockContext rockContext )
    {
        // The tool finds the medium by its well known entity type guid, and the
        // medium finds its transport by the transport's entity type guid. These
        // are seeded before the containers are first touched, because creating
        // a component caches its entity type, and would otherwise create one
        // with a random guid.
        var emailMediumEntityType = SeedComponentEntityType( rockContext, 9100, typeof( Rock.Communication.Medium.Email ), Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
        var smtpTransportEntityType = SeedComponentEntityType( rockContext, 9101, typeof( Rock.Communication.Transport.SMTP ), new System.Guid( "a9101000-0000-4000-8000-000000009101" ) );

        var emailMedium = Rock.Communication.MediumContainer.Instance.Components.Values
            .Select( c => c.Value )
            .First( c => c.GetType() == typeof( Rock.Communication.Medium.Email ) );

        var smtpTransport = Rock.Communication.TransportContainer.Instance.Components.Values
            .Select( c => c.Value )
            .First( c => c.GetType() == typeof( Rock.Communication.Transport.SMTP ) );

        var booleanFieldType = MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.BOOLEAN.AsGuid(), "Boolean", "Rock.Field.Types.BooleanFieldType" );
        var textFieldType = MockData.CreateFieldType( rockContext, Rock.SystemGuid.FieldType.TEXT.AsGuid(), "Text", "Rock.Field.Types.TextFieldType" );

        var originalMediumAttributes = emailMedium.Attributes;
        var originalMediumAttributeValues = emailMedium.AttributeValues;
        var originalTransportAttributes = smtpTransport.Attributes;
        var originalTransportAttributeValues = smtpTransport.AttributeValues;

        emailMedium.AttributeValues = new System.Collections.Generic.Dictionary<string, Rock.Web.Cache.AttributeValueCache>();
        emailMedium.Attributes = new System.Collections.Generic.Dictionary<string, Rock.Web.Cache.AttributeCache>
        {
            { "Active", SeedComponentAttribute( rockContext, 9110, emailMediumEntityType.Id, "Active", booleanFieldType.Id, "True" ) },
            { "TransportContainer", SeedComponentAttribute( rockContext, 9111, emailMediumEntityType.Id, "TransportContainer", textFieldType.Id, smtpTransportEntityType.Guid.ToString() ) }
        };

        smtpTransport.AttributeValues = new System.Collections.Generic.Dictionary<string, Rock.Web.Cache.AttributeValueCache>();
        smtpTransport.Attributes = new System.Collections.Generic.Dictionary<string, Rock.Web.Cache.AttributeCache>
        {
            { "Active", SeedComponentAttribute( rockContext, 9112, smtpTransportEntityType.Id, "Active", booleanFieldType.Id, "True" ) }
        };

        return () =>
        {
            emailMedium.Attributes = originalMediumAttributes;
            emailMedium.AttributeValues = originalMediumAttributeValues;
            smtpTransport.Attributes = originalTransportAttributes;
            smtpTransport.AttributeValues = originalTransportAttributeValues;
        };
    }

    /// <summary>
    /// Seeds the entity type for an extension component, so it resolves to the
    /// specified guid, and returns it.
    /// </summary>
    private static EntityType SeedComponentEntityType( Data.RockContext rockContext, int id, System.Type componentType, System.Guid guid )
    {
        var entityType = new EntityType
        {
            Id = id,
            Guid = guid,
            Name = componentType.FullName,
            FriendlyName = componentType.Name
        };

        rockContext.Set<EntityType>().Add( entityType );

        return entityType;
    }

    /// <summary>
    /// Seeds an attribute for an extension component with the specified
    /// default value, and returns it from the cache.
    /// </summary>
    private static Rock.Web.Cache.AttributeCache SeedComponentAttribute( Data.RockContext rockContext, int id, int entityTypeId, string key, int fieldTypeId, string defaultValue )
    {
        var attribute = new Rock.Model.Attribute
        {
            Id = id,
            Guid = System.Guid.NewGuid(),
            Key = key,
            Name = key,
            EntityTypeId = entityTypeId,
            FieldTypeId = fieldTypeId,
            DefaultValue = defaultValue,
            IsActive = true
        };

        rockContext.Set<Rock.Model.Attribute>().Add( attribute );

        return Rock.Web.Cache.AttributeCache.Get( attribute.Guid, rockContext );
    }

    #endregion
}
