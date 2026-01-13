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


## Fundo - Loan Management System (Extended Edition)
**Por Sebastián Rodríguez**

Este proyecto es una implementación profesional de un sistema de gestión de préstamos, basada en el codebase original de **Fundo**. El objetivo es extender la arquitectura base para soportar entornos de alta concurrencia y mejorar la observabilidad mediante telemetría moderna.

## Decisiones de Arquitectura y Diseño

### 1. Desacoplamiento de la Capa de Datos (DI & IoC)
Se implementó **Inyección de Dependencias** en la infraestructura de persistencia.
- **Implementación:** El sistema depende de la interfaz `IAppDbContext` en lugar de la clase concreta `AppDbContext`.
- **Beneficio:** Facilita la creación de **Pruebas Unitarias** (Mocking) y el cambio de motor de base de datos sin afectar la lógica de negocio.

### 2. Encapsulamiento y Seguridad (DTOs)
Uso de **Records de C# 12** para la transferencia de datos (`LoanDTO`).
- **Propósito:** Evitar la exposición de detalles internos (como el `Timestamp` de concurrencia) en los contratos de la API.
- **Ventaja:** Garantiza la inmutabilidad de los datos y un código más limpio.

### 3. Control de Concurrencia Optimista
Para asegurar la integridad de los saldos, se implementó un mecanismo de **Concurrency Tokens**.
- **Técnica:** Propiedad `RowVersion` gestionada por SQL Server para evitar bloqueos pesados (Pessimistic Locking).
- **Simulación:** Se integró un sistema de flags en `appsettings.json` para simular latencia de red y validar colisiones de datos:
  ```json
  "FeatureManagement": {
    "EnableConcurrencyTestDelay": true,
    "ConcurrencyDelayMs": 2000
  }

  
- **Justificación:** Esto permite simular latencia de red de forma controlada, facilitando la prueba de colisiones de datos y validando que el mecanismo de **Concurrencia Optimista** (RowVersion) responda correctamente.

### 4 Robustez y Lógica de Negocio

#### Validación de Precondiciones

El sistema no solo procesa datos, sino que actúa como guardián de las reglas de negocio, validando precondiciones críticas antes de cualquier persistencia:

- **Al crear un Préstamo (Create Loan):**
  - Validación de montos mínimos y máximos.
  - Verificación de integridad del cliente.
- **Al realizar un Pago (Make Payment):**
  - Validación de existencia del préstamo.
  - Comprobación de que el monto del pago no exceda el balance restante.
  - Verificación de que el préstamo no esté ya cancelado.

### 5 Diseño de API RESTful y Códigos de Estado

Se implementaron respuestas HTTP semánticas para una integración clara con el Frontend:

| **Escenario** | **HTTP Code** | **Razón** |
| --- | --- | --- |
| **Operación Exitosa** | 200 OK | El recurso se procesó o retornó correctamente. |
| **Recurso Creado** | 201 Created | Se confirma la creación de un nuevo préstamo. |
| **Error de Validación** | 400 Bad Request | Fallo en precondiciones (ej: pago mayor al balance). |
| **Conflicto de Datos** | 409 Conflict | Se detectó una colisión de concurrencia (RowVersion mismatch). |
| **No Encontrado** | 404 Not Found | El ID del préstamo solicitado no existe. |

## Estrategia de Pruebas (xUnit & Integration Tests)

Se desarrolló una suite completa de pruebas de integración que validan el flujo de extremo a extremo:

- **Integridad de Datos:** Pruebas que aseguran que el ID devuelto tras una creación sea el generado por la base de datos (Identity).
- **Validación Masiva (Theory):** Uso de InlineData para probar sistemáticamente todas las guardas de validación de la API.
- **Prueba de Concurrencia Crítica:** Un test que dispara peticiones simultáneas para verificar que el sistema maneje correctamente la excepción DbUpdateConcurrencyException, permitiendo una transacción y rechazando la otra para evitar inconsistencias de saldo.

Para asegurar la robustez de la plataforma **Fundo**, se implementó una suite de pruebas de integración utilizando xUnit y WebApplicationFactory. A diferencia de las pruebas unitarias tradicionales, estas validan el comportamiento real del sistema, incluyendo la persistencia en base de datos y los contratos HTTP.

### 1\. Validación de Contratos y Seguridad de Datos

Se implementaron pruebas para garantizar que la API se comporte de forma **RESTful** y proteja la información interna:

- **Encapsulamiento de DTOs:** Mediante el test GetBalances_ShouldReturnLoanDtoWithoutInternalFields, se verifica que el sistema utilice correctamente los Records de C#, asegurando que campos técnicos como rowVersion no se expongan al cliente, manteniendo la pureza del modelo de vista.
- **Disponibilidad del Servicio:** Se mantiene una prueba base (GetBalances_ShouldReturnExpectedResult) para asegurar que el endpoint principal responda correctamente bajo condiciones normales.

### 2\. Blindaje de Reglas de Negocio (Precondiciones)

La creación y gestión de préstamos cuenta con un sistema de validación riguroso:

- **Pruebas Exhaustivas de Creación:** Utilizando el atributo \[Theory\], se automatizó la validación de múltiples escenarios de error (ID inválido, montos negativos, balances inconsistentes o nombres vacíos). Esto garantiza que ninguna solicitud malformada llegue a la capa de persistencia.
- **Control de Saldos:** El test MakePayment_AmountExceedsBalance valida que no se permitan pagos superiores al balance actual, devolviendo códigos de error semánticos (400 Bad Request) que facilitan la integración con el frontend.

### 3\. Verificación de Concurrencia y Consistencia (Prueba Crítica)

El componente más avanzado de la suite es MakePayment_SimultaneousUpdates_ShouldReturnConflict. Esta prueba simula un entorno de alta demanda:

- **Escenario:** Crea un recurso dinámicamente y dispara dos peticiones de pago simultáneas aprovechando el ConcurrencyDelayMs configurado en el sistema.
- **Resultado:** Valida que el mecanismo de **Concurrencia Optimista** funcione, permitiendo una transacción exitosa y rechazando la otra con un código 409 Conflict. Esto asegura que nunca se produzca una pérdida de datos o un balance inconsistente debido a condiciones de carrera (Race Conditions).

## Observabilidad Estructurada (Serilog + Seq)

Se reemplazó el logging por defecto por un pipeline de **Logging Estructurado**.

- **Visualización:** Integración con **Seq** para centralizar y analizar logs. En la siguiente imagen se observa el rastreo de una transacción exitosa y el detalle de las consultas SQL:


## Consideraciones de Seguridad

- **Validación Robusta:** Se implementaron validaciones de entrada en la capa de aplicación para asegurar la integridad de los datos antes de llegar a la persistencia.
- **Protección de Contratos (DTOs):** Se utilizan Records inmutables para evitar ataques de sobre-asignación de datos y ocultar la estructura interna de la base de datos.
- **Manejo de Excepciones:** Se configuró un manejo global de errores para evitar la fuga de trazas de pila (stack traces) sensibles en las respuestas HTTP.
- **Próximos Pasos (Seguridad):** Para un entorno de Producción, se contempla la integración de **ASP.NET Core Identity con JWT** para asegurar que solo usuarios autenticados con el rol Admin puedan procesar pagos.

> **Nota sobre Autenticación:** En esta versión se priorizó la lógica de negocio y la consistencia de datos. En un entorno productivo, se integraría **ASP.NET Core Identity con JWT (JSON Web Tokens)** para implementar autenticación y autorización basada en roles (RBAC).

## Stack Tecnológico

- **Core:** .NET 8.0 (Web API)
- **ORM:** Entity Framework Core (SQL Server)
- **Database:** SQL Server 2022 (Dockerized)
- **Logging:** Serilog & Seq (Dockerized)
- **Tools:** Docker Compose, Swagger/OpenAPI
- **Patrones:** Repository Pattern, DTOs con Records, Inyección de Dependencias.
- **Infraestructura:** Docker & Docker Compose.
- **Calidad:** xUnit, Fluent Assertions.

## Ejecución Rápida

- **Levantar Infraestructura:** docker-compose up -d
- **Correr API:** dotnet run --project src/Fundo.Applications.WebApi
- **Explorar:** Acceder a <http://localhost:53296/swagger> 
