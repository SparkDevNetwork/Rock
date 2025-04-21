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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.Entity;
using Rock.Security;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Benevolence Request Statement Lava" )]
    [Category( "Finance" )]
    [Description( "Block for displaying a Lava based Benevolence Request detail." )]
    [CodeEditorField( "Lava Template", "The Lava template to use for the Benevolence Request statement.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 500, true, @"
{% capture pageTitle %}
    Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}
{% endcapture %}
{{ pageTitle | SetPageTitle }}

<div class=""row"">
    <div class=""col-md-6"">
        <div class=""pull-left"">
            <img src=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ 'Global' | Attribute:'EmailHeaderLogo' }}"" width=""100px"" />
        </div>
        
        <div class=""pull-left margin-l-md margin-t-sm"">
            <strong>{{ 'Global' | Attribute:'OrganizationName' }}</strong><br />
            {{ 'Global' | Attribute:'OrganizationAddress' }}<br />
            {{ 'Global' | Attribute:'OrganizationWebsite' }}
        </div>
    </div>
    <div class=""col-md-6 text-right hidden-print"">
        <h4>Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}</h4>
        <p>Requested: {{ Request.RequestDateTime | Date:'M/d/yyyy' }}<p>
        <p>Status: <strong>{{ Request.RequestStatusValue.Value }}</strong></p>
    </div>
</div>
<hr style=""opacity: .5;"" />
<div class=""row"">
    <div class=""col-xs-12"">
        <h4 class=""visible-print-block"">Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}
        <br />
        <small>Requested: {{ Request.RequestDateTime | Date:'M/d/yyyy' }} Status: <strong>{{ Request.RequestStatusValue.Value }}</strong></small></h4>
    </div>
</div>
<div class=""row"">
    <div class=""col-md-12"">
        <h4>Attributes:</h4>
        {% for attribute in Request.AttributeValues %}
            <p>{{ attribute.AttributeName }}: {{ attribute.ValueFormatted }}</p>
        {% endfor %}
    </div>
</div>
<div class=""row"">
    <div class=""col-xs-4"">
        <h4>Requested by</h3>
        <p>
            <strong>{{ Request.FirstName }} {{ Request.LastName }}</strong> <br />
            {{ Request.Location.FormattedHtmlAddress }} <br />
            {% if Request.HomePhoneNumber %}
               Home Phone: {{ Request.HomePhoneNumber }} <br />
            {% endif %}
            {% if Request.CellPhoneNumber %}
               Cell Phone: {{ Request.CellPhoneNumber }}
            {% endif %}
            {% if Request.WorkPhoneNumber %}
                {{ Request.WorkPhoneNumber }}
            {% endif %}
        </p>
    </div>
    <div class=""col-xs-4"">
        {% if Request.RequestText != empty %}
        <h4>Request</h4>
        <p>{{ Request.RequestText }} </p>
        {% endif %}
    </div>
    <div class=""col-xs-4"">
        <div class=""clearfix"">
            <div class=""pull-right"">
                <a href=""#"" class=""btn btn-primary hidden-print"" onClick=""window.print();""><i class=""fa fa-print""></i> Print Request</a> 
            </div>
        </div>
    </div>
</div>
<div class=""row"">
    <div class=""col-xs-4"">
        {% if Request.CaseWorkerPersonAlias != null %}
            {% assign caseworker = Request.CaseWorkerPersonAlias.Person %}
            <h4>Case Worker</h4>
            <p>
                <strong>{{ caseworker.FullName }}</strong> <br />
                {{ caseworker | Address:'Home' }} <br />
                {% assign CaseWorkerHome = caseworker | PhoneNumber:'Home' %}
                {% assign CaseWorkerCell = caseworker | PhoneNumber:'Mobile' %}
                {% assign CaseWorkerWork = caseworker | PhoneNumber:'Work' %}
                {% if CaseWorkerHome %}
                   Home Phone: {{ CaseWorkerHome }} <br />
                {% endif %}
                {% if CaseWorkerCell %}
                   Cell Phone: {{ CaseWorkerCell }} <br />
                {% endif %}
                {% if CaseWorkerWork %}
                   Work Phone: {{ CaseWorkerWork }}
                {% endif %}
            </p>
        {% endif %}
    </div>
    <div class=""col-xs-4"">
        <h4>Summary</h4>
        <p> {{ Request.ResultSummary }} </p>
    </div>
    <div class=""col-xs-4"">
        <h4>Next Steps</h4>
        <p> {{ Request.ProvidedNextSteps }} </p>
    </div>
</div>

        
{%if Request.BenevolenceResults != empty %}
    <div class=""row"">
        <div class=""col-xs-12"">
            <hr style=""opacity: .5;"" />
            
            <h4 class=""margin-t-md margin-b-md"">Results List</h4>
            
            <table class=""table table-bordered table-striped table-condensed"">
                <tr>
                    <th>Type</th>
                    <th>Amount</th>
                    <th>Details</th>
                </tr>
            
                {% for result in Request.BenevolenceResults %}
                    <tr>
                        <td>{{ result.ResultTypeValue.Value }}</td>
                        <td>{{ result.Amount }}</td>
                        <td>{{ result.ResultSummary }}</td>
                    </tr>
                {% endfor %}
            
            </table>
        </div>
    </div>
{% endif %}", order: 2 )]
    [Rock.SystemGuid.BlockTypeGuid( "C2D8FCA3-BC8F-44FF-85AA-440BF41CEF5D" )]
    public partial class BenevolenceRequestStatementLava : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        // Overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                DisplayResults();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            DisplayResults();
        }

        #endregion

        #region Methods

        private void DisplayResults()
        {
            RockContext rockContext = new RockContext();
            var benevolenceRequestId = PageParameter( "BenevolenceRequestId" ).AsInteger();
            if ( benevolenceRequestId > 0 )
            {
                var benevolenceRequest = new BenevolenceRequestService( rockContext ).Get( benevolenceRequestId );

                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add( "Request", benevolenceRequest );

                var template = GetAttributeValue( "LavaTemplate" );

                lResults.Text = template.ResolveMergeFields( mergeFields );
            }
        }

        #endregion
    }
}