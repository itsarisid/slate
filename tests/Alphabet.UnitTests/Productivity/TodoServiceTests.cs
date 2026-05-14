using Alphabet.Application.Common.Interfaces;
using Alphabet.Application.Features.Productivity.Todos.Commands;
using Alphabet.Domain.Entities;
using Alphabet.Domain.Interfaces;
using Alphabet.Domain.Interfaces.Productivity;
using FluentAssertions;
using Moq;
using Xunit;

namespace Alphabet.UnitTests.Productivity;

public sealed class TodoServiceTests
{
    [Fact]
    public async Task CreateTodo_Should_Return_Success_When_Request_Is_Valid()
    {
        var todoRepository = new Mock<ITodoRepository>();
        var tagRepository = new Mock<IRepository<Tag>>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.SetupGet(x => x.UserId).Returns((Guid?)Guid.NewGuid());

        todoRepository.Setup(x => x.AddAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        tagRepository.Setup(x => x.AddAsync(It.IsAny<Tag>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateTodoCommandHandler(todoRepository.Object, tagRepository.Object, unitOfWork.Object, currentUser.Object);
        var result = await handler.Handle(
            new CreateTodoCommand("Buy groceries", "Milk and eggs", Alphabet.Domain.Enums.Priority.High, DateTimeOffset.UtcNow.AddDays(1), 30, "Personal", ["shopping"], false, null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Title.Should().Be("Buy groceries");
        todoRepository.Verify(x => x.AddAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
