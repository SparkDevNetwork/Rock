<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DocumentTypeList.ascx.cs" Inherits="RockWeb.Blocks.Crm.DocumentTypeList" %>

<asp:UpdatePanel ID="upDocumentTypes" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-file-alt"></i>
                    Document Types
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:Grid ID="gDocumentTypes" runat="server">
                        <Columns>
                            <Rock:ReorderField />
                            <Rock:RockTemplateField ExcelExportBehavior="NeverInclude" HeaderStyle-Width="48px">
                                <ItemTemplate>
                                    <i class="fa-fw <%# Eval( "IconCssClass" ) %>"></i>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                            <Rock:RockBoundField DataField="FileTypeName" HeaderText="File Type" />
                            <Rock:RockBoundField DataField="EntityName" HeaderText="Entity type" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gDocumentTypes_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
