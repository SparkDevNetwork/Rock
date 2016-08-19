<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinSummary.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.CheckIn.CheckinSummary" %>

<asp:UpdatePanel ID="upCheckinScheduleBuilder" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> Check-in Summary</h1>
            </div>
            <div class="panel-body">

                <Rock:CategoryPicker ID="pCategory" runat="server" AllowMultiSelect="false" Label="Schedule Category" OnSelectItem="pCategory_SelectItem" />
                <Rock:RockCheckBoxList ID="cblSchedules" runat="server" DataValueField="Id" DataTextField="Name" RepeatDirection="Horizontal" OnSelectedIndexChanged="cblSchedules_SelectedIndexChanged" AutoPostBack="true" />
                <asp:LinkButton ID="lbUpdate" runat="server" Text="Update" OnClick="cblSchedules_SelectedIndexChanged" />
                <Rock:NotificationBox ID="nbNotification" runat="server" NotificationBoxType="Warning" />

                <div class="grid">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:GroupTypePicker ID="ddlGroupType" runat="server" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gGroupLocations" runat="server" AllowSorting="true" AllowPaging="false" OnRowDataBound="gGroupLocations_RowDataBound">
                        <Columns>
                            <Rock:RockLiteralField ID="lGroupName" HeaderText="Group" SortExpression="Group.Name" />
                            <Rock:RockLiteralField ID="lAbilityLevel" HeaderText="Ability Level" />
                            <Rock:RockLiteralField ID="lAge" HeaderText="Age" />
                            <Rock:RockLiteralField ID="lGrade" HeaderText="Grade" />
                            <Rock:RockLiteralField ID="lLocations" HeaderText="Locations" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

