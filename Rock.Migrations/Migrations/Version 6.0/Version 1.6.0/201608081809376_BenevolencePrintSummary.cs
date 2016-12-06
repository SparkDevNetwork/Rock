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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class BenevolencePrintSummary : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage("6DC7BAED-CA01-4703-B679-EC81143CDEDD", "2E169330-D7D7-4ECA-B417-72C64BE150F0", "Benevolence Request Summary", "", "D676A464-29A0-49F1-BA8C-752D9FE21026", ""); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType("Benevolence Request Statement Lava", "Block for displaying a Lava based Benevolence Request detail.", "~/Blocks/Finance/BenevolenceRequestStatementLava.ascx", "Finance", "C2D8FCA3-BC8F-44FF-85AA-440BF41CEF5D");
            // Add Block to Page: Benevolence Request Summary, Site: Rock RMS              
            RockMigrationHelper.AddBlock("D676A464-29A0-49F1-BA8C-752D9FE21026", "", "C2D8FCA3-BC8F-44FF-85AA-440BF41CEF5D", "Benevolence Request Statement Lava", "Main", "", "", 0, "1A70DC47-3675-4892-8520-A23FB530E0D0");

            // Attrib for BlockType: Benevolence Request Statement Lava:Lava Template              
            RockMigrationHelper.AddBlockTypeAttribute("C2D8FCA3-BC8F-44FF-85AA-440BF41CEF5D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "The Lava template to use for the Benevolence Request statement.", 2, @"  {% capture pageTitle %}      Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}  {% endcapture %}  {{ pageTitle | SetPageTitle }}    <div class=""row"">      <div class=""col-md-6"">          <div class=""pull-left"">              <img src=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ 'Global' | Attribute:'EmailHeaderLogo' }}"" width=""100px"" />          </div>                    <div class=""pull-left margin-l-md margin-t-sm"">              <strong>{{ 'Global' | Attribute:'OrganizationName' }}</strong><br />              {{ 'Global' | Attribute:'OrganizationAddress' }}<br />              {{ 'Global' | Attribute:'OrganizationWebsite' }}          </div>      </div>      <div class=""col-md-6 text-right hidden-print"">          <h4>Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}</h4>          <p>Requested: {{ Request.RequestDateTime | Date:'M/d/yyyy' }}<p>          <p>Status: <strong>{{ Request.RequestStatusValue.Value }}</strong></p>      </div>  </div>  <hr style=""opacity: .5;"" />  <div class=""row"">      <div class=""col-xs-12"">          <h4 class=""visible-print-block"">Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}          <br />          <small>Requested: {{ Request.RequestDateTime | Date:'M/d/yyyy' }} Status: <strong>{{ Request.RequestStatusValue.Value }}</strong></small></h4>      </div>  </div>    <div class=""row"">      <div class=""col-xs-4"">          <h4>Requested by</h3>          <p>              <strong>{{ Request.FirstName }} {{ Request.LastName }}</strong> <br />              {{ Request.Location.FormattedHtmlAddress }} <br />              {% if Request.HomePhoneNumber %}                 Home Phone: {{ Request.HomePhoneNumber }} <br />              {% endif %}              {% if Request.CellPhoneNumber %}                 Cell Phone: {{ Request.CellPhoneNumber }}              {% endif %}              {% if Request.WorkPhoneNumber %}                  {{ Request.WorkPhoneNumber }}              {% endif %}          </p>      </div>      <div class=""col-xs-4"">          {% if Request.RequestText != empty %}          <h4>Request</h4>          <p>{{ Request.RequestText }} </p>          {% endif %}      </div>      <div class=""col-xs-4"">          <div class=""clearfix"">              <div class=""pull-right"">                  <a href=""#"" class=""btn btn-primary hidden-print"" onClick=""window.print();""><i class=""fa fa-print""></i> Print Request</a>               </div>          </div>      </div>  </div>  <div class=""row"">      <div class=""col-xs-4"">          {% if Request.CaseWorkerPersonAlias != null %}              {% assign caseworker = Request.CaseWorkerPersonAlias.Person %}              <h4>Case Worker</h4>              <p>                  <strong>{{ caseworker.FullName }}</strong> <br />                  {{ Request.Location.FormattedHtmlAddress }} <br />                  {% if Request.HomePhoneNumber %}                     Home Phone: {{ Request.HomePhoneNumber }} <br />                  {% endif %}                  {% if Request.CellPhoneNumber %}                     Cell Phone: {{ Request.CellPhoneNumber }}                  {% endif %}                  {% if Request.WorkPhoneNumber %}                      {{ Request.WorkPhoneNumber }}                  {% endif %}              </p>          {% endif %}      </div>      <div class=""col-xs-4"">          <h4>Summary</h4>          <p> {{ Request.ResultSummary }} </p>      </div>      <div class=""col-xs-4"">          <h4>Next Steps</h4>          <p> {{ Request.ProvidedNextSteps  }} </p>      </div>  </div>              {%if Request.BenevolenceResults != empty %}      <div class=""row"">          <div class=""col-xs-12"">              <hr style=""opacity: .5;"" />                            <h4 class=""margin-t-md margin-b-md"">Results List</h4>                            <table class=""table table-bordered table-striped table-condensed"">                  <tr>                      <th>Type</th>                      <th>Amount</th>                      <th>Details</th>                  </tr>                                {% for result in Request.BenevolenceResults  %}                      <tr>                          <td>{{ result.ResultTypeValue.Value }}</td>                          <td>{{ result.Amount }}</td>                          <td>{{ result.ResultSummary }}</td>                      </tr>                  {% endfor %}                            </table>          </div>      </div>  {% endif %}", "7D78DD9F-F5D0-4660-A099-DEFDC70A6664");

            // Attrib for BlockType: Benevolence Request Detail:Benevolence Request Statement Page              
            RockMigrationHelper.AddBlockTypeAttribute("34275D0E-BC7E-4A9C-913E-623D086159A1", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Benevolence Request Statement Page", "BenevolenceRequestStatementPage", "", "The page which summarises a benevolence request for printing", 0, @"", "4D11BFF0-D253-49F9-8AD4-6662452F4E5E");

            // Attrib Value for Block:Benevolence Request Detail, Attribute:Benevolence Request Statement Page Page: Benevolence Detail, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("596CE410-99BF-420F-A86E-CFFDF0BB45F3", "4D11BFF0-D253-49F9-8AD4-6662452F4E5E", @"d676a464-29a0-49f1-ba8c-752d9fe21026");
            // Attrib Value for Block:Benevolence Request Statement Lava, Attribute:Lava Template Page: Benevolence Request Summary, Site: Rock RMS              
            RockMigrationHelper.AddBlockAttributeValue("1A70DC47-3675-4892-8520-A23FB530E0D0", "7D78DD9F-F5D0-4660-A099-DEFDC70A6664", @"{% capture pageTitle %}
    Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}
{% endcapture %}
{{ pageTitle | SetPageTitle }}

<div class=""row"">
    <div class=""col-md-12"">
        <div class=""pull-left"">
            <img src=""{{ 'Global' | Attribute:'PublicApplicationRoot' }}{{ 'Global' | Attribute:'EmailHeaderLogo' }}"" width=""100px"" />
        </div>
        
        <div class=""pull-left margin-l-md margin-t-sm"">
            <strong>{{ 'Global' | Attribute:'OrganizationName' }}</strong><br />
            {{ 'Global' | Attribute:'OrganizationAddress' }}<br />
            {{ 'Global' | Attribute:'OrganizationWebsite' }}
        </div>
    </div>
</div>
<div class=""row"">
    <div class=""col-md-6"">
        <h4>Benevolence Request for {{ Request.FirstName }} {{ Request.LastName }}</h4>
        
    </div>
    <div class=""col-md-6 text-right"">
        <p><strong>Requested:</strong> {{ Request.RequestDateTime | Date:'M/d/yyyy' }} </p>
        <p><strong>Status:</strong> {{ Request.RequestStatusValue.Value }}</p>
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
        {% endif %}
    </div>
    <div class=""col-xs-4"">
        <h4>Summary</h4>
        <p> {{ Request.ResultSummary }} </p>
    </div>
    <div class=""col-xs-4"">
        <h4>Next Steps</h4>
        <p> {{ Request.ProvidedNextSteps  }} </p>
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
            
                {% for result in Request.BenevolenceResults  %}
                    <tr>
                        <td>{{ result.ResultTypeValue.Value }}</td>
                        <td>{{ result.Amount }}</td>
                        <td>{{ result.ResultSummary }}</td>
                    </tr>
                {% endfor %}
            
            </table>
        </div>
    </div>
{% endif %}");
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Benevolence Request Detail:Benevolence Request Statement Page              
            RockMigrationHelper.DeleteAttribute("4D11BFF0-D253-49F9-8AD4-6662452F4E5E");
            // Attrib for BlockType: Benevolence Request Statement Lava:Lava Template              
            RockMigrationHelper.DeleteAttribute("7D78DD9F-F5D0-4660-A099-DEFDC70A6664");
            // Remove Block: Benevolence Request Statement Lava, from Page: Benevolence Request Summary, Site: Rock RMS              
            RockMigrationHelper.DeleteBlock("1A70DC47-3675-4892-8520-A23FB530E0D0");
            RockMigrationHelper.DeleteBlockType("C2D8FCA3-BC8F-44FF-85AA-440BF41CEF5D"); // Benevolence Request Statement Lava   
            RockMigrationHelper.DeletePage("D676A464-29A0-49F1-BA8C-752D9FE21026"); //  Page: Benevolence Request Summary, Layout: Blank, Site: Rock RMS	   
        }
    }
}
