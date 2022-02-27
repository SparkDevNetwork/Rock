<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CommunicationsBlock.ascx.cs" Inherits="RockWeb.Plugins.com_intulse.PbxComponent.CommunicationsBlock" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <section class="panel panel-persondetails">
            <div class="panel-heading clearfix">
                <h3 class="panel-title pull-left"><i class="fa fa-phone"></i> Intulse Communications</h3>
            </div>

            <Rock:NotificationBox ID="errorBox" runat="server" Visible="false" NotificationBoxType="Danger" Title="ERROR" Text="There was an error while getting your communication history - try adjusting the date filter. If the problem persists please contact Intulse." />

            <div class="grid grid-panel">
                <%--TODO: With updated Rock version (> 9) GridFilter.cs gets the FieldLayout property, then we can use the following block of code--%>
                <%--<Rock:GridFilter ID="gridFilterCommunications" runat="server" OnApplyFilterClick="gridFilterCommunications_ApplyFilterClick" FieldLayout="Custom">
                    <div class="row">
                        <div class="col-md-2">
                            <Rock:DateRangePicker ID="filterDates" runat="server" Label="Date Range"  />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockTextBox ID="filterName" runat="server" Label="Name"  />
                        </div>
                        <div class="col-md-3">
                            <Rock:RockTextBox ID="filterNumber" runat="server" Label="Number" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-1">
                            <Rock:RockCheckBox ID="filterShowCdr" runat="server" Label="Show Calls" Checked="true" />
                        </div>
                        <div class="col-md-1">
                            <Rock:RockCheckBox ID="filterShowSms" runat="server" Label="Show SMS" Checked="true" />
                        </div>
                    </div>
                </Rock:GridFilter>--%>
                <Rock:GridFilter ID="gridFilterCommunications" runat="server" OnApplyFilterClick="gridFilterCommunications_ApplyFilterClick">
                    <Rock:DateRangePicker ID="filterDates" runat="server" Label="Date Range"  />
                    <Rock:RockTextBox ID="filterName" runat="server" Label="Name"  />
                    <Rock:RockTextBox ID="filterNumber" runat="server" Label="Number" />
                    <Rock:RockCheckBox ID="filterShowCdr" runat="server" Label="Show Calls" Checked="true" />
                    <Rock:RockCheckBox ID="filterShowSms" runat="server" Label="Show SMS" Checked="true" />
                </Rock:GridFilter>
                <Rock:Grid ID="gridCommunications" runat="server" AllowSorting="true" AllowPaging="true" DataKeyNames="CommunicationId" OnRowDataBound="gridCommunications_RowDataBound" RowItemText="Communication">
                    <Columns>
                        <Rock:RockBoundField DataField="CommunicationDateUtc" HeaderText="Date" SortExpression="CommunicationDateUtc" HeaderStyle-Width="12%" />
                        <Rock:RockBoundField DataField="Type" HeaderText="Type" SortExpression="Type" HeaderStyle-Width="8%" />
                        <Rock:RockBoundField DataField="Source" HeaderText="From" SortExpression="Source" HeaderStyle-Width="20%" />
                        <Rock:RockBoundField DataField="Destination" HeaderText="To" SortExpression="Destination" HeaderStyle-Width="20%" />
                        <Rock:RockTemplateField  HeaderText="Message/Notes">
                            <ItemTemplate>
                                <asp:Literal ID="communicationNoteLiteral" runat="server"></asp:Literal>
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:EditField OnClick="gridCommunications_Edit" />
                    </Columns>
                </Rock:Grid>
                <Rock:ModalDialog ID="noteModal" runat="server" Title="Edit Note" OnSaveClick="noteModal_Save">
                    <Content>
                        <fieldset>
                            <Rock:RockTextBox ID="noteModalTextbox" runat="server" TextMode="MultiLine" Rows="5" />
                            <asp:HiddenField ID ="noteModalCommunicationId" runat="server"/>
                        </fieldset>
                    </Content>
                </Rock:ModalDialog>
            </div>
        </section>
    </ContentTemplate>
</asp:UpdatePanel>