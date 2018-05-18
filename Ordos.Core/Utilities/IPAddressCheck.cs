using Ordos.Core.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Ordos.Core.Utilities
{
    public class IPAddressCheck : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!(validationContext.ObjectInstance is Device model))
                throw new ArgumentException("Attribute not applied on a Device");

            var ipAddress = model.IPAddress;

            var result = IPAddress.TryParse(ipAddress, out var _);

            return result ? ValidationResult.Success : new ValidationResult("Not a valid IP Address");
        }

    }
}
