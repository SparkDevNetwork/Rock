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
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Model;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Lists the individual occurrences of an exception.
    /// </summary>
    /// <remarks>
    /// This is the drill-down partner of <see cref="ListExceptions"/>. Pass the
    /// exceptionType and description from a summary row to see that group's
    /// occurrences. Only outermost exceptions are listed; the stack trace and the
    /// inner exception chain of one occurrence come from <see cref="GetException"/>.
    /// </remarks>
    [Description( "Lists the individual occurrences of an exception, filtered by type and description, over a date range." )]
    [AgentPurpose( "Sees when and where a specific error has occurred." )]
    [AgentUsage( "The date range defaults to the last 7 days and cannot exceed 30 days. Pass the exceptionType and description from a ListExceptions summary row to narrow to one group, then call GetException for a full stack trace." )]
    [AgentToolPrerequisite( "Call ListExceptions to determine the exception type and description to drill into." )]
    [AgentToolGuid( "7B28303B-1081-4F93-9D5A-56F5CB98CDC7" )]
    public AgentToolResult ListExceptionInstances(
        DateTime? startDateTime = null,
        DateTime? endDateTime = null,
        [Description( "Narrows to exception types whose name contains this text." )]
        string exceptionType = null,
        [Description( "Narrows to occurrences whose description begins with this text, matching the description shown by ListExceptions." )]
        string description = null,
        string siteIdKey = null,
        string pageIdKey = null,
        int pageNumber = 1 )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        if ( !TryResolveExceptionDateRange( helper, startDateTime, endDateTime, out var start, out var end ) )
        {
            return helper.ErrorResult;
        }

        var siteId = ResolveOptionalEntityId<Rock.Model.Site>( helper, siteIdKey );
        var pageId = ResolveOptionalEntityId<Rock.Model.Page>( helper, pageIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var exceptionLogService = new ExceptionLogService( AgentRequestContext.RockContext );

        var query = exceptionLogService.Queryable()
            .Where( e => e.CreatedDateTime >= start && e.CreatedDateTime < end );

        query = exceptionLogService.FilterByOutermost( query );
        query = exceptionLogService.FilterByDescriptionPrefix( query, description );

        if ( exceptionType.IsNotNullOrWhiteSpace() )
        {
            query = query.Where( e => e.ExceptionType.Contains( exceptionType ) );
        }

        if ( siteId.HasValue )
        {
            query = query.Where( e => e.SiteId == siteId.Value );
        }

        if ( pageId.HasValue )
        {
            query = query.Where( e => e.PageId == pageId.Value );
        }

        // Project the lightweight columns before paging so the large fields (stack
        // trace, form, cookies, server variables) are never pulled for a list.
        var projected = query
            .Select( e => new
            {
                e.Id,
                e.Guid,
                e.ExceptionType,
                e.Description,
                e.Source,
                e.StatusCode,
                e.CreatedDateTime,
                HasInnerException = e.HasInnerException ?? false,
                SiteId = e.SiteId,
                SiteGuid = ( Guid? ) e.Site.Guid,
                SiteName = e.Site.Name,
                PageId = e.PageId,
                PageGuid = ( Guid? ) e.Page.Guid,
                PageName = e.Page.InternalName
            } )
            .OrderByDescending( e => e.CreatedDateTime )
            .ThenByDescending( e => e.Id );

        var page = helper.GetPaginatedItems( projected, pageNumber );

        var resultPage = page.WithItems( page.Items
            .Select( e => new ExceptionInstanceResult
            {
                Id = e.Id,
                Guid = e.Guid,
                CreatedDateTime = e.CreatedDateTime,
                ExceptionType = e.ExceptionType,
                Description = e.Description,
                Source = e.Source,
                StatusCode = e.StatusCode,
                HasInnerException = e.HasInnerException,
                Site = e.SiteId.HasValue
                    ? new KeyNameResult { Id = e.SiteId.Value, Guid = e.SiteGuid, Name = e.SiteName }
                    : null,
                Page = e.PageId.HasValue
                    ? new KeyNameResult { Id = e.PageId.Value, Guid = e.PageGuid, Name = e.PageName }
                    : null
            } )
            .ToList() );

        var historyPage = page.WithItems( page.Items
            .Select( e => new KeyNameResult { Id = e.Id, Guid = e.Guid, Name = e.ExceptionType } )
            .ToList() );

        // Page number rather than a cursor. Exception rows carry no per-row person
        // authorization; access is governed by the admin skill and the entity
        // type, matching the core Exception List block, which applies no per-row
        // filter. Skip/Take is therefore safe here.
        return helper.GetPaginatedResult( resultPage, historyPage );
    }

    #endregion
}
