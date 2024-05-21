using Microservices.Web.Frontend.Models.Dtos;
using Microservices.Web.Frontend.Services.BasketServices;
using Microservices.Web.Frontend.Services.DiscountServices;
using Microservices.Web.Frontend.Services.ProductServices;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microservices.Web.Frontend.Controllers
{
    public class BasketController : Controller
    {
        private readonly IBasketService basketService;
        private readonly IProductService productService;
        private readonly IDiscountService discountService;
        private readonly string UserId = "1";
        public BasketController(IBasketService basketService,
            IProductService productService,
            IDiscountService discountService)
        {
            this.basketService = basketService;
            this.productService = productService;
            this.discountService = discountService;
        }
        public IActionResult Index()
        {
            var basket = basketService.GetBasket(UserId);

            if (basket.discountId.HasValue)
            {
                var discount = discountService.GetDiscountById(basket.discountId.Value);
                basket.DiscountDetail = new DiscountInBasketDto
                {
                    Amount = discount.Data.Amount,
                    DiscountCode = discount.Data.Code,
                };
            }

            return View(basket);
        }

        public IActionResult Delete(Guid Id)
        {
            basketService.DeleteFromBasket(Id);
            return RedirectToAction("Index");
        }

        public IActionResult AddToBasket(Guid ProductId)
        {
            var product = productService.Getproduct(ProductId);
            var basket = basketService.GetBasket(UserId);

            AddToBasketDto item = new AddToBasketDto()
            {
                BasketId = basket.id,
                ImageUrl = product.image,
                ProductId = product.id,
                ProductName = product.name,
                Quantity = 1,
                UnitPrice = product.price,
            };
            basketService.AddToBasket(item, UserId);
            return RedirectToAction("Index");
        }

        public IActionResult Edit(Guid BasketItemId, int quantity)
        {
            basketService.UpdateQuantity(BasketItemId, quantity);
            return RedirectToAction("Index");

        }


        [HttpPost]
        public IActionResult ApplyDiscount(string DiscountCode)
        {
            if (string.IsNullOrWhiteSpace(DiscountCode))
            {
                return Json(new ResultDto
                {
                    IsSuccess = false,
                    Message = "لطفا کد تخفیف را وارد نمایید"
                });
            }
            var discount = discountService.GetDiscountByCode(DiscountCode);
            if (discount.IsSuccess == true)
            {
                if (discount.Data.Used)
                {
                    return Json(new ResultDto
                    {
                        IsSuccess = false,
                        Message = "این کد تخفیف قبلا استفاده شده است"
                    });
                }

                var basket = basketService.GetBasket(UserId);
                basketService.ApplyDiscountToBasket(Guid.Parse(basket.id), discount.Data.Id);
                discountService.UseDiscount(discount.Data.Id);
                return Json(new ResultDto
                {
                    IsSuccess = true,
                    Message = "کد تخفیف با موفقیت به سبد خرید شما اعمال شد",
                });
            }
            else
            {
                return Json(new ResultDto
                {
                    IsSuccess = false,
                    Message = discount.Message,
                });
            }
        }


        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(CheckoutDto checkout)
        {
            checkout.UserId = UserId;
            checkout.BasketId = Guid.Parse(basketService.GetBasket(UserId).id);
            var result = basketService.Checkout(checkout);
            if (result.IsSuccess)
                return RedirectToAction("OrderCreated");
            else
            {
                //افزودن پیام
                return View(checkout);
            }
        }

        public IActionResult OrderCreated()
        {
            return View();
        }
    }


}
