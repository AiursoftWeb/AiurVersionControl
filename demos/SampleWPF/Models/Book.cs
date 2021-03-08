using System;

namespace AiurVersionControl.SampleWPF.Models
{
    public class Book
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
    }
}
