# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Gym-APP is an ASP.NET Web Forms application for gym management, targeting .NET Framework 4.7.2. The project includes a comprehensive permission/access control system, student and trainer management, activity tracking, and event logging.

## Development Environment

- **Framework**: ASP.NET Web Forms (.NET Framework 4.7.2)
- **IDE**: Visual Studio 2017+ (solution uses VS 2017 format)
- **Web Server**: IIS Express (https://localhost:44378/)
- **Database**: SQL Server

## Building and Running

```bash
# Build the solution
msbuild Gym-APP/Gym-APP.sln

# Or using Visual Studio
# Open Gym-APP/Gym-APP.sln and press F5 to run with IIS Express
```

## Database Schema

The database schema is defined in `ScriptCreacion.sql`. Key entities:

- **USUARIOS**: User accounts with login attempt tracking (USUARIO_Intentos) and password history (USUARIO_Contras)
- **Permission System**: Familia (permission groups), Permiso (permissions), Perfiles (roles) with many-to-many relationships
- **Alumnos**: Students linked to user accounts
- **Entrenadores**: Trainers linked to user accounts
- **Actividades**: Gym activities with cost and scheduling
- **Rutinas**: Workout routines linking students, trainers, and activities
- **Evento**: Event logging/audit trail with severity levels (1=Info, 2=Advertencia, 3=Error, 4=Crítico)

All tables include `dvv` and `dvh` columns for data integrity verification.

## Project Structure

```
Gym-APP/
├── Gym-APP.sln          # Solution file
├── Gym-APP/             # Main project directory
│   ├── Gym-APP.csproj   # Project file
│   ├── Web.config       # Application configuration
│   ├── Properties/      # Assembly info
│   └── bin/             # Build output
└── ScriptCreacion.sql   # Database schema
```

## Notes

- The project is currently in early development with minimal source files
- Database uses SQL Server with comprehensive foreign key constraints
- Permission system supports hierarchical permissions (Familia.profundidad BIT)
- Event logging tracks all user actions with severity levels