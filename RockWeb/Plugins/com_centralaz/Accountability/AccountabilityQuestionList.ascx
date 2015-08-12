<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountabilityQuestionList.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Accountability.AccountabilityQuestionList" %>
<asp:UpdatePanel ID="upnlContent" runat="server">

    <ContentTemplate>
        <asp:Panel ID="pnlContent" runat="server">
            <asp:HiddenField ID="hfGroupTypeId" runat="server" />
            <div class="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i runat="server" id="iIcon"></i>
                        <asp:Literal ID="lTitle" runat="server" Text="Questions" /></h1>
                </div>
                <div class="panel-body">


                    <Rock:ModalAlert ID="maGridWarning" runat="server" />

                    <Rock:Grid ID="gAccountabilityQuestions" runat="server" AllowSorting="true" OnRowSelected="gAccountabilityQuestions_Edit" TooltipField="Description">
                        <Columns>
                            <asp:BoundField DataField="ShortForm" HeaderText="Short Form" SortExpression="ShortForm" />
                            <asp:BoundField DataField="LongForm" HeaderText="Long Form" SortExpression="LongForm" />
                            <Rock:DeleteField OnClick="gAccountabilityQuestions_Delete" />
                        </Columns>
                    </Rock:Grid>

                    <Rock:ModalDialog ID="mdDialog" runat="server" Title="Add Question" OnSaveClick="mdDialog_SaveClick">
                        <Content>
                            <asp:ValidationSummary ID="valSummaryValue" runat="server" CssClass="alert alert-error" />
                            <asp:HiddenField ID="hfQuestionId" runat="server" />
                            <fieldset>
                                <Rock:RockTextBox ID="tbMdShortForm" runat="server" Label="Short Form" Required="true" Placeholder="Read Bible" />
                                <Rock:RockTextBox ID="tbMdLongForm" runat="server" Label="Long Form" Required="true" Placeholder="Did you read the bible at least 3 days this week?" />
                            </fieldset>
                        </Content>
                    </Rock:ModalDialog>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
