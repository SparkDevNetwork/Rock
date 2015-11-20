<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DPSOffenderDetail.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.SafetySecurity.DPSOffenderDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" Visible="false">

            <asp:HiddenField ID="hfDPSOffenderId" runat="server" />
            <asp:HiddenField ID="hfDPSOffenderPersonAliasId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-flag"></i>
                    <asp:Literal ID="lActionTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <h1>DPS Offender Info</h1>
                <div class="row">
                    <div class="col-md-4">
                        <asp:Literal ID="lDPSOffenderInfoCol1" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <asp:Literal ID="lDPSOffenderInfoCol2" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <asp:Literal ID="lDPSOffenderInfoCol3" runat="server" />
                    </div>
                </div>

                <h2>Potential Matches</h2>

                <Rock:RockCheckBox ID="cbMatchZip" runat="server" Text="ZipCode" Help="Limit to records that have the same zip code for the home address" Checked="true" AutoPostBack="true" OnCheckedChanged="cbMatchZip_CheckedChanged" />
                <Rock:RockCheckBox ID="cbAge" runat="server" Text="Age" Help="Limit to records that are same age +-2 years " Checked="true" AutoPostBack="true" OnCheckedChanged="cbAge_CheckedChanged" />

                <Rock:Grid ID="gPotentialMatches" runat="server" DataKeyNames="Id" AllowSorting="true" OnRowDataBound="gPotentialMatches_RowDataBound">
                    <Columns>
                        <asp:HyperLinkField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand"
                            DataNavigateUrlFields="Id" DataNavigateUrlFormatString="~/Person/{0}" DataTextFormatString="<div class='btn btn-default btn-sm'><i class='fa fa-user'></i></div>" DataTextField="Id" />
                        <Rock:RockBoundField DataField="FirstName" SortExpression="FirstName" HeaderText="First Name" />
                        <Rock:RockBoundField DataField="NickName" SortExpression="NickName" HeaderText="Nick Name" />
                        <Rock:RockBoundField DataField="MiddleName" SortExpression="MiddleName" HeaderText="Middle Name" />
                        <Rock:RockBoundField DataField="LastName" SortExpression="LastName" HeaderText="Last Name" />
                        <Rock:RockBoundField DataField="ConnectionStatusValue" SortExpression="ConnectionStatus" HeaderText="Connection Status" />
                        <Rock:RockLiteralField ID="lPersonImage" HeaderText="Image" />
                        <Rock:RockBoundField DataField="Age" SortExpression="Age" HeaderText="Age" />
                        <Rock:RockLiteralField ID="lAddressInfo" HeaderText="Address" />
                        <Rock:AttributeField DataField="SO" HeaderText="SO" />
                        <Rock:RockTemplateField ItemStyle-HorizontalAlign="Right">
                            <ItemTemplate>
                                <p>
                                    <asp:LinkButton runat="server" ID="btnIsConfirmMatch" CssClass="btn btn-default " data-toggle="tooltip" title="Confirm Match" OnClick="btnIsConfirmMatch_Click" CommandName="ConfirmMatch" CommandArgument='<%# Eval("Id") %>'><i class="fa fa-flag fa-fw"></i></asp:LinkButton>
                                </p>
                                <p>
                                    <asp:LinkButton runat="server" ID="btnSetSOAttribute" CssClass="btn btn-default " data-toggle="tooltip" title="Set SO Attribute" OnClick="btnSetSOAttribute_Click" CommandName="SetSOAttribute" CommandArgument='<%# Eval("Id") %>'><i class="fa fa-asterisk fa-fw"></i></asp:LinkButton>
                                </p>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                    </Columns>
                </Rock:Grid>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
