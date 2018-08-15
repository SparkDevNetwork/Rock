<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EditFamily.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.NFCI.EditFamily" %>

<asp:UpdatePanel ID="upAddGroup" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-pencil"></i>Edit Family
                </h1>
            </div>
            <asp:Panel ID="pFamilyError" runat="server" Visible="false" CssClass="panel-body">
                <div class="alert alert-danger"><strong>Error</strong> There is no group selected or you do not have permission to view or edit this group.</div>
            </asp:Panel>
            <asp:Panel ID="pFamilyInfo" runat="server" Visible="false">
                <div class="panel-body">


                    <Rock:RockTextBox runat="server" ID="rtbGroupName" Label="Family Name" Required="true" RequiredErrorMessage="Family must have a Family Name." />

                    <h3>Family Members:</h3>

                    <asp:Repeater runat="server" ID="gMembers">
                        <ItemTemplate>
                            <div class="info-tile clearfix">
                                <div class="person-image" style="background-image: url(<%# Eval("PhotoUrl") %>); background-size: cover; background-position: 50%;"></div>
                                <h3><%# Eval("FullName") %></h3>
                                <small>(<%# Eval("RoleType") %>)</small>
                                <small><%# Eval("Age") %></small>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <h3>Addresses:</h3>

                    <asp:Repeater runat="server" ID="gAddresses">
                        <ItemTemplate>
                            <div class="info-tile clearfix">
                                <h4><%# Eval("GroupLocationTypeValue.Value") %> Address</h4>
                                <div>
                                    <%# Eval("Location.Street1") %><br>
                                    <%# !String.IsNullOrWhiteSpace(Eval("Location.Street2") as String) ? (Eval("Location.Street2") + "<br />") : "" %>
                                    <%# Eval("Location.City") %>, <%# Eval("Location.State") %> <%# Eval("Location.PostalCode") %>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                </div>
                <div class="panel-footer actions clearfix">
                    <asp:LinkButton runat="server" ID="lbCancel" CssClass="btn btn-lg btn-default pull-left" Text="Cancel" OnClientClick="history.back();" CausesValidation="false"></asp:LinkButton>
                    <asp:LinkButton runat="server" ID="lbSubmit" CssClass="btn btn-lg btn-success pull-right" Text="Save" OnClick="lbSubmit_Click"></asp:LinkButton>
                </div>
            </asp:Panel>
        </div>
        <div class="panel panel-block">
            <div class="panel-body text-center">
                <asp:LinkButton runat="server" ID="lbRequestChange" CssClass="btn btn-lg btn-warning" Text="Request Additional Change" CausesValidation="false" OnClick="lbRequestChange_Click"></asp:LinkButton>
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
