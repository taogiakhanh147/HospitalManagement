using System;
using System.Collections.Generic;

namespace Hospital.Models
{
    public partial class Role
    {
        public Role()
        {
            staff = new HashSet<staff>();
        }

        public int RoleId { get; set; }
        public string? RoleName { get; set; }

        public virtual ICollection<staff> staff { get; set; }
    }
}
