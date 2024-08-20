using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Company.FutureProject.UseCases.Contributors.Delete;

public record DeleteContributorCommand(int ContributorId) : ICommand<Result>;
