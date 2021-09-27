using System;

namespace Shared
{
    public class DeveloperCreatedEvent
    {
        public DeveloperCreatedEvent(Developer developer)
        {
            Id = Guid.NewGuid();
            CreationDate = DateTime.Now;
            Developer = developer;
        }

        public Guid Id{ get; set; }
        public DateTime CreationDate { get; set; }
        public Developer Developer { get; set; }
    }
}
