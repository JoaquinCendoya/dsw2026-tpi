using Dsw2026Tpi.Application.Models;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using System.Linq.Expressions;

namespace Dsw2026Tpi.Tests
{
    public class AvailabilityServiceTests
    {
        private readonly IUnitOfWork _mockUnitOfWork = Substitute.For<IUnitOfWork>();
        private readonly IHolidayService _mockHolidayService = Substitute.For<IHolidayService>();

        private readonly IRepository<Doctor> _mockDoctorRepo = Substitute.For<IRepository<Doctor>>();
        private readonly IRepository<AvailabilityRule> _mockRuleRepo = Substitute.For<IRepository<AvailabilityRule>>();
        private readonly IRepository<AvailabilitySlot> _mockSlotRepo = Substitute.For<IRepository<AvailabilitySlot>>();

        private static readonly Dictionary<DayOfWeek, string> DiasEnEspanol = new()
        {
            [DayOfWeek.Monday] = "LUNES",
            [DayOfWeek.Tuesday] = "MARTES",
            [DayOfWeek.Wednesday] = "MIERCOLES",
            [DayOfWeek.Thursday] = "JUEVES",
            [DayOfWeek.Friday] = "VIERNES",
            [DayOfWeek.Saturday] = "SABADO",
            [DayOfWeek.Sunday] = "DOMINGO",
        };

        private readonly Guid _testDoctorId = Guid.NewGuid();
        private readonly string _testDiaAtencion = DiasEnEspanol[DateTime.Today.DayOfWeek];
        private readonly TimeSpan _testHoraInicio = new TimeSpan(9, 0, 0);
        private readonly TimeSpan _testHoraFin = new TimeSpan(11, 0, 0);

        private readonly Specialty _testSpecialty = new Specialty("Cardiología", "Descripción válida de prueba");

        private readonly AvailabilityService _service;

        public AvailabilityServiceTests()
        {
            _mockUnitOfWork.Repository<Doctor>().Returns(_mockDoctorRepo);
            _mockUnitOfWork.Repository<AvailabilityRule>().Returns(_mockRuleRepo);
            _mockUnitOfWork.Repository<AvailabilitySlot>().Returns(_mockSlotRepo);

            _mockDoctorRepo.GetByIdAsync(_testDoctorId)
                .Returns(Task.FromResult<Doctor?>(new Doctor("Dr. Test", "MP-123", _testSpecialty, _testDoctorId)));

            _mockRuleRepo.FindAsync(Arg.Any<Expression<Func<AvailabilityRule, bool>>>())
                .Returns(new List<AvailabilityRule>());

            _service = new AvailabilityService(_mockUnitOfWork, _mockHolidayService, NullLogger<AvailabilityService>.Instance);
        }

        [Fact]
        public async Task GenerateMonthlyAvailabilityAsync_CuandoEsDiaHabil_EntoncesSeFraccionaEnBloquesDe30Minutos()
        {
            var request = new AvailabilityModel.Request(
                _testDoctorId,
                new List<AvailabilityModel.DayConfig>
                {
                    new AvailabilityModel.DayConfig(_testDiaAtencion, _testHoraInicio, _testHoraFin)
                }
            );

            _mockHolidayService.IsHolidayAsync(Arg.Any<DateTime>()).Returns(Task.FromResult(false));

            var result = await _service.GenerateMonthlyAvailabilityAsync(request);

            Assert.NotEmpty(result);
            await _mockSlotRepo.Received().AddAsync(
                Arg.Is<AvailabilitySlot>(s => s != null && (s.EndTime - s.StartTime) == TimeSpan.FromMinutes(30))
            );

            await _mockUnitOfWork.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task GenerateMonthlyAvailabilityAsync_CuandoEsFeriado_EntoncesNoSeAgreganTurnos()
        {
            var request = new AvailabilityModel.Request(
                _testDoctorId,
                new List<AvailabilityModel.DayConfig>
                {
                    new AvailabilityModel.DayConfig(_testDiaAtencion, _testHoraInicio, _testHoraFin)
                }
            );

            _mockHolidayService.IsHolidayAsync(Arg.Any<DateTime>()).Returns(Task.FromResult(true));

            var result = await _service.GenerateMonthlyAvailabilityAsync(request);

            Assert.Empty(result);
            await _mockSlotRepo.DidNotReceive().AddAsync(Arg.Any<AvailabilitySlot>());
        }

        [Fact]
        public async Task GenerateMonthlyAvailabilityAsync_CuandoHorarioEsInvertido_EntoncesProduceUnaExcepcion()
        {
            var request = new AvailabilityModel.Request(
                _testDoctorId,
                new List<AvailabilityModel.DayConfig>
                {
                    new AvailabilityModel.DayConfig(_testDiaAtencion, _testHoraFin, _testHoraInicio)
                }
            );

            await Assert.ThrowsAsync<BusinessRuleException>(() =>
                _service.GenerateMonthlyAvailabilityAsync(request)
            );
        }

        [Fact]
        public async Task GenerateMonthlyAvailabilityAsync_AlSobrescribir_ConservaSlotsPasadosYReservados()
        {
            var rule = new AvailabilityRule(
                new Doctor("Dr. Test", "MP-123", _testSpecialty, _testDoctorId),
                DateTime.Today.Month,
                DateTime.Today.Year,
                DateTime.Today.DayOfWeek,
                new TimeOnly(9, 0),
                new TimeOnly(11, 0));
            var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            while (futureDate.DayOfWeek != rule.DayOfWeek)
                futureDate = futureDate.AddDays(1);

            var futureAvailableSlot = new AvailabilitySlot(rule, futureDate, new TimeOnly(9, 0), new TimeOnly(9, 30));
            var bookedSlot = new AvailabilitySlot(rule, futureDate, new TimeOnly(9, 30), new TimeOnly(10, 0));
            bookedSlot.Book();
            var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var pastSlot = new AvailabilitySlot(rule, pastDate, new TimeOnly(9, 0), new TimeOnly(9, 30));

            _mockRuleRepo.FindAsync(Arg.Any<Expression<Func<AvailabilityRule, bool>>>())
                .Returns(new List<AvailabilityRule> { rule });
            _mockSlotRepo.FindAsync(Arg.Any<Expression<Func<AvailabilitySlot, bool>>>())
                .Returns(new List<AvailabilitySlot> { futureAvailableSlot, bookedSlot, pastSlot });
            _mockHolidayService.IsHolidayAsync(Arg.Any<DateTime>()).Returns(false);
            var request = new AvailabilityModel.Request(
                _testDoctorId,
                [new AvailabilityModel.DayConfig(_testDiaAtencion, _testHoraInicio, _testHoraFin)]);

            await _service.GenerateMonthlyAvailabilityAsync(request, overwrite: true);

            _mockSlotRepo.Received(1).Delete(futureAvailableSlot);
            _mockSlotRepo.DidNotReceive().Delete(bookedSlot);
            _mockSlotRepo.DidNotReceive().Delete(pastSlot);
            await _mockRuleRepo.DidNotReceive().AddAsync(Arg.Any<AvailabilityRule>());
            _mockRuleRepo.DidNotReceive().Delete(rule);
        }
    }
}
