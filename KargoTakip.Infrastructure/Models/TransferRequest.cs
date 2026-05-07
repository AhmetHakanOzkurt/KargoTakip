using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KargoTakip.Infrastructure.Models
{
    public class TransferRequest
    {
        public int Id { get; set; }
        public int RequesterBranchId { get; set; }
        public int TargetBranchId { get; set; }
        public string Status { get; set; } = "Bekliyor";
        public string? Note { get; set; }
        public int RequestedByUserId { get; set; }
        public int? RespondedByUserId { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }

        public Branch RequesterBranch { get; set; } = null!;
        public Branch TargetBranch { get; set; } = null!;
        public User RequestedByUser { get; set; } = null!;
        public User? RespondedByUser { get; set; }
        public ICollection<TransferRequestItem> Items { get; set; } = new List<TransferRequestItem>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
