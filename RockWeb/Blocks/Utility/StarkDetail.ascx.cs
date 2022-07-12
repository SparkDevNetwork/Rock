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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Tasks;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Stark Detail" )]
    [Category( "Utility" )]
    [Description( "Template block for developers to use to start a new detail block." )]

    #region Block Attributes

    [BooleanField(
        "Show Email Address",
        Key = AttributeKey.ShowEmailAddress,
        Description = "Should the email address be shown?",
        DefaultBooleanValue = true,
        Order = 1 )]

    [EmailField(
        "Email",
        Key = AttributeKey.Email,
        Description = "The Email address to show.",
        DefaultValue = "ted@rocksolidchurchdemo.com",
        Order = 2 )]

    #endregion Block Attributes
    public partial class StarkDetail : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            public const string ShowEmailAddress = "ShowEmailAddress";
            public const string Email = "Email";
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string StarkId = "StarkId";
        }

        #endregion PageParameterKeys

        #region Fields

        // Used for private variables.

        #endregion

        #region Properties

        // Used for public / protected properties.

        #endregion

        #region Base Control Methods

        // Overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );


            if ( !Page.IsPostBack )
            {
                // Added for your convenience.

                // To show the created/modified by date time details in the PanelDrawer do something like this:
                // pdAuditDetails.SetEntity( <YOUROBJECT>, ResolveRockUrl( "~" ) );
            }
        }

        private static void TestConcurrent_ProcessSendPaymentReceiptEmails()
        {
            Debug.WriteLine( "TestConcurrent_ProcessSendPaymentReceiptEmails Loop" );
            var financialTransactionList = new FinancialTransactionService( new RockContext() ).Queryable()
                .Where( a => a.AuthorizedPersonAlias.Person.Email != "" && a.AuthorizedPersonAlias.Person.Email != null )
                .Select( a => new
                {
                    a.Id,
                    a.AuthorizedPersonAlias.PersonId
                } ).ToList();


            var financialTransactionIds = financialTransactionList
                .GroupBy( a => a.PersonId ).Select( a => a.OrderBy( x => Guid.NewGuid() ).FirstOrDefault() )
                .Select( a => a.Id ).OrderBy( a => Guid.NewGuid() ).ToArray();

            var financialTransactionIds2 = financialTransactionIds.OrderBy( a => Guid.NewGuid() ).ToArray();

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 200 };

            // New Spring
            var systemEmailGuid = "72aa54e5-90bc-4866-a0f9-1e494e96d9a5".AsGuid();

            Debug.WriteLine( "Starting Loop" );

            Stopwatch stopwatch = Stopwatch.StartNew();
            int loopCount = 100; //            financialTransactionIds.Count() - 1;
            //LavaTemplateCache.EnableCache = true;

            var lavaTemplateNewSpring = new SystemCommunicationService( new RockContext() ).GetSelect( systemEmailGuid, s => s.Body );
            var lavaTemplateSmall = "Hello {{ SomeColor }} World";
            var lavaTemplateCore = new SystemCommunicationService( new RockContext() ).GetSelect( Rock.SystemGuid.SystemCommunication.FINANCE_GIVING_RECEIPT.AsGuid(), s => s.Body );
            var coreMergeFields = LavaHelper.GetCommonMergeFields( null );

            var lavaTemplateKitchenSink1 = @"
{% calendarevents calendarid:'1' audienceids:'' startdate:'1/1/2000' %}
    {% for item in EventScheduledInstances %}
        {{ item.Name }} 
        on {{ item.Date }}
        at {{ item.Time }}
        for {{ item.AudienceNames | Join:', ' }} 

        Contact: {{ item.EventItemOccurrence.ContactEmail }}
    {% endfor %}
{% endcalendarevents %}";

            var lavaTemplateKitchenSink2 = @"

{% execute import:'RestSharp,Newtonsoft.Json,Newtonsoft.Json.Linq' %}
    

    int i = 1 + 3;
    int x = 4 * i;

    return x.ToString();    
{% endexecute %}
";

            try
            {
                Parallel.For( 1, loopCount, parallelOptions, ( x ) =>
                {
                    try
                    {
                        var financialTransactionId = financialTransactionIds[x];


                        var threadMergeFields = new Dictionary<string, object>( coreMergeFields );
                        threadMergeFields.Add( Guid.NewGuid().ToString(), Guid.NewGuid() );
                        threadMergeFields.Add( "SomethingElse", Guid.NewGuid() );
                        threadMergeFields.Add( "SomeColor", "Blue" );
                        //threadMergeFields.Add( "Custom Person", person );

                        var result = lavaTemplateKitchenSink2.ResolveMergeFields( threadMergeFields, enabledLavaCommands: "all" );

                        var result2 = result.Trim();

                        if ( result2 != @"16" )
                        {
                            Debug.WriteLine( $" Oh OH {result2}" );
                        }


                        /*new ProcessSendPaymentReceiptEmails.Message
                        {
                            TransactionId = financialTransactionId,
                            SystemEmailGuid = systemEmailGuid
                        }.Send();


                        var financialTransactionId2 = financialTransactionIds2[x];
                        new ProcessSendPaymentReceiptEmails.Message
                        {
                            TransactionId = financialTransactionId2,
                            SystemEmailGuid = Rock.SystemGuid.SystemCommunication.FINANCE_GIVING_RECEIPT.AsGuid()
                        }.Send();
                        */
                    }
                    catch
                    {
                        //
                    }
                }
                );

            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex );
            }
            finally
            {

                //stopwatch.Stop();
                //Debug.WriteLine( $"{ stopwatch.Elapsed.TotalMilliseconds} ms, loopCount:{loopCount}, ms/loop:{stopwatch.Elapsed.TotalMilliseconds/loopCount}" );
            }

            Debug.WriteLine( "done" );
        }

        #endregion

        #region Events

        // Handlers called by the controls on your block.

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion

        protected void btnTest_Click( object sender, EventArgs e )
        {
            TestConcurrent_ProcessSendPaymentReceiptEmails();
        }
    }
}