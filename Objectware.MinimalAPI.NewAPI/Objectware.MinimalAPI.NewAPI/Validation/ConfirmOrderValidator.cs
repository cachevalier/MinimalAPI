
using FluentValidation;
using Objectware.MinimalAPI.NewAPI.Models;

namespace Objectware.MinimalAPI.NewAPI.Validation;
public class ConfirmOrderValidator : AbstractValidator<ConfirmOrderRequest>
{
    public ConfirmOrderValidator()
    {
        RuleFor(x => x.PaymentInfo).NotNull();
        RuleFor(x => x.PaymentInfo.CardToken).NotEmpty();
        RuleFor(x => x.PaymentInfo.Amount).GreaterThan(0);
    }
}
