---
name: sportio-project
description: Sportio gym management system project details and requirements
type: project
---

## Project Overview
Sportio is a gym management web application that centralizes all gym operations including user registration, subscription management, payment processing, and activity tracking.

**Why:** Modernize manual paper-based gym management system to improve efficiency, reduce errors, and provide better user experience for both staff and students.

**How to apply:** All task assignments and development work should align with these core requirements and business goals.

## Core Features

### 1. User Management
- User registration with unique usernames
- Password requirements: minimum 6 characters, 1 uppercase, 1 special character
- Student profiles with personal data (encrypted)
- Administrative and student user types

### 2. Security System
- Login with 3 attempt limit
- Account unlock via security questions
- Password change with history validation (no reused passwords)
- Reversible encryption for student data (AES-256)
- Irreversible encryption for passwords (SHA256)

### 3. Subscription & Payment Management
- Monthly subscription (abono mensual)
- Remote payment processing
- Payment history tracking
- Expiration date management
- Administrative payment processing

### 4. Permission System
- Singleton pattern for session management
- Composite pattern for permission hierarchy
- Role-based access control (Administrator, Receptionist, Student, Trainer)
- Granular permission control by module

## Technical Stack
- Framework: ASP.NET Web Forms (.NET Framework 4.7.2)
- Database: SQL Server
- CSS: Responsive design using `rem` units only
- Development Environment: Visual Studio 2017+

## Database Schema
Key tables:
- USUARIOS: User accounts with login tracking
- ALUMNOS: Student profiles with encrypted personal data
- PAGOS: Payment records
- HISTORIAL_CONTRASENAS: Password history
- PREGUNTAS_SEGURIDAD: Security questions
- EVENTO: Event logging with severity levels
- FAMILIA, PERMISO, PERFIL: Permission system tables

## Project Phases
1. Database Setup & Infrastructure (3 days)
2. Security & Authentication (5 days)
3. User Management (4 days)
4. Subscription & Payment System (5 days)
5. Permission System (6 days)
6. UI/UX & Frontend (4 days)
7. Testing & Deployment (3 days)

Total estimated duration: 30 days (8 weeks)

## Success Criteria
- All functional requirements implemented and tested
- Security system functioning correctly
- Intuitive and responsive UI
- Performance < 2 seconds response time
- Clean, documented, maintainable code
- All tests passing