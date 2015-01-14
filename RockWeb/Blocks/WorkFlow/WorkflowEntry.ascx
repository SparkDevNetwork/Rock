<%@ Control Language="C#" AutoEventWireup="true" CodeFile="WorkflowEntry.ascx.cs" Inherits="RockWeb.Blocks.WorkFlow.WorkflowEntry" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="row">
            <div id="divForm" runat="server" class="col-md-6">

                <div class="panel panel-block">

                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <asp:Literal ID="lIconHtml" runat="server" ><i class="fa fa-gear"></i></asp:Literal>
                            <asp:Literal ID="lTitle" runat="server" >Workflow Entry</asp:Literal>
                    </div>
                    <div class="panel-body">

                        <asp:Panel ID="pnlForm" CssClass="workflow-entry-panel" runat="server">

                            <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                            <asp:Literal ID="lheadingText" runat="server" />

                            <asp:PlaceHolder ID="phAttributes" runat="server" />
            
                            <asp:Literal ID="lFootingText" runat="server" />

                            <div class="actions">
                                <asp:PlaceHolder ID="phActions" runat="server" />
                            </div>

                        </asp:Panel>

                        <Rock:NotificationBox ID="nbMessage" runat="server" Dismissable="true" CssClass="margin-t-lg" />

                    </div>

                </div>

            </div>

            <div id="divNotes" runat="server" class="col-md-6">

                <Rock:NoteContainer ID="ncWorkflowNotes" runat="server" Term="Note" 
                    ShowHeading="true" Title="Notes" TitleIconCssClass="fa fa-comment"
                    DisplayType="Full" UsePersonIcon="false" ShowAlertCheckBox="true"
                    ShowPrivateCheckBox="false" ShowSecurityButton="false"
                    AllowAnonymousEntry="false" AddAlwaysVisible="false"
                    SortDirection="Descending" />

            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
