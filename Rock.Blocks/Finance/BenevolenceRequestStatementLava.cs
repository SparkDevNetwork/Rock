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

using System.ComponentModel;

using Rock.Attribute;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Finance.BenevolenceRequestStatementLava;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Finance
{
    /// <summary>
    /// Displays a Lava based statement for a single benevolence request.
    /// </summary>
    [DisplayName( "Benevolence Request Statement Lava" )]
    [Category( "Finance" )]
    [Description( "Block for displaying a Lava based Benevolence Request detail." )]

    #region Block Attributes

    [CodeEditorField( "Lava Template",
        Description = "The Lava template to use for the Benevolence Request statement.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 500,
        IsRequired = true,
        DefaultValue = @"
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
                <a href=""#"" class=""btn btn-primary hidden-print"" onClick=""window.print();""><i class=""ti ti-printer""></i> Print Request</a>
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
{% endif %}",
        Order = 0,
        Key = AttributeKey.LavaTemplate )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "23830585-B0F7-41FD-A3B7-E9B0C8D47A13" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "D645CF3D-B84B-4A4F-9FC0-62BB94B1258B" )]
    [Rock.SystemGuid.BlockTypeGuid( "C2D8FCA3-BC8F-44FF-85AA-440BF41CEF5D" )]
    public class BenevolenceRequestStatementLava : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string LavaTemplate = "LavaTemplate";
        }

        private static class PageParameterKey
        {
            public const string BenevolenceRequestId = "BenevolenceRequestId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CustomBlockBox<BenevolenceRequestStatementLavaBag, BenevolenceRequestStatementLavaOptionsBag>();

            box.Bag = new BenevolenceRequestStatementLavaBag
            {
                Content = RenderLavaContent()
            };

            box.Options = new BenevolenceRequestStatementLavaOptionsBag();

            return box;
        }

        /// <summary>
        /// Resolves the configured Lava template for the requested benevolence request.
        /// </summary>
        /// <returns>The rendered HTML content string.</returns>
        private string RenderLavaContent()
        {
            var benevolenceRequest = new BenevolenceRequestService( RockContext )
                .Get( PageParameter( PageParameterKey.BenevolenceRequestId ), !PageCache.Layout.Site.DisablePredictableIds );

            if ( benevolenceRequest == null )
            {
                return "<div class=\"alert alert-warning\">The requested benevolence request was not found.</div>";
            }

            // Load attributes so the template can iterate Request.AttributeValues.
            benevolenceRequest.LoadAttributes( RockContext );

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Request", benevolenceRequest );

            var template = GetAttributeValue( AttributeKey.LavaTemplate );

            return template.ResolveMergeFields( mergeFields );
        }

        #endregion Methods
    }
}
