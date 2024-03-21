<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BulkImportTool.ascx.cs" Inherits="RockWeb.Blocks.BulkImport.BulkImportTool" %>

<script src="/SignalR/hubs"></script>
<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('.js-show-additional-notes').off('click').on('click', function () {
            var isVisible = !$('.js-additional-notes').is(':visible');
            $('#<%=hfShowAdditionalNotes.ClientID %>').val(isVisible);
            $('.js-show-additional-notes').text(isVisible ? 'Hide Additional Notes' : 'Show Additional Notes');
            $('.js-additional-notes').slideToggle();
            return false;
        });

        if ($('#<%=hfShowAdditionalNotes.ClientID %>').val() == "true") {
            $('.js-additional-notes').show();
            $('.js-show-additional-notes').text('Hide Additional Notes');
        }
    });

    $(function ()
    {
        var proxy = $.connection.rockMessageHub;

        proxy.client.receiveNotification = function (name, message, results)
        {
            if (name == '<%=this.SignalRNotificationKey %>') {
                $('#<%=pnlProgress.ClientID%>').show();

                if (message) {
                    $('#<%=lProgressMessage.ClientID %>').html(message);
                }

                if (results) {
                    $('#<%=lProgressResults.ClientID %>').html(results);
                }
            }
        }

        proxy.client.showButtons = function (name, visible)
        {
            if (name == '<%=this.SignalRNotificationKey %>') {

                if (visible) {
                    $('#<%=pnlActions.ClientID%>').show();
                }
                else {
                    $('#<%=pnlActions.ClientID%>').hide();
                }
            }
        }

        $.connection.hub.start().done(function ()
        {
            // hub started... do stuff here if you want to let the user know something
        });
    })
</script>

<asp:UpdatePanel ID="upnlContent" runat="server" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="btnCheckForeignSystemKey" />
    </Triggers>
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-upload"></i>&nbsp;Bulk Import Tool</h1>
            </div>
            <div class="panel-body">
                <asp:ValidationSummary ID="vsBulkImport" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation"/>
                <Rock:RockTextBox ID="tbForeignSystemKey" runat="server" Required="true" Label="Foreign System Key" Help="The Key used to uniquely identify the source system. For example, use the domain of the source system's website: 'somechurch.ccbchurch.com'. Click 'Check Foreign System Key' to see what keys have already been used or to see if the key has already been used." />
                <asp:LinkButton ID="btnCheckForeignSystemKey" runat="server" CssClass="btn btn-xs btn-action margin-b-md" Text="Check Foreign System Key" CausesValidation="false" OnClick="btnCheckForeignSystemKey_Click" />
                <Rock:NotificationBox ID="nbCheckForeignSystemKey" runat="server" CssClass="margin-b-md" NotificationBoxType="Warning" Visible="false" Dismissable="true" />

                <asp:UpdatePanel ID="upnlFileUpload" runat="server">
                    <ContentTemplate>
                        <asp:HiddenField ID="hfMainSlingshotFileName" runat="server" />
                        <Rock:HiddenFieldValidator ID="hfMainSlingshotFileNameValidator" runat="server" ControlToValidate="hfMainSlingshotFileName" CssClass="danger" ErrorMessage="Slingshot File must be specified" Display="None" />
                        <Rock:FileUploader ID="fupSlingshotFile" runat="server" Label="Select Slingshot File" IsBinaryFile="false" RootFolder="~/App_Data/SlingshotFiles" DisplayMode="DropZone" OnFileUploaded="fupSlingshotFile_FileUploaded" AllowMultipleUploads="true" OnFileRemoved="fupSlingshotFile_FileRemoved" />
                        <asp:Label ID="lSlingshotFileInfo" runat="server" Text="" />
                        <asp:Label ID="lAdditionalSlingshotFilesInfo" runat="server" Text="" />
                    </ContentTemplate>
                </asp:UpdatePanel>

                <Rock:PanelWidget runat="server" ID="pwAdvanced" Title="Advanced Settings">
                    <Rock:RockControlWrapper ID="rcwImportOptions" runat="server" Label="Import Options">
                        <asp:Panel ID="pnlAdvanced" runat="server">
                            <div>
                                <Rock:RockRadioButton ID="rbAlwaysUpdate" runat="server" Text="Always Update" GroupName="ImportOptions" Checked="true" />
                                <Rock:HelpBlock ID="hbAlwaysUpdate" runat="server" Visible="true">All data from the import will be updated in Rock</Rock:HelpBlock>
                            </div>
                            <div>
                                <Rock:RockRadioButton ID="rbOnlyAddNewRecords" runat="server" Text="Only Add New Records" GroupName="ImportOptions" />
                                <Rock:HelpBlock ID="hbOnlyAddNewRecords" runat="server" Visible="true">Only new records that don't exist in Rock will be added. Existing records will not be updated.</Rock:HelpBlock>
                            </div>
                            
                            <%-- TODO. Set Visible to True once this is implemented --%>
                            <div style="display:none">
                                <Rock:RockRadioButton ID="rbMostRecentWins" runat="server" Text="Most Recent Wins" GroupName="ImportOptions" />
                                <Rock:HelpBlock ID="hbMostRecentWins" runat="server" Visible="true">The latest record will be used. Note, when determining the last update date from Rock the date will reflect the latest date any information about a  was updated. The import is not able to update each data point (phone number, email,  attribute) separately.</Rock:HelpBlock>
                            </div>

                        </asp:Panel>
                    </Rock:RockControlWrapper>

                    <asp:HiddenField ID="hfShowAdditionalNotes" runat="server" />
                    <a href="#" class="btn btn-xs btn-link js-show-additional-notes">Show Additional Notes</a>

                    <asp:Panel ID="pnlAdditionalNotes" runat="server" CssClass="js-additional-notes" Style="display: none;">
                            <pre>
