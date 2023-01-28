using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lacuna_genetics.Entity
{
    public class Job
    {
        #nullable disable warnings
        public string Id { get; set; }
        public string Type { get; set; }
        public string? Strand { get; set; }
        public string? StrandEncoded { get; set; }
        public string? GeneEncoded { get; set; }

    }
}
