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


# Fundo - Loan Management System (Extended Edition)
**Por Sebastián Rodríguez**

[cite_start]Este proyecto es una implementación profesional de un sistema de gestión de préstamos, basada en el codebase original de **Fundo**[cite: 1, 2]. [cite_start]El objetivo es extender la arquitectura base para soportar entornos de alta concurrencia y mejorar la observabilidad mediante telemetría moderna[cite: 3].

## Decisiones de Arquitectura y Diseño

### 1. Desacoplamiento de la Capa de Datos (DI & IoC)
[cite_start]Se implementó **Inyección de Dependencias** en la infraestructura de persistencia[cite: 7].
- [cite_start]**Implementación:** El sistema depende de la interfaz `IAppDbContext` en lugar de la clase concreta `AppDbContext`[cite: 8].
- [cite_start]**Beneficio:** Facilita la creación de **Pruebas Unitarias** (Mocking) y el cambio de motor de base de datos sin afectar la lógica de negocio[cite: 9].

### 2. Encapsulamiento y Seguridad (DTOs)
[cite_start]Uso de **Records de C# 12** para la transferencia de datos (`LoanDTO`)[cite: 11].
- [cite_start]**Propósito:** Evitar la exposición de detalles internos (como el `Timestamp` de concurrencia) en los contratos de la API[cite: 12].
- [cite_start]**Ventaja:** Garantiza la inmutabilidad de los datos y un código más limpio[cite: 13].

### 3. Control de Concurrencia Optimista
[cite_start]Para asegurar la integridad de los saldos, se implementó un mecanismo de **Concurrency Tokens**[cite: 15].
- [cite_start]**Técnica:** Propiedad `RowVersion` gestionada por SQL Server para evitar bloqueos pesados (Pessimistic Locking)[cite: 16, 17].
- [cite_start]**Simulación:** Se integró un sistema de flags en `appsettings.json` para simular latencia de red y validar colisiones de datos[cite: 18, 24]:
  ```json
  "FeatureManagement": {
    "EnableConcurrencyTestDelay": true,
    "ConcurrencyDelayMs": 2000
  }