<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ComplaintsList.aspx.cs" Inherits="ComplaintManagementSystem.ComplaintsList" EnableEventValidation="false" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Complaints List - Complaint Management System</title>
    <link href="Content/Site.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="tcil-header">
            <div class="tcil-header-content">
                <div class="tcil-logo">
                    <asp:Image ID="imgLogo" runat="server" ImageUrl="~/Content/tcil.png" AlternateText="TCIL Logo" CssClass="logo-img" />
                </div>
                <div class="tcil-title">
                    <h1>Telecommunications Consultants India Limited</h1>
                    <p class="tcil-subtitle">(A Government of India Enterprise)</p>
                </div>
            </div>
        </div>

        <div class="admin-container">
            <div class="admin-header">
                <div class="user-info">
                    <asp:Label ID="lblWelcome" runat="server" Text="Welcome, Admin"></asp:Label>
                    <asp:Button ID="btnAddNew" runat="server" Text="Add New Complaint" CssClass="btn-submit" PostBackUrl="~/AdminPanel.aspx" />
                    <asp:Button ID="btnUpdateComplaint" runat="server" Text="Update Complaint" CssClass="btn-submit" PostBackUrl="~/AdminPanel.aspx?view=update" />
                    <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="btn-logout" OnClick="btnLogout_Click" />
                </div>
            </div>

            <div class="admin-content">
                <div class="page-title-section">
                    <h2 class="system-title">Complaints List</h2>
                </div>

                <!-- Filters -->
                <asp:Panel ID="pnlFilters" runat="server" CssClass="filter-section">
                    <div class="filter-row-single">
                        <div class="filter-group-compact">
                            <asp:Label ID="lblSearchDescription" runat="server" Text="Description:" CssClass="label-compact"></asp:Label>
                            <asp:TextBox ID="txtSearchDescription" runat="server" CssClass="textbox-compact" placeholder="Keyword"></asp:TextBox>
                        </div>
                        <div class="filter-group-compact">
                            <asp:Label ID="lblFilterStatus" runat="server" Text="Status:" CssClass="label-compact"></asp:Label>
                            <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="dropdown-compact"></asp:DropDownList>
                        </div>
                        <div class="filter-group-compact">
                            <asp:Label ID="lblFilterAssigned" runat="server" Text="Assigned To:" CssClass="label-compact"></asp:Label>
                            <asp:DropDownList ID="ddlFilterAssigned" runat="server" CssClass="dropdown-compact"></asp:DropDownList>
                        </div>
                        <div class="filter-group-compact">
                            <asp:Label ID="lblDateFrom" runat="server" Text="Date From:" CssClass="label-compact"></asp:Label>
                            <asp:TextBox ID="txtDateFrom" runat="server" CssClass="textbox-compact" TextMode="Date"></asp:TextBox>
                        </div>
                        <div class="filter-group-compact">
                            <asp:Label ID="lblDateTo" runat="server" Text="Date To:" CssClass="label-compact"></asp:Label>
                            <asp:TextBox ID="txtDateTo" runat="server" CssClass="textbox-compact" TextMode="Date"></asp:TextBox>
                        </div>
                        <div class="filter-group-compact">
                            <asp:Label ID="lblSortBy" runat="server" Text="Sort By:" CssClass="label-compact"></asp:Label>
                            <asp:DropDownList ID="ddlSortBy" runat="server" CssClass="dropdown-compact"></asp:DropDownList>
                        </div>
                        <div class="filter-buttons-compact">
                            <asp:Button ID="btnApplyFilters" runat="server" Text="Apply" CssClass="btn-filter" OnClick="btnApplyFilters_Click" />
                            <asp:Button ID="btnClearFilters" runat="server" Text="Clear" CssClass="btn-filter btn-reset" OnClick="btnClearFilters_Click" />
                            <asp:Button ID="btnViewFullList" runat="server" Text="List" CssClass="btn-filter btn-reset" PostBackUrl="~/ComplaintsList.aspx?filters=off" />
                        </div>
                    </div>
                </asp:Panel>

                <div class="form-group">
                    <asp:Label ID="lblMessage" runat="server" CssClass="success-message" Visible="false"></asp:Label>
                </div>

                <div class="table-section">
                    <div class="table-container">
                        <asp:GridView ID="gvComplaints" runat="server" CssClass="complaints-table" AutoGenerateColumns="false"
                            HeaderStyle-CssClass="table-header" RowStyle-CssClass="table-row" AlternatingRowStyle-CssClass="table-row-alt"
                            AllowPaging="true" PageSize="10" OnPageIndexChanging="gvComplaints_PageIndexChanging"
                            OnRowCommand="gvComplaints_RowCommand" OnDataBound="gvComplaints_DataBound"
                            PagerSettings-Mode="NumericFirstLast" PagerSettings-Position="Bottom" 
                            PagerSettings-PageButtonCount="5" PagerStyle-CssClass="pager-style">
                            <Columns>
                                <asp:TemplateField HeaderText="S.No." ItemStyle-Width="50px" ItemStyle-CssClass="text-center">
                                    <ItemTemplate>
                                        <%# GetSerialNumber(Container.DataItemIndex) %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="UserDetails" HeaderText="User Details" ItemStyle-Width="120px" />
                                <asp:TemplateField HeaderText="Log Date" ItemStyle-Width="100px">
                                    <ItemTemplate>
                                        <%# ((DateTime)Eval("LogDate")).ToString("dd.MM.yyyy") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Log Time" ItemStyle-Width="100px">
                                    <ItemTemplate>
                                        <%# ((DateTime)Eval("LogDate")).ToString("HH:mm") %>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="AssignedTo" HeaderText="Assigned To" ItemStyle-Width="100px" />
                                <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-Width="100px" ItemStyle-CssClass="text-center" />
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="empty-data">
                                    <p>No complaints added yet. Please add a new complaint using the Add New Complaint button.</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>


