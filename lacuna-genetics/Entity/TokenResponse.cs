using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lacuna_genetics.Entity
{
    public class TokenResponse
    {
#nullable disable warnings
        public string? AccessToken { get; set; }
        public string Code { get; set; }
        public string? Message { get; set; }
    }
}
