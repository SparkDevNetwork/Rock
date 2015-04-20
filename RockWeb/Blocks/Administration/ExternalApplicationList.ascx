<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ExternalApplicationList.ascx.cs" Inherits="RockWeb.Blocks.Administration.ExternalApplicationList" %>

<asp:UpdatePanel ID="upBinaryFile" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-desktop"></i> External Application List</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ShowHeader="false" ID="gExternalApplication" CssClass="table-padded" runat="server" AllowSorting="true" DisplayType="Light" OnRowDataBound="gExternalApplication_RowDataBound">
                        <Columns>
                            <Rock:AttributeField DataField="Icon" HeaderText="" ItemStyle-CssClass="grid-icon" />
                            <Rock:RockTemplateField SortExpression="Name" HeaderText="">
                                <ItemTemplate>
                                    <h4>
                                        <asp:Literal ID="lAppName" runat="server" />
                                        <small>
                                            <asp:Literal ID="lVendorName" runat="server" />
                                        </small>
                                    </h4>
                                    <%# Eval( "Description" ) %>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <asp:HyperLinkField Text="Download" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
