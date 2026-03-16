
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ComplaintManagementSystem
{
    public partial class AdminPanel : System.Web.UI.Page
    {
        private int? EditingComplaintId
        {
            get
            {
                if (ViewState["EditingComplaintId"] != null)
                {
                    return (int)ViewState["EditingComplaintId"];
                }
                return null;
            }
            set
            {
                ViewState["EditingComplaintId"] = value;
            }
        }

        private string CurrentView
        {
            get
            {
                if (ViewState["CurrentView"] == null)
                {
                    return "AddNew";
                }
                return ViewState["CurrentView"].ToString();
            }
            set
            {
                ViewState["CurrentView"] = value;
            }
        }

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

                // Check for view parameter (from Update Complaint button)
                string viewParam = Request.QueryString["view"];
                if (viewParam == "update")
                {
                    CurrentView = "Update";
                    BindUpdateComplaintsGrid();
                    SetView(CurrentView);
                    return;
                }

                // If coming from ComplaintsList for editing
                string idParam = Request.QueryString["id"];
                if (int.TryParse(idParam, out int editId))
                {
                    var complaint = DatabaseHelper.GetComplaintById(editId);
                    if (complaint != null)
                    {
                        EditingComplaintId = editId;
                        // Mark that we came from ComplaintsList (not Update view)
                        ViewState["CameFromUpdateView"] = false;
                        // Populate status options FIRST before loading complaint data
                        PopulateStatusOptions();
                        LoadComplaintForEditing(complaint);
                        CurrentView = "AddNew";
                    }
                }
                else
                {
                    // Adding new complaint - hide edit-only fields
                    SetFormMode(false);

                    // New complaint: prefill auto log date & time for display only
                    DateTime now = DateTime.Now;
                    txtLogDateTimeAdd.Text = now.ToString("yyyy-MM-ddTHH:mm");
                    CurrentView = "AddNew";
                    PopulateStatusOptions();
                }

                BindUpdateComplaintsGrid();
                SetView(CurrentView);
            }
            else
            {
                // On postback, DO NOT repopulate status options here
                // This would clear the user's selection before form submission
                // Only populate if we're not in edit mode (to maintain add mode state)
                if (!EditingComplaintId.HasValue)
                {
                    PopulateStatusOptions();
                }
                // If in edit mode, status options should already be populated
                // and we should NOT clear them to preserve user's selection
                
                SetFormMode(EditingComplaintId.HasValue);
                SetView(CurrentView);
                
                // Refresh the update complaints grid on postback if in Update view
                if (CurrentView == "Update")
                {
                    BindUpdateComplaintsGrid();
                }
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

        protected void btnLogout_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            Response.Redirect("Login.aspx");
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate description category
                if (string.IsNullOrEmpty(ddlProblemCategory.SelectedValue))
                {
                    ShowMessage("Please select Description of Problem.", false);
                    return;
                }

                if (string.IsNullOrEmpty(ddlAssignedTo.SelectedValue))
                {
                    ShowMessage("Please select a Manager.", false);
                    return;
                }

                // Check status based on mode
                bool isEditMode = EditingComplaintId.HasValue;
                string statusValue = "";
                if (isEditMode)
                {
                    // In edit mode, check if dropdown has items and a selection
                    if (ddlStatus.Items.Count == 0)
                    {
                        ShowMessage("ERROR: Status options not loaded. SelectedIndex: " + ddlStatus.SelectedIndex + ", Items Count: " + ddlStatus.Items.Count + ". Please refresh the page.", false);
                        return;
                    }
                    
                    // CRITICAL FIX: Read status directly from Request.Form FIRST
                    // This is the most reliable way - gets the actual value submitted by browser
                    // The dropdown's SelectedValue might be reset to default before we read it
                    
                    // Debug: Log ALL form fields containing "Status" to find the correct field name
                    System.Diagnostics.Debug.WriteLine("=== FORM FIELDS DEBUG ===");
                    System.Diagnostics.Debug.WriteLine("ddlStatus.UniqueID: " + ddlStatus.UniqueID);
                    System.Diagnostics.Debug.WriteLine("ddlStatus.ClientID: " + ddlStatus.ClientID);
                    foreach (string key in Request.Form.AllKeys)
                    {
                        if (key != null && (key.Contains("Status") || key.Contains("ddlStatus") || key.ToLower().Contains("status")))
                        {
                            System.Diagnostics.Debug.WriteLine("Found Status Field: " + key + " = '" + Request.Form[key] + "'");
                        }
                    }
                    
                    string statusFromRequest = Request.Form[ddlStatus.UniqueID];
                    
                    // Try alternative form field names if first attempt fails
                    if (string.IsNullOrEmpty(statusFromRequest))
                    {
                        // Try with master page prefix
                        statusFromRequest = Request.Form["ctl00$" + ddlStatus.UniqueID];
                    }
                    if (string.IsNullOrEmpty(statusFromRequest))
                    {
                        // Try with ClientID
                        statusFromRequest = Request.Form[ddlStatus.ClientID];
                    }
                    if (string.IsNullOrEmpty(statusFromRequest))
                    {
                        // Try simple name
                        statusFromRequest = Request.Form["ddlStatus"];
                    }
                    // Try finding any field with "Status" in the name (but not "Add")
                    if (string.IsNullOrEmpty(statusFromRequest))
                    {
                        foreach (string key in Request.Form.AllKeys)
                        {
                            if (key != null && key.Contains("Status") && !key.Contains("Add"))
                            {
                                statusFromRequest = Request.Form[key];
                                System.Diagnostics.Debug.WriteLine("Found status in field: " + key + " = '" + statusFromRequest + "'");
                                break;
                            }
                        }
                    }
                    
                    // Debug: Log dropdown state
                    System.Diagnostics.Debug.WriteLine("=== STATUS DROPDOWN DEBUG ===");
                    System.Diagnostics.Debug.WriteLine("SelectedIndex: " + ddlStatus.SelectedIndex);
                    System.Diagnostics.Debug.WriteLine("Items Count: " + ddlStatus.Items.Count);
                    System.Diagnostics.Debug.WriteLine("SelectedValue: '" + ddlStatus.SelectedValue + "'");
                    System.Diagnostics.Debug.WriteLine("SelectedItem: " + (ddlStatus.SelectedItem != null ? ddlStatus.SelectedItem.Text : "NULL"));
                    System.Diagnostics.Debug.WriteLine("Request.Form[" + ddlStatus.UniqueID + "]: '" + (Request.Form[ddlStatus.UniqueID] ?? "NULL") + "'");
                    
                    if (!string.IsNullOrEmpty(statusFromRequest))
                    {
                        statusValue = statusFromRequest.Trim();
                        System.Diagnostics.Debug.WriteLine("✓ SUCCESS: Using Request.Form value: '" + statusValue + "'");
                    }
                    // Method 1: SelectedValue (fallback)
                    else if (!string.IsNullOrEmpty(ddlStatus.SelectedValue))
                    {
                        statusValue = ddlStatus.SelectedValue.Trim();
                        System.Diagnostics.Debug.WriteLine("Using SelectedValue: '" + statusValue + "'");
                    }
                    // Method 2: SelectedItem.Value
                    else if (ddlStatus.SelectedItem != null && !string.IsNullOrEmpty(ddlStatus.SelectedItem.Value))
                    {
                        statusValue = ddlStatus.SelectedItem.Value.Trim();
                        System.Diagnostics.Debug.WriteLine("Using SelectedItem.Value: '" + statusValue + "'");
                    }
                    // Method 3: SelectedItem.Text
                    else if (ddlStatus.SelectedItem != null && !string.IsNullOrEmpty(ddlStatus.SelectedItem.Text))
                    {
                        statusValue = ddlStatus.SelectedItem.Text.Trim();
                        System.Diagnostics.Debug.WriteLine("Using SelectedItem.Text: '" + statusValue + "'");
                    }
                    // Method 4: Use SelectedIndex
                    else if (ddlStatus.SelectedIndex >= 0 && ddlStatus.SelectedIndex < ddlStatus.Items.Count)
                    {
                        ListItem item = ddlStatus.Items[ddlStatus.SelectedIndex];
                        statusValue = !string.IsNullOrEmpty(item.Value) ? item.Value.Trim() : item.Text.Trim();
                        System.Diagnostics.Debug.WriteLine("Using SelectedIndex: '" + statusValue + "'");
                    }
                    else
                    {
                        string errorMsg = "ERROR: Please select Status. SelectedIndex: " + ddlStatus.SelectedIndex + ", Items Count: " + ddlStatus.Items.Count;
                        System.Diagnostics.Debug.WriteLine(errorMsg);
                        ShowMessage(errorMsg, false);
                        return;
                    }
                    
                    // Verify status value is valid
                    if (statusValue != "Pending" && statusValue != "In Progress" && statusValue != "Closed")
                    {
                        string errorMsg = "ERROR: Invalid status value: '" + statusValue + "'. Please select a valid status (Pending, In Progress, or Closed).";
                        System.Diagnostics.Debug.WriteLine(errorMsg);
                        ShowMessage(errorMsg, false);
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine("Final Status Value: '" + statusValue + "'");
                }
                else
                {
                    if (string.IsNullOrEmpty(ddlStatusAdd.SelectedValue))
                    {
                        ShowMessage("Please select Status.", false);
                        return;
                    }
                    statusValue = ddlStatusAdd.SelectedValue.Trim();
                }

                // Determine log date and time (merged into single DateTime)
                DateTime logDateTime;

                if (isEditMode)
                {
                    // In edit mode, allow manual adjustment using the control value
                    if (string.IsNullOrEmpty(txtLogDateTime.Text))
                    {
                        ShowMessage("Please select Log Date & Call Received Time.", false);
                        return;
                    }

                    if (!DateTime.TryParse(txtLogDateTime.Text, out logDateTime))
                    {
                        ShowMessage("Invalid Log Date & Call Received Time.", false);
                        return;
                    }
                }
                else
                {
                    // New complaint: set automatically to current server time
                    logDateTime = DateTime.Now;
                }

                DateTime? resolutionDate = null;
                if (isEditMode && !string.IsNullOrEmpty(txtResolutionDate.Text))
                {
                    DateTime tempDate;
                    if (DateTime.TryParse(txtResolutionDate.Text, out tempDate))
                    {
                        resolutionDate = tempDate;
                    }
                }

                // Create complaint object
                string descriptionCombined = ddlProblemCategory.SelectedItem.Text;
                if (!string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    descriptionCombined += " - " + txtDescription.Text.Trim();
                }

                // Ensure statusValue is not empty before creating complaint object
                if (string.IsNullOrEmpty(statusValue))
                {
                    ShowMessage("ERROR: Status value is empty. Cannot proceed with update.", false);
                    return;
                }
                
                var complaint = new Complaint
                {
                    Description = descriptionCombined,
                    UserDetails = txtUserDetails.Text.Trim(),
                    LogDate = logDateTime, // Now contains both date and time
                    AssignedTo = ddlAssignedTo.SelectedValue,
                    ResolutionDateTime = resolutionDate,
                    ActionTaken = isEditMode ? txtActionTaken.Text.Trim() : "",
                    Status = statusValue // Use the validated status value
                };
                
                // Double-check the status is set correctly
                System.Diagnostics.Debug.WriteLine("Complaint object created with Status: '" + complaint.Status + "'");
                System.Diagnostics.Debug.WriteLine("statusValue variable: '" + statusValue + "'");

                // Insert or Update
                if (EditingComplaintId.HasValue)
                {
                    // Update existing complaint
                    complaint.Id = EditingComplaintId.Value;
                    var existingComplaint = DatabaseHelper.GetComplaintById(EditingComplaintId.Value);
                    if (existingComplaint != null)
                    {
                        complaint.SNo = existingComplaint.SNo;
                    }
                    
                    // Verify the complaint object has the correct status before updating
                    if (string.IsNullOrEmpty(complaint.Status))
                    {
                        ShowMessage("Error: Status value is empty. Cannot update complaint.", false);
                        return;
                    }
                    
                    // Show what we're about to save (for user feedback)
                    string statusToSave = complaint.Status.Trim();
                    
                    // Log what we're trying to save
                    System.Diagnostics.Debug.WriteLine("=== UPDATING COMPLAINT ===");
                    System.Diagnostics.Debug.WriteLine("Complaint ID: " + complaint.Id);
                    System.Diagnostics.Debug.WriteLine("Status to save: '" + statusToSave + "'");
                    System.Diagnostics.Debug.WriteLine("Full complaint Status property: '" + complaint.Status + "'");
                    
                    // Update the complaint in database
                    bool updateSuccess = DatabaseHelper.UpdateComplaint(complaint);
                    
                    if (updateSuccess)
                    {
                        // Verify the update by reading back from database
                        var updatedComplaint = DatabaseHelper.GetComplaintById(complaint.Id);
                        string actualStatus = updatedComplaint != null ? updatedComplaint.Status : "Unknown";
                        
                        // Clear editing state immediately
                        EditingComplaintId = null;
                        
                        // Check if we came from Update Complaint view - if so, stay on AdminPanel and refresh the grid
                        bool cameFromUpdateView = ViewState["CameFromUpdateView"] != null && (bool)ViewState["CameFromUpdateView"];
                        
                        if (cameFromUpdateView)
                        {
                            // Stay on AdminPanel, switch to Update view, refresh grid, and show success message
                            CurrentView = "Update";
                            SetView(CurrentView);
                            // Force clear cache and reload
                            ViewState.Remove("TotalComplaints");
                            ViewState.Remove("CorrectedPageIndex");
                            BindUpdateComplaintsGrid();
                            ShowMessage("Complaint updated successfully! Status changed to: " + statusToSave + " (Database shows: " + actualStatus + ")", true);
                            // Clear the flag
                            ViewState["CameFromUpdateView"] = null;
                        }
                        else
                        {
                            // Came from ComplaintsList - redirect back with success message
                            string redirectUrl = "ComplaintsList.aspx?updated=1&id=" + complaint.Id + "&status=" + Server.UrlEncode(statusToSave) + "&t=" + DateTime.Now.Ticks;
                            Response.Redirect(redirectUrl, true); // endResponse=true to stop execution
                            return;
                        }
                    }
                    else
                    {
                        ShowMessage("ERROR: Failed to update complaint in database. Please check database connection and try again.", false);
                        return;
                    }
                }
                else
                {
                    // Insert new complaint
                    int newId = DatabaseHelper.InsertComplaint(complaint);
                    if (newId > 0)
                    {
                        ShowMessage("Complaint added successfully! ID: " + newId, true);
                    }
                    else
                    {
                        ShowMessage("Failed to add complaint.", false);
                        return;
                    }
                }

                // After add/update, go to complaints list page with cache-busting parameter
                Response.Redirect("ComplaintsList.aspx?refresh=" + DateTime.Now.Ticks);
            }
            catch (Exception ex)
            {
                ShowMessage("An error occurred: " + ex.Message, false);
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            ddlProblemCategory.SelectedIndex = 0;
            txtDescription.Text = "";
            txtUserDetails.Text = "";
            txtLogDateTime.Text = "";
            txtLogDateTimeAdd.Text = "";
            txtResolutionDate.Text = "";
            txtActionTaken.Text = "";
            ddlAssignedTo.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            lblMessage.Visible = false;
            EditingComplaintId = null;
            SetFormMode(false);
            
            // Reset log date/time to current
            DateTime now = DateTime.Now;
            txtLogDateTimeAdd.Text = now.ToString("yyyy-MM-ddTHH:mm");
            
            // Reset status dropdown for add mode - set to Pending
            PopulateStatusOptions();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("ComplaintsList.aspx");
        }

        private void LoadComplaintForEditing(Complaint complaint)
        {
            EditingComplaintId = complaint.Id;

            // Try to split stored description into category + optional details
            string[] knownCategories = new[]
            {
                "PC/Laptop Software Operations",
                "PC/Laptop Connectivity",
                "WIFI / Network",
                "Printer / Scanner",
                "ERP",
                "Others"
            };

            string selectedCategory = "";
            string details = complaint.Description ?? string.Empty;

            foreach (var cat in knownCategories)
            {
                if (!string.IsNullOrEmpty(complaint.Description) &&
                    complaint.Description.StartsWith(cat, StringComparison.OrdinalIgnoreCase))
                {
                    selectedCategory = cat;
                    if (complaint.Description.Length > cat.Length + 3 &&
                        complaint.Description.Substring(cat.Length, 3) == " - ")
                    {
                        details = complaint.Description.Substring(cat.Length + 3);
                    }
                    else
                    {
                        details = "";
                    }
                    break;
                }
            }

            if (!string.IsNullOrEmpty(selectedCategory))
            {
                var item = ddlProblemCategory.Items.FindByText(selectedCategory);
                if (item != null)
                {
                    ddlProblemCategory.ClearSelection();
                    item.Selected = true;
                }
            }

            txtDescription.Text = details;
            txtUserDetails.Text = complaint.UserDetails;

            // LogDate now contains both date and time
            txtLogDateTime.Text = complaint.LogDate.ToString("yyyy-MM-ddTHH:mm");

            ddlAssignedTo.SelectedValue = complaint.AssignedTo;
            
            // Set status - use FindByValue to handle case sensitivity and ensure it exists
            // First ensure status dropdown is populated
            PopulateStatusOptions();
            
            if (!string.IsNullOrEmpty(complaint.Status))
            {
                // Trim and normalize the status value
                string statusToMatch = complaint.Status.Trim();
                
                var statusItem = ddlStatus.Items.FindByValue(statusToMatch);
                if (statusItem != null)
                {
                    ddlStatus.ClearSelection();
                    statusItem.Selected = true;
                }
                else
                {
                    // If exact match not found, try case-insensitive match by text
                    statusItem = ddlStatus.Items.FindByText(statusToMatch);
                    if (statusItem != null)
                    {
                        ddlStatus.ClearSelection();
                        statusItem.Selected = true;
                    }
                    else
                    {
                        // Try case-insensitive value match
                        foreach (ListItem item in ddlStatus.Items)
                        {
                            if (string.Equals(item.Value, statusToMatch, StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(item.Text, statusToMatch, StringComparison.OrdinalIgnoreCase))
                            {
                                ddlStatus.ClearSelection();
                                item.Selected = true;
                                break;
                            }
                        }
                        
                        // If still no match, default to first item
                        if (ddlStatus.SelectedIndex < 0)
                        {
                            ddlStatus.SelectedIndex = 0;
                        }
                    }
                }
            }
            else
            {
                ddlStatus.SelectedIndex = 0;
            }
            
            txtActionTaken.Text = complaint.ActionTaken ?? "";
            if (complaint.ResolutionDateTime.HasValue)
            {
                txtResolutionDate.Text = complaint.ResolutionDateTime.Value.ToString("yyyy-MM-ddTHH:mm");
            }

            SetFormMode(true);
            
            // Double-check all edit mode fields are visible
            statusField.Style["display"] = "block";
            statusField.Visible = true;
            logDateField.Style["display"] = "block";
            logDateField.Visible = true;
            
            // Ensure Resolution Date and Action Taken fields are visible
            var resolutionDateFieldControl = row1Fields.FindControl("resolutionDateField") as System.Web.UI.HtmlControls.HtmlGenericControl;
            var actionTakenFieldControl = row1Fields.FindControl("actionTakenField") as System.Web.UI.HtmlControls.HtmlGenericControl;
            if (resolutionDateFieldControl != null)
            {
                resolutionDateFieldControl.Style["display"] = "block";
                resolutionDateFieldControl.Visible = true;
            }
            if (actionTakenFieldControl != null)
            {
                actionTakenFieldControl.Style["display"] = "block";
                actionTakenFieldControl.Visible = true;
            }
            
            // Ensure row is set to 7 columns for all fields
            row1Fields.Attributes["class"] = "form-row-seven";
        }

        private void SetFormMode(bool isEditMode)
        {
            // Show/hide edit-only fields
            pnlEditFields.Visible = isEditMode;
            row2Fields.Visible = false; // Always hidden - not used anymore
            row2Fields.Style["display"] = "none"; // Ensure it's hidden
            addModeActions.Visible = !isEditMode;

            // Update form title and enable/disable status dropdown
            if (isEditMode)
            {
                lblFormTitle.Text = "Edit Complaint";
                btnSubmit.Text = "Update Complaint";
                btnCancel.Visible = true;
                ddlStatus.Enabled = true; // Enable status dropdown in edit mode
                
                // Change row 1 to 7 columns layout for edit mode (all fields side by side)
                row1Fields.Attributes["class"] = "form-row-seven";
                // Show all edit mode fields in row 1
                logDateField.Style["display"] = "block";
                logDateField.Visible = true;
                statusField.Style["display"] = "block";
                statusField.Visible = true;
                
                // Show Resolution Date and Action Taken fields
                var resolutionDateFieldControl = row1Fields.FindControl("resolutionDateField") as System.Web.UI.HtmlControls.HtmlGenericControl;
                var actionTakenFieldControl = row1Fields.FindControl("actionTakenField") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (resolutionDateFieldControl != null)
                {
                    resolutionDateFieldControl.Style["display"] = "block";
                    resolutionDateFieldControl.Visible = true;
                }
                if (actionTakenFieldControl != null)
                {
                    actionTakenFieldControl.Style["display"] = "block";
                    actionTakenFieldControl.Visible = true;
                }
                // Hide add mode fields
                var logDateFieldAddControl = row1Fields.FindControl("logDateFieldAdd") as System.Web.UI.HtmlControls.HtmlGenericControl;
                var statusFieldAddControl = row1Fields.FindControl("statusFieldAdd") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (logDateFieldAddControl != null) logDateFieldAddControl.Style["display"] = "none";
                if (statusFieldAddControl != null) statusFieldAddControl.Style["display"] = "none";
            }
            else
            {
                lblFormTitle.Text = "Add New Complaint";
                btnSubmitAdd.Text = "Add Complaint";
                btnCancel.Visible = false;
                ddlStatusAdd.Enabled = false; // Disable status dropdown in add mode
                
                // Change row 1 to 5 columns layout for add mode (all fields side by side)
                row1Fields.Attributes["class"] = "form-row-five";
                // Show Log Date and Status in row 1 for add mode
                var logDateFieldAddControl = row1Fields.FindControl("logDateFieldAdd") as System.Web.UI.HtmlControls.HtmlGenericControl;
                var statusFieldAddControl = row1Fields.FindControl("statusFieldAdd") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (logDateFieldAddControl != null)
                {
                    logDateFieldAddControl.Style["display"] = "block";
                    logDateFieldAddControl.Visible = true;
                }
                if (statusFieldAddControl != null)
                {
                    statusFieldAddControl.Style["display"] = "block";
                    statusFieldAddControl.Visible = true;
                }
                // Hide edit mode fields
                logDateField.Style["display"] = "none";
                logDateField.Visible = false;
                statusField.Style["display"] = "none";
                statusField.Visible = false;
                // Hide edit mode resolution and action taken fields
                var resolutionDateFieldControl = row1Fields.FindControl("resolutionDateField") as System.Web.UI.HtmlControls.HtmlGenericControl;
                var actionTakenFieldControl = row1Fields.FindControl("actionTakenField") as System.Web.UI.HtmlControls.HtmlGenericControl;
                if (resolutionDateFieldControl != null)
                {
                    resolutionDateFieldControl.Style["display"] = "none";
                    resolutionDateFieldControl.Visible = false;
                }
                if (actionTakenFieldControl != null)
                {
                    actionTakenFieldControl.Style["display"] = "none";
                    actionTakenFieldControl.Visible = false;
                }
                // Hide row 2 in add mode
                row2Fields.Style["display"] = "none";
                row2Fields.Visible = false;
            }
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = isSuccess ? "success-message" : "error-message";
            lblMessage.Visible = true;
        }

        private void PopulateStatusOptions()
        {
            ddlStatus.Items.Clear();
            ddlStatusAdd.Items.Clear();
            
            if (EditingComplaintId.HasValue)
            {
                // Edit mode - show all status options and enable dropdown
                // Ensure both Text and Value are set explicitly
                ddlStatus.Items.Add(new ListItem("Pending", "Pending"));
                ddlStatus.Items.Add(new ListItem("In Progress", "In Progress"));
                ddlStatus.Items.Add(new ListItem("Closed", "Closed"));
                ddlStatus.Enabled = true;
                ddlStatus.Visible = true;
            }
            else
            {
                // Add mode - only Pending, disabled
                ddlStatus.Items.Add(new ListItem("Pending", "Pending"));
                ddlStatus.SelectedIndex = 0;
                ddlStatus.Enabled = false;
                
                // Populate ddlStatusAdd with Pending as default
                ddlStatusAdd.Items.Add(new ListItem("Pending", "Pending"));
                ddlStatusAdd.SelectedIndex = 0;
                ddlStatusAdd.Enabled = false;
            }
        }

        protected void btnUpdateComplaint_Click(object sender, EventArgs e)
        {
            CurrentView = "Update";
            SetView(CurrentView);
            BindUpdateComplaintsGrid();
        }

        protected void btnAddNewComplaint_Click(object sender, EventArgs e)
        {
            CurrentView = "AddNew";
            SetView(CurrentView);
            // Reset form for new complaint
            SetFormMode(false);
            EditingComplaintId = null;
            DateTime now = DateTime.Now;
            txtLogDateTime.Text = now.ToString("yyyy-MM-ddTHH:mm");
            PopulateStatusOptions();
            // Clear form fields
            txtUserDetails.Text = "";
            ddlProblemCategory.SelectedIndex = 0;
            txtDescription.Text = "";
            ddlAssignedTo.SelectedIndex = 0;
            txtResolutionDate.Text = "";
            txtActionTaken.Text = "";
            lblFormTitle.Text = "Add New Complaint";
            btnSubmit.Text = "Add Complaint";
        }

        protected void btnAddComplaintFromUpdate_Click(object sender, EventArgs e)
        {
            // Redirect to same handler as header button
            btnAddNewComplaint_Click(sender, e);
        }

        private void SetView(string view)
        {
            if (view == "Update")
            {
                pnlAddNew.Visible = false;
                pnlUpdateComplaint.Visible = true;
            }
            else
            {
                pnlAddNew.Visible = true;
                pnlUpdateComplaint.Visible = false;
            }
        }

        private void BindUpdateComplaintsGrid()
        {
            try
            {
                // Clear any cached data and load fresh from database
                ViewState.Remove("TotalComplaints");
                ViewState.Remove("CorrectedPageIndex");
                
                // Force fresh data load - clear any potential caching
                gvUpdateComplaints.DataSource = null;
                gvUpdateComplaints.DataBind();
                
                var complaints = DatabaseHelper.GetAllComplaints();
                complaints = complaints.OrderByDescending(c => c.LogDate).ToList();
                
                // Ensure PageIndex is valid for the current data set
                int totalComplaints = complaints.Count;
                int maxPageIndex = totalComplaints > 0 ? (totalComplaints - 1) / gvUpdateComplaints.PageSize : 0;
                if (gvUpdateComplaints.PageIndex < 0) gvUpdateComplaints.PageIndex = 0;
                if (gvUpdateComplaints.PageIndex > maxPageIndex) gvUpdateComplaints.PageIndex = maxPageIndex;
                
                // Store the corrected PageIndex in ViewState to ensure it persists
                ViewState["CorrectedPageIndex"] = gvUpdateComplaints.PageIndex;
                
                gvUpdateComplaints.DataSource = complaints;
                gvUpdateComplaints.DataBind();
            }
            catch (Exception ex)
            {
                ShowMessage("Error loading complaints: " + ex.Message, false);
            }
        }

        protected void gvUpdateComplaints_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // Ensure the new page index is valid
            if (e.NewPageIndex >= 0)
            {
                gvUpdateComplaints.PageIndex = e.NewPageIndex;
            }
            else
            {
                gvUpdateComplaints.PageIndex = 0;
            }
            BindUpdateComplaintsGrid();
        }

        protected void gvUpdateComplaints_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName == "EditComplaint")
                {
                    if (e.CommandArgument != null)
                    {
                        int id = Convert.ToInt32(e.CommandArgument);
                        // Set EditingComplaintId FIRST - this is critical for PopulateStatusOptions
                        EditingComplaintId = id;
                        
                        // Mark that we came from Update view so we know to stay on AdminPanel after update
                        ViewState["CameFromUpdateView"] = true;
                        
                        // CRITICAL: Populate status options BEFORE loading complaint
                        // This ensures dropdown has all three options when editing
                        PopulateStatusOptions();
                        
                        // Switch to Add New view to show the edit form
                        CurrentView = "AddNew";
                        SetView(CurrentView);
                        
                        var complaint = DatabaseHelper.GetComplaintById(id);
                        if (complaint != null)
                        {
                            LoadComplaintForEditing(complaint);
                        }
                        else
                        {
                            ShowMessage("Error: Complaint not found.", false);
                        }
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
                            BindUpdateComplaintsGrid();
                            ShowMessage("Complaint deleted successfully!", true);
                        }
                        else
                        {
                            ShowMessage("Failed to delete complaint.", false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error processing request: " + ex.Message, false);
            }
        }

        protected string GetSerialNumber(int dataItemIndex)
        {
            try
            {
                // Use the corrected PageIndex from ViewState if available, otherwise use current PageIndex
                int pageIndex = ViewState["CorrectedPageIndex"] != null 
                    ? (int)ViewState["CorrectedPageIndex"] 
                    : gvUpdateComplaints.PageIndex;
                int pageSize = gvUpdateComplaints.PageSize;
                
                // Get total complaints count from the data source to validate page index
                var complaints = DatabaseHelper.GetAllComplaints();
                int totalComplaints = complaints.Count;
                
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
