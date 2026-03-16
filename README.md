# Complaint Management System - C# .NET Web Forms

A professional complaint management system built with ASP.NET Web Forms for admin to manage complaints.

## Features

- **Login Page**: Secure admin authentication
- **Admin Panel**: Complete complaint management interface with:
  - User Desk No
  - Log Date
  - Call Received Date & Time
  - Resolution Date & Time
  - Assigned To (Dropdown: Kuldeep, Kamal)
  - Status (Dropdown: Pending, In Progress, Closed)
  - Description of Problem
  - Action Taken
  - Image Upload

## Project Structure

```
├── Login.aspx / Login.aspx.cs      - Login page
├── AdminPanel.aspx / AdminPanel.aspx.cs - Admin panel
├── Default.aspx / Default.aspx.cs   - Default redirect page
├── Web.config                       - Configuration file
├── Global.asax                      - Global application file
├── Content/
│   └── Site.css                    - Professional styling
└── Uploads/                         - Image upload directory (created automatically)
```

## Setup Instructions

1. Open the project in Visual Studio
2. Build the solution (Ctrl+Shift+B)
3. Run the project (F5)

## Default Login Credentials

- **Username**: admin
- **Password**: admin123

## Notes

- The project uses Forms Authentication
- Image uploads are saved in the `Uploads` folder
- In production, integrate with a database to store complaint data
- The current implementation shows success messages; database integration is needed for persistence

## Technologies Used

- ASP.NET Web Forms
- C#
- CSS3 (Modern styling with gradients and animations)
- Forms Authentication

