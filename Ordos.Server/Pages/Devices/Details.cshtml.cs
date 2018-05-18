using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ordos.Core.Models;
using Ordos.DataService.Data;

namespace DRM.Pages_Devices
{
    public class DetailsModel : PageModel
    {
        private readonly SystemContext _context;

        public DetailsModel(SystemContext context)
        {
            _context = context;
        }

        public Device Device { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Device = await _context.Devices
                                   .Include(x=>x.DisturbanceRecordings).AsNoTracking()
                                   //.Include(d => d.DeviceType)
                                   .FirstOrDefaultAsync(m => m.Id == id);

            if (Device == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
