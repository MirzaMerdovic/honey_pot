using MediatR;

namespace HoneyPot.Api
{
    internal class HoneySentNotification : INotification
    {
        public string Name { get; set; }

        public double TimeTook { get; set; }
    }
}