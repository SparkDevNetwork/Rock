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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class WorkflowNotes2 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Workflow", "Workflow Note Type", "The type of notes that can be associated with a workflow.", "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782" );
            RockMigrationHelper.AddDefinedTypeAttribute( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Icon Class Name", "IconClass", "The class name to use when rendering an icon for notes of this type", 0, "", "629CFBFF-3A95-4294-B13C-37F4FED04FE7" );

            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "User Note", "User entered note", "534489FB-E239-4C51-8F5D-9ECF85E9CDE2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "534489FB-E239-4C51-8F5D-9ECF85E9CDE2", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-comment" );

            RockMigrationHelper.AddDefinedValue( "FDC7A191-717E-4CA6-9DCF-A2B5BB09C782", "System Note", "System entered note", "414E9F98-4709-4895-AEBA-E41773BB7EB8" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "414E9F98-4709-4895-AEBA-E41773BB7EB8", "629CFBFF-3A95-4294-B13C-37F4FED04FE7", "fa fa-file-text" );

            Sql( @"
    DECLARE @DefinedTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = 'FDC7A191-717E-4CA6-9DCF-A2B5BB09C782' )
    DECLARE @WorkflowEntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '3540E9A7-FE30-43A9-8B0A-A372B63DFC93' )
    IF @DefinedTypeId IS NOT NULL AND @WorkflowEntityTypeId IS NOT NULL 
    BEGIN
	    IF EXISTS ( SELECT [Id] FROM [NoteType] WHERE [EntityTypeId] = @WorkflowEntityTypeId AND ( [Name] = 'WorkflowNote' OR [Name] = 'Workflow Note' ) )
	    BEGIN
		    UPDATE [NoteType] SET 
    			[IsSystem] = 1,
                [Name] = 'Workflow Note',
			    [SourcesTypeId] = @DefinedTypeId,
			    [Guid] = 'A6CE445C-3B49-4401-82E6-312BF7946A6B'
		    WHERE [EntityTypeId] = @WorkflowEntityTypeId 
		    AND ( [Name] = 'WorkflowNote' OR [Name] = 'Workflow Note' )
	    END
	    ELSE
	    BEGIN
		    INSERT INTO [NoteType] ( [IsSystem], [EntityTypeId], [Name], [SourcesTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Guid] )
		    VALUES ( 1, @WorkflowEntityTypeId, 'Workflow Note', @DefinedTypeId, '', '', 'A6CE445C-3B49-4401-82E6-312BF7946A6B')
	    END
    END
