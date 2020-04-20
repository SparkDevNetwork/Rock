<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FamilyDetailLava.ascx.cs" Inherits="RockWeb.Plugins.org_hfbc.Legacy685.FamilyDetailLava" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlGroupView" runat="server">
            <asp:Literal ID="lContent" runat="server"></asp:Literal>

            <asp:Literal ID="lDebug" runat="server"></asp:Literal>
        </asp:Panel>

        <asp:Panel ID="pnlGroupEdit" runat="server" Visible="false">
            <asp:ValidationSummary ID="vsGroupEdit" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

            <h2>
                <asp:Literal ID="lEditTitle" runat="server" />

            </h2>

            <div class="row">
                <div class="col-md-3">
                    <fieldset>
                        <Rock:RockTextBox ID="tbGroupName" runat="server" Label="Family Name" CssClass="input-medium" Required="true" AutoPostBack="true" />
                    </fieldset>
                </div>
                <div class="col-md-3">
                    <fieldset>
                        <Rock:CampusPicker ID="cpCampus" runat="server" Required="true" AutoPostBack="true" />
                    </fieldset>
                </div>
                <div class="col-md-3">
                    <fieldset>
                        <Rock:RockDropDownList ID="ddlRecordStatus" runat="server" Label="Record Status" AutoPostBack="true" OnSelectedIndexChanged="ddlRecordStatus_SelectedIndexChanged" /><br />
                    </fieldset>
                </div>
                <div class="col-md-3">
                    <fieldset>
                        <Rock:RockDropDownList ID="ddlReason" runat="server" Label="Reason" Visible="false" AutoPostBack="true"></Rock:RockDropDownList>
                    </fieldset>
                </div>
            </div>

            </div>
            <div class="row">
                <div class="col-md-6">
                    <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="true" />
                </div>
                <div class="col-md-6">
                    <div class="panel panel-widget">
                        <div class="panel-heading clearfix">
                            <h4 class="panel-title pull-left">Addresses</h4>
                            <div class="pull-right">
                                <asp:LinkButton ID="lbMoved" runat="server" CssClass="btn btn-action btn-xs" OnClick="lbMoved_Click" CausesValidation="false"><i class="fa fa-truck fa-flip-horizontal"></i> Family Moved</asp:LinkButton>
                            </div>
                        </div>

                        <div class="panel-body">

                            <div class="grid grid-panel">
                                <Rock:Grid ID="gLocations" runat="server" AllowSorting="true" AllowPaging="false" DisplayType="Light" RowItemText="Address">
                                    <Columns>
                                        <Rock:RockTemplateField HeaderText="Type">
                                            <ItemTemplate>
                                                <%# Eval("LocationTypeName") %>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <Rock:RockDropDownList ID="ddlLocType" runat="server" DataTextField="Value" DataValueField="Id" />
                                            </EditItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockTemplateField HeaderText="Address">
                                            <ItemTemplate>
                                                <%# Eval("FormattedAddress") %><br />
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <Rock:AddressControl ID="acAddress" runat="server" Required="true" RequiredErrorMessage="Address is required" />
                                            </EditItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockTemplateField HeaderText="Mailing" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                            <ItemTemplate>
                                                <%# ((bool)Eval("IsMailing")) ? "<i class=\"fa fa-check\"></i>" : "" %>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:CheckBox ID="cbMailing" runat="server" Checked='<%# Eval("IsMailing") %>' />
                                            </EditItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockTemplateField HeaderText="Map Location" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center">
                                            <ItemTemplate>
                                                <%# ((bool)Eval("IsLocation")) ? "<i class=\"fa fa-check\"></i>" : "" %>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <%# ((bool)Eval("IsLocation")) ? "<i class=\"fa fa-check\"></i>" : "" %>
                                                <asp:CheckBox ID="cbLocation" runat="server" Checked='<%# Eval("IsLocation") %>' Visible='<%# !(bool)Eval("IsLocation") %>' />
                                            </EditItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:RockTemplateField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="span1" ItemStyle-CssClass="grid-columncommand" ItemStyle-Wrap="false">
                                            <ItemTemplate>
                                                <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CommandName="Edit" CssClass="btn btn-default btn-sm" CausesValidation="false"><i class="fa fa-pencil"></i></asp:LinkButton>
                                            </ItemTemplate>
                                            <EditItemTemplate>
                                                <asp:LinkButton ID="lbSave" runat="server" Text="Save" CommandName="Update" CssClass="btn btn-sm btn-success"><i class="fa fa-check"></i></asp:LinkButton>
                                                <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CommandName="Cancel" CssClass="btn btn-sm btn-warning" CausesValidation="false"><i class="fa fa-minus"></i></asp:LinkButton>
                                            </EditItemTemplate>
                                        </Rock:RockTemplateField>
                                        <Rock:DeleteField Visible="false" OnClick="gLocation_RowDelete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>

                        </div>
                    </div>
                </div>
            </div>

            <div class="actions">
                <asp:Button ID="btnSaveGroup" runat="server" AccessKey="s" CssClass="btn btn-primary" Text="Save" OnClick="btnSaveGroup_Click" />
                <asp:LinkButton ID="lbCancelGroup" runat="server" AccessKey="c" CssClass="btn btn-link" OnClick="lbCancelGroup_Click" CausesValidation="false">Cancel</asp:LinkButton>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
