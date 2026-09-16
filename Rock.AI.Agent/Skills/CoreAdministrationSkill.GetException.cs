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

using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single logged exception in full detail, including its stack trace and
    /// the exceptions nested beneath it.
    /// </summary>
    /// <remarks>
    /// This is the only exception tool that returns the stack trace, which is why
    /// the list tools omit it. The inner exceptions are returned as references
    /// rather than inline, so a caller walks the chain one call at a time rather
    /// than receiving an unbounded tree.
    /// </remarks>
    [Description( "Gets a single logged exception in full detail, including its stack trace and any inner exceptions." )]
    [AgentPurpose( "Reads one exception's stack trace and details to diagnose it." )]
    [AgentToolPrerequisite( "Call ListExceptions or ListExceptionInstances to determine the exceptionIdKey." )]
    [AgentToolGuid( "257A4CE8-3ED9-42F5-BFA1-AB11805261A6" )]
    public AgentToolResult GetException( string exceptionIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var exceptionLog = helper.GetRequiredEntity<Rock.Model.ExceptionLog>( exceptionIdKey, checkSecurity: false );

        if ( exceptionLog == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListExceptions )} function to determine the available exceptions." );
        }

        var exceptionLogService = new ExceptionLogService( AgentRequestContext.RockContext );

        var innerExceptions = exceptionLogService.GetByParentId( exceptionLog.Id )
            .Select( e => new { e.Id, e.Guid, e.ExceptionType } )
            .ToList()
            .Select( e => new KeyNameResult { Id = e.Id, Guid = e.Guid, Name = e.ExceptionType } )
            .ToList();

        KeyNameResult parentException = null;

        if ( exceptionLog.ParentId.HasValue )
        {
            var parent = exceptionLogService.Queryable()
                .Where( e => e.Id == exceptionLog.ParentId.Value )
                .Select( e => new { e.Id, e.Guid, e.ExceptionType } )
                .FirstOrDefault();

            if ( parent != null )
            {
                parentException = new KeyNameResult { Id = parent.Id, Guid = parent.Guid, Name = parent.ExceptionType };
            }
        }

        var site = exceptionLog.SiteId.HasValue
            ? SiteCache.Get( exceptionLog.SiteId.Value, AgentRequestContext.RockContext )
            : null;

        var page = exceptionLog.PageId.HasValue
            ? PageCache.Get( exceptionLog.PageId.Value, AgentRequestContext.RockContext )
            : null;

        var createdByPerson = exceptionLog.CreatedByPersonAlias?.Person;

        var result = new ExceptionDetailResult
        {
            Id = exceptionLog.Id,
            Guid = exceptionLog.Guid,
            CreatedDateTime = exceptionLog.CreatedDateTime,
            ExceptionType = exceptionLog.ExceptionType,
            Description = exceptionLog.Description,
            Source = exceptionLog.Source,
            StatusCode = exceptionLog.StatusCode,
            StackTrace = exceptionLog.StackTrace,
            PageUrl = exceptionLog.PageUrl,
            Site = KeyNameResult.FromCache( site ),
            Page = KeyNameResult.FromCache( page ),
            CreatedByPerson = createdByPerson != null
                ? new KeyNameResult { Id = createdByPerson.Id, Guid = createdByPerson.Guid, Name = createdByPerson.FullName }
                : null,
            ParentException = parentException,
            InnerExceptions = innerExceptions
        };

        return Success( result )
            .WithHistoryContent( new KeyNameResult( exceptionLog.Id, exceptionLog.Guid, exceptionLog.ExceptionType ) );
    }

    #endregion
}