" );

            Sql( @"
    UPDATE Q 
	    SET [Value] = 'General^General Inquiry,
Login^Login / Username / Password Assistance,
Website^Feedback about the web site,
Finance^Contributions / Finance,
Missions^Missions / Global Trips,
Pastor^Talk to a Pastor'
    FROM [Attribute] A
    INNER JOIN [AttributeQualifier] Q 
	    ON Q.[AttributeId] = A.[Id]
	    AND Q.[Key] = 'values'
    WHERE A.[Guid] = 'DA61CA95-0106-49EE-962B-F70042E1464E'
    AND Q.[Value] = 'General:General Inquiry,
Login:Login / Username / Password Assistance,
Website:Feedback about the web site,
Finance:Contributions / Finance,
Missions:Missions / Global Trips,
Pastor:Talk to a Pastor'
" );

            // Add Block to Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "BA547EED-5537-49CF-BD4E-C583D760788C", "", "2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3", "Notes", "Main", "", "", 1, "3A289F81-3048-419B-8A78-2B15967CC42B" );
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "3CB0A7DF-996B-4D6C-B3B6-9BBCC40BDC69", @"Notes" );
            // Attrib Value for Block:Notes, Attribute:Heading Icon CSS Class Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "B69937BE-000A-4B94-852F-16DE92344392", @"fa fa-comment" );
            // Attrib Value for Block:Notes, Attribute:Note Term Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "FD0727DC-92F4-4765-82CB-3A08B7D864F8", @"Note" );
            // Attrib Value for Block:Notes, Attribute:Display Type Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "5232BFAE-4DC8-4270-B38F-D29E1B00AB5E", @"Full" );
            // Attrib Value for Block:Notes, Attribute:Use Person Icon Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "C05757C0-E83E-4170-8CBF-C4E1ABEC36E1", @"False" );
            // Attrib Value for Block:Notes, Attribute:Allow Anonymous Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "EB9CBD02-2B0F-4BA3-9112-BC73D54159E7", @"False" );
            // Attrib Value for Block:Notes, Attribute:Add Always Visible Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "8E0BDD15-6B92-4BB0-9138-E9382B60F3A9", @"False" );
            // Attrib Value for Block:Notes, Attribute:Display Order Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "C9FC2C09-1BF5-4711-8F97-0B96633C46B1", @"Descending" );
            // Attrib Value for Block:Notes, Attribute:Entity Type Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "F1BCF615-FBCA-4BC2-A912-C35C0DC04174", @"3540e9a7-fe30-43a9-8b0a-a372b63dfc93" );
            // Attrib Value for Block:Notes, Attribute:Note Type Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "4EC3F5BD-4CD9-4A47-A49B-915ED98203D6", @"Workflow Note" );
            // Attrib Value for Block:Notes, Attribute:Show Private Checkbox Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "D68EE1F5-D29F-404B-945D-AD0BE76594C3", @"False" );
            // Attrib Value for Block:Notes, Attribute:Show Security Button Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "00B6EBFF-786D-453E-8746-119D0B45CB3E", @"False" );
            // Attrib Value for Block:Notes, Attribute:Show Alert Checkbox Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "3A289F81-3048-419B-8A78-2B15967CC42B", "20243A98-4802-48E2-AF61-83956056AC65", @"True" );

            // Add/Update PageContext for Page:Workflow Detail, Entity: Rock.Model.Workflow, Parameter: workflowid
            RockMigrationHelper.UpdatePageContext( "BA547EED-5537-49CF-BD4E-C583D760788C", "Rock.Model.Workflow", "workflowid", "55B1F94F-6498-4616-A1EC-4891E3FF2299" );


            // Migration Rollups

            Sql(@"
    UPDATE [HtmlContent]
    SET [Content] = '<ul class=""list-group list-group-panel"">
<li class=""list-group-item""><a href=""http://www.rockrms.com/"">Rock RMS Website</a></li>
<li class=""list-group-item""><a href=""~/page/1"">External Website</a></li>
</ul>'
    WHERE [Guid] = '007EA905-D5D3-4DC5-AD0B-2C1E3935E452'
");

            Sql( @"
UPDATE [SystemEmail]
SET [Body] = '{{ GlobalAttribute.EmailHeader }}

<p>{{ Person.FirstName }},</p>
<p>The following {{ Workflow.WorkflowType.Name }} requires action:<p>
<p>{{ Workflow.WorkflowType.WorkTerm}}: <a href=''{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}''>{{ Workflow.Name }}</a></p>

{% assign RequiredFields = false %}

<h3 class=""separator"">Details</h3>



{% for attribute in Action.FormAttributes %}

    
    {% if attribute.IsVisible and attribute.Value != '' %}
        <div>
            <strong>{{ attribute.Name }}:</strong>
            <br />
            
                {% if attribute.Url and attribute.Url != '' %}
                    <a href=''{{ attribute.Url }}''>{{ attribute.Value }}</a>
                {% else %}
                    {{ attribute.Value }}
                {% endif %}

        </div>
        <br />
    {% endif %}
    
        
    {% if attribute.IsRequired && attribute.Value == Empty %}
        {% assign RequiredFields = true %}
    {% endif %}

{% endfor %}



<table width=""100%"">
    <tr>
        <td>

    <table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
     <tr>
       <td>
    
    		<div><!--[if mso]>
    		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#269abc"" fillcolor=""#31b0d5"">
    			<w:anchorlock/>
    			<center style=""color:#ffffff;font-family:sans-serif;font-size:14px;font-weight:normal;"">View Details</center>
    		  </v:roundrect>
    		<![endif]--><a href=""{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}""
    		style=""background-color:#31b0d5;border:1px solid #269abc;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:14px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">View Details</a></div>
    
    	</td>
     </tr>
    </table>

    {% if Action.ActionType.WorkflowForm.IncludeActionsInNotification == true %}

        {% if RequiredFields != true %}
    
            {% for button in Action.ActionType.WorkflowForm.Buttons %}
                {% capture ButtonLinkSearch %}{% raw %}{{ ButtonLink }}{% endraw %}{% endcapture %}
                {% capture ButtonLinkReplace %}{{ GlobalAttribute.InternalApplicationRoot }}WorkflowEntry/{{ Workflow.WorkflowTypeId }}?WorkflowGuid={{ Workflow.Guid }}&action={{ button.Name }}{% endcapture %}
                {% capture ButtonHtml %}{{ button.EmailHtml | Replace: ButtonLinkSearch, ButtonLinkReplace }}{% endcapture %}

                {% capture ButtonTextSearch %}{% raw %}{{ ButtonText }}{% endraw %}{% endcapture %}
                {% capture ButtonTextReplace %}{{ button.Name }}{% endcapture %}
                {{ ButtonHtml | Replace: ButtonTextSearch, ButtonTextReplace }}
            {% endfor %}
        {% endif %}

    {% endif %}

        </td>
    </tr>
</table>


{{ GlobalAttribute.EmailFooter }}'
WHERE [Guid] = '88C7D1CC-3478-4562-A301-AE7D4D7FFF6D'
" );

            Sql( @"
/*
<doc>
	<summary>
 		This function returns the address of the person provided.
	</summary>

	<returns>
		Address of the person.
	</returns>
	<remarks>
		This function allows you to request an address for a specific person. It will return
		the first address of that type (multiple address are possible if the individual is in
		multiple families). 
		
		You can provide the address type by specifing 'Home', 'Previous', 
		'Work'. For custom address types provide the AddressTypeId like '19'.

		You can also determine which component of the address you'd like. Values include:
			+ 'Full' - the full address 
			+ 'Street1'
			+ 'Street2'
			+ 'City'
			+ 'State'
			+ 'PostalCode'
			+ 'Country'

	</remarks>
	<code>
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Full')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street1')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Street2')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'City')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'State')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'PostalCode')
		SELECT [dbo].[ufnCrm_GetAddress](3, 'Home', 'Country')
	</code>
</doc>
*/

CREATE FUNCTION [dbo].[ufnCrm_GetAddress](
	@PersonId int, 
	@AddressType varchar(20),
	@AddressComponent varchar(20)) 

RETURNS nvarchar(250) AS

BEGIN
	DECLARE @AddressTypeId int

	-- get address type
	IF (@AddressType = 'Home')
		BEGIN
		SET @AddressTypeId = 19
		END
	ELSE IF (@AddressType = 'Work')
		BEGIN
		SET @AddressTypeId = 20
		END
	ELSE IF (@AddressType = 'Previous')
		BEGIN
		SET @AddressTypeId = 137
		END
	ELSE
		SET @AddressTypeId = CAST(@AddressType AS int)

	-- return address component
	IF (@AddressComponent = 'Street1')
		BEGIN
		RETURN (SELECT [Street1] FROM [Location] WHERE [Id] = (SELECT [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'Street2')
		BEGIN
		RETURN (SELECT [Street2] FROM [Location] WHERE [Id] = (SELECT [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'City')
		BEGIN
		RETURN (SELECT [City] FROM [Location] WHERE [Id] = (SELECT [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'State')
		BEGIN
		RETURN (SELECT [State] FROM [Location] WHERE [Id] = (SELECT [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'PostalCode')
		BEGIN
		RETURN (SELECT [PostalCode] FROM [Location] WHERE [Id] = (SELECT [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE IF (@AddressComponent = 'Country')
		BEGIN
		RETURN (SELECT [Country] FROM [Location] WHERE [Id] = (SELECT [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END
	ELSE 
		BEGIN
		RETURN (SELECT [Street1] + ' ' + [Street2] + ' ' + [City] + ', ' + [State] + ' ' + [PostalCode] FROM [Location] WHERE [Id] = (SELECT [LocationId] FROM [GroupLocation] WHERE  [GroupLocationTypeValueId] = @AddressTypeId AND  [GroupId] = (SELECT TOP 1 [GroupId] FROM [GroupMember] gm INNER JOIN [Group] g ON g.[Id] = gm.[GroupId] WHERE [PersonId] = @PersonId AND g.[GroupTypeId] = 10))) 
		END

	RETURN ''
END
" );
        
        Sql(@"
    UPDATE [BinaryFileData]
    SET Content = 0x0A3C212D2D2073617665642066726F6D2075726C3D283030373629687474703A2F2F6C6F63616C686F73743A363232392F47657446696C652E617368783F677569643D66393865396236312D336662342D343366342D386333302D346565636432663534393637202D2D3E0A3C68746D6C3E3C686561643E3C6D65746120687474702D65717569763D22436F6E74656E742D547970652220636F6E74656E743D22746578742F68746D6C3B20636861727365743D5554462D38223E3C2F686561643E3C626F64793E3C707265207374796C653D22776F72642D777261703A20627265616B2D776F72643B2077686974652D73706163653A207072652D777261703B223E1043547E7E43442C7E43435E7E43547E0A5E58417E54413030307E4A534E5E4C54305E4D4E575E4D54445E504F4E5E504D4E5E4C48302C305E4A4D415E5052362C367E534431355E4A55535E4C524E5E4349305E585A0A5E58410A5E4D4D540A5E50573831320A5E4C4C303430360A5E4C53300A5E46543434332C3131395E41304E2C3133352C3133345E46423333332C312C302C525E46485C5E46445757575E46530A5E465431322C3236385E41304E2C3133352C3134365E46485C5E4644355E46530A5E465431342C3332375E41304E2C34352C34355E46485C5E4644365E46530A5E464F3632362C3334305E474236302C35362C35365E46530A5E46543632362C3338345E41304E2C34352C34355E464237302C312C302C435E46525E46485C5E46444141415E46530A5E464F3731392C3334305E474236302C35362C35365E46530A5E46543731392C3338345E41304E2C34352C34355E464237302C312C302C435E46525E46485C5E46444C4C4C5E46530A5E46543333362C3130335E41304E2C3130322C3130305E46485C5E4644325E46530A5E46543430312C3130335E41304E2C3130322C3130305E46485C5E4644335E46530A5E46543334322C3133305E41304E2C32382C32385E46485C5E4644345E46530A5E46543333382C3338355E41304E2C32382C32385E46485C5E464431305E46530A5E465431332C3338355E41304E2C32382C32385E46485C5E4644395E46530A5E4C52595E464F302C305E47423831322C302C3133365E46535E4C524E0A5E5051312C302C312C595E585A0A3C2F7072653E3C2F626F64793E3C2F68746D6C3E
    WHERE [Guid] = 'DF44734E-473B-4009-9099-7BA25C8EA36A'
");
        }
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Notes, from Page: Workflow Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "3A289F81-3048-419B-8A78-2B15967CC42B" );

            // Delete PageContext for Page:Workflow Detail, Entity: Rock.Model.Workflow, Parameter: workflowid
            RockMigrationHelper.DeletePageContext( "55B1F94F-6498-4616-A1EC-4891E3FF2299" );

        }
    }
}
