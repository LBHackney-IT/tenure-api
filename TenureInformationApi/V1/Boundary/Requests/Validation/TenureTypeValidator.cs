using FluentValidation;
using Hackney.Core.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hackney.Shared.Tenure;

namespace TenureInformationApi.V1.Boundary.Requests.Validation
{
    public class TenureTypeValidator : AbstractValidator<TenureType>
    {
        public TenureTypeValidator()
        {
            RuleFor(x => x.Code).NotXssString()
                         .WithErrorCode(ErrorCodes.XssCheckFailure);
            RuleFor(x => x.Description).NotXssString()
                         .WithErrorCode(ErrorCodes.XssCheckFailure);
        }
    }
}
