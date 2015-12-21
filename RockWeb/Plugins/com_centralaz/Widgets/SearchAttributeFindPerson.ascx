<%@ Control Language="C#" AutoEventWireup="false" CodeFile="SearchAttributeFindPerson.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Widgets.SearchAttributeFindPerson" %>

<asp:UpdatePanel runat="server" ID="upnlSearch" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>
        <script>
            Sys.Application.add_load(function () {
                $("div.photo-round").lazyload({
                    effect: "fadeIn"
                });
            });
        </script>
        <Rock:NotificationBox ID="nbConfiguration" runat="server" NotificationBoxType="Danger" Visible="false" />

        <asp:Panel ID="pnlView" runat="server">
            <div class="panel panel-block">
                <div class="panel-heading clearfix">
                    <h1 class="panel-title pull-left">
                        <i class="fa fa-users"></i>
                        <asp:Literal ID="lHeading" runat="server" Text="Search People by Attribute" />
                    </h1>
                </div>

                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:GridFilter ID="rFilter" runat="server" OnDisplayFilterValue="rFilter_DisplayFilterValue">
                            <asp:PlaceHolder ID="phAttributeFilter" runat="server" />
                        </Rock:GridFilter>
                        <Rock:Grid ID="gPeople" runat="server" EmptyDataText="No People Found" AllowSorting="true" OnRowSelected="gPeople_RowSelected">
                            <Columns>
                                <Rock:SelectField />
                                <Rock:RockTemplateField HeaderText="Person" SortExpression="LastName,FirstName">
                                    <ItemTemplate>
                                        <asp:Literal ID="lPerson" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:RockBoundField
                                    ItemStyle-HorizontalAlign="Right"
                                    HeaderStyle-HorizontalAlign="Right"
                                    DataField="Age"
                                    HeaderText="Age"
                                    SortExpression="BirthYear desc,BirthMonth desc,BirthDay desc"
                                    ColumnPriority="Desktop" />
                                <Rock:DefinedValueField
                                    DataField="ConnectionStatusValueId"
                                    HeaderText="Connection Status"
                                    ColumnPriority="Tablet" />
                                <Rock:DefinedValueField
                                    DataField="RecordStatusValueId"
                                    HeaderText="Record Status"
                                    ColumnPriority="Desktop" />
                                <Rock:RockTemplateField HeaderText="Campus" ColumnPriority="Tablet">
                                    <ItemTemplate>
                                        <asp:Literal ID="lCampus" runat="server" />
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEdit" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbOk_Click" SaveButtonText="Save" Title="Block Configuration">
                <Content>
                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <Rock:RockDropDownList ID="ddlAttribute" runat="server" Label="Attribute" />
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
