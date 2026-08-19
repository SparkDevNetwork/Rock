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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.WorkflowBuilderSkill;
using Rock.Field;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

/// <summary>
/// Authors Rock workflow types: discovering the available actions and their
/// settings, then creating, editing, and removing workflow structure.
/// </summary>
/// <remarks>
/// <para>
/// This is distinct from <see cref="WorkflowSkill"/>, which <em>executes</em> a
/// configured allow-list of workflow types. This skill authors them, and is
/// deliberately not constrained by that allow-list. The two overlap in name only;
/// do not merge them.
/// </para>
/// <para>
/// There is no transaction across tool calls. Each write opens its own context
/// and saves independently, so a failed call can leave a workflow half built. The
/// skill does not try to solve that. The agent recovers by reading the current
/// state back with GetWorkflowType and continuing from what is actually there,
/// which is why that tool returns the whole tree in one call.
/// </para>
/// </remarks>
[Description( "Provides the ability to author Rock workflow types: discovering actions and their settings, and creating, editing, and removing workflow structure.\n\nIdentifiers: every parameter in this skill takes an idKey, and the skill converts it to whatever Rock stores internally. Never put a guid in a parameter. A few values are different, because what you send is written into Rock's own configuration unchanged. Those must hold a record's guid rather than its idKey when they reference another record, and each one says so in its own description." )]
[AgentSkillGuid( "A74514DD-9955-49D6-8DC3-A33033797B0A" )]
[EntityTypeGuid( "7A9C6D45-947B-4B32-A09E-23718F0C8A08" )]
internal sealed partial class WorkflowBuilderSkill : AgentSkillComponent
{
    #region Constants

    /// <summary>
    /// The setting keys every action inherits from the component base class and
    /// which do nothing on a workflow action. Hidden from reads and refused on
    /// writes, so an agent never sees them and cannot be misled by them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Rock.Extension.Component"/> declares both as attributes, and
    /// <see cref="Rock.Workflow.ActionComponent"/> overrides both to ignore the
    /// stored value: <c>Order</c> always returns 0 and <c>IsActive</c> always
    /// returns true. Rock's own comments on those overrides say ordering lives on
    /// <see cref="WorkflowActionType.Order"/> and activation on the action type.
    /// </para>
    /// <para>
    /// <c>Order</c> is the dangerous one. It reads as though it positions the
    /// action, so a caller trying to reorder a workflow writes it, the write
    /// succeeds, and nothing moves.
    /// </para>
    /// <para>
    /// Hiding them is not the same as not storing them. <c>Order</c> is declared
    /// required with no default, so a value still has to exist or saving any
    /// setting fails; see SeedVestigialSettings in AddOrUpdateWorkflowActionType.
    /// </para>
    /// </remarks>
    private static readonly HashSet<string> VestigialSettingKeys =
        new HashSet<string>( StringComparer.OrdinalIgnoreCase ) { "Active", "Order" };

    /// <summary>
    /// Gets the values a setting accepts, when its field type keeps a fixed list.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 8/18/26 - CLAUDE
    ///
    /// Read from the attribute's own configuration rather than from the field type,
    /// because the framework's field hints are implemented by only a handful of field
    /// types and the select family is not among them. That left the settings tool
    /// promising "allowed values" while reporting none for exactly the settings whose
    /// values cannot be guessed.
    ///
    /// Reason: The select field types describe their values in configuration and
    /// nothing was reading it.
    /// </para>
    /// <para>
    /// Parsing is left to Rock's own helper rather than done here, because the
    /// configuration has three forms and only one of them is obvious. It is a comma
    /// separated list where each entry is either <c>value^label</c> or a bare value
    /// serving as both, and it is never pipe separated. But it may instead hold a SQL
    /// SELECT returning Value and Text columns, optionally with Lava in it, which a
    /// naive split would turn into nonsense options. Roughly a quarter of the select
    /// settings in a normal Rock database are that third form.
    /// </para>
    /// </remarks>
    /// <param name="attribute">The setting to describe.</param>
    /// <returns>The values, or <c>null</c> when the field type does not keep a list.</returns>
    private static List<ListItemBag> GetSelectableValues( AttributeCache attribute )
    {
        if ( attribute?.ConfigurationValues == null || !attribute.ConfigurationValues.ContainsKey( "values" ) )
        {
            return null;
        }

        Dictionary<string, string> configuredValues;

        try
        {
            configuredValues = Rock.Field.Helper.GetConfiguredValues( attribute.ConfigurationValues );
        }
        catch
        {
            // Intentionally swallowed: a SQL backed list runs a query, and a list this
            // code cannot read is only a list it cannot describe or check against. The
            // caller treats that the same as a field type that keeps no list, which
            // leaves the setting exactly as permissive as it was before.
            return null;
        }

        if ( configuredValues == null || !configuredValues.Any() )
        {
            return null;
        }

        return configuredValues
            .Select( v => new ListItemBag { Value = v.Key, Text = v.Value } )
            .ToList();
    }

    /// <summary>
    /// The point at which a single setting value, form header, or form footer is
    /// clipped by <see cref="GetWorkflowType"/>.
    /// </summary>
    /// <remarks>
    /// The purpose of the clipped text is recognition, not reading: enough to tell
    /// which template is in a setting, not enough to review it. A larger figure
    /// would let a thirty action workflow reach tens of kilobytes, which is the
    /// case worth avoiding. <see cref="GetWorkflowActionType"/> returns one
    /// action's values whole, which is what makes clipping safe.
    /// </remarks>
    private const int MaximumValueLength = 500;

    /// <summary>
    /// How many instance rows are removed per save when a delete has to clear
    /// history by hand. Matches Rock's own workflow type block.
    /// </summary>
    private const int InstanceDeleteBatchSize = 100;

    /// <summary>
    /// The class prefix current Rock themes use for icons. A guessed icon class
    /// renders as nothing and raises no error, so a value that does not start with
    /// this is worth reporting back.
    /// </summary>
    private const string TablerIconPrefix = "ti ti-";

    #endregion

    #region Fields

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private readonly ILogger _logger;

    #endregion

    #region Constructors

