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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.Utilities;

namespace Rock.Reporting.DataFilter.Person
{
    /// <summary>
    /// 
    /// </summary>
    [Description( "Filter people based by the communication status on the specific communication." )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Communication Status Filter" )]
    public class CommunicationStatusFilter : DataFilterComponent
    {
        private const string _CtlCommunicationId = "tbCommunicationId";
        private const string _CtlCommunicationStatus = "ddlCommmunicationStatus";

        private enum CommunicationStatusType
        {
            Open = 0,
            Clicked = 1,
            Unopened = 2,
            Failed = 3
        }

        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return typeof( Rock.Model.Person ).FullName; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return "Additional Filters"; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <value>
        /// The title.
        /// </value>
        public override string GetTitle( Type entityType )
        {
            return "Communication Status";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return @"
        function ()
        {    
            var result = 'Communication Id:';
    
            var communicationId = $('.js-communicationId', $content).val();
            result += ' ""' + communicationId + '""';

            var communicationStatus = $('.js-filter-status option:selected', $content).text();    
            if (communicationStatus)
            {
               result = result + ', with Status:' + communicationStatus;
            }

           return result;
        }
";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            string result = "Communication Status ";
            string[] selectionValues = selection.Split( '|' );
            if ( selectionValues.Length >= 2 )
            {
                var communicationIdText = selectionValues[0];
                if ( communicationIdText.IsNotNullOrWhiteSpace() )
                {
                    result = $"Communication Id: '{communicationIdText}'";
                }

                var communicationStatus = selectionValues[1].ConvertToEnum<CommunicationStatusType>();
                result += $", with Status: {communicationStatus.ToString()}";
            }

            return result;
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl )
        {
            var tbCommunicationId = new NumberBox();
            tbCommunicationId.Label = "Communication Id";
            tbCommunicationId.ID = filterControl.GetChildControlInstanceName( _CtlCommunicationId );
            tbCommunicationId.CssClass = "js-communicationId";
            tbCommunicationId.Required = true;
            tbCommunicationId.NumberType = ValidationDataType.Integer;
            filterControl.Controls.Add( tbCommunicationId );

            // Define Control: Communication Status DropDown List
            var ddlCommunicationStatus = new RockDropDownList();
            ddlCommunicationStatus.ID = filterControl.GetChildControlInstanceName( _CtlCommunicationStatus );
            ddlCommunicationStatus.Label = "Communication Status";
            ddlCommunicationStatus.Help = "Specifies the type of Communication Status that the recipient must have to be included in the result.";
            ddlCommunicationStatus.Required = true;
            ddlCommunicationStatus.AddCssClass( "js-filter-status" );
            ddlCommunicationStatus.BindToEnum<CommunicationStatusType>();
            filterControl.Controls.Add( ddlCommunicationStatus );

            return new Control[] { tbCommunicationId, ddlCommunicationStatus };
        }

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls )
        {
            if ( controls.Count() >= 2 )
            {
                var communicationId = ( controls[0] as NumberBox ).Text.AsInteger();

                var communicationStatusType = ( controls[1] as RockDropDownList ).SelectedValueAsEnum<CommunicationStatusType>();

                return string.Format( "{0}|{1}", communicationId, communicationStatusType );
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( controls.Count() >= 2 )
            {
                NumberBox nbCommunicationId = controls[0] as NumberBox;
                RockDropDownList ddlCommunicationStatus = controls[1] as RockDropDownList;

                string[] selectionValues = selection.Split( '|' );
                if ( selectionValues.Length >= 2 )
                {
                    nbCommunicationId.Text = selectionValues[0];
                    ddlCommunicationStatus.SetValue( ( int ) selectionValues[1].ConvertToEnum<CommunicationStatusType>() );
                }
            }
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="serviceInstance">The service instance.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            string[] selectionValues = selection.Split( '|' );
            var rockContext = ( RockContext ) serviceInstance.Context;
            if ( selectionValues.Length >= 2 )
            {
                var communicationId = selectionValues[0].AsInteger();
                var communicationStatusType = selectionValues[1].ConvertToEnum<CommunicationStatusType>();
                var communicationRecipients = new CommunicationRecipientService( rockContext ).GetByCommunicationId( communicationId );

                var interactionChannelCommunication = new InteractionChannelService( rockContext ).Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );
                var interactionQuery = new InteractionService( rockContext ).Queryable()
                                                .Where( a => a.InteractionComponent.ChannelId == interactionChannelCommunication.Id &&
                                                a.InteractionComponent.EntityId.Value == communicationId );
                CommunicationRecipientStatus[] sentStatus = new CommunicationRecipientStatus[] { CommunicationRecipientStatus.Opened, CommunicationRecipientStatus.Delivered };

                switch ( communicationStatusType )
                {
                    case CommunicationStatusType.Open:
                        {
                            interactionQuery = interactionQuery.Where( a => a.Operation == "Opened" );
                            communicationRecipients = communicationRecipients.Where( a => sentStatus.Contains( a.Status ) && interactionQuery.Any(b=>b.EntityId==a.Id) );
                        }
                        break;
                    case CommunicationStatusType.Clicked:
                        {
                            interactionQuery = interactionQuery.Where( a => a.Operation == "Click" );
                            communicationRecipients = communicationRecipients.Where( a => sentStatus.Contains( a.Status ) && interactionQuery.Any( b => b.EntityId == a.Id ) );
                        }
                        break;
                    case CommunicationStatusType.Unopened:
                        {
                            interactionQuery = interactionQuery.Where( a => a.Operation == "Click" || a.Operation == "Opened" );
                            communicationRecipients = communicationRecipients.Where( a => sentStatus.Contains( a.Status ) && !interactionQuery.Any( b => b.EntityId == a.Id ) );
                        }
                        break;
                    case CommunicationStatusType.Failed:
                    default:
                        {
                            CommunicationRecipientStatus[] failedStatus = new CommunicationRecipientStatus[] { CommunicationRecipientStatus.Failed };
                            communicationRecipients = communicationRecipients.Where( a => failedStatus.Contains( a.Status ) );
                        }
                        break;
                }

                var qry = new PersonService( ( RockContext ) serviceInstance.Context ).Queryable()
                    .Where( p => communicationRecipients.Any( x => x.PersonAlias.PersonId == p.Id ) );

                return FilterExpressionExtractor.Extract<Rock.Model.Person>( qry, parameterExpression, "p" );
            }

            return null;
        }

        #endregion
    }
}