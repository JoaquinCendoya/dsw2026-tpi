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
                Id: Guid.NewGuid(),
                Specialty: "Cardiología",
                Doctor: "Dr. Juan Pérez",
                AvailableTime: new DateTime(2026, 8, 10, 9, 0, 0),
                Status: "Confirmado"
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

            // Act
            var result = await _controller.CancelAsync(id);

            // Assert
            await _serviceMock.Received(1).CancelAsync(id);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByPatientAsync_RetornaOk_ConRespuestaDelService()
        {
            // Arrange
            long dni = 30123456;
            var expected = new List<AppointmentModel.SearchResponse>
        {
            new(
                Id: Guid.NewGuid(),
                Specialty: "Pediatría",
                Doctor: "Dra. Ana Gómez",
                AvailableTime: new DateTime(2026, 8, 15, 14, 30, 0),
                Status: "Confirmado"
            )
        };

            _serviceMock.GetByPatientDniAsync(Arg.Is<AppointmentModel.PatientDto>(p => p.Dni == dni))
                .Returns(expected);

            // Act
            var result = await _controller.GetByPatientAsync(dni);

            // Assert
            var ok = result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public async Task GetByDateAsync_RetornaOk_ConPaginacionCorrecta()
        {
            // Arrange
            var date = new DateOnly(2026, 8, 10);
            int pageSize = 10;
            int pageIndex = 1;

            var data = new List<AppointmentModel.SearchResponse>
    {
        new(
            Id: Guid.NewGuid(),
            Specialty: "Traumatología",
            Doctor: "Dr. Carlos Ruiz",
            AvailableTime: new DateTime(2026, 8, 10, 11, 0, 0),
            Status: "Confirmado"
        )
    };

            var expected = new Pagination<AppointmentModel.SearchResponse>(pageSize, pageIndex, data.Count, data);

            _serviceMock.GetByDateAsync(date, pageSize, pageIndex).Returns(expected);

            // Act
            var result = await _controller.GetByDateAsync(date, pageSize, pageIndex);

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

            // Act
            await _controller.SearchAsync(pageSize, pageIndex, specialtyId, doctorId, dni, date);

            // Assert
            await _serviceMock.Received(1).SearchAsync(
                pageSize, pageIndex, specialtyId, doctorId,
                Arg.Is<AppointmentModel.PatientDto?>(p => p != null && p.Dni == dni),
                date
            );
        }

        [Fact]
        public async Task SearchAsync_PatientDtoEsNull_CuandoDniNoTieneValor()
        {
            // Arrange
            long? dni = null;

            // Act
            await _controller.SearchAsync(10, 1, null, null, dni, null);

            // Assert
            await _serviceMock.Received(1).SearchAsync(
                10, 1, null, null,
                Arg.Is<AppointmentModel.PatientDto?>(p => p == null),
                null
            );
        }
    }
}
