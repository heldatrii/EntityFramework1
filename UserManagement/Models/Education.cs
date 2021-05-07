using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace UserManagement.Models
{
    [Table("tb_m_education")]
    public class Education
    {
        public int EducationID { get; set; }
        public string Degree { get; set; }
        public string GPA { get; set; }
        public int UniversityID { get; set; }
        
        public virtual University University { get; set; }
        
        public virtual ICollection<Profiling> Profilings { get; set; }
        
    }
}
