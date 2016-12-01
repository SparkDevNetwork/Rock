<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SharedStoryList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SharedStory.SharedStoryList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlWorkflowList" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><asp:Literal ID="lHeadingIcon" runat="server" ><i class="fa fa-list"></i></asp:Literal> <asp:Literal ID="lGridTitle" runat="server" Text="Workflows" /></h1>
            </div>
            <div class="panel-body">

	            <Rock:ModalAlert ID="mdGridWarning" runat="server" />

                <div class="grid grid-panel">
            	    <Rock:GridFilter ID="gfWorkflows" runat="server">
                        <Rock:RockTextBox ID="tbStory" runat="server" Label="Story Contents"></Rock:RockTextBox>
                        <Rock:RockTextBox ID="tbDifference" runat="server" Label="Difference Contents"></Rock:RockTextBox>
                        <Rock:RockTextBox ID="tbScripture" runat="server" Label="Scripture Contents"></Rock:RockTextBox>
                        <asp:PlaceHolder ID="phAttributeFilters" runat="server" />
	                </Rock:GridFilter>
	                <Rock:Grid ID="gWorkflows" runat="server" AllowSorting="true" DisplayType="Full" OnRowSelected="gWorkflows_Edit">
	                    <Columns>
	                        <Rock:RockBoundField DataField="WorkflowId" HeaderText="Id" SortExpression="WorkflowId" />
                            <Rock:RockBoundField DataField="SubmittedDate" HeaderText="Date Submitted" SortExpression="CreatedDateTime" />
	                    </Columns>
    	            </Rock:Grid>
                </div>

            </div> 

            

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
