using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfol.io.Identity.ViewModels.ResponseModels.AuthResponseModels
{
    public class ConfirmEmailResponse
    {
        public string? message { get; set; }
        public string? returnUrl { get; set; }
    }
}
