<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PayerList.ascx.cs" Inherits="RockWeb.Plugins.com_pushPay.RockRMS.PayerList" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-random"></i> Payer Keys</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" >
                        <Rock:RockTextBox ID="tbPayerKey" runat="server" Label="Payer Key" />
                        <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" IncludeBusinesses="true" />
                    </Rock:GridFilter>
                    <Rock:ModalAlert ID="mdGridWarning" runat="server" />
                    <Rock:Grid ID="rGrid" runat="server" AllowSorting="true" RowItemText="Payer" OnRowSelected="rGrid_Edit" TooltipField="Description" >
                        <Columns>
                            <Rock:RockBoundField DataField="Id" HeaderText="Id" Visible="false" />
                            <Rock:RockBoundField DataField="PayerKey" HeaderText="Key" SortExpression="PayerKey" />
                            <Rock:RockTemplateField HeaderText="Person" SortExpression="PersonAlias.Person.LastName,PersonAlias.Person.NickName">
                                <ItemTemplate>
                                    <asp:Label ID="lblFullName" runat="server"><%# Eval("LastName") %><%# Eval("NickName") %></asp:Label>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <asp:HyperLinkField ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="grid-columncommand" HeaderStyle-CssClass="grid-columncommand"
                                DataNavigateUrlFields="PersonId" DataNavigateUrlFormatString="/Person/{0}"
                                DataTextFormatString="<div class='btn btn-default btn-sm'><i class='fa fa-user'></i></div>"
                                DataTextField="PersonId" />
                            <Rock:DeleteField OnClick="rGrid_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

            <Rock:ModalDialog ID="modalValue" runat="server" Title="Pushpay Payer" ValidationGroup="Value" >
                <Content>

                    <asp:HiddenField ID="hfPayerId" runat="server" />
                    <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Value" />

                    <Rock:RockLiteral ID="lPayerKey" runat="server" Label="Payer Key" />
                    <Rock:PersonPicker ID="ppPersonEdit" runat="server" Label="Person" IncludeBusinesses="true" ValidationGroup="Value" Required="true" />

                </Content>
            </Rock:ModalDialog>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
