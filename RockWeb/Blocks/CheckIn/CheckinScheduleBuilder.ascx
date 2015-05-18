<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinScheduleBuilder.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.CheckinScheduleBuilder" %>

<asp:UpdatePanel ID="upCheckinScheduleBuilder" runat="server">
    <ContentTemplate>
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-calendar"></i> Schedule Builder</h1>
            </div>
            <div class="panel-body">

                <Rock:CategoryPicker ID="pCategory" runat="server" AllowMultiSelect="false" Label="Schedule Category" OnSelectItem="pCategory_SelectItem" />
                <Rock:NotificationBox ID="nbNotification" runat="server" NotificationBoxType="Warning" />

                <div class="grid">
                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:GroupTypePicker ID="ddlGroupType" runat="server" />
                        <Rock:LocationItemPicker ID="pkrParentLocation" runat="server" Label="Parent Location" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gGroupLocationSchedule" runat="server" AllowSorting="true" AllowPaging="false" OnRowDataBound="gGroupLocationSchedule_RowDataBound" >
                        <Columns>
		                    <Rock:RockTemplateField HeaderText="Group" SortExpression="Group.Name">
		                        <ItemTemplate>
		                            <%#Eval("GroupName")%><br />
		                            <small><%#Eval("GroupPath")%></small>
		                        </ItemTemplate>
		                    </Rock:RockTemplateField>
		                    <Rock:RockTemplateField HeaderText="Location" SortExpression="Location.Name">
		                        <ItemTemplate>
		                            <%#Eval("LocationName")%><br />
		                            <small><%#Eval("LocationPath")%></small>
		                        </ItemTemplate>
		                    </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

