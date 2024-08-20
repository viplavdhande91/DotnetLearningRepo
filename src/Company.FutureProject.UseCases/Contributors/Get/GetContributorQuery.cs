using Ardalis.Result;
using Ardalis.SharedKernel;

namespace Company.FutureProject.UseCases.Contributors.Get;

public record GetContributorQuery(int ContributorId) : IQuery<Result<ContributorDTO>>;
