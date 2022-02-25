using FluentValidation;

namespace Ordering.Application.Features.Orders.Commands.CheckoutOrder
{
    //Este validator é chamado pelo MediatR e é corrido andtes do CommandHandler ser chamado
    public class CheckoutOrderCommandValidator : AbstractValidator<CheckoutOrderCommand>
    {
        public CheckoutOrderCommandValidator()
        {
            RuleFor(p => p.UserName)
                .NotEmpty().WithMessage("{Username} is required")
                .NotNull()
                .MaximumLength(50).WithMessage("{Username} must not exceed 50 chars");

            RuleFor(p => p.EmailAddress)
                .NotEmpty().WithMessage("{EmailAddress} is required");

            RuleFor(p => p.TotalPrice)
                .NotEmpty().WithMessage("{TotalPrice} is requied")
                .GreaterThan(0).WithMessage("{TotalPrice} should be greater than 0");
        }
    }
}