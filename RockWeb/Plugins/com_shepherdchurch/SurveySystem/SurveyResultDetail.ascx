<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SurveyResultDetail.ascx.cs" Inherits="RockWeb.Plugins.com_shepherdchurch.SurveySystem.SurveyResultDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:NotificationBox ID="nbUnauthorized" runat="server" NotificationBoxType="Warning"></Rock:NotificationBox>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>

                <span class="panel-labels">
                    <Rock:HighlightLabel ID="hlDidPass" runat="server" />
                    <Rock:HighlightLabel ID="hlTestResult" runat="server" LabelType="Info" />
                </span>
            </div>

            <div class="panel-body">
                <div class="row">
                    <div class="col-md-6">
                        <dl>
                            <dt>Completed By</dt>
                            <dd><asp:Literal ID="lCreatedBy" runat="server" /></dd>
                        </dl>
                    </div>

                    <div class="col-md-3">
                        <dl>
                            <dt>Date Completed</dt>
                            <dd><asp:Literal ID="lCreatedDate" runat="server" /></dd>
                        </dl>
                    </div>

                    <div class="col-md-3">
                    </div>
                </div>
 
                <asp:PlaceHolder ID="phAttributes" runat="server" Visible="false" />

                <Rock:Grid ID="gAttributes" runat="server" CssClass="margin-b-md" AllowSorting="false" ShowActionRow="false" AllowPaging="false">
                    <Columns>
                        <Rock:RockBoundField DataField="Key" HeaderText="Question" />
                        <Rock:RockBoundField DataField="Value" HeaderText="Response" HtmlEncode="false" />
                        <Rock:RockBoundField DataField="Answer" HeaderText="Answer" />
                        <Rock:RockBoundField DataField="IsCorrect" HtmlEncode="false" HeaderText="Is Correct" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" />
                    </Columns>
                </Rock:Grid>

                <div class="actions">
                    <asp:LinkButton ID="lbDone" runat="server" CssClass="btn btn-primary" Text="Done" OnClick="lbDone_Click" />
                    <asp:LinkButton ID="lbDelete" runat="server" CssClass="btn btn-link" Text="Delete" OnClick="lbDelete_Click" OnClientClick="return Rock.dialogs.confirmDelete(event, 'survey result');" />
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>