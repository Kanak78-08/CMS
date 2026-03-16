using System;
using System.Web;
using System.Web.UI;

namespace ComplaintManagementSystem
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Required for WebForms UnobtrusiveValidationMode (validators) to load jQuery without crashing.
            ScriptManager.ScriptResourceMapping.AddDefinition(
                "jquery",
                new ScriptResourceDefinition
                {
                    // Some WebForms versions still require Path/DebugPath (or an embedded ResourceName) to be non-empty.
                    // Using the same CDN URLs here avoids the "ResourceName and Path cannot both be empty" exception.
                    Path = "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.7.1.min.js",
                    DebugPath = "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.7.1.js",
                    CdnPath = "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.7.1.min.js",
                    CdnDebugPath = "https://ajax.aspnetcdn.com/ajax/jQuery/jquery-3.7.1.js",
                    CdnSupportsSecureConnection = true,
                    LoadSuccessExpression = "window.jQuery",
                });
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            // Code that runs at the beginning of each request
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            // Code that runs to authenticate the request
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }

        protected void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends
        }

        protected void Application_End(object sender, EventArgs e)
        {
            // Code that runs on application shutdown
        }
    }
}

