# Loan Management System

A full-stack application designed to manage loan applications and process payments. This project was developed as a technical take-home test, showcasing a robust **.NET Core** backend and a modern **Angular** frontend, fully integrated via **Docker Compose**.

## 🚀 Tech Stack

### Backend
* **.NET 8** (ASP.NET Core Web API)
* **Entity Framework Core** (Code-First Approach)
* **SQL Server** (Database)
* **xUnit** (Unit & Integration Testing)
* **Serilog & Seq** (Structured Logging)

### Frontend
* **Angular 17+** (Standalone Components)
* **Angular Material** (UI Components)
* **RxJS** (Reactive State Management)
* **Nginx** (Production-ready web server for Docker)

### DevOps
* **Docker & Docker Compose** (Full stack orchestration)
* **GitHub Actions** (CI Pipeline for Backend Build & Test)

---

## 🐳 Setup & Running (Docker)

To simplify the review process and ensure consistency across environments, **I have dockerized both the Backend and the Frontend.**

### Prerequisites
* [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running.

### Instructions

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/srodriguezMebilor/take-home-test.git](https://github.com/srodriguezMebilor/take-home-test.git)
    cd take-home-test
    ```

2.  **Run the entire stack:**
    Execute the following command in the root directory:
    ```bash
    docker-compose up --build
    ```

3.  **Wait for initialization:**
    * Wait a moment for SQL Server to initialize.
    * The backend will automatically apply migrations and **seed the database** with initial data.

---

## 🌐 Accessing the Application

Once the containers are running, you can access the services at the following URLs:

| Service | URL | Description |
| :--- | :--- | :--- |
| **Frontend (App)** | **[http://localhost:4200](http://localhost:4200)** | The main Angular application. |
| **Backend (Swagger)** | **[http://localhost:5050/swagger](http://localhost:5050/swagger)** | API Documentation and testing interface. |
| **Seq (Logs)** | **[http://localhost:5341](http://localhost:5341)** | Structured logging dashboard. |

---

## ✨ Features Implemented

### 1. Loan Management (API & UI)
* **Create Loan:** Users can register new loans via a modal form with validations.
* **List Loans:** A responsive data table displays all loans with formatted currency.
* **Loan Details:** Endpoint available to retrieve specific loan data.

### 2. Payment Processing
* **Make Payment:** A dedicated feature to deduct amounts from the `currentBalance`.
* **Smart Validation:** The system prevents payments exceeding the current debt.
* **Auto-Status:** When the balance reaches `0`, the status automatically updates to `Paid`.

### 3. Architecture & DevOps
* **Dockerized Frontend:** Unlike the minimum requirement, the frontend is served via Nginx in a container to ensure it runs perfectly on any machine without needing local Node.js installation.
* **CI Pipeline:** A GitHub Actions workflow (`.github/workflows/backend-ci.yml`) is configured to build the backend and run tests on every push.
* **Seed Data:** The database initializes with sample data for immediate testing.

---

## 🧪 Testing

To run the backend unit and integration tests locally (outside of Docker):

1.  Navigate to the solution folder.
2.  Run the command:
    ```bash
    docker-compose up tests
    ```

---

## 🏗️ Design Decisions

* **Docker for Everything:** I decided to dockerize the Frontend in addition to the Backend to provide a true "cloud-native" experience. This removes "it works on my machine" issues related to Node versions.
* **Repository Pattern:** Used in the API to decouple business logic from EF Core, making the application more testable and maintainable.
* **Angular Standalone Components:** Leveraged the latest Angular features to reduce boilerplate and improve application structure.

---

## 📝 Future Improvements

Given more time, I would implement the following features:

1.  **Authentication (JWT):** Secure the API endpoints to ensure only authorized users can manage loans.
2.  **Pagination:** Implement server-side pagination for the `GET /loans` endpoint to handle large datasets efficiently.
3.  **Advanced Error Handling:** Create a global exception handler middleware to standardize error responses further.
4.  **Frontend Tests:** Add Jasmine/Karma unit tests for the Angular components.

---

**Author:** Sebastian Rodriguez