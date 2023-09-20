<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PeopleAndTimeSelect.ascx.cs" Inherits="RockWeb.Plugins.org_lakepointe.Checkin.PeopleAndTimeSelect" %>
<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>
<div class="checkin-person-time-select">

    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1>
            <asp:Literal ID="lTitle" runat="server" />
            <div class="checkin-sub-title"><asp:Literal ID="lCaption" runat="server" /></div>
        </h1>
    </div>

    <div class="checkin-body">
        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-person-time-select">
                    <div class="controls checkin-timelist btn-group" data-toggle="buttons-checkbox">
                        <asp:GridView ID="gSelection" runat="server" AutoGenerateColumns="false" HeaderStyle-CssClass="first-row" >
                            <Columns>
                                <asp:BoundField DataField="Name" HeaderText="" ReadOnly="True" ItemStyle-CssClass="first-col" />
                                <asp:TemplateField ShowHeader="true" HeaderText="Placeholder" >
                                    <ItemTemplate>
                                        <button type="button"
                                            style='display:  <%# Eval("Schedule0") == null ? "none !important" : "inline-block" %>;'
                                            schedule-id='<%# Eval("Schedule0") == null ? 0 : Eval("Schedule0.Schedule.Id") %>'
                                            person-id='<%# Eval("PersonId") == null ? 0 : Eval("PersonId") %>'
                                            class='btn btn-default btn-lg btn-checkbox <%# Eval("Schedule0") == null ? "" : ((bool)Eval("Schedule0.PreSelected") ? " active" : "") %>'>
                                            <i class="fa fa-square-o"></i>
                                        </button>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField ShowHeader="true" >
                                    <ItemTemplate>
                                        <button type="button"
                                            style='display:  <%# Eval("Schedule1") == null ? "none !important" : "inline-block" %>;'
                                            schedule-id='<%# Eval("Schedule1") == null ? 0 : Eval("Schedule1.Schedule.Id") %>'
                                            person-id='<%# Eval("PersonId") == null ? 0 : Eval("PersonId") %>'
                                            class='btn btn-default btn-lg btn-checkbox <%# Eval("Schedule1") == null ? "" : ((bool)Eval("Schedule1.PreSelected") ? " active" : "" ) %>'>
                                            <i class="fa fa-square-o"></i>
                                        </button>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField ShowHeader="true" >
                                    <ItemTemplate>
                                        <button type="button"
                                            style='display:  <%# Eval("Schedule2") == null ? "none !important" : "inline-block" %>;'
                                            schedule-id='<%# Eval("Schedule2") == null ? 0 : Eval("Schedule2.Schedule.Id") %>'
                                            person-id='<%# Eval("PersonId") == null ? 0 : Eval("PersonId") %>'
                                            class='btn btn-default btn-lg btn-checkbox <%# Eval("Schedule2") == null ? "" : ((bool)Eval("Schedule2.PreSelected") ? " active" : "") %>'>
                                            <i class="fa fa-square-o"></i>
                                        </button>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField ShowHeader="true" >
                                    <ItemTemplate>
                                        <button type="button"
                                            style='display:  <%# Eval("Schedule3") == null ? "none !important" : "inline-block" %>;'
                                            schedule-id='<%# Eval("Schedule3") == null ? 0 : Eval("Schedule3.Schedule.Id") %>'
                                            person-id='<%# Eval("PersonId") == null ? 0 : Eval("PersonId") %>'
                                            class='btn btn-default btn-lg btn-checkbox <%# Eval("Schedule3") == null ? "" : ((bool)Eval("Schedule3.PreSelected") ? " active" : "") %>'>
                                            <i class="fa fa-square-o"></i>
                                        </button>
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>

                <asp:HiddenField ID="hfTimes" runat="server"></asp:HiddenField>

            </div>
        </div>
    </div>

    <div class="checkin-footer">
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-primary" ID="lbSelect" runat="server" OnClientClick="return GetTimeSelection();" OnClick="lbSelect_Click" />
            <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
        </div>
    </div>

</div>
</ContentTemplate>
</asp:UpdatePanel>
