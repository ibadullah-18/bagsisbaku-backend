using bagsisbaku.Application.Orders;
using bagsisbaku.Domain.Localization;
using bagsisbaku.Domain.Orders;

namespace bagsisbaku.Infrastructure.Orders;

internal static class CheckoutModelMapper
{
    public static PlacedOrderModel ToModel(
        Order order,
        SupportedLanguage language)
    {
        ArgumentNullException.ThrowIfNull(order);

        return new PlacedOrderModel(
            order.Id,
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
            order.CanBeCancelled,
            order.Items
                .Select(ToItemModel)
                .ToArray());
    }

    private static PlacedOrderItemModel ToItemModel(
        OrderItem item)
    {
        return new PlacedOrderItemModel(
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
            item.LineTotal);
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