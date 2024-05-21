using BasketService.Model.Dtos;
using BasketService.Model.Services.BasketServices;
using BasketService.Model.Services.DiscountServices;
using System;
using System.Threading.Tasks;

namespace BasketService.Model.Services
{
    public interface IBasketService
    {
        BasketDto GetOrCreateBasketForUser(string UserId);
        BasketDto GetBasket(string UserId);
        void AddItemToBasket(AddItemToBasketDto item);
        void RemoveItemFromBasket(Guid ItemId);
        void SetQuantities(Guid itemId, int quantity);
        void TransferBasket(string anonymousId, string UserId);
        void ApplyDiscountToBasket(Guid BasketId, Guid DiscountId);
        ResultDto CheckoutBasket(CheckoutBasketDto checkoutBasket, IDiscountService discountService);
    }
}
