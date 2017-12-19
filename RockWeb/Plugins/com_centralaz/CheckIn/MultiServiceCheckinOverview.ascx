<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MultiServiceCheckinOverview.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.CheckIn.MultiServiceCheckinOverview" %>

<asp:UpdatePanel ID="upCheckinScheduleBuilder" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"> </i>Multi-Service Checkin Overview</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-md-4">
                        <Rock:CategoryPicker ID="pCategory" runat="server" AllowMultiSelect="false" Label="Schedule Category" OnSelectItem="pCategory_SelectItem" />
                    </div>
                    <div class="col-md-8">
                        <Rock:RockCheckBoxList ID="cblSchedules" runat="server" Label="Schedules" DataValueField="Id" DataTextField="Name" RepeatDirection="Horizontal" />
                    </div>
                </div>

                <asp:LinkButton ID="lbUpdate" runat="server" Text="Update" OnClick="cblSchedules_SelectedIndexChanged" CssClass="btn btn-primary" />
                <Rock:NotificationBox ID="nbNotification" runat="server" NotificationBoxType="Warning" />

                <div class="grid">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:GroupTypePicker ID="ddlGroupType" runat="server" />
                        <Rock:LocationItemPicker ID="pkrParentLocation" runat="server" Label="Parent Location" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gGroupLocations" runat="server" AllowSorting="true" AllowPaging="false" OnRowDataBound="gGroupLocations_RowDataBound">
                        <Columns>
                            <Rock:RockLiteralField ID="lGroupName" HeaderText="Group" SortExpression="Group.Name" />
                            <Rock:RockLiteralField ID="lAbilityLevel" HeaderText="Ability Level" />
                            <Rock:RockLiteralField ID="lAge" HeaderText="Age" />
                            <Rock:RockLiteralField ID="lGrade" HeaderText="Grade" />
                            <Rock:RockLiteralField ID="lLastName" HeaderText="Last Name" />
                            <Rock:RockLiteralField ID="lSpecialNeeds" HeaderText="Special Needs" />
                            <Rock:RockLiteralField ID="lGroup" HeaderText="Attendance Rule" />
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

