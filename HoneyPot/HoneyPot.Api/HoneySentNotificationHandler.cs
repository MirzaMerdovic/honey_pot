using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HoneyPot.Api
{
    internal class HoneySentNotificationHandler : INotificationHandler<HoneySentNotification>
    {
        private readonly Queue<HoneySentNotification> _notifications;

        public HoneySentNotificationHandler(Queue<HoneySentNotification> notifications)
        {
            _notifications = notifications;
        }

        public Task Handle(HoneySentNotification notification, CancellationToken cancellationToken)
        {
            _notifications.Enqueue(notification);

            return Task.CompletedTask;
        }
    }
}
