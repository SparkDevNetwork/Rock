<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MSSearchConnector.ascx.cs" Inherits="RockWeb.Plugins.com_CentralAZ.Utility.MSSearchConnector" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <div class="search-connector">
            <asp:Panel id="pnlQuickSearch" runat="server">
                <Rock:RockTextBox  ID="txtSearch" runat="server" />
            </asp:Panel>

	        <div id="divHeader" runat="server" class="searchResultSummary"></div>

            <Rock:NotificationBox ID="nbNoResults" runat="server" Title="No Results" NotificationBoxType="Warning" Visible="false">
                No results matching your search were found.
	            <ul>
		            <li>Check your spelling. Are the words in your query spelled correctly?</li>
		            <li>Try using synonyms. Maybe what you're looking for uses slightly different words.</li>
		            <li>Make your search more general. Try more general terms in place of specific ones.</li>
	            </ul>
            </Rock:NotificationBox>

            <Rock:NotificationBox ID="nbErrorMessage" runat="server" Title="Error" NotificationBoxType="Danger" Visible="false">
                Unable to perform serarch at this time.
            </Rock:NotificationBox>

	        <asp:DataGrid ID="dgSearchResults" runat="server" ShowHeader="false" CssClass="table table-striped" AlternatingItemStyle-CssClass="alt-row" BorderWidth="0" GridLines="none" AutoGenerateColumns="False" AllowPaging="true" PageSize="10" OnPageIndexChanged="dgSearchResults_PageIndexChanged" Width="100%">
		        <Columns> 
			        <asp:TemplateColumn> 
				        <ItemTemplate> 
						    <h4><a href="<%# DataBinder.Eval(Container.DataItem, "Path") %>"><%# HighlightKeywords( DataBinder.Eval( Container.DataItem, "Title" ) )%></a></h4>
						    <div class="text-muted">
							    <span class="searchResultLink"><a href="<%# DataBinder.Eval(Container.DataItem, "Path") %>"><%# DataBinder.Eval(Container.DataItem, "Path") %></a></span> - <%# FormatSize( DataBinder.Eval(Container.DataItem, "Size")) %>
						    </div>
                            <div class="searchResultDescription"><%# HighlightKeywords( DataBinder.Eval( Container.DataItem, "HitHighlightedSummary" ) ) %></div>						        
				        </ItemTemplate>
			        </asp:TemplateColumn>
		        </Columns>
		        <PagerStyle HorizontalAlign="Center" Mode="NumericPages" CssClass="bg-primary" />
	        </asp:DataGrid>
    
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
