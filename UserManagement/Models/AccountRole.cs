using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Models
{ [Table("tb_m_accountrole")]
    public class AccountRole
    {
        [Key]
        public string NIK { get; set; }
        [JsonIgnore]
        public virtual Account Account { get; set; }
        public int RoleId { get; set; }
        [JsonIgnore]
        public virtual Role Role { get; set; }
    }
}
