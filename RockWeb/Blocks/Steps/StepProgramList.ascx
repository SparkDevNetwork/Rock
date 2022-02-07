<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StepProgramList.ascx.cs" Inherits="RockWeb.Blocks.Steps.StepProgramList" %>

<asp:UpdatePanel ID="upStepProgramList" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="mdGridWarning" runat="server" />
        
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-map-marked-alt"></i> Step Programs</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">
                    <Rock:GridFilter ID="rFilter" runat="server" OnClearFilterClick="rFilter_ClearFilterClick">
                        <Rock:CategoryPicker ID="cpCategory" runat="server" Label="Category" Required="false" EntityTypeName="Rock.Model.StepProgram" />
                        <Rock:RockDropDownList ID="ddlActiveFilter" runat="server" Label="Active Status">
                            <asp:ListItem></asp:ListItem>
                            <asp:ListItem Text="Active" Value="Active"></asp:ListItem>
                            <asp:ListItem Text="Inactive" Value="Inactive"></asp:ListItem>
                        </Rock:RockDropDownList>
                    </Rock:GridFilter>
                    <Rock:Grid ID="gStepProgram" runat="server" AllowSorting="false">
                        <Columns>
                            <Rock:ReorderField />
                             <Rock:RockTemplateField ExcelExportBehavior="NeverInclude" HeaderStyle-Width="48px">
                                <ItemTemplate>
                                   <i class="fa-fw <%# Eval( "IconCssClass" ) %>"></i>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockBoundField DataField="Name" HeaderText="Name" SortExpression="Name" Visible="true" ExcelExportBehavior="AlwaysInclude" />
                            <Rock:RockBoundField DataField="Category" HeaderText="Category"/>
                            <Rock:RockBoundField DataField="StepTypeCount" HeaderText="Step Types" DataFormatString="{0:N0}" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                            <Rock:RockBoundField DataField="StepCompletedCount" HeaderText="Steps Taken" DataFormatString="{0:N0}" HeaderStyle-HorizontalAlign="Right" ItemStyle-HorizontalAlign="Right" />
                            <Rock:SecurityField TitleField="Name" />
                            <Rock:DeleteField OnClick="gStepProgram_Delete" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
