using TaskManager.Application.UseCases.Queries.Tasks;
using TaskManager.Application.Validators.Tasks;

namespace TaskManager.Tests.Application;

public class GetTasksQueryValidatorTests
{
    private static bool IsValid(GetTasksQuery query) => new GetTasksQueryValidator().Validate(query).IsValid;

    [Theory]
    [InlineData(null)]
    [InlineData("asc")]
    [InlineData("desc")]
    [InlineData("DESC")]
    public void SortDirection_AcceptsKnownValues(string? sortDirection)
    {
        Assert.True(IsValid(new GetTasksQuery(null, null, null, null, sortDirection, null, null)));
    }

    [Theory]
    [InlineData("up")]
    [InlineData("0")]
    public void SortDirection_RejectsUnknownValues(string sortDirection)
    {
        Assert.False(IsValid(new GetTasksQuery(null, null, null, null, sortDirection, null, null)));
    }
}
