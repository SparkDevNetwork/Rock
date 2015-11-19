<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendance.ascx.cs" Inherits="com.reallifeministries.Attendance.GroupAttendance" %>

<Rock:RockUpdatePanel runat="server" ID="rupMain">
    <ContentTemplate>
        <Rock:PanelWidget runat="server" ID="pnlMain" Title="Group Attendance" Expanded="true">
            <Rock:NotificationBox runat="server" ID="nbMessage" Visible="false" />
            
          <asp:Panel runat="server" ID="pnlResults" Visible="true">
              <h4>Attendees</h4>
              <div class="table-responsive">
                  <table class="table table-striped table-bordered table-hover">
                      <thead>
                        <tr>
                            <th>
                                <asp:LinkButton runat="server" OnCommand="SortGrid" CommandName="PersonName" Text="Person"/>
                            </th>
                            <th><asp:LinkButton runat="server" OnCommand="SortGrid" CommandName="attendedWeekend" Text="Last Attended Service"/></th>
                            <th><asp:LinkButton runat="server" OnCommand="SortGrid" CommandName="attendedGroup" Text="Last Attended Group"/></th>
                            <th><asp:CheckBox runat="server" CssClass="checkall" ID="cbCheckall" /></th>
                        </tr>
                      </thead>
                      <tbody>
                          <asp:Repeater ID="rptAttendees" runat="server">
                              <ItemTemplate>
                                  <tr>
                                      <td>
                                          <%#Eval("Person.FullName") %>
                                          <span class="label label-default"><%#Eval("Role.Name") %></span>
                                      </td>
                                      <td><%#ElaspedTime((System.DateTime?)Eval( "lastAttendedService" )) %></td>
                                      <td><%#ElaspedTime((System.DateTime?)Eval( "lastAttendedGroup" )) %></td>
                                      <td>
                                          <asp:CheckBox runat="server" ID="didAttend" />
                                          <asp:HiddenField runat="server" ID="personId" Value='<%#Eval("Person.Id")%>' />
                                      </td>
                                  </tr>
                              </ItemTemplate>
                          </asp:Repeater>
                      </tbody>
                      <tfoot>
                          <tr>
                              <td colspan="4" class="text-right form-inline">
                                      <Rock:DateTimePicker runat="server" ID="dpAttendedDate" Label="Record Attendance For " />
                                      <Rock:BootstrapButton runat="server" ID="btnRecordAttendance" cssclass="btn btn-primary" Text="Record" OnClick="btnRecordAttendance_Click" />
                                  
                              </td>
                          </tr>
                      </tfoot>
                  </table>
              </div>
              <script>
                  var $checkAll = $("#<%=cbCheckall.ClientID%>");
                  $checkAll.on('change', function () {
                      $checkAll.closest('table').find('tbody input[type=checkbox]').prop('checked', $checkAll.prop('checked'));
                  });
              </script>
          </asp:Panel>
          
         
        </Rock:PanelWidget> 
    </ContentTemplate>
</Rock:RockUpdatePanel>