    /// <summary>
    /// The constructor for the Workflow Builder Skill.
    /// </summary>
    /// <param name="logger">Logger for diagnostics and error reporting.</param>
    public WorkflowBuilderSkill( ILogger<WorkflowBuilderSkill> logger )
    {
        _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    #endregion

    #region Shared Helpers

    /// <summary>
    /// Clips a value that is too long to include in full, reporting whether it
    /// was clipped.
    /// </summary>
    /// <param name="value">The value to clip.</param>
    /// <param name="isTruncated">Set to <c>true</c> when the value was clipped, otherwise <c>null</c> so it is omitted from the result.</param>
    /// <returns>The value, clipped to <see cref="MaximumValueLength"/> characters.</returns>
    private static string ClipValue( string value, out bool? isTruncated )
    {
        if ( value == null || value.Length <= MaximumValueLength )
        {
            isTruncated = null;

            return value;
        }

        isTruncated = true;

        return value.Substring( 0, MaximumValueLength );
    }

    /// <summary>
    /// Places an item among its ordered siblings and renumbers the whole set.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Callers position by naming a neighbour rather than by supplying a number,
    /// because a raw order value is only meaningful against the current set and a
    /// caller working from an earlier read may hold stale numbers. Naming the
    /// neighbour survives anything that shifted in between.
    /// </para>
    /// <para>
    /// The whole set is renumbered from zero on every call. Leaving gaps would work
    /// until two inserts landed on the same number, and the sets here are small
    /// enough that rewriting them costs nothing.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSibling">The type being ordered.</typeparam>
    /// <param name="orderedSiblings">Every sibling including <paramref name="item"/>, in their current order.</param>
    /// <param name="item">The item being placed.</param>
    /// <param name="placement">Where the item should go, resolved from the caller's parameters.</param>
    /// <param name="getId">Reads a sibling's identifier.</param>
    /// <param name="setOrder">Writes a sibling's order.</param>
    private static void PlaceAmongSiblings<TSibling>(
        List<TSibling> orderedSiblings,
        TSibling item,
        SiblingPlacement placement,
        Func<TSibling, int> getId,
        Action<TSibling, int> setOrder )
    {
        orderedSiblings.Remove( item );

        var insertIndex = -1;

        if ( placement.AfterSiblingId.HasValue )
        {
            var afterIndex = orderedSiblings.FindIndex( s => getId( s ) == placement.AfterSiblingId.Value );

            insertIndex = afterIndex < 0 ? -1 : afterIndex + 1;
        }
        else if ( placement.BeforeSiblingId.HasValue )
        {
            insertIndex = orderedSiblings.FindIndex( s => getId( s ) == placement.BeforeSiblingId.Value );
        }

        // A named neighbour that could not be found falls back to the end, which is
        // where an unpositioned item goes anyway.
        if ( insertIndex < 0 )
        {
            orderedSiblings.Add( item );
        }
        else
        {
            orderedSiblings.Insert( insertIndex, item );
        }

        for ( var index = 0; index < orderedSiblings.Count; index++ )
        {
            setOrder( orderedSiblings[index], index );
        }
    }

    /// <summary>
    /// Where an item should sit among its siblings.
    /// </summary>
    private class SiblingPlacement
    {
        /// <summary>
        /// The identifier of the sibling this item follows, when one was named.
        /// </summary>
        public int? AfterSiblingId { get; set; }

        /// <summary>
        /// The identifier of the sibling this item precedes, when one was named.
        /// </summary>
        public int? BeforeSiblingId { get; set; }

        /// <summary>
        /// Indicates that the caller asked for a position at all. When false an
        /// existing item keeps its place and a new one goes at the end.
        /// </summary>
        public bool IsSpecified => AfterSiblingId.HasValue || BeforeSiblingId.HasValue;
    }

    /// <summary>
    /// Resolves where a new or moved item should sit from the caller's parameters.
    /// </summary>
    /// <remarks>
    /// Position is expressed by naming a neighbour rather than by supplying a
    /// number. A number is only meaningful against the current set, and a caller
    /// working from an earlier read may hold stale ones. Naming the neighbour
    /// survives anything that shifted in between, and it matches how the request is
    /// usually phrased: "after the approval step".
    /// </remarks>
    /// <param name="insertAfterIdKey">The key of the sibling to follow, if any.</param>
    /// <param name="insertBeforeIdKey">The key of the sibling to precede, if any.</param>
    /// <param name="helper">The helper to record errors on.</param>
    /// <returns>The resolved placement, or <c>null</c> when the parameters conflict.</returns>
    private static SiblingPlacement ResolveSiblingPlacement( string insertAfterIdKey, string insertBeforeIdKey, AgentToolHelper helper )
    {
        var hasAfter = insertAfterIdKey.IsNotNullOrWhiteSpace();
        var hasBefore = insertBeforeIdKey.IsNotNullOrWhiteSpace();

        // Both together have no single meaning, and guessing which one wins would
        // put the item somewhere the caller did not ask for.
        if ( hasAfter && hasBefore )
        {
            helper.AddError( $"Supply at most one of {nameof( insertAfterIdKey )} or {nameof( insertBeforeIdKey )}, not both." );

            return null;
        }

        return new SiblingPlacement
        {
            AfterSiblingId = hasAfter ? IdHasher.Instance.GetId( insertAfterIdKey ) : null,
            BeforeSiblingId = hasBefore ? IdHasher.Instance.GetId( insertBeforeIdKey ) : null
        };
    }

    /// <summary>
    /// Counts the saved instances of a workflow type.
    /// </summary>
    /// <remarks>
    /// Used by the changes that are only safe on a workflow nothing has run yet.
    /// Only persisted workflows are counted, because only those have stored values
    /// that a change could strand. A workflow type that runs entirely in memory has
    /// nothing to protect.
    /// </remarks>
    /// <param name="workflowTypeId">The workflow type to count instances of.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The number of saved instances.</returns>
    private static int GetWorkflowInstanceCount( int workflowTypeId, RockContext rockContext )
    {
        return new WorkflowService( rockContext ).Queryable()
            .Count( w => w.WorkflowTypeId == workflowTypeId );
    }

    /// <summary>
    /// Removes instance rows in batches, saving as it goes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The workflow structure tables cascade to each other, but the instance tables
    /// deliberately do not cascade from the structure that defines them.
    /// <c>WorkflowAction.ActionType</c> and <c>WorkflowActivity.ActivityType</c> are
    /// both declared with <c>WillCascadeOnDelete( false )</c>. A structure row
    /// therefore cannot be removed until its instance rows are, and the generated
    /// <c>CanDelete</c> refuses until they are gone.
    /// </para>
    /// <para>
    /// Batched rather than deleted in one statement because a busy workflow type can
    /// hold a very large number of instance rows, and one enormous transaction is
    /// how a delete turns into an outage. The batch size matches what Rock's own
    /// workflow type block uses.
    /// </para>
    /// </remarks>
    /// <typeparam name="TEntity">The instance entity being removed.</typeparam>
    /// <param name="service">The service to delete through.</param>
    /// <param name="instances">The rows to remove, as an unexecuted query so each batch re-reads what is left.</param>
    /// <param name="rockContext">The context to save through.</param>
    /// <returns>The number of rows removed.</returns>
    private static int DeleteInstancesInBatches<TEntity>( Service<TEntity> service, IQueryable<TEntity> instances, RockContext rockContext )
        where TEntity : Rock.Data.Entity<TEntity>, new()
    {
        var deletedCount = 0;

        while ( true )
        {
            var batch = instances.Take( InstanceDeleteBatchSize ).ToList();

            if ( batch.Count == 0 )
            {
                break;
            }

            service.DeleteRange( batch );
            rockContext.SaveChanges();

            deletedCount += batch.Count;
        }

        return deletedCount;
    }

    /// <summary>
    /// Deletes the forms belonging to a set of actions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Deleting an action does not delete its form. Every other link in the
    /// workflow chain cascades, so an activity takes its actions with it and a
    /// workflow type takes its activities, but the link from an action to its form
    /// is deliberately not a cascade. Nothing else cleans these up, so every delete
    /// tool has to do it by hand or leave rows behind that nothing can reach.
    /// </para>
    /// <para>
    /// The form's own sections and fields do cascade from the form, so removing the
    /// form is enough to take them with it.
    /// </para>
    /// </remarks>
    /// <param name="actionTypes">The actions whose forms should be removed.</param>
    /// <param name="rockContext">The context to write through.</param>
    private static void DeleteFormsForActionTypes( IEnumerable<WorkflowActionType> actionTypes, RockContext rockContext )
    {
        var formIds = actionTypes
            .Where( at => at.WorkflowFormId.HasValue )
            .Select( at => at.WorkflowFormId.Value )
            .Distinct()
            .ToList();

        if ( !formIds.Any() )
        {
            return;
        }

        // The reference is cleared before the forms are removed, because the
        // foreign key from the action still points at them.
        foreach ( var actionType in actionTypes.Where( at => at.WorkflowFormId.HasValue ) )
        {
            actionType.WorkflowFormId = null;
            actionType.WorkflowForm = null;
        }

        rockContext.SaveChanges();

        var formService = new WorkflowActionFormService( rockContext );
        var forms = formService.Queryable().Where( f => formIds.Contains( f.Id ) ).ToList();

        formService.DeleteRange( forms );
    }

    /// <summary>
    /// Builds the result for a single configured action.
    /// </summary>
    /// <remarks>
    /// Shared by the tree read and the single action read so that a setting reads
    /// the same either way. The only difference between the two is whether long
    /// values are clipped, which is why that is a parameter rather than two
    /// renderers.
    /// </remarks>
    /// <param name="actionType">The action to describe.</param>
    /// <param name="rockContext">The context to load attributes through.</param>
    /// <param name="clipLongValues"><c>true</c> to clip settings and form markup to <see cref="MaximumValueLength"/>.</param>
    /// <returns>The action result.</returns>
    private WorkflowActionTypeResult GetActionTypeResult( WorkflowActionType actionType, RockContext rockContext, bool clipLongValues, IDictionary<Guid, string> referenceNames = null )
    {
        if ( actionType.Attributes == null )
        {
            actionType.LoadAttributes( rockContext );
        }

        var actionEntityType = EntityTypeCache.Get( actionType.EntityTypeId, rockContext );

        var result = new WorkflowActionTypeResult
        {
            Id = actionType.Id,
            Guid = actionType.Guid,
            Name = actionType.Name,
            Order = actionType.Order,
            ActionEntityTypeIdKey = actionType.EntityTypeId.AsIdKey(),
            ActionClassName = actionEntityType?.Name,
            ActionName = actionEntityType?.FriendlyName,
            IsActionCompletedOnSuccess = actionType.IsActionCompletedOnSuccess,
            IsActivityCompletedOnSuccess = actionType.IsActivityCompletedOnSuccess,
            IsActionCompletedIfCriteriaUnmet = actionType.IsActionCompletedIfCriteriaUnmet,
            Criteria = GetCriteriaResult( actionType ),
            Settings = new Dictionary<string, WorkflowActionSettingResult>()
        };

        // Every setting is returned, with no per attribute permission filter.
        // Settings are the action's configuration rather than someone's data, and
        // the caller already had to pass the VIEW check on the workflow type to get
        // here. Filtering individual settings would also produce a tree that
        // contradicts itself: a form field pointing at a setting that the same
        // response says does not exist.
        foreach ( var attribute in actionType.Attributes.Values
            .Where( a => !VestigialSettingKeys.Contains( a.Key ) )
            .OrderBy( a => a.Order )
            .ThenBy( a => a.Key ) )
        {
            var value = actionType.GetAttributeValue( attribute.Key );

            // Resolved from the whole value, before any clipping, so a long value
            // that happens to start with an identifier is not mistaken for one.
            var referenceName = ResolveReferenceName( value, referenceNames );

            bool? isTruncated = null;

            if ( clipLongValues )
            {
                value = ClipValue( value, out isTruncated );
            }

            result.Settings.Add( attribute.Key, new WorkflowActionSettingResult
            {
                Value = value,
                IsTruncated = isTruncated,
                ReferenceName = referenceName
            } );
        }

        if ( actionType.WorkflowForm != null )
        {
            result.Form = GetActionFormResult( actionType, rockContext, clipLongValues );
        }

        return result;
    }

    /// <summary>
    /// Builds a lookup of the things inside a workflow that a setting value can
    /// point at.
    /// </summary>
    /// <remarks>
    /// Built once per read rather than per action, because the same handful of
    /// activities and attributes are referenced by every action in the workflow.
    /// </remarks>
    /// <param name="workflowTypeId">The workflow type being read.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>Unique identifiers mapped to a readable description of what they name.</returns>
    private static Dictionary<Guid, string> GetWorkflowReferenceNames( int workflowTypeId, RockContext rockContext )
    {
        var referenceNames = new Dictionary<Guid, string>();

        var activityTypes = new WorkflowActivityTypeService( rockContext ).Queryable()
            .Where( at => at.WorkflowTypeId == workflowTypeId )
            .Select( at => new { at.Id, at.Guid, at.Name } )
            .ToList();

        foreach ( var activityType in activityTypes )
        {
            referenceNames[activityType.Guid] = $"Activity: {activityType.Name}";
        }

        foreach ( var attribute in GetWorkflowAttributes( workflowTypeId, rockContext ) )
        {
            referenceNames[attribute.Guid] = $"Attribute: {attribute.Name}";
        }

        // Activity attributes are included because an action's setting can hold one
        // just as readily as a workflow attribute. Left out, those values would read
        // as identifiers that point at nothing.
        foreach ( var activityType in activityTypes )
        {
            foreach ( var attribute in GetActivityAttributes( activityType.Id, rockContext ) )
            {
                referenceNames[attribute.Guid] = $"Attribute: {attribute.Name} (in {activityType.Name})";
            }
        }

        return referenceNames;
    }

    /// <summary>
    /// Resolves a setting value that is a unique identifier to what it names.
    /// </summary>
    /// <param name="value">The stored setting value.</param>
    /// <param name="referenceNames">The lookup for this workflow, or <c>null</c> when references are not being resolved.</param>
    /// <returns>A readable description, or <c>null</c> when the value does not name anything in this workflow.</returns>
    private static string ResolveReferenceName( string value, IDictionary<Guid, string> referenceNames )
    {
        if ( referenceNames == null || value.IsNullOrWhiteSpace() )
        {
            return null;
        }

        var referenceGuid = value.Trim().AsGuidOrNull();

        if ( !referenceGuid.HasValue )
        {
            return null;
        }

        return referenceNames.TryGetValue( referenceGuid.Value, out var referenceName ) ? referenceName : null;
    }

    /// <summary>
    /// Builds the condition that decides whether an action runs.
    /// </summary>
    /// <param name="actionType">The action to describe.</param>
    /// <returns>The criteria, or <c>null</c> when the action always runs.</returns>
    private static WorkflowActionCriteriaResult GetCriteriaResult( WorkflowActionType actionType )
    {
        if ( !actionType.CriteriaAttributeGuid.HasValue || actionType.CriteriaAttributeGuid.Value.IsEmpty() )
        {
            return null;
        }

        var criteriaAttribute = AttributeCache.Get( actionType.CriteriaAttributeGuid.Value );

        return new WorkflowActionCriteriaResult
        {
            AttributeKey = criteriaAttribute?.Key,
            AttributeName = criteriaAttribute?.Name,
            ComparisonType = actionType.CriteriaComparisonType,
            Value = actionType.CriteriaValue
        };
    }

    /// <summary>
    /// Builds the result for the form attached to a user entry action.
    /// </summary>
    /// <param name="actionType">The action the form belongs to, needed to resolve the activities its buttons can activate.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <param name="clipLongValues"><c>true</c> to clip the header and footer.</param>
    /// <returns>The form result.</returns>
    private WorkflowActionFormResult GetActionFormResult( WorkflowActionType actionType, RockContext rockContext, bool clipLongValues )
    {
        var form = actionType.WorkflowForm;

        var header = form.Header;
        var footer = form.Footer;
        bool? isHeaderTruncated = null;
        bool? isFooterTruncated = null;

        if ( clipLongValues )
        {
            header = ClipValue( header, out isHeaderTruncated );
            footer = ClipValue( footer, out isFooterTruncated );
        }

        var result = new WorkflowActionFormResult
        {
            Id = form.Id,
            Guid = form.Guid,
            Header = header,
            IsHeaderTruncated = isHeaderTruncated,
            Footer = footer,
            IsFooterTruncated = isFooterTruncated,
            AllowNotes = form.AllowNotes ?? false,
            AllowPersonEntry = form.AllowPersonEntry,

            // Only when it is on. A form without person entry is the common case, and
            // omitting the block there keeps the tree the size it was.
            PersonEntry = form.AllowPersonEntry
                ? GetPersonEntryResult( form, rockContext, clipLongValues )
                : null,
            Buttons = GetFormButtonResults( actionType, rockContext ),
            Fields = GetFormFieldResults( form )
        };

        if ( form.NotificationSystemCommunicationId.HasValue )
        {
            var systemCommunication = new SystemCommunicationService( rockContext )
                .Get( form.NotificationSystemCommunicationId.Value );

            if ( systemCommunication != null )
            {
                result.NotificationSystemCommunication = new KeyNameResult
                {
                    Id = systemCommunication.Id,
                    Guid = systemCommunication.Guid,
                    Name = systemCommunication.Title
                };
            }
        }

        return result;
    }

    /// <summary>
    /// Builds the buttons for a form.
    /// </summary>
    /// <remarks>
    /// Buttons are stored as one delimited string rather than as rows, so they are
    /// parsed with the same helper the workflow entry block uses. Each button names
    /// the activity it starts, which is how a workflow branches, so the activity is
    /// resolved to a key and a name rather than left as a raw unique identifier.
    /// </remarks>
    /// <param name="actionType">The action whose form is being described.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The buttons, in the order they appear on the form.</returns>
    private static List<WorkflowFormButtonResult> GetFormButtonResults( WorkflowActionType actionType, RockContext rockContext )
    {
        var form = actionType.WorkflowForm;

        if ( form.Actions.IsNullOrWhiteSpace() )
        {
            return new List<WorkflowFormButtonResult>();
        }

        // Sibling activities of the action's own activity, which are the only ones a
        // button can activate.
        var workflowTypeId = actionType.ActivityType?.WorkflowTypeId;

        var activityTypes = workflowTypeId.HasValue
            ? new WorkflowActivityTypeService( rockContext ).Queryable()
                .Where( at => at.WorkflowTypeId == workflowTypeId.Value )
                .Select( at => new { at.Id, at.Guid, at.Name } )
                .ToList()
            : null;

        return WorkflowActionFormUserAction.FromUriEncodedString( form.Actions )
            .Select( button =>
            {
                var activateActivityGuid = button.ActivateActivityTypeGuid.AsGuidOrNull();

                var activityType = activateActivityGuid.HasValue && activityTypes != null
                    ? activityTypes.FirstOrDefault( at => at.Guid == activateActivityGuid.Value )
                    : null;

                var buttonStyleGuid = button.ButtonTypeGuid.AsGuidOrNull();

                return new WorkflowFormButtonResult
                {
                    Name = button.ActionName,
                    ButtonStyleGuid = buttonStyleGuid,
                    ButtonStyleName = buttonStyleGuid.HasValue
                        ? DefinedValueCache.Get( buttonStyleGuid.Value, rockContext )?.Value
                        : null,
                    ActivateActivityIdKey = activityType?.Id.AsIdKey(),
                    ActivateActivityName = activityType?.Name,
                    ResponseText = button.ResponseText
                };
            } )
            .ToList();
    }

    /// <summary>
    /// Builds the visibility conditions attached to a field or a section.
    /// </summary>
    /// <remarks>
    /// Rock's typed accessor returns an empty rule set rather than <c>null</c> when the
    /// column is blank, so emptiness is what has to be tested. A null check would put
    /// an empty array on every field of every form.
    /// </remarks>
    /// <param name="rules">The stored rules.</param>
    /// <returns>The rules, or <c>null</c> when there are none.</returns>
    private static List<WorkflowFormVisibilityRuleResult> GetVisibilityRuleResults( FieldVisibilityRules rules )
    {
        if ( rules == null || rules.RuleList == null || !rules.RuleList.Any() )
        {
            return null;
        }

        return rules.RuleList
            .Select( rule =>
            {
                // In a workflow form the compared-to identifier is an attribute's,
                // which is the branch FieldVisibilityRules.Evaluate falls back to when
                // the registration template lookup misses.
                var attribute = rule.ComparedToFormFieldGuid.HasValue
                    ? AttributeCache.Get( rule.ComparedToFormFieldGuid.Value )
                    : null;

                return new WorkflowFormVisibilityRuleResult
                {
                    ComparedToAttributeIdKey = attribute?.Id.AsIdKey(),
                    ComparedToAttributeKey = attribute?.Key,
                    ComparedToAttributeName = attribute?.Name,
                    ComparisonType = rule.ComparisonType,
                    ComparedToValue = rule.ComparedToValue
                };
            } )
            .ToList();
    }

    /// <summary>
    /// Builds the visibility conditions to store on a field or a section.
    /// </summary>
    /// <remarks>
    /// Shared by fields and sections because the two store the same type in different
    /// columns. Keeping one implementation is deliberate: the last time this skill had
    /// the same rule expressed in two places, the two drifted and produced a tool that
    /// demanded a value it also refused to accept.
    /// </remarks>
    /// <param name="ruleInputs">The conditions the caller supplied.</param>
    /// <param name="match">Whether every rule must pass or only one.</param>
    /// <param name="referenceableAttributes">The attributes the form's action can reach, by id.</param>
    /// <param name="ownerDescription">How to name the field or section in an error.</param>
    /// <param name="helper">The helper to record errors on.</param>
    /// <returns>The rules to store, or <c>null</c> when there are none.</returns>
    private static FieldVisibilityRules BuildVisibilityRules(
        List<WorkflowFormVisibilityRuleInput> ruleInputs,
        FilterExpressionType? match,
        Dictionary<int, AttributeCache> referenceableAttributes,
        string ownerDescription,
        AgentToolHelper helper )
    {
        if ( ruleInputs == null || !ruleInputs.Any() )
        {
            return null;
        }

        // Only the two grouping values mean anything for a form. The others exist for
        // reporting filters, and offering them invites a caller to pick one that
        // silently never matches.
        if ( match.HasValue && match.Value != FilterExpressionType.GroupAll && match.Value != FilterExpressionType.GroupAny )
        {
            helper.AddError( $"The visibility rule match on {ownerDescription} must be GroupAll or GroupAny." );

            return null;
        }

        var rules = new FieldVisibilityRules
        {
            FilterExpressionType = match ?? FilterExpressionType.GroupAll
        };

        foreach ( var ruleInput in ruleInputs )
        {
            var attributeId = IdHasher.Instance.GetId( ruleInput.ComparedToAttributeIdKey );
            var attribute = attributeId.HasValue && referenceableAttributes.TryGetValue( attributeId.Value, out var found )
                ? found
                : null;

            // Refused rather than stored. An identifier that resolves to nothing makes
            // Evaluate skip the rule, so the field is simply always visible and nothing
            // reports why.
            if ( attribute == null )
            {
                helper.AddError( $"The visibility rule on {ownerDescription} compares against '{ruleInput.ComparedToAttributeIdKey}', which is not an attribute of this workflow type or of the activity the form belongs to." );

                continue;
            }

            rules.RuleList.Add( new FieldVisibilityRule
            {
                ComparedToFormFieldGuid = attribute.Guid,
                ComparisonType = ruleInput.ComparisonType,
                ComparedToValue = ruleInput.ComparedToValue
            } );
        }

        return rules;
    }

    /// <summary>
    /// Builds the person entry block of a form.
    /// </summary>
    /// <remarks>
    /// The markup fields clip in a tree read the same way the form's header and footer
    /// do, so a long person entry preamble cannot inflate the whole workflow. The
    /// single-action read returns them whole.
    /// </remarks>
    /// <param name="form">The form to describe.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <param name="clipLongValues">Whether long markup should be clipped.</param>
    /// <returns>The person entry configuration.</returns>
    private static WorkflowFormPersonEntryResult GetPersonEntryResult( WorkflowActionForm form, RockContext rockContext, bool clipLongValues )
    {
        var preHtml = form.PersonEntryPreHtml;
        var postHtml = form.PersonEntryPostHtml;
        bool? isPreHtmlTruncated = null;
        bool? isPostHtmlTruncated = null;

        if ( clipLongValues )
        {
            preHtml = ClipValue( preHtml, out isPreHtmlTruncated );
            postHtml = ClipValue( postHtml, out isPostHtmlTruncated );
        }

        return new WorkflowFormPersonEntryResult
        {
            Title = form.PersonEntryTitle,
            Description = form.PersonEntryDescription,
            IsHeadingSeparatorShown = form.PersonEntryShowHeadingSeparator,
            SectionTypeValue = form.PersonEntrySectionTypeValueId.HasValue
                ? GetDefinedValueKeyName( form.PersonEntrySectionTypeValueId.Value, rockContext )
                : null,
            PreHtml = preHtml,
            IsPreHtmlTruncated = isPreHtmlTruncated,
            PostHtml = postHtml,
            IsPostHtmlTruncated = isPostHtmlTruncated,

            PersonAttribute = GetAttributeKeyName( form.PersonEntryPersonAttributeGuid, rockContext ),
            SpouseAttribute = GetAttributeKeyName( form.PersonEntrySpouseAttributeGuid, rockContext ),
            FamilyAttribute = GetAttributeKeyName( form.PersonEntryFamilyAttributeGuid, rockContext ),

            AddressOption = form.PersonEntryAddressEntryOption,
            BirthdateOption = form.PersonEntryBirthdateEntryOption,
            EmailOption = form.PersonEntryEmailEntryOption,
            EthnicityOption = form.PersonEntryEthnicityEntryOption,
            GenderOption = form.PersonEntryGenderEntryOption,
            MaritalStatusOption = form.PersonEntryMaritalStatusEntryOption,
            MobilePhoneOption = form.PersonEntryMobilePhoneEntryOption,
            RaceOption = form.PersonEntryRaceEntryOption,
            SpouseOption = form.PersonEntrySpouseEntryOption,
            SmsOptInOption = form.PersonEntrySmsOptInEntryOption,
            SpouseLabel = form.PersonEntrySpouseLabel,

            IsAutofillCurrentPersonEnabled = form.PersonEntryAutofillCurrentPerson,
            IsCampusVisible = form.PersonEntryCampusIsVisible,

            // Absent means true, matching how Rock reads it, so the same fallback is
            // applied here rather than reporting false for a form that never set it.
            IsInactiveCampusIncluded = form.GetAdditionalSettingsOrNull<Rock.Workflow.FormBuilder.PersonEntryAdditionalSettings>()?.IncludeInactiveCampus ?? true,
            IsHiddenIfCurrentPersonKnown = form.PersonEntryHideIfCurrentPersonKnown,

            ConnectionStatusValue = form.PersonEntryConnectionStatusValueId.HasValue
                ? GetDefinedValueKeyName( form.PersonEntryConnectionStatusValueId.Value, rockContext )
                : null,
            RecordStatusValue = form.PersonEntryRecordStatusValueId.HasValue
                ? GetDefinedValueKeyName( form.PersonEntryRecordStatusValueId.Value, rockContext )
                : null,
            RecordSourceValue = form.PersonEntryRecordSourceValueId.HasValue
                ? GetDefinedValueKeyName( form.PersonEntryRecordSourceValueId.Value, rockContext )
                : null,
            AddressTypeValue = form.PersonEntryGroupLocationTypeValueId.HasValue
                ? GetDefinedValueKeyName( form.PersonEntryGroupLocationTypeValueId.Value, rockContext )
                : null,
            CampusStatusValue = form.PersonEntryCampusStatusValueId.HasValue
                ? GetDefinedValueKeyName( form.PersonEntryCampusStatusValueId.Value, rockContext )
                : null,
            CampusTypeValue = form.PersonEntryCampusTypeValueId.HasValue
                ? GetDefinedValueKeyName( form.PersonEntryCampusTypeValueId.Value, rockContext )
                : null
        };
    }

    /// <summary>
    /// Builds a reference to an attribute held by unique identifier.
    /// </summary>
    /// <remarks>
    /// Person entry stores its three attribute bindings as raw Guid columns rather
    /// than as foreign keys, so there is no navigation property to read a name from.
    /// </remarks>
    /// <param name="attributeGuid">The attribute's unique identifier, if set.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The reference, or <c>null</c> when unset or no longer present.</returns>
    private static KeyNameResult GetAttributeKeyName( Guid? attributeGuid, RockContext rockContext )
    {
        if ( !attributeGuid.HasValue )
        {
            return null;
        }

        var attribute = AttributeCache.Get( attributeGuid.Value, rockContext );

        if ( attribute == null )
        {
            return null;
        }

        return new KeyNameResult
        {
            Id = attribute.Id,
            Guid = attribute.Guid,
            Name = attribute.Name
        };
    }

    /// <summary>
    /// Builds the fields of a form as one flat list.
    /// </summary>
    /// <remarks>
    /// This skill never writes sections, but Form Builder does, and a form it
    /// created can still be read here. Those fields are flattened in section order
    /// and then field order rather than dropped, because a field missing from the
    /// result would read as a field that is not on the form. Fields belonging to no
    /// section sort first, which is where this skill's own fields all land.
    /// </remarks>
    /// <param name="form">The form to describe.</param>
    /// <returns>The fields, in the order they appear on the form.</returns>
    private static List<WorkflowFormFieldResult> GetFormFieldResults( WorkflowActionForm form )
    {
        var sectionOrders = form.FormSections.ToDictionary( s => s.Id, s => s.Order );

        // A field with no section sorts ahead of every section, matching how both
        // renderers place them.
        int GetSectionOrder( WorkflowActionFormAttribute formAttribute )
        {
            if ( !formAttribute.ActionFormSectionId.HasValue )
            {
                return int.MinValue;
            }

            return sectionOrders.TryGetValue( formAttribute.ActionFormSectionId.Value, out var order )
                ? order
                : int.MaxValue;
        }

        return form.FormAttributes
            .OrderBy( fa => GetSectionOrder( fa ) )
            .ThenBy( fa => fa.Order )
            .ThenBy( fa => fa.Id )
            .Select( fa =>
            {
                var attribute = AttributeCache.Get( fa.AttributeId );
                var visibilityRules = GetVisibilityRuleResults( fa.FieldVisibilityRules );

                return new WorkflowFormFieldResult
                {
                    Id = fa.Id,
                    Guid = fa.Guid,
                    AttributeIdKey = fa.AttributeId.AsIdKey(),
                    AttributeKey = attribute?.Key,
                    AttributeName = attribute?.Name,
                    Order = fa.Order,
                    IsVisible = fa.IsVisible,
                    IsRequired = fa.IsRequired,
                    IsReadOnly = fa.IsReadOnly,
                    HideLabel = fa.HideLabel,
                    PreHtml = fa.PreHtml,
                    PostHtml = fa.PostHtml,
                    VisibilityRules = visibilityRules,
                    VisibilityRuleMatch = visibilityRules != null ? fa.FieldVisibilityRules.FilterExpressionType : ( FilterExpressionType? ) null
                };
            } )
            .ToList();
    }

    /// <summary>
    /// Builds the attributes belonging to a workflow type, which are the variables
    /// its actions read and write.
    /// </summary>
    /// <remarks>
    /// Workflow attributes have no foreign key to the workflow type. They are
    /// Attribute rows on the Workflow entity, qualified by the workflow type's id,
    /// which is why they are read this way and why deleting a workflow type has to
    /// remove them by hand.
    /// </remarks>
    /// <param name="workflowTypeId">The workflow type to read attributes for.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The attributes, in order.</returns>
    private List<WorkflowAttributeResult> GetWorkflowAttributeResults( int workflowTypeId, RockContext rockContext )
    {
        // Unfiltered, for the same reason the settings are. These are the
        // workflow's variables, and hiding one would break the tree rather than
        // protect anything: form fields and action criteria reference attributes by
        // key, so a missing definition turns a valid workflow into one that appears
        // to point at nothing.
        return GetWorkflowAttributes( workflowTypeId, rockContext )
            .Select( a => GetWorkflowAttributeResult( a, WorkflowAttributeScope.Workflow ) )
            .ToList();
    }

    /// <summary>
    /// Builds the attributes belonging to one activity, which are the variables only
    /// that activity's actions can read and write.
    /// </summary>
    /// <param name="activityTypeId">The activity type to read attributes for.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The attributes, in order.</returns>
    private static List<WorkflowAttributeResult> GetActivityAttributeResults( int activityTypeId, RockContext rockContext )
    {
        return GetActivityAttributes( activityTypeId, rockContext )
            .Select( a => GetWorkflowAttributeResult( a, WorkflowAttributeScope.Activity ) )
            .ToList();
    }

    /// <summary>
    /// Builds the result for one workflow or activity attribute.
    /// </summary>
    /// <param name="attribute">The attribute to describe.</param>
    /// <param name="scope">How far the attribute reaches.</param>
    /// <returns>The attribute result.</returns>
    private static WorkflowAttributeResult GetWorkflowAttributeResult( AttributeCache attribute, WorkflowAttributeScope scope )
    {
        if ( attribute == null )
        {
            return null;
        }

        var result = new WorkflowAttributeResult
        {
            Id = attribute.Id,
            Guid = attribute.Guid,
            Scope = scope,
            Key = attribute.Key,
            Name = attribute.Name,
            Description = attribute.Description.IsNullOrWhiteSpace() ? null : attribute.Description,
            IsRequired = attribute.IsRequired,
            Order = attribute.Order,
            DefaultValue = attribute.DefaultValue,
            ConfigurationValues = attribute.ConfigurationValues != null && attribute.ConfigurationValues.Any()
                ? new Dictionary<string, string>( attribute.ConfigurationValues )
                : null
        };

        if ( attribute.FieldType != null )
        {
            // Built with an object initializer rather than the three argument
            // constructor, which does not assign its guid.
            result.FieldType = new WorkflowFieldTypeResult
            {
                Id = attribute.FieldType.Id,
                Guid = attribute.FieldType.Guid,
                Name = attribute.FieldType.Name,
                Class = attribute.FieldType.Class
            };
        }

        return result;
    }

    /// <summary>
    /// Gets the setting definitions an action component declares.
    /// </summary>
    /// <remarks>
    /// Settings are attributes on WorkflowActionType qualified by the component's
    /// entity type, which is why they are found this way rather than through the
    /// action instance. The same query answers "what may this action be given" and
    /// "which stored values belonged to the component it used to be".
    /// </remarks>
    /// <param name="actionEntityTypeId">The action component's entity type.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The setting definitions, in order.</returns>
    private static List<AttributeCache> GetActionSettingAttributes( int actionEntityTypeId, RockContext rockContext )
    {
        var workflowActionTypeEntityTypeId = EntityTypeCache.GetId<WorkflowActionType>();

        return AttributeCache.All( rockContext )
            .Where( a => a.EntityTypeId == workflowActionTypeEntityTypeId )
            .Where( a => a.EntityTypeQualifierColumn == "EntityTypeId" )
            .Where( a => a.EntityTypeQualifierValue == actionEntityTypeId.ToString() )
            .OrderBy( a => a.Order )
            .ThenBy( a => a.Key )
            .ToList();
    }

    /// <summary>
    /// Gets the attribute definitions belonging to a workflow type.
    /// </summary>
    /// <param name="workflowTypeId">The workflow type to read attributes for.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The attributes, in order.</returns>
    private static List<AttributeCache> GetWorkflowAttributes( int workflowTypeId, RockContext rockContext )
    {
        var workflowEntityTypeId = EntityTypeCache.GetId<Rock.Model.Workflow>();

        return AttributeCache.All( rockContext )
            .Where( a => a.EntityTypeId == workflowEntityTypeId )
            .Where( a => a.EntityTypeQualifierColumn == "WorkflowTypeId" )
            .Where( a => a.EntityTypeQualifierValue == workflowTypeId.ToString() )
            .OrderBy( a => a.Order )
            .ThenBy( a => a.Name )
            .ThenBy( a => a.Id )
            .ToList();
    }

    /// <summary>
    /// Builds a key and name reference for a category.
    /// </summary>
    /// <param name="categoryId">The identifier of the category.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The reference, or <c>null</c> when the category no longer exists.</returns>
    private static KeyNameResult GetCategoryKeyName( int categoryId, RockContext rockContext )
    {
        var category = CategoryCache.Get( categoryId, rockContext );

        if ( category == null )
        {
            return null;
        }

        return new KeyNameResult
        {
            Id = category.Id,
            Guid = category.Guid,
            Name = category.Name
        };
    }

    /// <summary>
    /// Gets the attribute definitions belonging to one activity type.
    /// </summary>
    /// <remarks>
    /// Activity attributes are Attribute rows on the WorkflowActivity entity
    /// qualified by the activity type, which is a different entity and a different
    /// qualifier from the workflow's own attributes. Nothing links them to the
    /// activity by foreign key, so like workflow attributes they have to be found
    /// this way and removed by hand.
    /// </remarks>
    /// <param name="activityTypeId">The activity type to read attributes for.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The attributes, in order.</returns>
    private static List<AttributeCache> GetActivityAttributes( int activityTypeId, RockContext rockContext )
    {
        var activityEntityTypeId = EntityTypeCache.GetId<WorkflowActivity>();

        return AttributeCache.All( rockContext )
            .Where( a => a.EntityTypeId == activityEntityTypeId )
            .Where( a => a.EntityTypeQualifierColumn == "ActivityTypeId" )
            .Where( a => a.EntityTypeQualifierValue == activityTypeId.ToString() )
            .OrderBy( a => a.Order )
            .ThenBy( a => a.Name )
            .ThenBy( a => a.Id )
            .ToList();
    }

    /// <summary>
    /// Gets every attribute an action inside an activity may legitimately reference.
    /// </summary>
    /// <remarks>
    /// A form field or an action's criteria can point at the workflow's own
    /// variables or at the containing activity's, and Rock's workflow type block
    /// offers both in its pickers. Validating against only one of the two rejects
    /// forms that the UI produces, so both are gathered here and every caller that
    /// checks a reference uses this.
    /// </remarks>
    /// <param name="activityType">The activity the action belongs to.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The referenceable attributes.</returns>
    private static List<AttributeCache> GetReferenceableAttributes( WorkflowActivityType activityType, RockContext rockContext )
    {
        if ( activityType == null )
        {
            return new List<AttributeCache>();
        }

        var attributes = GetWorkflowAttributes( activityType.WorkflowTypeId, rockContext );

        attributes.AddRange( GetActivityAttributes( activityType.Id, rockContext ) );

        return attributes;
    }

    /// <summary>
    /// Describes which workflow or activity an attribute belongs to.
    /// </summary>
    private class WorkflowAttributeOwner
    {
        /// <summary>
        /// How far the attribute reaches.
        /// </summary>
        public WorkflowAttributeScope Scope { get; set; }

        /// <summary>
        /// The workflow type the attribute ultimately belongs to, whichever scope it
        /// is in.
        /// </summary>
        public Rock.Model.WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// The owning activity, when the attribute is activity scoped.
        /// </summary>
        public WorkflowActivityType ActivityType { get; set; }
    }

    /// <summary>
    /// Resolves the workflow or activity an attribute belongs to.
    /// </summary>
    /// <remarks>
    /// Both scopes carry their owner in a qualifier column rather than a foreign
    /// key, so this both finds the owner and proves the attribute is a workflow
    /// attribute at all. Without the check, any attribute in Rock could be reached
    /// through these tools.
    /// </remarks>
    /// <param name="attribute">The attribute to resolve.</param>
    /// <param name="helper">The helper to record errors on.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The owner, or <c>null</c> when the attribute belongs to neither scope.</returns>
    private static WorkflowAttributeOwner GetAttributeOwner( Rock.Model.Attribute attribute, AgentToolHelper helper, RockContext rockContext )
    {
        var workflowEntityTypeId = EntityTypeCache.GetId<Rock.Model.Workflow>();
        var activityEntityTypeId = EntityTypeCache.GetId<WorkflowActivity>();

        if ( attribute.EntityTypeId == workflowEntityTypeId && attribute.EntityTypeQualifierColumn == "WorkflowTypeId" )
        {
            var workflowTypeId = attribute.EntityTypeQualifierValue.AsIntegerOrNull();

            if ( !workflowTypeId.HasValue )
            {
                helper.AddError( "That workflow attribute is not attached to a workflow type." );

                return null;
            }

            var workflowType = new WorkflowTypeService( rockContext ).Get( workflowTypeId.Value );

            if ( workflowType == null )
            {
                helper.AddError( "The workflow type this attribute belongs to no longer exists." );

                return null;
            }

            return new WorkflowAttributeOwner
            {
                Scope = WorkflowAttributeScope.Workflow,
                WorkflowType = workflowType
            };
        }

        if ( attribute.EntityTypeId == activityEntityTypeId && attribute.EntityTypeQualifierColumn == "ActivityTypeId" )
        {
            var activityTypeId = attribute.EntityTypeQualifierValue.AsIntegerOrNull();

            if ( !activityTypeId.HasValue )
            {
                helper.AddError( "That activity attribute is not attached to an activity." );

                return null;
            }

            var activityType = new WorkflowActivityTypeService( rockContext ).Get( activityTypeId.Value );

            if ( activityType == null )
            {
                helper.AddError( "The activity this attribute belongs to no longer exists." );

                return null;
            }

            return new WorkflowAttributeOwner
            {
                Scope = WorkflowAttributeScope.Activity,
                WorkflowType = activityType.WorkflowType,
                ActivityType = activityType
            };
        }

        helper.AddError( "That attribute is not a workflow or activity attribute." );

        return null;
    }

    /// <summary>
    /// Builds a key and name reference for a defined value.
    /// </summary>
    /// <param name="definedValueId">The identifier of the defined value.</param>
    /// <param name="rockContext">The context to read through.</param>
    /// <returns>The reference, or <c>null</c> when the value no longer exists.</returns>
    private static KeyNameResult GetDefinedValueKeyName( int definedValueId, RockContext rockContext )
    {
        var definedValue = DefinedValueCache.Get( definedValueId, rockContext );

        if ( definedValue == null )
        {
            return null;
        }

        return new KeyNameResult
        {
            Id = definedValue.Id,
            Guid = definedValue.Guid,
            Name = definedValue.Value
        };
    }

    #endregion
}
