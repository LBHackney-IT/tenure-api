using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hackney.Shared.Tenure;

namespace TenureInformationApi.V1.Boundary.Requests
{
    public class EditTenureDetailsRequestObject
    {
        public DateTime? StartOfTenureDate { get; set; }
        public DateTime? EndOfTenureDate { get; set; }
        public TenureType TenureType { get; set; }
    }
}
