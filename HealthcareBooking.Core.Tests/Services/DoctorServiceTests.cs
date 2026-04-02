using Moq;
using HealthcareBooking.Core.DTOs;
using HealthcareBooking.Core.Entities;
using HealthcareBooking.Core.Interfaces;
using HealthcareBooking.Core.Repositories;
using HealthcareBooking.Core.Services;

namespace HealthcareBooking.Core.Tests.Services;

public class DoctorServiceTests
{
    private readonly Mock<IDoctorRepository> _doctorRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();
    private readonly DoctorService _doctorService;

    public DoctorServiceTests()
    {
        _doctorService = new DoctorService(
            _doctorRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _cacheServiceMock.Object);
    }

    #region AddDoctorAsync
    
    // 測試當新增醫生時，應該保存醫生並清除所有醫生的快取
    [Fact]
    public async Task AddDoctorAsync_ValidDoctor_SavesAndInvalidatesAllDoctorsCache()
    {
        // Arrange
        var doctor = new Doctor("Dr. Test");
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _doctorService.AddDoctorAsync(doctor);

        // Assert
        Assert.Equal(doctor.Id, result);
        _doctorRepositoryMock.Verify(r => r.AddAsync(doctor), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("doctors:all"), Times.Once);
    }

    #endregion

    #region GetAllDoctorsAsync

    // 測試當快取命中時，應該直接回傳快取的醫生列表，而不查詢資料庫
    [Fact]
    public async Task GetAllDoctorsAsync_CacheHit_ReturnsCachedListWithoutQueryingDb()
    {
        // Arrange
        IReadOnlyList<DoctorDto> cachedList = [new DoctorDto(1, "Dr. A")];
        _cacheServiceMock
            .Setup(c => c.GetAsync<IReadOnlyList<DoctorDto>>("doctors:all"))
            .ReturnsAsync(cachedList);

        // Act
        var result = await _doctorService.GetAllDoctorsAsync();

        // Assert
        Assert.Same(cachedList, result);
        _doctorRepositoryMock.Verify(r => r.GetAllAsync(), Times.Never); // 快取命中，不應該查詢資料庫，所以 GetAllAsync 不應該被呼叫
    }

    // 測試當快取未命中時，應該查詢資料庫並將結果寫入快取
    [Fact]
    public async Task GetAllDoctorsAsync_CacheMiss_QueriesDbAndPopulatesCache()
    {
        // Arrange
        IReadOnlyList<Doctor> doctors = [new Doctor("Dr. B")];
        _cacheServiceMock
            .Setup(c => c.GetAsync<IReadOnlyList<DoctorDto>>("doctors:all"))
            .ReturnsAsync((IReadOnlyList<DoctorDto>?)null);
        _doctorRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(doctors);

        // Act
        var result = await _doctorService.GetAllDoctorsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("Dr. B", result[0].Name);
        _doctorRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        _cacheServiceMock.Verify(
            c => c.SetAsync("doctors:all", It.IsAny<IReadOnlyList<DoctorDto>>(), It.IsAny<TimeSpan>()),
            Times.Once);
    }

    #endregion

    #region GetDoctorByIdAsync

    [Fact]
    public async Task GetDoctorByIdAsync_CacheHitWithValue_ReturnsCachedDtoWithoutQueryingDb()
    {
        // Arrange
        var cachedDto = new DoctorDto(1, "Dr. C");
        _cacheServiceMock
            .Setup(c => c.GetAsync<CacheEntry<DoctorDto>>("doctor:1"))
            .ReturnsAsync(new CacheEntry<DoctorDto>(cachedDto));

        // Act
        var result = await _doctorService.GetDoctorByIdAsync(1);

        // Assert
        Assert.Equal(cachedDto, result);
        _doctorRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetDoctorByIdAsync_NullCacheEntry_ThrowsKeyNotFoundExceptionWithoutQueryingDb()
    {
        // Arrange
        // 空值快取（CacheEntry.Value == null）代表快取記錄「此醫生不存在」，防止快取穿透
        _cacheServiceMock
            .Setup(c => c.GetAsync<CacheEntry<DoctorDto>>("doctor:1"))
            .ReturnsAsync(new CacheEntry<DoctorDto>(null));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _doctorService.GetDoctorByIdAsync(1));
        _doctorRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetDoctorByIdAsync_CacheMissAndDoctorNotFound_WritesNullCacheEntryAndThrows()
    {
        // Arrange
        _cacheServiceMock
            .Setup(c => c.GetAsync<CacheEntry<DoctorDto>>("doctor:1"))
            .ReturnsAsync((CacheEntry<DoctorDto>?)null);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Doctor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _doctorService.GetDoctorByIdAsync(1));

        // 應寫入空值快取以防止快取穿透
        _cacheServiceMock.Verify(
            c => c.SetAsync(
                "doctor:1",
                It.Is<CacheEntry<DoctorDto>>(e => e.Value == null),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDoctorByIdAsync_CacheMissAndDoctorFound_WritesCacheEntryAndReturnsDto()
    {
        // Arrange
        var doctor = new Doctor("Dr. D");
        _cacheServiceMock
            .Setup(c => c.GetAsync<CacheEntry<DoctorDto>>("doctor:1"))
            .ReturnsAsync((CacheEntry<DoctorDto>?)null);
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(doctor);

        // Act
        var result = await _doctorService.GetDoctorByIdAsync(1);

        // Assert
        Assert.Equal("Dr. D", result.Name);
        _cacheServiceMock.Verify(
            c => c.SetAsync(
                "doctor:1",
                It.Is<CacheEntry<DoctorDto>>(e => e.Value != null && e.Value.Name == "Dr. D"),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    #endregion

    #region UpdateDoctorAsync

    [Fact]
    public async Task UpdateDoctorAsync_DoctorNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Doctor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _doctorService.UpdateDoctorAsync(1, "New Name"));
    }

    [Fact]
    public async Task UpdateDoctorAsync_ValidRequest_UpdatesNameAndInvalidatesBothCaches()
    {
        // Arrange
        var doctor = new Doctor("Old Name");
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(doctor);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _doctorService.UpdateDoctorAsync(1, "New Name");

        // Assert
        Assert.Equal("New Name", doctor.Name);
        _doctorRepositoryMock.Verify(r => r.UpdateAsync(doctor), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("doctor:1"), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("doctors:all"), Times.Once);
    }

    #endregion

    #region DeleteDoctorAsync

    [Fact]
    public async Task DeleteDoctorAsync_DoctorNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((Doctor?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _doctorService.DeleteDoctorAsync(1));
    }

    [Fact]
    public async Task DeleteDoctorAsync_ValidRequest_DeletesAndInvalidatesBothCaches()
    {
        // Arrange
        var doctor = new Doctor("Dr. E");
        _doctorRepositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(doctor);
        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _doctorService.DeleteDoctorAsync(1);

        // Assert
        _doctorRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("doctor:1"), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveAsync("doctors:all"), Times.Once);
    }

    #endregion
}
