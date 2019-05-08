<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinCodeLookup.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.ParentPage.CheckinCodeLookup" %>


<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <Rock:ModalAlert ID="mdWorkflowLaunched" runat="server" />

        <style>
            .text-bottom {
                vertical-align: bottom;
            }
        </style>


        <div class="row" style="margin-bottom: 15px;">
            <asp:Panel ID="pnlCheckinCode" runat="server" CssClass="col-md-6 form-inline">
                <Rock:RockTextBox ID="rtbCheckinCode" runat="server" Label="Checkin Code"></Rock:RockTextBox>
                <%--<Rock:BootstrapButton ID="rbbSearch" runat="server" Text="Search" CssClass="btn btn-primary text-bottom" DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Searching" OnClick="rbbSearch_Click" />--%>
                <asp:Button ID="rbbSearch" runat="server" Text="Search" CssClass="btn btn-primary text-bottom" DataLoadingText="&lt;i class='fa fa-refresh fa-spin'&gt;&lt;/i&gt; Searching" OnClick="rbbSearch_Click" />
            </asp:Panel>
            <asp:Panel ID="pnlSearchedCheckinCode" runat="server" CssClass="col-md-6 form-inline" Visible="false">
                <Rock:RockLiteral ID="rlCheckinCode" runat="server" Label="Checkin Code" />
            </asp:Panel>
            <asp:Panel ID="pnlSelectedPerson" runat="server" CssClass="col-md-6 form-inline" Visible="false">
                <Rock:RockLiteral ID="rlSelectedPerson" runat="server" Label="Selected Person" />
            </asp:Panel>
        </div>

        <asp:Panel ID="pnlAttendanceSearch" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Search Results</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gSearchResults" runat="server" RowClickEnabled="true" ShowActionRow="false" AllowPaging="false" OnRowSelected="gSearchResults_RowSelected" EmptyDataText="No Matching Check-ins Found">
                        <Columns>
                            <Rock:RockBoundField DataField="PersonAlias.Person.FullName" HeaderText="Person" />
                            <Rock:RockBoundField DataField="PersonAlias.Person.Age" HeaderText="Age" DataFormatString="{0}yrs" />
                            <asp:TemplateField HeaderText="Checked Into">
                                <ItemTemplate>
                                    <%# FormatCheckedIntoString((string)Eval("Group.Name"), (string)Eval("Location.Name")) %>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <Rock:RockBoundField DataField="Schedule.Name" HeaderText="Schedule" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </asp:Panel>

        <asp:Panel ID="pnlRelationSearch" runat="server" CssClass="panel panel-block" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i> Related People</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gReleventPeople" runat="server" RowClickEnabled="true" ShowActionRow="false" AllowPaging="false" OnRowSelected="gReleventPeople_RowSelected" OnRowDataBound="gReleventPeople_RowDataBound">
                        <Columns>
                            <Rock:RockBoundField DataField="FullName" HeaderText="Person" />
                            <Rock:RockBoundField DataField="Roles" HeaderText="Relationship" />
                            <Rock:RockBoundField DataField="HomePhone" HeaderText="Home Phone" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="MobilePhone" HeaderText="Mobile Phone" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </asp:Panel>


        <%--



        <asp:Panel ID="pnlMessage" CssClass="panel panel-block" runat="server" Visible="false">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-user"></i>Send Text Message</h1>
            </div>

            <div class="panel-body">

                <Rock:RockTextBox runat="server" Rows="4" ID="rtMessage" Label="Message" />

                <Rock:BootstrapButton ID="lbSend" runat="server" Text="Send Message" CssClass="btn btn-danger" OnClick="lbSend_Click"
                    DataLoadingText="&lt;i class='fa fa-refresh fa-spin fa-2x'&gt;&lt;/i&gt; Sending" />


            </div>

        </asp:Panel>--%>
    </ContentTemplate>
</asp:UpdatePanel>

