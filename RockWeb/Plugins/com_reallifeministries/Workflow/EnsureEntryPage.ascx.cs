using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Web.UI;
using System.Linq;
using System.Data.Entity;

using Rock;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Security;
using Rock.Workflow;

namespace com.reallifeministries.Workflow
{

    [DisplayName( "Ensure Entry Page" )]
    [Category( "WorkFlow" )]
    [Description( "Used with entry page workflow action. Watches the specified entry page for the workflow, and redirects to the specified page if this page doesn't match." )]

    [TextField("Workflow Attributes To Page Parameters", "The attributes of the workflow to watch, and set as parameters to the page. (comma separated)",false,"","",0,"WorkflowAttributes")]
    public partial class EnsureEntryPage : Rock.Web.UI.RockBlock
    {

        #region Fields

         private RockContext _rockContext = null;
         private WorkflowService _workflowService = null;
         private Rock.Model.Workflow _workflow = null;
         private WorkflowActivity _activity = null;
         private int? _workflowId;

        #endregion

        #region Properties
                /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        public int? WorkflowId
        {
            get { return _workflowId; }
            set { _workflowId = value; }
        }

        #endregion
        
        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            if ( _workflowService == null )
            {
                _workflowService = new WorkflowService( _rockContext );
            }
            var paramWorkflowId = PageParameter( "WorkflowId" );
            WorkflowId = paramWorkflowId.AsIntegerOrNull();
            if ( !WorkflowId.HasValue )
            {
                Guid guid = PageParameter( "WorkflowGuid" ).AsGuid();
                if ( !guid.IsEmpty() )
                {
                    _workflow = _workflowService.Queryable()
                        .Where( w => w.Guid.Equals( guid ) )
                        .FirstOrDefault();
                    if ( _workflow != null )
                    {
                        WorkflowId = _workflow.Id;
                    }
                }
            }

            if ( WorkflowId.HasValue )
            {
                if ( _workflow == null )
                {
                    _workflow = _workflowService.Queryable()
                        .Where( w => w.Id == WorkflowId.Value )
                        .FirstOrDefault();
                }
                 //-------------------------------
                if ( _workflow != null )
                {
                    _workflow.LoadAttributes();
                    
                }

            }

            if ( _workflow != null )
            {
                if ( _workflow.IsActive )
                {
                    int personId = CurrentPerson != null ? CurrentPerson.Id : 0;
                    foreach ( var activity in _workflow.Activities
                        .Where( a =>
                            a.IsActive &&
                            (
                                ( !a.AssignedGroupId.HasValue && !a.AssignedPersonAliasId.HasValue ) ||
                                ( a.AssignedPersonAlias != null && a.AssignedPersonAlias.PersonId == personId ) ||
                                ( a.AssignedGroup != null && a.AssignedGroup.Members.Any( m => m.PersonId == personId ) )
                            )
                        )
                        .OrderBy( a => a.ActivityType.Order ) )
                    {
                        if ( ( activity.ActivityType.IsAuthorized( Authorization.VIEW, CurrentPerson ) ) )
                        {
                            foreach ( var action in activity.ActiveActions )
                            {
                                if ( action.ActionType.WorkflowForm != null && action.IsCriteriaValid )
                                {
                                    _activity = activity;
                                    _activity.LoadAttributes(_rockContext);
                                }
                            }
                        }
                    }

                    if (_activity != null) 
                    {
                      
                        var entryPage = _workflow.GetAttributeValue("EntryFormPage");
                        
                        if (!String.IsNullOrEmpty(entryPage))
                        {
                            var queryParams = new Dictionary<string, string>();
                            queryParams.Add( "WorkflowTypeId", _activity.Workflow.WorkflowTypeId.ToString() );
                            queryParams.Add( "WorkflowId", _activity.WorkflowId.ToString() );

                            var attrsToSend = GetAttributeValue( "WorkflowAttributes" );

                            if (!String.IsNullOrWhiteSpace( attrsToSend ))
                            {
                                foreach (var attr in attrsToSend.Split( ',' ))
                                {
                                    var attrName = attr.Trim();
                                    if (!String.IsNullOrEmpty( _activity.GetAttributeValue( attrName ) ))
                                    {
                                        queryParams.Add( attrName, _activity.GetAttributeValue( attrName ) );
                                    }
                                    else if (!String.IsNullOrEmpty( _activity.Workflow.GetAttributeValue( attrName ) ))
                                    {
                                        queryParams.Add( attrName, _activity.Workflow.GetAttributeValue( attrName ) );
                                    }
                                }
                            }

                            var pageReference = new Rock.Web.PageReference( entryPage, queryParams );

                            bool paramsDiffer = false;

                            foreach (var pair in queryParams) {
                                if(pair.Value != PageParameter(pair.Key)) {
                                    paramsDiffer = true;
                                    break;
                                }
                            }

                            if (paramsDiffer || (pageReference.PageId != CurrentPageReference.PageId))
                            {
                                Response.Redirect(pageReference.BuildUrl(), true);
                            }
                        }
                    }
                
                }

            }       
        }


        private void ShowMessage( NotificationBoxType type, string title, string message )
        {
            nbMessage.NotificationBoxType = type;
            nbMessage.Title = title;
            nbMessage.Text = message;
            nbMessage.Visible = true;
            nbMessage.Dismissable = false;

        }

    }
}