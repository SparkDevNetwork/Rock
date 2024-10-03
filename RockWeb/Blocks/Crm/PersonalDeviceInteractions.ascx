<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalDeviceInteractions.ascx.cs" Inherits="RockWeb.Blocks.Crm.PersonalDeviceInteractions" %>

<asp:UpdatePanel ID="upPersonBadge" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-mobile"></i>
                    <asp:Label ID="lblHeading" runat="server" />
                </h1>
            </div>
            <div class="panel-body">
                <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                <asp:HiddenField ID="hfPersonalDevice" runat="server" />
                <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" Visible="false"></Rock:NotificationBox>

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="gfInteractions" runat="server">
                        <Rock:SlidingDateRangePicker id="sdpDateRange" runat="server" Label="Date Range" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" />
                        <Rock:RockCheckBox ID="cbShowUnassignedDevices" runat="server" Label="Show Unassigned Devices" />
                        <Rock:RockCheckBox ID="cbPresentDevices" runat="server" Label="Present Devices" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gInteractions" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="InteractionDateTime" HeaderText="Date / Time" SortExpression="InteractionDateTime" />
                            <Rock:RockTemplateField>
                                <ItemTemplate>
                                    <%# IsCurrentlyPresent((DateTime?) Eval( "InteractionEndDateTime" )) ? "<span class='label label-success'>Currently Present</span>":"" %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="InteractionSummary" HeaderText="Details" SortExpression="InteractionSummary" />
                            <Rock:RockTemplateField HeaderText="Assigned Individual">
                                <ItemTemplate>
                                    <%#  Eval( "PersonalDevice.PersonAlias" ) != null ? "<a href='"+ string.Format( "{0}{1}", ResolveRockUrl( "~/Person/" ) , Eval("PersonalDevice.PersonAlias.PersonId")) +"'>"+ Eval("PersonalDevice.PersonAlias.Person.FullName") +"</a>":"" %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
