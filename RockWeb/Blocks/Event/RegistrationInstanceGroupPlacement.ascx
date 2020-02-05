<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RegistrationInstanceGroupPlacement.ascx.cs" Inherits="RockWeb.Blocks.Event.RegistrationInstanceGroupPlacement" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

            <div class="panel panel-block">

                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />

                <asp:Panel ID="pnlGroupPlacement" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-link"></i>
                            Group Placement
                        </h1>
                    </div>
                    <Rock:GridFilter ID="fGroupPlacements" runat="server" OnDisplayFilterValue="fGroupPlacements_DisplayFilterValue" OnClearFilterClick="fGroupPlacements_ClearFilterClick">
                        <Rock:SlidingDateRangePicker ID="sdrpGroupPlacementsDateRange" runat="server" Label="Registration Date Range" />
                        <Rock:RockTextBox ID="tbGroupPlacementsFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbGroupPlacementsLastName" runat="server" Label="Last Name" />
                        <Rock:RockDropDownList ID="ddlGroupPlacementsInGroup" runat="server" Label="In Group"  />
                        <Rock:RockDropDownList ID="ddlGroupPlacementsSignedDocument" runat="server" Label="Signed Document" />
                        <asp:PlaceHolder ID="phGroupPlacementsFormFieldFilters" runat="server" />
                    </Rock:GridFilter>
                    <div class="panel-body">
                        <Rock:NotificationBox ID="nbPlacementNotifiction" runat="server" Visible="false" />
                        <div class="row margin-t-md">
                            <div class="col-sm-6">
                                <Rock:GroupPicker ID="gpGroupPlacementParentGroup" runat="server" Label="Parent Group"
                                    OnSelectItem="gpGroupPlacementParentGroup_SelectItem" />
                            </div>
                            <div class="col-sm-6">
                                <Rock:RockCheckBox ID="cbSetGroupAttributes" runat="server" Label="Set Group Member Attributes" Text="Yes"
                                    Help="If there are group member attributes on the target group that have the same key as a registrant attribute, should the registrant attribute value be copied to the group member attribute value?" />
                            </div>
                        </div>
                        <Rock:ModalAlert ID="mdGroupPlacementGridWarning" runat="server" />
                        <Rock:Grid ID="gGroupPlacements" runat="server" DisplayType="Light" EnableResponsiveTable="false" AllowSorting="false" RowItemText="Registrant" ExportSource="ColumnOutput">
                            <Columns>
                                <Rock:RockTemplateField HeaderText="Registrant" SortExpression="PersonAlias.Person.LastName, PersonAlias.Person.NickName">
                                    <ItemTemplate>
                                        <asp:Literal ID="lRegistrant" runat="server"></asp:Literal>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                            </Columns>
                        </Rock:Grid>
                        <div class="actions">
                            <asp:LinkButton ID="lbPlaceInGroup" runat="server" OnClick="lbPlaceInGroup_Click" Text="Place" CssClass="btn btn-primary" />
                        </div>
                    </div>
                </asp:Panel>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
