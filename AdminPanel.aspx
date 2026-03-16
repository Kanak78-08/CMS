<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminPanel.aspx.cs" Inherits="ComplaintManagementSystem.AdminPanel" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Admin Panel - Complaint Management System</title>
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
                    <asp:Button ID="btnAddNewComplaint" runat="server" Text="Add New Complaint" CssClass="btn-submit" OnClick="btnAddNewComplaint_Click" />
                    <asp:Button ID="btnGoToList" runat="server" Text="Go to Complaints List" CssClass="btn-submit" PostBackUrl="~/ComplaintsList.aspx" />
                    <asp:Button ID="btnUpdateComplaint" runat="server" Text="Update Complaint" CssClass="btn-submit" OnClick="btnUpdateComplaint_Click" />
                    <asp:Button ID="btnLogout" runat="server" Text="Logout" CssClass="btn-logout" OnClick="btnLogout_Click" />
                </div>
            </div>

            <div class="admin-content">
                <div class="page-title-section">
                    <h2 class="system-title">Complaint Management System - Admin Panel</h2>
                </div>

                <!-- Add New Complaint Form -->
                <asp:Panel ID="pnlAddNew" runat="server">
                <div class="form-section">
                    <h2>
                        <asp:Label ID="lblFormTitle" runat="server" Text="Add New Complaint"></asp:Label>
                    </h2>
                    <div class="form-container">
                        <!-- Row 1: All 5 fields side by side (Add Mode) or All 5 fields (Edit Mode) -->
                        <div id="row1Fields" runat="server" class="form-row-five">
                            <div class="form-group">
                                <asp:Label ID="lblDescription" runat="server" Text="Description of Problem:" CssClass="label"></asp:Label>
                                <asp:DropDownList ID="ddlProblemCategory" runat="server" CssClass="dropdown">
                                    <asp:ListItem Value="">-- Select Problem Type --</asp:ListItem>
                                    <asp:ListItem>PC/Laptop Software Operations</asp:ListItem>
                                    <asp:ListItem>PC/Laptop Connectivity</asp:ListItem>
                                    <asp:ListItem>WIFI / Network</asp:ListItem>
                                    <asp:ListItem>Printer / Scanner</asp:ListItem>
                                    <asp:ListItem>ERP</asp:ListItem>
                                    <asp:ListItem>Others</asp:ListItem>
                                </asp:DropDownList>
                                <asp:TextBox ID="txtDescription" runat="server" CssClass="textbox small-textbox" placeholder="Additional details (optional)"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <asp:Label ID="lblUserDetails" runat="server" Text="User Name / Desk No:" CssClass="label"></asp:Label>
                                <asp:TextBox ID="txtUserDetails" runat="server" CssClass="textbox" placeholder="e.g., Rahul - Desk 2505"></asp:TextBox>
                            </div>
                            <div class="form-group">
                                <asp:Label ID="lblAssignedTo" runat="server" Text="Assigned To:" CssClass="label"></asp:Label>
                                <asp:DropDownList ID="ddlAssignedTo" runat="server" CssClass="dropdown">
                                    <asp:ListItem Value="">-- Select Manager --</asp:ListItem>
                                    <asp:ListItem Value="Kuldeep">Kuldeep</asp:ListItem>
                                    <asp:ListItem Value="Kamal">Kamal</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="form-group" id="logDateFieldAdd" runat="server">
                                <asp:Label ID="lblLogDateTimeAdd" runat="server" Text="Log Date & Call Received Time (auto):" CssClass="label"></asp:Label>
                                <asp:TextBox ID="txtLogDateTimeAdd" runat="server" CssClass="textbox" TextMode="DateTimeLocal" ReadOnly="true" placeholder="Will be set automatically"></asp:TextBox>
                            </div>
                            <div class="form-group" id="statusFieldAdd" runat="server">
                                <asp:Label ID="lblStatusAdd" runat="server" Text="Status:" CssClass="label"></asp:Label>
                                <asp:DropDownList ID="ddlStatusAdd" runat="server" CssClass="dropdown">
                                </asp:DropDownList>
                            </div>
                            <!-- Edit Mode Only - Hidden in Add Mode -->
                            <div class="form-group" id="logDateField" runat="server" style="display: none;">
                                <asp:Label ID="lblLogDateTime" runat="server" Text="Log Date & Call Received Time (auto):" CssClass="label"></asp:Label>
                                <asp:TextBox ID="txtLogDateTime" runat="server" CssClass="textbox" TextMode="DateTimeLocal" ReadOnly="true" placeholder="Will be set automatically"></asp:TextBox>
                            </div>
                            <div class="form-group" id="statusField" runat="server" style="display: none;">
                                <asp:Label ID="lblStatus" runat="server" Text="Status:" CssClass="label"></asp:Label>
                                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="dropdown">
                                </asp:DropDownList>
                            </div>
                            <!-- Edit Mode Only - Resolution Date & Action Taken (Hidden in Add Mode) -->
                            <div class="form-group" id="resolutionDateField" runat="server" style="display: none;">
                                <asp:Label ID="lblResolutionDate" runat="server" Text="Resolution Date & Time:" CssClass="label"></asp:Label>
                                <asp:TextBox ID="txtResolutionDate" runat="server" CssClass="textbox" TextMode="DateTimeLocal"></asp:TextBox>
                            </div>
                            <div class="form-group" id="actionTakenField" runat="server" style="display: none;">
                                <asp:Label ID="lblActionTaken" runat="server" Text="Action Taken with Details:" CssClass="label"></asp:Label>
                                <asp:TextBox ID="txtActionTaken" runat="server" CssClass="textbox textarea-small" TextMode="MultiLine" Rows="1" placeholder="Enter action taken (max 15 words)"></asp:TextBox>
                            </div>
                        </div>

                        <!-- Row 2: Hidden in Add Mode, used for Edit Mode (Resolution, Action Taken, Buttons) -->
                        <div id="row2Fields" runat="server" class="form-row-two" style="display: none;">
                        </div>

                        <!-- Edit Mode Only Fields - Buttons Centered -->
                        <asp:Panel ID="pnlEditFields" runat="server" Visible="false">
                            <div class="form-actions-edit" style="text-align: center; margin-top: 20px;">
                                <asp:Button ID="btnSubmit" runat="server" Text="Update Complaint" CssClass="btn-submit" OnClick="btnSubmit_Click" />
                                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn-reset" OnClick="btnCancel_Click" />
                            </div>
                        </asp:Panel>

                        <!-- Add Mode Actions - Centered -->
                        <div id="addModeActions" runat="server" class="form-actions" style="text-align: center; margin-top: 20px;">
                            <asp:Button ID="btnSubmitAdd" runat="server" Text="Add Complaint" CssClass="btn-submit" OnClick="btnSubmit_Click" />
                            <asp:Button ID="btnResetAdd" runat="server" Text="Reset" CssClass="btn-reset" OnClick="btnReset_Click" />
                        </div>

                        <div class="form-group">
                            <asp:Label ID="lblMessage" runat="server" CssClass="success-message" Visible="false"></asp:Label>
                        </div>
                    </div>
                </div>
                </asp:Panel>

                <!-- Update Complaint Table -->
                <asp:Panel ID="pnlUpdateComplaint" runat="server" Visible="false">
                    <div class="page-title-section">
                        <h2 class="system-title">Update Complaint</h2>
                    </div>
                    <div class="table-section">
                        <div class="table-container">
                            <asp:GridView ID="gvUpdateComplaints" runat="server" CssClass="complaints-table" AutoGenerateColumns="false"
                                HeaderStyle-CssClass="table-header" RowStyle-CssClass="table-row" AlternatingRowStyle-CssClass="table-row-alt"
                                AllowPaging="true" PageSize="10" OnPageIndexChanging="gvUpdateComplaints_PageIndexChanging"
                                OnRowCommand="gvUpdateComplaints_RowCommand">
                                <Columns>
                                    <asp:TemplateField HeaderText="S.No." ItemStyle-Width="50px" ItemStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <%# GetSerialNumber(Container.DataItemIndex) %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="Description" HeaderText="Description" ItemStyle-Width="100px" />
                                    <asp:BoundField DataField="UserDetails" HeaderText="User Details" ItemStyle-Width="70px" />
                                    <asp:TemplateField HeaderText="Log Date" ItemStyle-Width="65px">
                                        <ItemTemplate>
                                            <%# ((DateTime)Eval("LogDate")).ToString("dd.MM.yyyy") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:TemplateField HeaderText="Time" ItemStyle-Width="50px">
                                        <ItemTemplate>
                                            <%# ((DateTime)Eval("LogDate")).ToString("HH:mm") %>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="AssignedTo" HeaderText="Assigned To" ItemStyle-Width="60px" />
                                    <asp:TemplateField HeaderText="Resolution" ItemStyle-Width="100px">
                                        <ItemTemplate>
                                            <asp:Label ID="lblResolutionDateTime" runat="server"
                                                Text='<%# Eval("ResolutionDateTime") != null ? ((DateTime)Eval("ResolutionDateTime")).ToString("dd-MM-yyyy HH:mm") : "" %>'></asp:Label>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="ActionTaken" HeaderText="Action Taken" ItemStyle-Width="100px" />
                                    <asp:BoundField DataField="Status" HeaderText="Status" ItemStyle-Width="60px" ItemStyle-CssClass="text-center" />
                                    <asp:TemplateField HeaderText="Actions" ItemStyle-Width="50px" ItemStyle-CssClass="text-center actions-cell" HeaderStyle-CssClass="text-center">
                                        <ItemTemplate>
                                            <div class="action-buttons-compact">
                                                <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn-edit-compact"
                                                    CommandName="EditComplaint" CommandArgument='<%# Eval("Id") %>' 
                                                    CausesValidation="false" />
                                                <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn-delete-compact"
                                                    CommandName="DeleteComplaint" CommandArgument='<%# Eval("Id") %>'
                                                    OnClientClick="return confirm('Are you sure you want to delete this complaint?');"
                                                    CausesValidation="false" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>
                                </Columns>
                                <EmptyDataTemplate>
                                    <div class="empty-data">
                                        <p>No complaints found.</p>
                                    </div>
                                </EmptyDataTemplate>
                            </asp:GridView>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </form>
</body>
</html>
