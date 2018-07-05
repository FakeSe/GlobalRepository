using System;
using System.Collections.Generic;
using System.Text;

namespace YourProject.Models
{
    public abstract class BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public DateTime DeletedAt { get; set; } = DateTime.MinValue;
        public bool IsDeleted { get; set; } = false;
    }

    
}
