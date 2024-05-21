using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OrderService.Infrastructure.Context;
using OrderService.MessagingBus;
using OrderService.MessagingBus.Messages;
using OrderService.MessagingBus.SendMessage;
using OrderService.Model.Dto;
using OrderService.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Model.Services
{
    public interface IOrderService
    {

        List<OrderDto> GetOrdersForUser(string UserId);
        OrderDetailDto GetOrderById(Guid Id);
        ResultDto RequestPayment(Guid OrderId);

    }


    public class OrderService : IOrderService
    {
        private readonly OrderDataBaseContext context;
        private readonly IMessageBus messageBus;
        private readonly string QueueName_OrderSendToPayment;
        public OrderService(OrderDataBaseContext context, IMessageBus messageBus,
            IOptions<RabbitMqConfiguration> rabbitMqOptions)
        {
            this.context = context;
            this.messageBus = messageBus;
            QueueName_OrderSendToPayment = rabbitMqOptions.Value.QueueName_OrderSendToPayment;
        }

        public OrderDetailDto GetOrderById(Guid Id)
        {
            var orders = context.Orders.
                   Include(p => p.OrderLines)
                  .ThenInclude(p => p.Product)
                 .FirstOrDefault(p => p.Id == Id);

            if (orders == null)
                throw new Exception("Order Not Found");
            var result = new OrderDetailDto
            {
                Id = orders.Id,
                Address = orders.Address,
                FirstName = orders.FirstName,
                LastName = orders.LastName,
                PhoneNumber = orders.PhoneNumber,
                UserId = orders.UserId,
                OrderPaid = orders.OrderPaid,
                OrderPlaced = orders.OrderPlaced,
                TotalPrice = orders.TotalPrice,
                PaymentStatus = orders.PaymentStatus,
                OrderLines = orders.OrderLines.Select(ol => new OrderLineDto
                {
                    Id = ol.Id,
                    Name = ol.Product.Name,
                    Price = ol.Product.Price,
                    Quantity = ol.Quantity,

                }).ToList(),

            };
            return result;

        }

        public List<OrderDto> GetOrdersForUser(string UserId)
        {
            var orders = context.Orders.
             Include(p => p.OrderLines)
            .Where(p => p.UserId == UserId)
            .Select(p => new OrderDto
            {
                Id = p.Id,
                OrderPaid = p.OrderPaid,
                OrderPlaced = p.OrderPlaced,
                ItemCount = p.OrderLines.Count(),
                TotalPrice = p.TotalPrice,
                PaymentStatus = p.PaymentStatus
            }).ToList();
            return orders;
        }

        public ResultDto RequestPayment(Guid OrderId)
        {
            var order = context.Orders.SingleOrDefault(p => p.Id == OrderId);
            if (order == null)
            {
                return new ResultDto
                {
                    IsSuccess = false,
                    Message = "سفارش پیدا نشد"
                };
            }
            // ارسال پیام پرداخت برای سرویس پرداخت
            SendOrderToPaymentMessage paymentMessage = new SendOrderToPaymentMessage()
            {
                Amount = order.TotalPrice,
                Creationtime = DateTime.Now,
                MessageId = Guid.NewGuid(),
                OrderId = order.Id,
            };
            messageBus.SendMessage(paymentMessage, QueueName_OrderSendToPayment);
            //تغییر وضعیت پرداخت سفارش
            order.RequestPayment();
            context.SaveChanges();
            return new ResultDto
            {
                IsSuccess = true,
                Message = "درخواست پرداخت ثبت شد"
            };
        }
    }



    public class OrderDto
    {
        public Guid Id { get; set; }
        public int ItemCount { get; set; }
        public int TotalPrice { get; set; }
        public bool OrderPaid { get; set; }
        public DateTime OrderPlaced { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

    }


    public class OrderDetailDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public DateTime OrderPlaced { get; set; }
        public bool OrderPaid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public int TotalPrice { get; set; }
        public List<OrderLineDto> OrderLines { get; set; }
        public PaymentStatus PaymentStatus { get; set; }

    }

    public class OrderLineDto
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }
    }
}
