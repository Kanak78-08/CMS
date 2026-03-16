<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="ComplaintManagementSystem.Login" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Admin Login - Complaint Management System</title>
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
        <div class="login-container">
            <div class="login-box">
                <div class="login-header">
                    <h2 class="system-title">Complaint Management System</h2>
                    <h3>Admin Login</h3>
                </div>
                <div class="login-body">
                    <div class="form-group">
                        <asp:Label ID="lblUsername" runat="server" Text="Username:" CssClass="label"></asp:Label>
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="textbox" placeholder="Enter username"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:Label ID="lblPassword" runat="server" Text="Password:" CssClass="label"></asp:Label>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="textbox" placeholder="Enter password"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn-login" OnClick="btnLogin_Click" />
                    </div>
                    <div class="form-group">
                        <asp:Label ID="lblError" runat="server" CssClass="error-message" Visible="false"></asp:Label>
                    </div>
                </div>
            </div>
        </div>
    </form>
</body>
</html>

