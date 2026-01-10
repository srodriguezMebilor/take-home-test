## Running the Backend

To build the backend, navigate to the `src` folder and run:  
```sh
dotnet build
```

To run all tests:  
```sh
dotnet test
```

To start the main API:  
```sh
cd Fundo.Applications.WebApi  
dotnet run
```

The following endpoint should return **200 OK**:  
```http
GET -> https://localhost:5001/loan
```

## Notes  

Feel free to modify the code as needed, but try to **respect and extend the current architecture**, as this is intended to be a replica of the Fundo codebase.

oan Management System - Technical Challenge
🚀 Overview
Este proyecto es una solución Full Stack para la gestión de préstamos, desarrollada como parte del proceso de selección para BA Global Talent. La aplicación permite la creación, visualización y gestión de pagos de préstamos, asegurando la integridad de los datos y una experiencia de usuario fluida.

🛠️ Tech Stack & Decisions
Backend: .NET 8.0 (Modernization)
Decisión: Se migró el proyecto original de .NET 6.0 a .NET 8.0 (LTS).

Razón: Aprovechar las mejoras de rendimiento y las últimas características de seguridad de la versión de soporte a largo plazo más reciente, alineándome con los objetivos de modernización del equipo técnico.

Database & Persistence: SQL Server + Entity Framework Core
Enfoque: Se utilizó EF Core con un enfoque Code First.

Automatización: Se implementó una estrategia de auto-migración en el inicio de la aplicación (db.Database.Migrate()) para asegurar que la base de datos esté lista sin intervención manual.

Seed Data: Se incluyeron datos iniciales para facilitar la evaluación inmediata de las funcionalidades.

Infrastructure: Docker & Docker Compose
Contenerización: Se diseñó un entorno basado en Docker Compose para orquestar la API y el motor de SQL Server.

Beneficio: Esto garantiza la portabilidad de la solución, permitiendo que cualquier evaluador levante el entorno completo con un solo comando, eliminando el problema de "funciona en mi máquina".

🧠 Challenges & Solutions
Environment Setup: Ante la ausencia de un servidor SQL local, se optó por una infraestructura efímera en contenedores, lo que aceleró el desarrollo y mejoró la mantenibilidad.

RESTful Compliance: Se diseñaron los endpoints (POST /loans, GET /loans, etc.) siguiendo estrictamente los principios REST y manejando códigos de estado HTTP apropiados para cada caso de negocio.

📈 Future Improvements
Security: Implementar autenticación JWT para proteger los endpoints sensibles.

Testing: Aumentar la cobertura de pruebas unitarias en la lógica de cálculo de balances.

