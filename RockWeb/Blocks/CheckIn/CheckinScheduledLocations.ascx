<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckinScheduledLocations.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.CheckinScheduledLocations" %>

<asp:UpdatePanel ID="upCheckinScheduleBuilder" runat="server">
    <ContentTemplate>
        <style>
            .scheduled-locations-scroll-panel {
                bottom: 100px;
            }

            .scheduled-locations-actions {
                position: absolute;
                bottom: 0px;
                margin-bottom: 10px;
                left: 75%;
            }

            
        </style>

        <div class="checkin-header">
            <h1>Scheduled Locations</h1>
        </div>

        <div class="checkin-body">

            <div class="checkin-scroll-panel scheduled-locations-scroll-panel">
                <div class="scroller">

                    <Rock:Grid ID="gGroupLocationSchedule" runat="server" AllowSorting="true" AllowPaging="false" DisplayType="Light" ShowHeader="true">
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
            </div>
        </div>

        <div class="controls checkin-actions scheduled-locations-actions">
            <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary btn-checkin-select btn-large" OnClick="btnSave_Click" />
            <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-default btn-checkin-select btn-large" CausesValidation="false" OnClick="btnCancel_Click" />
        </div>

    </ContentTemplate>
</asp:UpdatePanel>

