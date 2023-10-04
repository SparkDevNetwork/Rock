
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache;

namespace rocks.pillars.Jobs
{
    [DisplayName( "Sync DataView To Tag" )]
    [Description( "" )]
    [DisallowConcurrentExecution]

    #region Job Attributes

    [DataViewField( "Data View", "The data view to sync to a tag.", true, "", "Rock.Model.Person", "", 0, AttributeKey.DataView )]
    [CustomDropdownListField( "Organizational Tag", "", "SELECT [Name] AS [Text], [Guid] AS [Value] FROM [Tag] WHERE [EntityTypeId] = 15 AND [OwnerPersonAliasId] IS NULL ORDER BY [Name]", true, "", "", 1, AttributeKey.OrganizationalTag )]

    #endregion

    public class SyncDataViewToTag : IJob
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DataView = "DataView";

            public const string OrganizationalTag = "OrganizationalTag";
        }

        #endregion

        /// <summary>
        /// Perform the job using the parameters supplied in the execution context.
        /// </summary>
        /// <param name="context"></param>
        public void Execute( IJobExecutionContext context )
        {
            int peopleRemovedFromTag = 0;
            int peopleAddedToTag = 0;

            // Get the configuration settings for this job instance.
            var dataMap = context.JobDetail.JobDataMap;

            var dataViewGuid = dataMap.GetString( AttributeKey.DataView ).AsGuidOrNull();
            var organizationTag = dataMap.GetString( AttributeKey.OrganizationalTag ).AsGuidOrNull();

            if ( dataViewGuid == null )
            {
                throw new Exception( "Data View is not configured." );
            }

            if ( organizationTag == null )
            {
                throw new Exception( "Organizational Tag is not configured." );
            }

            var rockContext = new RockContext();
            rockContext.Database.CommandTimeout = 600;

            var dataViewService = new DataViewService( rockContext );
            var personService = new PersonService( rockContext ).Queryable().AsNoTracking();
            var tagService = new TagService( rockContext );
            var taggedItemService = new TaggedItemService( rockContext );

            var personEntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.PERSON.AsGuid() ).Id;

            var dataView = dataViewService.Get( dataViewGuid.Value );
            var tag = tagService.Get( organizationTag.Value );

            if ( dataView == null )
            {
                throw new Exception( "Data View not found." );
            }

            if ( tag == null )
            {
                throw new Exception( "Organizational Tag not found." );
            }

            var errorMessages = new List<string>();

            // get a list of personGuids from the data view
            var dvArgs = new DataViewGetQueryArgs
            {
                DbContext = rockContext
            };
            var qry = dataView.GetQuery( dvArgs );
            IQueryable<Guid> personGuids = personService.Where( p => qry.Select( e => e.Id ).Contains( p.Id ) ).Select( p => p.Guid );

            // remove anyone from the tag that is not in the data view.
            var taggedItemsToDelete = taggedItemService.Queryable()
                .Where( ti => ti.TagId == tag.Id &&
                              !personGuids.Contains( ti.EntityGuid ) )
                .ToList();

            peopleRemovedFromTag = taggedItemsToDelete.Count;

            taggedItemService.DeleteRange( taggedItemsToDelete );
            rockContext.SaveChanges();

            // add anyone from the data view that isn't already in the tag.
            var peopleGuidsInTag = taggedItemService.Queryable().AsNoTracking()
                .Where( ti => ti.TagId == tag.Id )
                .Select( ti => ti.EntityGuid );

            var peopleWhoNeedTag = personService.Where( p => !peopleGuidsInTag.Contains( p.Guid ) && personGuids.Contains( p.Guid ) ).Select( p => p.Guid ).ToList();

            foreach ( var personGuid in peopleWhoNeedTag )
            {
                var tagContext = new RockContext();
                taggedItemService = new TaggedItemService( tagContext );

                TaggedItem item = new TaggedItem();
                item.EntityTypeId = personEntityTypeId;
                item.EntityGuid = personGuid;
                item.TagId = tag.Id;

                taggedItemService.Add( item );

                peopleAddedToTag++;
                
                tagContext.SaveChanges();
            }

            context.Result = string.Format( "{0} people removed from tag. {1} people added to tag.", peopleRemovedFromTag, peopleAddedToTag );
        }
    }
}

