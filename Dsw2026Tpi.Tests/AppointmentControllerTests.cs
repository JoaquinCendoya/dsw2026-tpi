using Dsw2026Tpi.Api.Controllers;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Dsw2026Tpi.Tests
{
    public class AppointmentControllerTests
    {
        private readonly IAppointmentService _serviceMock;
        private readonly AppointmentController _controller;

        public AppointmentControllerTests()
        {
            _serviceMock = Substitute.For<IAppointmentService>();
            _controller = new AppointmentController(_serviceMock);
        }

        [Fact]
        public async Task BookAsync_RetornaCreated_CuandoServiceResuelveOk()
        {
            // Arrange
            var request = new AppointmentModel.Request(
                DoctorId: Guid.NewGuid(),
                AvailabilitySlotId: Guid.NewGuid(),
                Patient: new AppointmentModel.PatientDto(Dni: 30123456),
                Reason: "Control anual"
            );

            var expectedResponse = new AppointmentModel.SearchResponse(
                AppointmentsId: Guid.NewGuid(),
                AppointmentsStatus: "Confirmado",
                Patient: new AppointmentModel.PatientSummary(Dni: 30123456, FullName: "Juan Pérez"),
                Doctor: new AppointmentModel.DoctorSummary(
                    DoctorId: Guid.NewGuid(),
                    Name: "Dr. Juan Pérez",
                    Specialty: new AppointmentModel.SpecialtySummary(Guid.NewGuid(), "Cardiología")
                )
            );

            _serviceMock.BookAsync(request).Returns(expectedResponse);

            // Act
            var result = await _controller.BookAsync(request);

            // Assert
            var created = result.Should().BeOfType<ObjectResult>().Subject;
            created.StatusCode.Should().Be(StatusCodes.Status201Created);
            created.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task CancelAsync_LlamaAlServiceConIdCorrecto()
        {
            // Arrange
            var id = Guid.NewGuid();
            var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "42797948")
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await _controller.CancelAsync(id);

            // Assert
            await _serviceMock.Received(1).CancelAsync(id, 42797948L);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByPatientAsync_RetornaOk_ConRespuestaDelService()
        {
            // Arrange
            long dni = 30123456;
            var user = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, dni.ToString())
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var list = new List<AppointmentModel.SearchResponse>
            {
                new(
                    AppointmentsId: Guid.NewGuid(),
                    AppointmentsStatus: "Confirmado",
                    Patient: new AppointmentModel.PatientSummary(Dni: dni, FullName: "Ana Gómez"),
                    Doctor: new AppointmentModel.DoctorSummary(
                        DoctorId: Guid.NewGuid(),
                        Name: "Dra. Ana Gómez",
                        Specialty: new AppointmentModel.SpecialtySummary(Guid.NewGuid(), "Pediatría")
                    )
                )
            };

            var expectedPagination = new Pagination<AppointmentModel.SearchResponse>(
                1,
                10,
                list.Count,
                list
            );

            _serviceMock.GetByPatientDniAsync(Arg.Is<AppointmentModel.PatientDto>(p => p!.Dni == dni), 10, 1)
                .Returns(Task.FromResult(expectedPagination));

            // Act
            var result = await _controller.GetByPatientAsync(10, 1);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(expectedPagination);
        }

        [Fact]
        public async Task GetByDateAsync_RetornaOk_ConPaginacionCorrecta()
        {
            // Arrange
            var request = new AppointmentModel.DailyRequest(Date: new DateOnly(2026, 8, 10), PageSize: 10, PageIndex: 1);

            var data = new List<AppointmentModel.SearchResponse>
    {
        new(
            AppointmentsId: Guid.NewGuid(),
            AppointmentsStatus: "Confirmado",
            Patient: new AppointmentModel.PatientSummary(Dni: 30123456, FullName: "Carlos Ruiz"),
            Doctor: new AppointmentModel.DoctorSummary(
                DoctorId: Guid.NewGuid(),
                Name: "Dr. Carlos Ruiz",
                Specialty: new AppointmentModel.SpecialtySummary(Guid.NewGuid(), "Traumatología")
            )
        )
    };

            var expected = new Pagination<AppointmentModel.SearchResponse>(request.PageSize, request.PageIndex, data.Count, data);

            _serviceMock.GetByDateAsync(request).Returns(expected);

            // Act
            var result = await _controller.GetByDateAsync(request);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task SearchAsync_ConstruyePatientDto_CuandoDniTieneValor()
        {
            // Arrange
            long dni = 30123456;
            var specialtyId = Guid.NewGuid();
            var doctorId = Guid.NewGuid();
            var date = new DateOnly(2026, 8, 10);
            int pageSize = 10, pageIndex = 1;

            // Envía los 10 parámetros requeridos por el constructor del record
            var request = new AppointmentModel.SearchRequest(
                PageSize: pageSize,
                PageIndex: pageIndex,
                SpecialtyId: specialtyId,
                DoctorId: doctorId,
                Dni: dni,
                Date: date,
                StartDate: null,
                EndDate: null,
                Status: null
            );

            // Act
            await _controller.SearchAsync(request);

            // Assert
            await _serviceMock.Received(1).SearchAsync(
                Arg.Is<AppointmentModel.SearchRequest>(r => r == request)
            );
        }

        [Fact]
        public async Task SearchAsync_PatientDtoEsNull_CuandoDniNoTieneValor()
        {
            // Arrange
            long? dni = null;

            // Pasa los 10 parámetros requeridos por el constructor del record en orden
            var request = new AppointmentModel.SearchRequest(
                PageSize: 10,
                PageIndex: 1,
                SpecialtyId: null,
                DoctorId: null,
                Dni: dni,
                Date: null,
                StartDate: null,
                EndDate: null,
                Status: null
            );

            // Act
            await _controller.SearchAsync(request);

            // Assert
            await _serviceMock.Received(1).SearchAsync(
                Arg.Is<AppointmentModel.SearchRequest>(r => r == request)
            );
        }

        [Fact]
        public async Task MarkAttendanceAsync_RetornaOk_CuandoServiceEjecutaCorrectamente()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new AppointmentModel.AttendanceRequest(Attended: true);

            // Act
            var result = await _controller.MarkAttendanceAsync(id, request);

            // Assert
            await _serviceMock.Received(1).MarkAttendanceAsync(id, true);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
