<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PageViews.ascx.cs" Inherits="RockWeb.Blocks.Administration.PageViews" %>

<asp:UpdatePanel ID="upPageViews" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-file-alt"></i>
                    <asp:Literal ID="lBlockTitle" runat="server" />
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue" OnClearFilterClick="rFilter_ClearFilterClick" FieldLayout="Custom">
                        <Rock:SlidingDateRangePicker ID="sdrpDateRange" runat="server" Label="Date Range" />
                        <Rock:RockRadioButtonList ID="rblIsAuthenticated" runat="server" Label="Login Status" DataValueField="Value" DataTextField="Key" />
                        <Rock:RockTextBox ID="tbUrlContains" runat="server" Label="URL Contains" />
                    </Rock:GridFilter>
                    <Rock:Grid ID="gInteractions" runat="server">
                        <Columns>
                            <Rock:DateTimeField DataField="InteractionDateTime" HeaderText="View Date Time" SortExpression="InteractionDateTime" />
                            <Rock:RockBoundField DataField="TimeToServeFormatted" HeaderText="Time To Serve" ItemStyle-HorizontalAlign="Right" HeaderStyle-HorizontalAlign="Right" SortExpression="TimeToServe" />
                            <Rock:RockBoundField DataField="PersonFullName" HeaderText="Logged In User" SortExpression="PersonLastName,PersonNickName" />
                            <Rock:RockBoundField DataField="Url" HeaderText="URL" SortExpression="Url" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
