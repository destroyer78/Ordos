using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ordos.Core.Models;
using Ordos.DataService.Data;
using Ordos.DataService.Services;

namespace DRM.Pages_Devices
{
    public class DeleteModel : PageModel
    {
        private readonly SystemContext _context;

        public DeleteModel(SystemContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Device Device { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Device = await _context.Devices
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Device == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Device = await _context.Devices.FindAsync(id);

            if (Device != null)
            {
                _context.Devices.Remove(Device);
                await _context.SaveChangesAsync();
            }

            DatabaseService.Init();

            return RedirectToPage("./Index");
        }
    }
}
