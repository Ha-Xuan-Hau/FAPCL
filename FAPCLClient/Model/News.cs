using System;
using System.Collections.Generic;

namespace FAPCLClient.Model
{
    public partial class News
    {
        public int NewsId { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string CreatedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsPublished { get; set; }

        public virtual AspNetUser CreatedByNavigation { get; set; } = null!;
    }
}
