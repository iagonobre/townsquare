# Townsquare

Townsquare is a simple ASP.NET Core MVC project where users can create events, manage their profile, and RSVP to events created by others.

## Technologies
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- Identity (for authentication)

##
When a user is deleted by an admin, their events are **not removed**.  
Instead, the events become **orphaned** (IsDeleted = true) and remain visible in the system.  
We chose this approach to avoid losing event data and because it fits the requirement from the specification.

## How to Run

1. Clone the repository
   ```
   git clone <repository-url>
   ```

2. Enter the project folder
   ```
   cd Townsquare
   ```

3. Restore dependencies
   ```
   dotnet restore
   ```

4. Apply database migrations
   ```
   dotnet ef database update
   ```

5. Run the project
   ```
   dotnet run
   ```


## Admin Account

An admin user is automatically created:

- Email: **admin@townsquare.com**
- Password: **Admin123!**
