namespace Safexchange.Services;

public interface ICheckoutService
{
    Task EnsureReferenceDataAsync(CancellationToken cancellationToken = default);

    Task<CheckoutResult> PlaceOrderAsync(CheckoutInput input, CancellationToken cancellationToken = default);
}
