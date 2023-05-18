<%@ Control Language="C#" AutoEventWireup="true" CodeFile="SnippetTypeList.ascx.cs" Inherits="RockWeb.Blocks.Communication.SnippetTypeList" %>

<asp:UpdatePanel ID="upSnippetTypes" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-file-alt"></i>
                    Snippet Types
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gSnippetTypes" runat="server" AllowSorting="true">
                        <Columns>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                            <Rock:RockBoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                            <Rock:RockBoundField DataField="HelpText" HeaderText="Help Text" SortExpression="HelpText" />
                            <Rock:BoolField DataField="IsPersonalAllowed" HeaderText="Is Personal Allowed" SortExpression="IsPersonalAllowed" />
                            <Rock:BoolField DataField="IsSharedAllowed" HeaderText="Is Shared Allowed" SortExpression="IsSharedAllowed" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gSnippetTypes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>