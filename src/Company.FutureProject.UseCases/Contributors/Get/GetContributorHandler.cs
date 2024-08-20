using Ardalis.Result;
using Ardalis.SharedKernel;
using Company.FutureProject.Core.ContributorAggregate;
using Company.FutureProject.Core.ContributorAggregate.Specifications;

namespace Company.FutureProject.UseCases.Contributors.Get;

/// <summary>
/// Queries don't necessarily need to use repository methods, but they can if it's convenient
/// </summary>
public class GetContributorHandler(IReadRepository<Contributor> _repository)
  : IQueryHandler<GetContributorQuery, Result<ContributorDTO>>
{
  public async Task<Result<ContributorDTO>> Handle(GetContributorQuery request, CancellationToken cancellationToken)
  {
    var spec = new ContributorByIdSpec(request.ContributorId);
    var entity = await _repository.FirstOrDefaultAsync(spec, cancellationToken);
    if (entity == null) return Result.NotFound();

    return new ContributorDTO(entity.Id, entity.Name, entity.PhoneNumber?.Number ?? "");
  }
}
