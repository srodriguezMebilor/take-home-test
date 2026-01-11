using Fundo.Applications.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Integration
{
    public class LoanManagementControllerTests : IClassFixture<WebApplicationFactory<Fundo.Applications.WebApi.Startup>>
    {
        private readonly HttpClient _client;

        public LoanManagementControllerTests(WebApplicationFactory<Fundo.Applications.WebApi.Startup> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task GetBalances_ShouldReturnExpectedResult()
        {
            var response = await _client.GetAsync("/loan");

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }

        // Este test asegura que el sistema devuelva 400 Bad Request si intentamos pagar más del saldo disponible
        [Fact]
        public async Task MakePayment_AmountExceedsBalance_ShouldReturnBadRequest()
        {
            // Arrange: Preparamos un pago por un monto excesivo
            var payment = new { Amount = 9999999 };

            // Act: Enviamos el POST a un préstamo existente (ej: ID 5)
            var response = await _client.PostAsJsonAsync("/loan/5/payment", payment);

            // Assert: Validamos el código de error semántico
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Este test verifica que la respuesta use el LoanDto y no exponga campos sensibles o internos
        [Fact]
        public async Task GetBalances_ShouldReturnLoanDtoWithoutInternalFields()
        {
            // Act
            var response = await _client.GetAsync("/loan");
            var content = await response.Content.ReadAsStringAsync();

            // Assert: Verificamos que el JSON no contenga el campo "rowVersion" o similar
            Assert.DoesNotContain("rowVersion", content);
            Assert.Contains("applicantName", content);
        }

        // 1. Caso de Éxito en la creación del Test.
        [Fact]
        public async Task CreateLoan_WithValidData_ReturnsCreated()
        {
            // Arrange
            var newLoan = new LoanDto
            {
                Id = 0,
                ApplicantName = "Test User",
                Amount = 1000,
                CurrentBalance = 1000,
                Status = "active"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/loan", newLoan);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
            var createdLoan = await response.Content.ReadFromJsonAsync<LoanDto>();
            Assert.Equal("Test User", createdLoan.ApplicantName);
            Assert.True(createdLoan.Id > 0, "El ID devuelto por la API debería ser mayor a 0 después de persistir en la DB.");
        }



        // 2. Casos de Validación Fallida (Uso de Theory para ser más eficiente)
        [Theory]
        [InlineData(100, 1000, 1000, "User", "active", "The Id of the loan must be zero")]
        [InlineData(0, -50, -50, "User", "active", "The amount must be greater than zero")]
        [InlineData(0, 1000, 500, "User", "active", "The amount must be equals to current balance")]
        [InlineData(0, 1000, 1000, "", "active", "The applicant name can not be empty")]
        [InlineData(0, 1000, 1000, "User", "inactive", "The status must be active")]
        public async Task CreateLoan_InvalidData_ReturnsBadRequestWithCorrectMessage(
            int id, decimal amount, decimal balance, string name, string status, string expectedMessage)
        {
            // Arrange
            var invalidLoan = new LoanDto
            {
                Id = id,
                Amount = amount,
                CurrentBalance = balance,
                ApplicantName = name,
                Status = status
            };

            // Act
            var response = await _client.PostAsJsonAsync("/loan", invalidLoan);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);

            // Validar que el mensaje de error sea el esperado
            var errorResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedMessage, errorResponse);
        }

        // Este test aprovecha el ConcurrencyDelayMs que configuramos en el appsettings.json para disparar dos peticiones casi simultáneas y verificar que una de ellas devuelva 409 Conflict
        [Fact]
        public async Task MakePayment_SimultaneousUpdates_ShouldReturnConflict()
        {
            // 1. ARRANGE: Creamos un préstamo nuevo exclusivamente para este test
            var newLoan = new { ApplicantName = "Concurrency Test", Amount = 1000m, CurrentBalance = 1000m, Status = "active" };
            var createResponse = await _client.PostAsJsonAsync("/loan", newLoan);
            var createdLoan = await createResponse.Content.ReadFromJsonAsync<LoanDto>();

            // Usamos el ID recién creado para asegurar que tiene saldo
            var loanId = createdLoan.Id;
            decimal paymentAmount = 10.00m;

            // 2. ACT: Disparamos los pagos simultáneos
            var task1 = _client.PostAsJsonAsync($"/loan/{loanId}/payment", paymentAmount);
            var task2 = _client.PostAsJsonAsync($"/loan/{loanId}/payment", paymentAmount);

            var responses = await Task.WhenAll(task1, task2);

            // 3. ASSERT: Ahora sí, uno debería ser 200 y el otro 409
            Assert.Contains(responses, r => r.StatusCode == System.Net.HttpStatusCode.OK);
            Assert.Contains(responses, r => r.StatusCode == System.Net.HttpStatusCode.Conflict);
        }
    }
}
