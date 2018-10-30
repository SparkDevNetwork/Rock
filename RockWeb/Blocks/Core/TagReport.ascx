﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagReport.ascx.cs" Inherits="RockWeb.Blocks.Core.TagReport" %>

<asp:UpdatePanel ID="upPanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <Rock:ModalAlert ID="mdGridWarning" runat="server" />
            
            <asp:Panel ID="pnlGrid" runat="server" cssClass="panel panel-block">
                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-check-square"></i> <asp:Literal ID="lTaggedTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">
                    <div class="grid grid-panel">
                        <Rock:Grid ID="gReport" runat="server" AllowSorting="true" RowItemText="Tag" EmptyDataText="No Results" OnRowSelected="gReport_RowSelected" ExportSource="ColumnOutput" ExportTitleName="Tagged People" >
                            <Columns>
                                <Rock:SelectField Visible="false" />
                                <Rock:RockBoundField DataField="Id" Visible="false" />
                                <Rock:RockBoundField DataField="PersonId" Visible="false" />
                                <Rock:RockTemplateField HeaderText="Item">
                                    <ItemTemplate><%# GetItemName( (int)Eval( "EntityTypeId" ), (Guid)Eval( "EntityGuid" ) ) %></ItemTemplate>
                                </Rock:RockTemplateField>
                                <Rock:DateField HeaderText="Date Tagged" DataField="CreatedDateTime" SortExpression="CreatedDateTime" />
                                <Rock:DeleteField OnClick="gReport_Delete" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </div>
            </asp:Panel>

        </asp:Panel>

        <Rock:ModalDialog ID="mdAddPerson" runat="server" Title="Add Person" OnSaveClick="mdAddPerson_SaveClick" OnSaveThenAddClick="mdAddPerson_SaveThenAddClick" ValidationGroup="AddPerson">
            <Content>
                <asp:ValidationSummary ID="vsAddPerson" runat="server" CssClass="alert alert-danger" ValidationGroup="AddPerson" />
                <Rock:NotificationBox ID="nbAddPersonExists" runat="server" NotificationBoxType="Danger" Visible="false">
                    Person already exists in the tag.
                </Rock:NotificationBox>

                <Rock:PersonPicker ID="ppNewPerson" runat="server" Label="Person" Required="true" CssClass="js-newperson" ValidationGroup="AddPerson" />
            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
