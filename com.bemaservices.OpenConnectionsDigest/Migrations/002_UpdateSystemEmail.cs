using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.bemaservices.OpenConnectionsDigest.SystemGuid;
using Rock.Plugin;

namespace com.bemaservices.Jobs.Migrations
{
	[MigrationNumber(2, "1.8.0")]
	public class UpdateSystemEmail : Migration
	{
		/// <summary>
		/// The commands to run to migrate plugin to the specific version
		/// </summary>
		public override void Up()
		{
            // System Email Text
            string systemEmailTemplateText = @"
                <style>
                    @media screen and (max-device-width: 768px) {
                        div[class=mobilecontent]{
                            display: block !important;
                            max-height: none !important;
                        }
                    }
                </style>
                {% assign totalNewRequests = 0 %}
                {% assign totalRequests = 0 %}
                {% assign totalIdleRequests = 0 %}
                {% assign totalCriticalRequests = 0 %}
                {% for newRequest in NewConnectionRequests %}
                    {% assign newRequestCount = newRequest | Size %}
                    {% assign totalNewRequests = totalNewRequests | Plus: newRequestCount %}
                {% endfor %}
                {% for requests in ConnectionRequests %}
                    {% assign requestCount = requests | Size %}
                    {% assign totalRequests = totalRequests | Plus: requestCount %}
                {% endfor %}
                {% for idleRequest in IdleConnectionRequestIds %}
                    {% assign idleRequestCount = idleRequest | Size %}
                    {% assign totalIdleRequests = totalIdleRequests | Plus: idleRequestCount %}
                {% endfor %}
                {% for criticalRequest in CriticalConnectionRequestIds %}
                    {% assign criticalRequestCount = criticalRequest | Size %}
                    {% assign totalCriticalRequests = totalCriticalRequests | Plus: criticalRequestCount %}
                {% endfor %}
                {{ 'Global' | Attribute:'EmailHeader' }}

                <table width='100%'>
                    <tr>
                    <td align='center' width='25%' style='border-right: 3px solid #FFF;background: #88bb54; text-align:center; color: #fff; font-family: sans-serif; padding-top:20px;'><h1 style='color: #fff; margin-top: 20px; font-family: sans-serif;'>{{totalNewRequests}}</h1>New*</td>
                    <td align='center' width='25%' style='border-right: 3px solid #FFF;background: #4099ad; text-align:center; color: #fff; font-family: sans-serif; padding-top:20px;'><h1 style='color: #fff; margin-top: 20px; font-family: sans-serif;'>{{totalRequests}}</h1>Active</td>
                    <td align='center' width='25%' style='border-right: 3px solid #FFF;background: #ee7624; text-align:center; color: #fff; font-family: sans-serif; padding-top:20px;'><h1 style='color: #fff; margin-top: 20px; font-family: sans-serif;'>{{totalCriticalRequests}}</h1>Critical</td>
                    <td align='center' width='25%' style='background: #bb5454; text-align:center; color: #fff; font-family: sans-serif; padding-top:20px;'><h1 style='color: #fff; margin-top: 20px; font-family: sans-serif;'>{{totalIdleRequests}}</h1>Idle</td>
                </tr>
                </table>
                <br />
                {% if totalNewRequests > 0 %}
                    <h2 style='font-family: sans-serif;margin-bottom: 5px;'>New Connection Requests</h2>
                    <table border='0' cellspacing='0' cellpadding='3' width='100%' style='border: 1px solid #ccc; font-family: sans-serif;'>
                    <thead>
                        <tr>
                            <th align='left' style='background: #999; color:#fff; font-family: sans-serif;'>Requestor</th>
                            <th align='left' style='background: #999; color:#fff; font-family: sans-serif;'>Status</th>
                            <!--[if gte mso 9]>
                            <th align='left' style='background: #999; color:#fff; font-family: sans-serif;'>Connection</th>
                            <![endif]-->
                        </tr>
                    </thead>
                    <tbody>
                        {% for newRequests in NewConnectionRequests %}
                            {% for request in newRequests %}
                                <tr style='background: {% cycle '#FFF', '#DDD' %}'>
                                    <td><a style='color: #999' href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}/page/408?ConnectionRequestId={{request.Id}}&ConnectionOpportunityId={{request.ConnectionOpportunityId}}'>{{request.PersonAlias.Person.FullName}}</a></td>
                                    <td>{{request.ConnectionStatus.Name}}</td>
                                    <!--[if gte mso 9]>
                                    <td>{{request.ConnectionOpportunity.Name}}</td>
                                    <![endif]-->
                                </tr>
                            {% endfor %}
                        {% endfor %}
                    </tbody>
                    </table>
                {% endif %}
                {% if IncludeOpportunityBreakdown %}
                <br />
                <!--[if gte mso 9]>
                <h2 style='font-family: sans-serif;margin-bottom: 5px;'>Connection Opportunity Breakdown</h2>
                <table border='0' cellspacing='0' cellpadding='3' width='100%' style='border: 1px solid #ccc; font-family: sans-serif;'>
                    <thead>
                        <tr>
                            <th align='left' style='background: #999; color:#fff; font-family: sans-serif;'>Connection Opportunity</th>
                            <th colspan='4' style='background: #999; color:#fff; font-family: sans-serif;'>Statuses</th>
                        </tr>
                    </thead>
                    <tbody>
                    {% for ConnectionOpportunity in ConnectionOpportunities %}
                        {% assign totalNewRequests = 0 %}
                        {% assign totalRequests = 0 %}
                        {% assign totalIdleRequests = 0 %}
                        {% assign totalCriticalRequests = 0 %}
                        {% for requests in NewConnectionRequests %}
                            {% for request in requests %}
                                {% if request.ConnectionOpportunity.Id == ConnectionOpportunity.Id %}
                                    {% assign totalNewRequests = totalNewRequests | Plus: 1 %}
                                {% endif %}
                            {% endfor %}
                        {% endfor %}
                        {% for requests in ConnectionRequests %}
                            {% for request in requests %}
                                {% if request.ConnectionOpportunity.Id == ConnectionOpportunity.Id %}
                                    {% assign totalRequests = totalRequests | Plus: 1 %}
                                {% endif %}
                            {% endfor %}
                        {% endfor %}
                        {% for idleRequests in IdleConnectionRequestIds %}
                            {% for request in idleRequests %}
                                {% if request.ConnectionOpportunityId == ConnectionOpportunity.Id %}
                                    {% assign totalIdleRequests = totalIdleRequests | Plus: 1 %}
                                {% endif %}
                            {% endfor %}
                        {% endfor %}
                        {% for criticalRequests in CriticalConnectionRequestIds %}
                            {% for request in criticalRequests %}
                                {% if request.ConnectionOpportunityId == ConnectionOpportunity.Id %}
                                    {% assign totalCriticalRequests = totalCriticalRequests | Plus: 1 %}
                                {% endif %}
                            {% endfor %}
                        {% endfor %}
                        <tr>
                            <td style='border-right: 1px solid #ccc; border-top: 1px solid #ccc; font-family: sans-serif;'><a style='color: #999' href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}/page/407' target='_blank'>{{ConnectionOpportunity.Name}}</a></td>
                            <td style='background: #88bb54; color: #fff;text-align:center;border-right: 1px solid #ccc; border-top: 1px solid #ccc; font-family: sans-serif;'>{{totalNewRequests}}</td>
                            <td style='background: #4099ad; color: #fff;text-align:center;border-right: 1px solid #ccc; border-top: 1px solid #ccc; font-family: sans-serif;'>{{totalRequests}}</td>
                            <td style='background: #ee7624; color: #fff;text-align:center;border-right: 1px solid #ccc; border-top: 1px solid #ccc; font-family: sans-serif;'>{{totalCriticalRequests}}</td>
                            <td style='background: #bb5454; color: #fff;text-align:center;border-top: 1px solid #ccc; font-family: sans-serif;'>{{totalIdleRequests}}</td>
                        </tr>
                        {% capture mobileContent %}
                        {{mobileContent}}
                        <tr>
                            <td colspan='4' style='border-right: 1px solid #ccc; border-top: 1px solid #ccc; font-family: sans-serif;'><a style='color: #999' href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}/page/407' target='_blank'>{{ConnectionOpportunity.Name}}</a></td>
                        </tr>
                        <tr>
                            <td style='background: #88bb54; color: #fff;text-align:center;border-right: 1px solid #ccc; border-top: 1px solid #ccc; font-family: sans-serif;'>{{totalNewRequests}}</td>
                            <td style='background: #4099ad; color: #fff;text-align:center;border-right: 1px solid #ccc; border-top: 1px solid #ccc; font-family: sans-serif;'>{{totalRequests}}</td>
                            <td style='background: #ee7624; color: #fff;text-align:center;border-right: 1px solid #ccc; border-top: 1px solid #ccc; font-family: sans-serif;'>{{totalCriticalRequests}}</td>
                            <td style='background: #bb5454; color: #fff;text-align:center;border-top: 1px solid #ccc; font-family: sans-serif;'>{{totalIdleRequests}}</td>
                        </tr>
                        {% endcapture %}
                    {% endfor %}
                    </tbody>
                </table>
                <![endif]-->

                <!--[if !mso]><!-->
                <div class='mobilecontent' style='display:none;max-height:0px;overflow:hidden;'>
                    <table border='0' cellspacing='0' cellpadding='3' width='100%' style='border: 1px solid #ccc; font-family: sans-serif;'>
                        <thead>
                            <tr>
                                <th colspan='4' align='left' style='background: #999; color:#fff; font-family: sans-serif;'>Connection Opportunity</th>
                            </tr>
                        </thead>
                        <tbody>
                        {{mobileContent}}
                        </tbody>
                    </table>
                </div>
                <!--<![endif]-->
                {% endif %}

                {% if IncludeAllRequests %}
                    <h2>Your Connection Requests</h2>
                    <blockquote>
                    {% for r in Requests %}
			                    {% if forloop.First == true or lastOpportunity != r.ConnectionOpportunityId %}

		                    <h4>{{ r.ConnectionOpportunity.Name }}</h4>

	                    {% endif %}
			                    {% assign lastOpportunity = r.ConnectionOpportunityId %}
			                    {% assign lastActivity = r.ConnectionRequestActivities | Last %}

	                    <li style='list-style:none;'><a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}page/408?ConnectionRequestId={{ r.Id }}' >{{ r.PersonAlias.Person.FullName }}

			                    {% if lastActivity == null or lastActivity == empty %}

		                    <i> (Created {{ r.CreatedDateTime | HumanizeTimeSpan:'Now' }}
			                    ago)</i>

	                    {% else %}

		                    <i> (Updated {{ lastActivity.CreatedDateTime | HumanizeTimeSpan:'Now' }}
			                    ago)</i>

	                    {% endif %}

	                    </a ></li>
                    {% endfor %}
                    </blockquote>
                {% endif %}
                <br />
                <br />
                <small>*Since last successful run date/time: {{LastRunDate}}</small>
                {{ 'Global' | Attribute:'EmailFooter' }}
            ";

            RockMigrationHelper.UpdateSystemEmail(
                category: "Plugin",
                title: "Open Connection Requests Digest",
                from: "",
                fromName: "",
                to: "",
                cc: "",
                bcc: "",
                subject: "Open Connection Requests",
                body: systemEmailTemplateText,
                guid: SystemEmail.OPEN_CONNECTS_DIGEST
            );	
		}

		/// <summary>
		/// The commands to undo a migration from a specific version
		/// </summary>
		public override void Down()
		{
            // Reverting to the previous system email is unnecessary
		}
	}
}
