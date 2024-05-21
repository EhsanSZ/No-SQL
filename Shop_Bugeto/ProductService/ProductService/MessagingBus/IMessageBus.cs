namespace ProductService.MessagingBus
{
    public interface IMessageBus
    {
        void SendMessage(BaseMessage message, string exchange);
    }

}
