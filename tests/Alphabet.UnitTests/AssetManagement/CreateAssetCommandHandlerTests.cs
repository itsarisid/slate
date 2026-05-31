using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Common.Interfaces.AssetManagement;
using Alphabet.Application.Features.AssetManagement.Assets.Commands;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Enums;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.AssetManagement;
using FluentAssertions;
using Moq;
using Xunit;

namespace Alphabet.UnitTests.AssetManagement;

public sealed class CreateAssetCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Asset_And_Generate_Tag_When_None_Is_Provided()
    {
        var repository = new Mock<IAssetRepository>();
        var tagGenerator = new Mock<IAssetTagGenerator>();
        var barcodeService = new Mock<IAssetBarcodeService>();
        var currentUser = new Mock<ICurrentUserService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository.Setup(x => x.AssetTagExistsAsync("AST-000001", null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        tagGenerator.Setup(x => x.GenerateAsync(It.IsAny<CancellationToken>())).ReturnsAsync("AST-000001");
        barcodeService
            .Setup(x => x.GeneratePayload(It.IsAny<Asset>()))
            .Returns(new AssetCodePayload("qr://AST-000001", "AST-000001"));
        currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateAssetCommandHandler(
            repository.Object,
            tagGenerator.Object,
            barcodeService.Object,
            currentUser.Object,
            unitOfWork.Object);

        var command = new CreateAssetCommand(
            null,
            "Dell XPS 15",
            "Developer laptop",
            Guid.NewGuid(),
            "Laptop",
            "Dell",
            "XPS 15 9520",
            "SN-123",
            new DateOnly(2026, 1, 1),
            new DateOnly(2028, 1, 1),
            1999.99m,
            "USD",
            AssetStatus.Available,
            AssetCondition.Good,
            Guid.NewGuid(),
            null,
            new Dictionary<string, string> { ["ram"] = "32GB" },
            ["image.png"],
            ["invoice.pdf"]);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Message.Should().Contain("created successfully");
        repository.Verify(x => x.AddAssetAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(x => x.AddActivityAsync(It.IsAny<AssetActivityLog>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Asset_Tag_Already_Exists()
    {
        var repository = new Mock<IAssetRepository>();
        var tagGenerator = new Mock<IAssetTagGenerator>();
        var barcodeService = new Mock<IAssetBarcodeService>();
        var currentUser = new Mock<ICurrentUserService>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository.Setup(x => x.AssetTagExistsAsync("AST-000001", null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new CreateAssetCommandHandler(
            repository.Object,
            tagGenerator.Object,
            barcodeService.Object,
            currentUser.Object,
            unitOfWork.Object);

        var command = new CreateAssetCommand(
            "AST-000001",
            "Dell XPS 15",
            "Developer laptop",
            Guid.NewGuid(),
            "Laptop",
            "Dell",
            "XPS 15 9520",
            null,
            null,
            null,
            1999.99m,
            "USD",
            AssetStatus.Available,
            AssetCondition.Good,
            Guid.NewGuid(),
            null,
            null,
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        repository.Verify(x => x.AddAssetAsync(It.IsAny<Asset>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
