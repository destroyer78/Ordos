using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ordos.DataService.Data;
using Ordos.Core.Models;

namespace DRM.Pages_Devices
{
    public class IndexModel : PageModel
    {
        private readonly SystemContext _context;

        public IndexModel(SystemContext context)
        {
            _context = context;
        }

        public IList<Device> Device { get;set; }

        public async Task OnGetAsync()
        {
            Device = await _context.Devices
                .ToListAsync();
        }
    }
}
