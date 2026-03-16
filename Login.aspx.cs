using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace ComplaintManagementSystem
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("AdminPanel.aspx");
            }

            // Check if logo file exists
            string logoPath = Server.MapPath("~/Content/tcil.png");
            if (System.IO.File.Exists(logoPath))
            {
                imgLogo.Visible = true;
            }
            else
            {
                imgLogo.Visible = false;
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Simple authentication (in production, use database authentication)
            if (username == "admin" && password == "admin123")
            {
                FormsAuthentication.SetAuthCookie(username, false);
                Session["Username"] = username;
                Response.Redirect("AdminPanel.aspx");
            }
            else
            {
                lblError.Text = "Invalid username or password. Please try again.";
                lblError.Visible = true;
            }
        }
    }
}

