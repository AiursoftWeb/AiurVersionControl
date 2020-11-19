using System;

namespace AiurEventSyncer.Tests.Models
{
    public class Book
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("D");
        public string Name { get; set; }
    }
}
