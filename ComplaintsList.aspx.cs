using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;

namespace ComplaintManagementSystem
{
    public partial class ComplaintsList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("Login.aspx");
            }

            // Prevent caching to ensure fresh data
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
            Response.Cache.SetValidUntilExpires(false);

            if (!IsPostBack)
            {
                lblWelcome.Text = "Welcome, " + (Session["Username"] ?? "Admin");
                PopulateFilters();
                ToggleFiltersVisibility();
                gvComplaints.PageIndex = 0; // Ensure we start at page 1 (index 0)
                
                // Show success message if redirected after update
                if (Request.QueryString["updated"] == "1")
                {
                    string updatedStatus = Request.QueryString["status"] ?? "";
                    if (!string.IsNullOrEmpty(updatedStatus))
                    {
                        ShowMessage("Complaint updated successfully! Status changed to: " + updatedStatus, true);
                    }
                    else
                    {
                        ShowMessage("Complaint updated successfully!", true);
                    }
                    
                    // Force clear any cached grid data to ensure fresh data is loaded
                    ViewState.Remove("TotalComplaints");
                    ViewState.Remove("CorrectedPageIndex");
                }
            }

            BindComplaintsGrid();

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

        private void PopulateFilters()
        {
            // Status filter
            ddlFilterStatus.Items.Clear();
            ddlFilterStatus.Items.Add(new ListItem("All", ""));
            ddlFilterStatus.Items.Add(new ListItem("Pending", "Pending"));
            ddlFilterStatus.Items.Add(new ListItem("In Progress", "In Progress"));
            ddlFilterStatus.Items.Add(new ListItem("Closed", "Closed"));

            // Assigned filter
            ddlFilterAssigned.Items.Clear();
            ddlFilterAssigned.Items.Add(new ListItem("All", ""));
            ddlFilterAssigned.Items.Add(new ListItem("Kuldeep", "Kuldeep"));
            ddlFilterAssigned.Items.Add(new ListItem("Kamal", "Kamal"));

            // Sort options
            ddlSortBy.Items.Clear();
            ddlSortBy.Items.Add(new ListItem("Log Date (Newest first)", "LogDateDesc"));
            ddlSortBy.Items.Add(new ListItem("Log Date (Oldest first)", "LogDateAsc"));
        }

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        private void BindComplaintsGrid()
        {
            try
            {
                // Clear any cached data and load fresh from database on each request
                ViewState.Remove("TotalComplaints");
                ViewState.Remove("CorrectedPageIndex");
                
                // Force fresh data load - clear any potential caching
                gvComplaints.DataSource = null;
                gvComplaints.DataBind();
                
                var complaints = DatabaseHelper.GetAllComplaints();

                bool filtersEnabled = pnlFilters.Visible;

                if (filtersEnabled)
                {
                    // Apply filters
                    string keyword = txtSearchDescription.Text.Trim().ToLower();
                    string status = ddlFilterStatus.SelectedValue;
                    string assigned = ddlFilterAssigned.SelectedValue;
                    DateTime fromDate, toDate;

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        complaints = complaints.Where(c => (c.Description ?? "").ToLower().Contains(keyword)).ToList();
                    }

                    if (!string.IsNullOrEmpty(status))
                    {
                        complaints = complaints.Where(c => c.Status == status).ToList();
                    }

                    if (!string.IsNullOrEmpty(assigned))
                    {
                        complaints = complaints.Where(c => c.AssignedTo == assigned).ToList();
                    }

                    if (DateTime.TryParse(txtDateFrom.Text, out fromDate))
                    {
                        complaints = complaints.Where(c => c.LogDate.Date >= fromDate.Date).ToList();
                    }

                    if (DateTime.TryParse(txtDateTo.Text, out toDate))
                    {
                        complaints = complaints.Where(c => c.LogDate.Date <= toDate.Date).ToList();
                    }

                    // Sort
                    switch (ddlSortBy.SelectedValue)
                    {
                        case "LogDateAsc":
                            complaints = complaints.OrderBy(c => c.LogDate).ToList();
                            break;
                        default: // LogDateDesc
                            complaints = complaints.OrderByDescending(c => c.LogDate).ToList();
                            break;
                    }
                }
                else
                {
                    // Default sorting when filters are hidden
                    complaints = complaints.OrderByDescending(c => c.LogDate).ToList();
                }

                // Store total count in ViewState for pagination display
                int totalComplaints = complaints.Count;
                ViewState["TotalComplaints"] = totalComplaints;
                
                // Ensure PageIndex is valid for the current data set
                int maxPageIndex = totalComplaints > 0 ? (totalComplaints - 1) / gvComplaints.PageSize : 0;
                if (gvComplaints.PageIndex < 0) gvComplaints.PageIndex = 0;
                if (gvComplaints.PageIndex > maxPageIndex) gvComplaints.PageIndex = maxPageIndex;
                
                // Store the corrected PageIndex in ViewState to ensure it persists
                ViewState["CorrectedPageIndex"] = gvComplaints.PageIndex;
                
                gvComplaints.DataSource = complaints;
                gvComplaints.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading complaints: " + ex.Message, false);
            }
        }

        protected void gvComplaints_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvComplaints.PageIndex = e.NewPageIndex;
            BindComplaintsGrid();
        }

        protected void gvComplaints_DataBound(object sender, EventArgs e)
        {
            // Ensure PageIndex is still valid after data binding
            int totalRecords = ViewState["TotalComplaints"] != null ? (int)ViewState["TotalComplaints"] : 0;
            if (totalRecords > 0)
            {
                int maxPageIndex = (totalRecords - 1) / gvComplaints.PageSize;
                if (gvComplaints.PageIndex > maxPageIndex)
                {
                    gvComplaints.PageIndex = maxPageIndex;
                }
            }
            
            // Add custom pagination info - ensure it matches the serial number calculation
            GridViewRow pagerRow = gvComplaints.BottomPagerRow;
            if (pagerRow != null && pagerRow.Cells.Count > 0)
            {
                int pageSize = gvComplaints.PageSize;
                int pageIndex = gvComplaints.PageIndex; // 0-based index (0 = page 1, 1 = page 2, etc.)
                
                // Calculate start and end record numbers (matching the serial number formula exactly)
                // Formula: (PageIndex * PageSize) + DataItemIndex + 1
                // For first row on page: (PageIndex * PageSize) + 0 + 1 = (PageIndex * PageSize) + 1
                // For last row on page: (PageIndex * PageSize) + (PageSize-1) + 1 = (PageIndex + 1) * PageSize
                int startRecord = totalRecords > 0 ? (pageIndex * pageSize) + 1 : 0;
                int endRecord = totalRecords > 0 ? Math.Min((pageIndex + 1) * pageSize, totalRecords) : 0;
                
                // Create pagination info label
                Label lblPagerInfo = new Label();
                if (totalRecords > 0)
                {
                    lblPagerInfo.Text = string.Format("Showing {0}-{1} of {2} complaints", startRecord, endRecord, totalRecords);
                }
                else
                {
                    lblPagerInfo.Text = "No complaints found";
                }
                lblPagerInfo.CssClass = "pager-info";
                
                // Add the label to the first cell of the pager row (before existing controls)
                pagerRow.Cells[0].Controls.Add(new LiteralControl("<div class='pager-info-wrapper'>"));
                pagerRow.Cells[0].Controls.Add(lblPagerInfo);
                pagerRow.Cells[0].Controls.Add(new LiteralControl("</div>"));
            }
        }

        protected void gvComplaints_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "EditComplaint")
                {
                    if (e.CommandArgument != null)
                    {
                        int id = Convert.ToInt32(e.CommandArgument);
                        Response.Redirect("AdminPanel.aspx?id=" + id);
                    }
                    else
                    {
                        ShowMessage("Invalid complaint ID for editing.", false);
                    }
                }
                else if (e.CommandName == "DeleteComplaint")
                {
                    if (e.CommandArgument != null)
                    {
                        int id = Convert.ToInt32(e.CommandArgument);
                        if (DatabaseHelper.DeleteComplaint(id))
                        {
                            DatabaseHelper.RenumberSNo();
                            // Reload and rebind
                            BindComplaintsGrid();
                            ShowMessage("Complaint deleted successfully!", true);
                        }
                        else
                        {
                            ShowMessage("Failed to delete complaint.", false);
                        }
                    }
                    else
                    {
                        ShowMessage("Invalid complaint ID for deletion.", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error processing request: " + ex.Message, false);
            }
        }

        protected void btnApplyFilters_Click(object sender, EventArgs e)
        {
            gvComplaints.PageIndex = 0; // Reset to first page when filters are applied
            BindComplaintsGrid();
        }

        protected void btnClearFilters_Click(object sender, EventArgs e)
        {
            txtSearchDescription.Text = "";
            ddlFilterStatus.SelectedIndex = 0;
            ddlFilterAssigned.SelectedIndex = 0;
            txtDateFrom.Text = "";
            txtDateTo.Text = "";
            ddlSortBy.SelectedIndex = 0;
            gvComplaints.PageIndex = 0; // Reset to first page when filters are cleared
            BindComplaintsGrid();
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isSuccess ? "success-message" : "error-message";
            lblMessage.Visible = true;
        }

        private void ToggleFiltersVisibility()
        {
            // If querystring filters=off, hide filters and show full list
            string filtersParam = Request.QueryString["filters"];
            bool hideFilters = !string.IsNullOrEmpty(filtersParam) && filtersParam.Equals("off", StringComparison.OrdinalIgnoreCase);
            pnlFilters.Visible = !hideFilters;
        }

        protected string GetSerialNumber(int dataItemIndex)
        {
            try
            {
                // Use the corrected PageIndex from ViewState if available, otherwise use current PageIndex
                int pageIndex = ViewState["CorrectedPageIndex"] != null 
                    ? (int)ViewState["CorrectedPageIndex"] 
                    : gvComplaints.PageIndex;
                int pageSize = gvComplaints.PageSize;
                
                // Get total complaints count to validate page index
                int totalComplaints = ViewState["TotalComplaints"] != null ? (int)ViewState["TotalComplaints"] : 0;
                
                // Calculate maximum valid page index
                int maxPageIndex = totalComplaints > 0 ? (totalComplaints - 1) / pageSize : 0;
                
                // Ensure pageIndex is valid (not negative and not beyond max)
                if (pageIndex < 0) pageIndex = 0;
                if (pageIndex > maxPageIndex) pageIndex = maxPageIndex;
                
                // Calculate serial number: (page number - 1) * items per page + item position on page + 1
                // For page 1 (index 0): (0 * 10) + 0 + 1 = 1
                // For page 2 (index 1): (1 * 10) + 0 + 1 = 11
                int serialNumber = (pageIndex * pageSize) + dataItemIndex + 1;
                
                return serialNumber.ToString();
            }
            catch
            {
                // Fallback to simple calculation
                return (dataItemIndex + 1).ToString();
            }
        }
    }
}


