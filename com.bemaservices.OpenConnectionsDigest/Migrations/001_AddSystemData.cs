using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
using com.bemaservices.OpenConnectionsDigest.SystemGuid;

namespace com.bemaservices.OpenConnectionsDigest.Migrations
{
    [MigrationNumber( 1, "1.8.0" )]
    public class AddSystemData : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // System Email Text
            string systemEmailTemplateText = @"
                    {{ 'Global' | Attribute:'EmailHeader' }}
					<p> Hello {{ Person.NickName }},<br/><p>
			
					<br/>

					<p> Here are the current connection Requests that are assigned to you.
						<br/>
						<br/>
					</p>

					{% for r in Requests %}
								{% if forloop.First == true or lastOpportunity != r.ConnectionOpportunityId %}

							<h4>{{ r.ConnectionOpportunity.Name }}</h4>

						{% endif %}
								{% assign lastOpportunity = r.ConnectionOpportunityId %}
								{% assign lastActivity = r.ConnectionRequestActivities | Last %}

						<li style='list-style:none;'><a href='{{ 'Global' | Attribute:'InternalApplicationRoot' }}page/408?ConnectionRequestId={{ r.Id }}' >{{ r.PersonAlias.Person.FullName }}

								{% if lastActivity == null or lastActivity == empty %}

							<i> (Created {{ r.CreatedDateTime | HumanizeTimeSpan:""Now"" }}
								ago)</i>

						{% else %}

							<i> (Updated {{ lastActivity.CreatedDateTime | HumanizeTimeSpan:'Now' }}
								ago)</i>

						{% endif %}

						</a ></li>
					{% endfor %}

					{{ 'Global' | Attribute:'EmailFooter' }}
            ";

            RockMigrationHelper.UpdateSystemEmail(
                category: "System",
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

            /*
            // Create the system Email
            Sql(@"
			INSERT INTO [SystemEmail] (IsSystem,Title,Subject,Body,Guid, CategoryId)
			Select
			1
			, 'Open Connection Requests Digest'
			, 'Open Connection Requests'
			, '{{ ""Global"" | Attribute:""EmailHeader"" }}
					<p> Hello {{ Person.NickName }},<br/><p>
			
					<br/>

					<p> Here are the current connection Requests that are assigned to you.
						<br/>
						<br/>
					</p>

					{% for r in Requests %}
								{% if forloop.First == true or lastOpportunity != r.ConnectionOpportunityId %}

							<h4>{{ r.ConnectionOpportunity.Name }}</h4>

						{% endif %}
								{% assign lastOpportunity = r.ConnectionOpportunityId %}
								{% assign lastActivity = r.ConnectionRequestActivities | Last %}

						<li style=""list-style:none;""><a href=""{{ ""Global"" | Attribute:""InternalApplicationRoot"" }}page/408?ConnectionRequestId={{ r.Id }}"" >{{ r.PersonAlias.Person.FullName }}

								{% if lastActivity == null or lastActivity == empty %}

							<i> (Created {{ r.CreatedDateTime | HumanizeTimeSpan:""Now"" }}
								ago)</i>

						{% else %}

							<i> (Updated {{ lastActivity.CreatedDateTime | HumanizeTimeSpan:""Now"" }}
								ago)</i>

						{% endif %}

						</a ></li>
					{% endfor %}

					{{ ""Global"" | Attribute:""EmailFooter"" }}'
			, 'A1911882-19DD-4197-A8D8-63CBD8A7D80B'
			,(
			SELECT [Id] From Category
			WHERE [Name] = 'System'
			) as CategoryId
            ");
            */

            // Create the new Rock Job
            Sql( string.Format( @"
                DELETE
                FROM [ServiceJob]
                Where [Guid] = '{0}'

			    INSERT INTO ServiceJob
			    (
				    IsSystem
				    , IsActive
				    , Name
				    , Description
				    , Class
				    , CronExpression
				    , NotificationStatus
				    , Guid
			    )
			    SELECT
				    0
				    ,0
				    ,'Send Open Connection Requests Digest'
				    ,'Sends out a system email to all connectors in the selected group that have active or future follow up past due connection requests.'
				    ,'com.bemaservices.OpenConnectionsDigest.Jobs.SendOpenConnectionsDigestEmail'
				    ,'0 0 14 ? * WED *'
				    ,3
				    ,'{0}'
            ", SystemJob.OPEN_CONNECTS_DIGEST ) );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteSystemEmail( SystemEmail.OPEN_CONNECTS_DIGEST );
            Sql( string.Format( @"DELETE FROM ServiceJob WHERE [GUID] = '{0}'", SystemJob.OPEN_CONNECTS_DIGEST ) );
        }
    }
}
