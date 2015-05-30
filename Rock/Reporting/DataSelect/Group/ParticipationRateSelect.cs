// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Reporting.DataSelect.Group
{
    /// <summary>
    /// A Report Field that shows the number of Group Members that also exist in a population defined by a Person Data View.
    /// </summary>
    [Description( "Shows the number of Group Members that are participating from a set of candidates defined by a Person Data View" )]
    [Export( typeof( DataSelectComponent ) )]
    [ExportMetadata( "ComponentName", "Participation Rate" )]
    public class ParticipationRateSelect : DataSelectComponent
    {
        #region Properties

        public override string AppliesToEntityType
        {
            get
            {
                return typeof( Rock.Model.Group ).FullName;
            }
        }

        public override string Section
        {
            get
            {
                return "Statistics";
            }
        }

        public override string ColumnPropertyName
        {
            get
            {
                return "Participation Rate";
            }
        }

        public override Type ColumnFieldType
        {
            get { return typeof( decimal? ); }
        }

        public override string ColumnHeaderText
        {
            get
            {
                return "Participation Rate";
            }
        }

        #endregion

        #region Methods

        public override string GetTitle( Type entityType )
        {
            return "Participation Rate";
        }

        public override Expression GetExpression( RockContext context, MemberExpression entityIdProperty, string selection )
        {
            var settings = new ParticipationRateSelectSettings(selection);

            if (!settings.IsValid())
            {
                return this.GetDefaultSelectExpression( context, entityIdProperty );
            }

            // Get the Person Data View that defines the set of candidates from which matching Group Members can be selected.
            DataView dataView = null;

            if ( settings.DataViewGuid.HasValue )
            {
                var dsService = new DataViewService( context );

                dataView = dsService.Get( settings.DataViewGuid.Value );

                // Verify that there is not a child filter that uses this view (would result in stack-overflow error)
                if ( dsService.IsViewInFilter( dataView.Id, dataView.DataViewFilter ) )
                {
                    throw new Exception( "Filter issue(s): One of the filters contains a circular reference to the Data View itself." );
                }
            }

            if ( dataView == null
                || dataView.DataViewFilter == null )
            {
                return this.GetDefaultSelectExpression( context, entityIdProperty );
            }

            // Evaluate the Data View that defines the candidate population.
            List<string> errorMessages;

            var personService = new PersonService( context );

            var personQuery = personService.Queryable();

            var paramExpression = personService.ParameterExpression;

            var whereExpression = dataView.GetExpression( personService, paramExpression, out errorMessages );

            if ( errorMessages.Any() )
            {
                throw new Exception( "Filter issue(s): " + errorMessages.AsDelimited( "; " ) );
            }

            personQuery = personQuery.Where( paramExpression, whereExpression, null );

            var populationIds = personQuery.Select( x => x.Id );

            // Construct the Query to return the measure of matches for each Group.
            IQueryable<decimal> resultQuery;

            switch ( settings.MeasureType )
            {
                case MeasureTypeSpecifier.ParticipationRateOfGroup:
                    {
                        // Percentage of Group Members that are also in the candidate population.
                        resultQuery = new GroupService( context ).Queryable()
                                                                 .Select( p => ( p.Members.Count == 0 ) ? 0 : ( (decimal)p.Members.Count( a => ( populationIds.Contains( a.PersonId ) ) ) / (decimal)p.Members.Count ) * 100 );                         
                    }
                    break;
                case MeasureTypeSpecifier.ParticipationRateOfCandidates:
                    {
                        // Percentage of candidate population that are also Group Members.
                        decimal populationCount = populationIds.Count();

                        resultQuery = new GroupService( context ).Queryable()
                                                                 .Select( p => ( p.Members.Count == 0 ) ? 0 : ( (decimal)p.Members.Count( a => ( populationIds.Contains( a.PersonId ) ) ) / populationCount ) * 100 );
                    }
                    break;
                case MeasureTypeSpecifier.NumberOfParticipants:
                default:
                    {
                        // Number
                        resultQuery = new GroupService( context ).Queryable()
                                                                 .Select( p => (decimal)p.Members.Count( a => populationIds.Contains( a.PersonId ) ) );
                    }
                    break;
            }

            var selectExpression = SelectExpressionExtractor.Extract<Rock.Model.Group>( resultQuery, entityIdProperty, "p" );

            return selectExpression;
        }

        private Expression GetDefaultSelectExpression( RockContext context, MemberExpression entityIdProperty )
        {
            var resultQuery = new GroupService( context ).Queryable().Select( p => (decimal)0 );

            var selectExpression = SelectExpressionExtractor.Extract<Rock.Model.Group>( resultQuery, entityIdProperty, "p" );

            return selectExpression;
        }

        public override System.Web.UI.Control[] CreateChildControls( System.Web.UI.Control parentControl )
        {
            // Define Control: Person Data View Picker
            int entityTypeId = EntityTypeCache.Read( typeof( Rock.Model.Person ) ).Id;

            var ddlDataView = new DataViewPicker();
            ddlDataView.ID = string.Format( "{0}_ddlDataView", parentControl.ID );
            ddlDataView.Label = "Candidate Data View";
            ddlDataView.Help = "The Data View that returns the set of people from which participation in the Group is measured.";

            parentControl.Controls.Add( ddlDataView );

            ddlDataView.EntityTypeId = entityTypeId;

            RockDropDownList ddlFormat = new RockDropDownList();
            ddlFormat.ID = string.Format( "{0}_ddlFormat", parentControl.ID );
            ddlFormat.Label = "Measure Type";
            ddlFormat.Items.Add( new ListItem( "Number of Participants in Group", MeasureTypeSpecifier.NumberOfParticipants.ToString() ) );
            ddlFormat.Items.Add( new ListItem( "Participation Rate of Group", MeasureTypeSpecifier.ParticipationRateOfGroup.ToString() ) );
            ddlFormat.Items.Add( new ListItem( "Participation Rate of Candidates", MeasureTypeSpecifier.ParticipationRateOfCandidates.ToString() ) );
            parentControl.Controls.Add( ddlFormat );

            return new System.Web.UI.Control[] { ddlDataView, ddlFormat };
        }

        public override string GetSelection( System.Web.UI.Control[] controls )
        {
            var ddlDataView = (DataViewPicker)controls[0];
            var ddlFormat = (DropDownList)controls[1];

            var settings = new ParticipationRateSelectSettings();

            settings.ParseMeasureType(ddlFormat.SelectedValue);
            settings.ParseDataViewId(ddlDataView.SelectedValue);

            return settings.ToSelectionString();
        }

        public override void SetSelection( System.Web.UI.Control[] controls, string selection )
        {
            var settings = new ParticipationRateSelectSettings( selection );

            if ( !settings.IsValid() )
            {
                return;
            }

            var ddlDataView = (DataViewPicker)controls[0];
            var ddlFormat = (DropDownList)controls[1];

            if ( settings.DataViewGuid.HasValue )
            {
                var dsService = new DataViewService( new RockContext() );

                var dataView = dsService.Get( settings.DataViewGuid.Value );

                if ( dataView != null )
                {
                    ddlDataView.SelectedValue = dataView.Id.ToString();
                }
            }

            ddlFormat.SelectedValue = settings.MeasureType.ToString();
        }

        #endregion

        #region Settings

        private enum MeasureTypeSpecifier
        {
            NumberOfParticipants = 0,
            ParticipationRateOfGroup = 1,
            ParticipationRateOfCandidates = 2
        }

        /// <summary>
        ///     Settings for the Data Select Component "Group Participation Rate".
        /// </summary>
        private class ParticipationRateSelectSettings
        {
            public MeasureTypeSpecifier MeasureType = MeasureTypeSpecifier.NumberOfParticipants;
            public Guid? DataViewGuid;

            public ParticipationRateSelectSettings()
            {
                //
            }

            public ParticipationRateSelectSettings( string settingsString )
            {
                FromSelectionString( settingsString );
            }

            public bool IsValid()
            {
                return ( DataViewGuid.HasValue );
            }

            /// <summary>
            ///     Set values from a string representation of the settings.
            /// </summary>
            /// <param name="selectionString"></param>
            public void FromSelectionString( string selectionString )
            {
                var selectionValues = selectionString.Split( '|' );

                // If selection string is invalid, ignore.
                if ( selectionValues.Length < 2 )
                {
                    return;
                }

                DataViewGuid = selectionValues[0].AsGuidOrNull();

                this.ParseMeasureType(selectionValues[1]);
            }

            public void ParseMeasureType(string measureTypeName)
            {
                if ( measureTypeName != null )
                {
                    Enum.TryParse( measureTypeName, true, out MeasureType );
                }
                else
                {
                    MeasureType = MeasureTypeSpecifier.NumberOfParticipants;
                }                
            }

            public void ParseDataViewId(string dataViewId)
            {
                var id = dataViewId.AsIntegerOrNull();

                if ( id != null )
                {
                    var dsService = new DataViewService( new RockContext() );

                    var dataView = dsService.Get( id.Value );

                    DataViewGuid = dataView.Guid;
                }
                else
                {
                    DataViewGuid = null;
                }                
            }

            public string ToSelectionString()
            {
                return DataViewGuid + "|" + ( (int)MeasureType );
            }
        }

        #endregion
    }
}
