using Alphabet.Application.Features.Products.Commands.CreateProduct;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Alphabet.UnitTests.Application.Products;

public sealed class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Product_When_Command_Is_Valid()
    {
        var repository = new Mock<IRepository<Product>>();
        var unitOfWork = new Mock<IUnitOfWork>();

        repository
            .Setup(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateProductCommandHandler(repository.Object, unitOfWork.Object);
        var command = new CreateProductCommand("Laptop", "High-end ultrabook", 2499.99m, "USD");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Laptop");
        repository.Verify(x => x.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Price_Is_Negative()
    {
        var repository = new Mock<IRepository<Product>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var handler = new CreateProductCommandHandler(repository.Object, unitOfWork.Object);
        var command = new CreateProductCommand("Laptop", "High-end ultrabook", -1m, "USD");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
