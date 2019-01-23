using System.Threading.Tasks;
using CountingStrings.API.Bus;
using NServiceBus;
using NServiceBus.Logging;

namespace CountingStrings.Service
{
    public class MyHandler : IHandleMessages<MyMessage>
    {
        private static readonly ILog Log = LogManager.GetLogger<MyHandler>();

        #region MessageHandler
        public Task Handle(MyMessage message, IMessageHandlerContext context)
        {
            Log.Info("Message received at endpoint");
            return Task.CompletedTask;
        }
        #endregion
    }
}
