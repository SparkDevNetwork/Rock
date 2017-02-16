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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 17, "1.6.1" )]
    public class FixBackgroundCheckOptionalCampus : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
	-- Fixes for issue #1701

    -- Change Campus optional form field to be *not* required if it has not yet been modified by the admin
	IF EXISTS ( SELECT [Id] FROM [WorkflowActionFormAttribute] WHERE [Guid] = '18346714-25CC-4673-A4AF-BDDA3C2F1BDA' AND [ModifiedDateTime] IS NULL )
	BEGIN
		UPDATE [WorkflowActionFormAttribute] SET
			[IsRequired] = 0
		WHERE [Guid] = '18346714-25CC-4673-A4AF-BDDA3C2F1BDA'
	END

" );

            // Add new Cancel type button
            // cancel button
            RockMigrationHelper.UpdateDefinedValue( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Cancel", "Used for Cancel when you don't want the page/form to validate.", "5683E775-B9F3-408C-80AC-94DE0E51CF3A", true );

            // cancel button html (attribute value)
            RockMigrationHelper.AddDefinedValueAttributeValue( "5683E775-B9F3-408C-80AC-94DE0E51CF3A", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"<a  href=""{{ ButtonLink }}"" 
    onclick=""return true;"" 
    data-loading-text=""<i class='fa fa-refresh fa-spin'></i> {{ ButtonText }}""
    class=""btn btn-default"" >
    {{ ButtonText }}
</a>" );

            // cancel button email html (attribute value)
            RockMigrationHelper.AddDefinedValueAttributeValue( "5683E775-B9F3-408C-80AC-94DE0E51CF3A", "8DA98984-BA25-473C-93BA-8BA2FD58C4C7", @"<table align=""left"" style=""width: 29%; min-width: 190px; margin-bottom: 12px;"" cellpadding=""0"" cellspacing=""0"">
 <tr>
   <td>

		<div><!--[if mso]>
		  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ ButtonLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#adadad""  fillcolor=""#e6e6e6"">
			<w:anchorlock/>
			<center style=""color:#333333;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ ButtonText }}</center>
		  </v:roundrect>
		<![endif]--><a href=""{{ ButtonLink }}""
		style=""background-color:#e6e6e6;border:1px solid #adadad;border-radius:4px;color:#333333;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ ButtonText }}</a></div>

	</td>
 </tr>
</table>" );

            // Background Check:Initial Request:Get Details
            // Background Check:Review Denial:Review
            // Update the WorkflowActionForm to use the new Cancel type button if they haven't modified it already
            Sql( @"
    -- Background Check:Initial Request:Get Details
    -- Update the WorkflowActionForm to use the new Cancel type button if they haven't modified it already
	IF EXISTS ( SELECT [Id] FROM [WorkflowActionForm] WHERE [Guid] = '328B74E5-6058-4C4E-9EF8-EC10985F18A8' AND [ModifiedDateTime] IS NULL )
	BEGIN
		UPDATE [WorkflowActionForm] SET
			[Actions] = 'Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^342BCBFC-2CA7-426E-ABBB-A7C461A05736^Your request has been submitted successfully.|Cancel^5683E775-B9F3-408C-80AC-94DE0E51CF3A^F47C3F69-4485-4A6A-BFCE-C44FE628DF3E^The request has been cancelled.|'
		WHERE [Guid] = '328B74E5-6058-4C4E-9EF8-EC10985F18A8'
	END

    -- Background Check:Review Denial:Review
    -- Update the WorkflowActionForm to use the new Cancel type button if they haven't modified it already
	IF EXISTS ( SELECT [Id] FROM [WorkflowActionForm] WHERE [Guid] = '91C87731-05BC-44FA-AB84-881F73EDDA20' AND [ModifiedDateTime] IS NULL )
	BEGIN
		UPDATE [WorkflowActionForm] SET
            [Actions] = 'Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^342BCBFC-2CA7-426E-ABBB-A7C461A05736^The request has been submitted again to the security team for approval.|Cancel Request^5683E775-B9F3-408C-80AC-94DE0E51CF3A^F47C3F69-4485-4A6A-BFCE-C44FE628DF3E^The request has been cancelled.|'
		WHERE [Guid] = '91C87731-05BC-44FA-AB84-881F73EDDA20'
	END
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
