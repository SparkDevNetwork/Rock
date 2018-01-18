using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Security;
using church.ccv.Pastoral.Model;

namespace RockWeb.Plugins.church_ccv.Pastoral
{
    [DisplayName("Care Request Statement Lava")]
    [Category( "Pastoral" )]
    [Description( "Block for displaying a Lava based Care Request detail." )]
    [CodeEditorField("Lava Template", "The Lava template to use for the Care Request statement.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 500, true, @"
{% capture pageTitle %}
    Care Request for {{ Request.FirstName }} {{ Request.LastName }}
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
        <h4>Care Request for {{ Request.FirstName }} {{ Request.LastName }}</h4>
        <p>Requested: {{ Request.RequestDateTime | Date:'M/d/yyyy' }}<p>
    </div>
</div>
<hr style=""opacity: .5;"" />
<div class=""row"">
    <div class=""col-xs-12"">
        <h4 class=""visible-print-block"">Care Request for {{ Request.FirstName }} {{ Request.LastName }}
        <br />
        <small>Requested: {{ Request.RequestDateTime | Date:'M/d/yyyy' }}</small></h4>
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
        {% if Request.WorkerPersonAlias != null %}
            {% assign worker = Request.WorkerPersonAlias.Person %}
            <h4>Worker</h4>
            <p>
                <strong>{{ worker.FullName }}</strong> <br />
                {% if worker.WorkPhoneNumber %}
                    {{ worker.WorkPhoneNumber }}
                {% endif %}
            </p>
        {% endif %}
    </div>
    <div class=""col-xs-4"">
        <h4>Summary</h4>
        <p> {{ Request.ResultSummary }} </p>
    </div>
</div>

        
{%if Request.CareResults != empty %}
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
            
                {% for result in Request.CareResults  %}
                    <tr>
                        <td>{{ result.ResultTypeValue.Value }}</td>
                        <td>{{ result.Amount }}</td>
                        <td>{{ result.ResultSummary }}</td>
                    </tr>
                {% endfor %}
            
            </table>
        </div>
    </div>
{% endif %}", order: 2)]
    [BooleanField("Enable Debug", "Shows the merge fields available for the Lava", order:3)]
    public partial class CareRequestStatementLava : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                DisplayResults();
            }
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

            if (Request["CareRequestId"] != null)
            {
                int id;
                int.TryParse(Request["CareRequestId"].ToString(), out id);

                var careRequest = new Service<CareRequest>(rockContext).Get(id);

                var mergeFields = new Dictionary<string, object>();
                mergeFields.Add("Request", careRequest);      

                var template = GetAttributeValue("LavaTemplate");

                lResults.Text = template.ResolveMergeFields(mergeFields);

                // show debug info
                if (GetAttributeValue("EnableDebug").AsBoolean() && IsUserAuthorized(Authorization.EDIT))
                {
                    lDebug.Visible = true;
                    lDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
        }

        #endregion
        
    }
}