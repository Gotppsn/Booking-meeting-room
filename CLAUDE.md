# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Meeting Room Booking System built as an ASP.NET Core MVC web application with .NET 9.0. It's a multi-language (English, Thai, Japanese) enterprise application featuring user authentication, role-based access control, and real-time room booking management.

## Common Commands

### Build and Run
- **Build**: `dotnet build`
- **Run (Development)**: `dotnet run --launch-profile http`
  - Access at: http://localhost:5058
- **Run (HTTPS)**: `dotnet run --launch-profile https`
  - Access at: https://localhost:7297
- **Publish**: `dotnet publish -c Release`

### Development
- **Watch Run**: `dotnet watch run` (auto-reloads on file changes)
- **Clean**: `dotnet clean`
- **Restore packages**: `dotnet restore`

### Entity Framework Migrations
- **Add Migration**: `dotnet ef migrations add <MigrationName>`
- **Update Database**: `dotnet ef database update`
- **Remove Last Migration**: `dotnet ef migrations remove`
- **Drop Database**: `dotnet ef database drop`

## Architecture

### Core Technology Stack
- **Backend**: ASP.NET Core MVC (.NET 9.0)
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **UI Framework**: Tailwind CSS (via CDN)
- **Localization**: Resource files (.resx) with multi-language support

### Entity Model Design
The application uses a domain-driven entity model with these core entities:
- **ApplicationUser**: Extends IdentityUser with department association and custom properties
- **Department**: Organizational units for user grouping
- **Room**: Meeting rooms with capacity, equipment, and multi-language names
- **Booking**: Meeting bookings with conflict detection and status tracking

All entities support soft relationships and include audit properties (CreatedAt, UpdatedAt).

### Database Architecture
- Uses `ApplicationDbContext` extending `IdentityDbContext<ApplicationUser>`
- Implements foreign key relationships with appropriate cascade behaviors
- Includes seed data for departments and default admin user
- Database connection uses SQL Server LocalDB by default

### Authentication & Authorization System
- **Three roles**: Admin, Manager, Employee
- **Admin features**: User management, department management, room creation
- **Manager features**: Department-level booking oversight
- **Employee features**: Personal booking management
- Password policy: 6+ chars, mixed case, digit required, no special chars required
- Default admin: `admin@meetingrooms.com` / `Admin123!`

### Multi-Language Implementation
- **Supported cultures**: English (en), Thai (th), Japanese (ja)
- **Resource files**: Located in `Resources/` directory with `.resx` files
- **Culture detection**: Cookie-based persistence with query string and Accept-Language fallbacks
- **Dynamic fonts**: Language-specific font loading (Inter, Noto Sans Thai, Noto Sans JP)
- **Localization scope**: All UI strings, navigation, forms, and error messages

### Service Architecture
- **IEmailService**: Abstracted email notifications for booking lifecycle events
- **Dependency Injection**: All services registered in Program.cs with appropriate lifetimes
- **Email templates**: HTML email generation with responsive design
- Current email implementation logs to console (production implementation needed)

### View and Controller Conventions
- **Dashboard**: Real-time statistics with `DashboardViewModel` aggregating room/booking data
- **Booking conflict detection**: Overlapping time slot validation in `BookingsController`
- **Room availability**: Dynamic availability checking with calendar integration
- **Responsive design**: Mobile-first with Tailwind CSS utilities

### Configuration Files
- **appsettings.json**: Production configuration with SQL Server connection string
- **appsettings.Development.json**: Development-specific overrides
- **launchSettings.json**: Development server profiles (HTTP/HTTPS)

### Key Business Logic
- **Booking conflicts**: Automated detection of overlapping reservations
- **Room capacity validation**: Attendee count vs room capacity checking
- **Role-based access**: Controllers decorated with `[Authorize(Roles = "...")]`
- **Email notifications**: Automatic booking confirmation/cancellation/reminder emails
- **Multi-language room names**: Support for localized room names in database

### Development Workflow
When adding new features:
1. **Entities**: Add to `Models/Entities/` and update `ApplicationDbContext`
2. **Controllers**: Create in `Controllers/` with proper authorization attributes
3. **Views**: Add to `Views/{ControllerName}/` with localization injection
4. **Localization**: Update all three resource files (`.resx`, `.th.resx`, `.ja.resx`)
5. **Services**: Register in Program.cs dependency injection
6. **Database**: Create and run EF migrations

### Database Connection
- **Development**: Uses SQL Server LocalDB with database name `MeetingRoomBookingDb`
- **Connection String**: Supports multiple active result sets and trusted connection
- **Automatic setup**: Database and initial data created on first run via Program.cs

### Styling and UI
- **Framework**: Tailwind CSS loaded via CDN with custom configuration
- **Font strategy**: Dynamic loading based on current culture
- **Responsive breakpoints**: Mobile-first design with md/lg breakpoints
- **Color scheme**: Blue-based palette with semantic color usage
- **Icons**: Heroicons SVG icons embedded in markup