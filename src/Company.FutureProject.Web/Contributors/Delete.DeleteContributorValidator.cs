using FastEndpoints;
using FluentValidation;

namespace Company.FutureProject.Web.Contributors;

/// <summary>
/// See: https://fast-endpoints.com/docs/validation
/// </summary>
public class DeleteContributorValidator : Validator<DeleteContributorRequest>
{
  public DeleteContributorValidator()
  {
    RuleFor(x => x.ContributorId)
      .GreaterThan(0);
  }
}