Before Importing
-- Backup the Customer’s Database
-- Verify that Rock > Home / General Settings / File Types / ‘Person Image’, has the Storage Type set to what you want.  Slingshot will use that when importing Person and Family Photos
-- Verify that Rock > Home / General Settings / File Types / ‘Transaction Image’, has the Storage Type set to what you want.  Slingshot will use that when importing FinancialTransaction Images

After Importing
-- Go the General Settings / Group Types and filter by Check-in Template. This will show you the group types that already a Check-in Template
-- Now, in a separate window, go to Power Tools / SQL Command

// Use this SQL to figure out what GroupTypes were involved in the Attendance Import, and what their Parent Group Type is

SELECT gt.NAME [GroupType.Name], gt.Id, max(gt.CreatedDateTime) [GroupType.CreateDateTime]
       ,count(*) [AttendanceCount]
       ,(
              SELECT TOP 1 pgt.NAME
              FROM GroupTypeAssociation gta
              INNER JOIN GroupType pgt ON pgt.Id = gta.GroupTypeId
              WHERE ChildGroupTypeId = gt.id
              ) [Parent Group Type]
FROM Attendance a
INNER JOIN [Group] g ON g.Id = (SELECT GroupId from AttendanceOccurrence WHERE Id = a.OccurrenceId)
INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.id
GROUP BY gt.NAME,gt.Id
order by gt.Id desc

// To see a break down by Group Name and Type, this SQL is handy

SELECT gt.NAME [GroupType.Name]
,gt.Id
     ,g.Name [Group.Name]
       ,count(*) [AttendanceCount]
       ,MAX(PGT.NAME) [Parent Group Type]
       ,MAX(PGT.GroupTypePurpose) [Parent Group Type Purpose]
	   ,max(gt.CreatedDateTime) [GroupType.CreateDateTime]
FROM Attendance a
INNER JOIN [Group] g ON g.Id = (SELECT GroupId from AttendanceOccurrence WHERE Id = a.OccurrenceId)
INNER JOIN [GroupType] gt ON g.GroupTypeId = gt.id
OUTER APPLY (
       SELECT TOP 1 pgt.NAME
              ,dv.Value [GroupTypePurpose]
       FROM GroupTypeAssociation gta
       INNER JOIN GroupType pgt ON pgt.Id = gta.GroupTypeId
       LEFT JOIN DefinedValue dv ON pgt.GroupTypePurposeValueId = dv.Id
       WHERE gta.ChildGroupTypeId = gt.id
       ) PGT
GROUP BY gt.NAME
       ,gt.Id
       ,g.Name
order by Gt.Id, Gt.Name, g.Name

-- Now, back to Rock > Home / General Settings / Group Types, select a Checkin-Template group type.  For example, Weekly Service Check-in Area
	-- Using the SQL Results, add the Child Group Types to the appropriate Checkin-Template group type. 
	-- Ones that sound like Weekend Check-in will go in Weekend Check-in GroupType, then the 'General' panelwidget | Child Group Types
	-- Ones that sound like Volunteer Check-in will go in Volunteer Check-in GroupType, then the 'General' panelwidget | Child Group Types

-- Now Attendance Analytics will be able to show the import Attendance Data
                            </pre>
                        </asp:Panel>

                </Rock:PanelWidget>

                <asp:Panel ID="pnlProgress" runat="server" CssClass="js-messageContainer" Style="display: none">
                    <strong>Progress</strong><br />
                    <div class="alert alert-info">
                        <asp:Label ID="lProgressMessage" CssClass="js-progressMessage" runat="server" />
                    </div>

                    <strong>Details</strong><br />
                    <div class="alert alert-info">
                        <pre><asp:Label ID="lProgressResults" CssClass="js-progressResults" runat="server" /></pre>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlActions" runat="server" CssClass="actions">
                    <asp:LinkButton ID="btnImport" runat="server" CssClass="btn btn-primary" Text="Import" OnClick="btnImport_Click" />
                    <asp:LinkButton ID="btnImportPhotos" runat="server" CssClass="btn btn-primary" Text="Import Photos" OnClick="btnImportPhotos_Click" />
                </asp:Panel>
            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>

<div>
    <asp:LinkButton ID="btnDownloadLog" runat="server" CssClass="btn btn-primary" Text="Download Log" OnClick="btnDownloadLog_Click" CausesValidation="false" />
</div>
