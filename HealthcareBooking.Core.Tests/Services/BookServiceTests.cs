using Moq;
using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Interfaces;
using HealthcareBooking.Core.Repositories;
using HealthcareBooking.Core.Services;

namespace HealthcareBooking.Core.Tests.Services;

public class BookServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly Mock<IClinicRepository> _clinicRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IJobScheduler> _jobSchedulerMock = new();
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _bookService = new BookService(
            _appointmentRepositoryMock.Object,
            _clinicRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _jobSchedulerMock.Object);
    }

    #region BookAppointmentAsync

    // 測試當診所不存在時，預約服務應該拋出 KeyNotFoundException
    [Fact]
    public async Task BookAppointmentAsync_ClinicNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _clinicRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Clinic?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _bookService.BookAppointmentAsync(1, 99));
    }

    // 測試當診所已滿額時，預約服務應該拋出 InvalidOperationException
    [Fact]
    public async Task BookAppointmentAsync_ClinicFull_ThrowsInvalidOperationException()
    {
        // Arrange
        var clinic = new Clinic(1, DateTime.UtcNow, 1);
        clinic.AddBooking(); // 使名額滿載（CurrentBooked == MaxQuota）

        _clinicRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(clinic);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _bookService.BookAppointmentAsync(1, 1));
    }

    // 測試當預約成功時，服務應該回傳新預約的 ID
    [Fact]
    public async Task BookAppointmentAsync_ValidRequest_ReturnsAppointmentId()
    {
        // Arrange
        const int expectedId = 42;
        SetupSuccessfulBooking(assignedId: expectedId);

        // Act
        var result = await _bookService.BookAppointmentAsync(1, 1);

        // Assert
        Assert.Equal(expectedId, result);
    }

    // 測試當預約成功時，診所的已預約數量應該增加 1
    [Fact]
    public async Task BookAppointmentAsync_ValidRequest_IncreasesClinicBookingCount()
    {
        // Arrange
        var clinic = SetupSuccessfulBooking();
        var initialBooked = clinic.CurrentBooked;

        // Act
        await _bookService.BookAppointmentAsync(1, 1);

        // Assert
        Assert.Equal(initialBooked + 1, clinic.CurrentBooked);
    }

    // 測試當預約成功時，單位工作應該被呼叫以保存變更
    [Fact]
    public async Task BookAppointmentAsync_ValidRequest_SavesChanges()
    {
        // Arrange
        SetupSuccessfulBooking();

        // Act
        await _bookService.BookAppointmentAsync(1, 1);

        // Assert
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // 測試當預約成功時，應該排程發送預約成功通知
    [Fact]
    public async Task BookAppointmentAsync_ValidRequest_EnqueuesBookingNotification()
    {
        // Arrange
        const int patientId = 1;
        const int clinicId = 1;
        SetupSuccessfulBooking(patientId: patientId, clinicId: clinicId);

        // Act
        await _bookService.BookAppointmentAsync(patientId, clinicId);

        // Assert
        _jobSchedulerMock.Verify(j => j.EnqueueBookingNotification(patientId, clinicId), Times.Once); // 驗證排程方法被呼叫一次，且呼叫時使用了正確的參數
    }
    #endregion

    // 建立成功預約情境所需的 Mock 設定，回傳使用的 Clinic 實體以供驗證
    private Clinic SetupSuccessfulBooking(int patientId = 1, int clinicId = 1, int assignedId = 1)
    {
        var clinic = new Clinic(1, DateTime.UtcNow, 5);

        _clinicRepositoryMock
            .Setup(r => r.GetByIdAsync(clinicId))
            .ReturnsAsync(clinic);

        _appointmentRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Appointment>()))
            .Callback<Appointment>(a => a.Id = assignedId)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return clinic;
    }
}
