# HR-Platform-Project

This project is developed as part of the SoftUni ASP .NET Advanced course. It is a web-based HR platform that allows users to interact with job postings and manage team-related operations.

## Features
- **Users** can apply for jobs.
- **Recruiters** can post, review, delete, approve, and deny job applications.
- **Managers** can manage teams, set salaries, create/edit teams, and remove people from teams.
- **Employees** can book holidays, with **managers** able to approve or deny them.
- **Admins** have full access to all features, including the ability to edit and delete user accounts.

## Testing
Seed data and users for testing purposes can be found in the `Data` folder.

## Technologies
- ASP.NET Core
- MS SQL
- SignalR (for real-time features)
- ASP.NET Identity (for user management)

## Setup
1. Clone the repository.
2. Ensure that the `appsettings.json` is configured correctly for your local or production environment.
3. Apply any necessary migrations for the database.
4. Run the application locally or deploy to your desired server.