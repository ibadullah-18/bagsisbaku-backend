using bagsisbaku.Application.Orders;
using bagsisbaku.Application.Orders.Administration;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Domain.Orders;

namespace bagsisbaku.Infrastructure.Orders.Administration;

internal static class AdminOrderModelMapper
{
    public static AdminOrderSummaryModel ToSummary(
        Order order,
        string customerFullName,
        string customerEmail)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new AdminOrderSummaryModel(
            order.Id,
            order.UserId,
            customerFullName,
            customerEmail,
            order.OrderNumber,
            order.RecipientFullName,
            order.PhoneNumber,
            (int)order.Status,
            (int)order.DeliveryType,
            (int)order.PaymentMethod,
            order.Items.Count,
            order.Items.Sum(item => item.Quantity),
            order.Total,
            order.PlacedAtUtc,
            order.CanBeCancelled);
    }

    public static AdminOrderDetailsModel ToDetails(
        Order order,
        string customerFullName,
        string customerEmail,
        SupportedLanguage language)
    {
        ArgumentNullException.ThrowIfNull(order);

        var items =
            order.Items
                .OrderBy(item => item.Id)
                .Select(
                    item =>
                        new PlacedOrderItemModel(
                            item.Id,
                            item.ProductId,
                            item.ProductVariantId,
                            item.ProductName,
                            item.ProductCode,
                            (int)item.ProductType,
                            item.BrandName,
                            item.ImageUrl,
                            item.SizeId,
                            item.Size,
                            item.ColorId,
                            item.Color,
                            item.HexCode,
                            item.OriginalUnitPrice,
                            item.UnitPrice,
                            item.UnitDiscountAmount,
                            item.Quantity,
                            item.LineSubtotal,
                            item.LineDiscountAmount,
                            item.LineTotal))
                .ToArray();

        var statusHistory =
            order.StatusHistory
                .OrderBy(history => history.ChangedAtUtc)
                .Select(
                    history =>
                        new CustomerOrderStatusHistoryModel(
                            history.Id,
                            history.PreviousStatus.HasValue
                                ? (int)history.PreviousStatus.Value
                                : null,
                            (int)history.NewStatus,
                            history.Note,
                            history.ChangedAtUtc))
                .ToArray();

        return new AdminOrderDetailsModel(
            order.Id,
            order.UserId,
            customerFullName,
            customerEmail,
            order.OrderNumber,
            ToLanguageCode(language),
            (int)order.Status,
            (int)order.DeliveryType,
            (int)order.PaymentMethod,
            order.CustomerAddressId,
            order.RecipientFullName,
            order.PhoneNumber,
            order.City,
            order.District,
            order.AddressLine,
            order.PostalCode,
            order.DeliveryNote,
            order.CustomerNote,
            order.Subtotal,
            order.DiscountAmount,
            order.DeliveryFee,
            order.Total,
            order.PlacedAtUtc,
            order.ConfirmedAtUtc,
            order.OnDeliveryAtUtc,
            order.DeliveredAtUtc,
            order.CancelledAtUtc,
            order.CanBeCancelled,
            items,
            statusHistory);
    }

    private static string ToLanguageCode(
        SupportedLanguage language)
    {
        return language switch
        {
            SupportedLanguage.Russian => "ru",
            SupportedLanguage.English => "en",
            _ => "az"
        };
    }
}