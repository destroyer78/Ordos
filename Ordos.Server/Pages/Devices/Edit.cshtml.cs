using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ordos.DataService.Data;
using Ordos.Core.Models;
using Ordos.DataService;

namespace DRM.Pages_Devices
{
    public class EditModel : PageModel
    {
        private readonly SystemContext _context;

        public EditModel(SystemContext context)
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Device).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DeviceExists(Device.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            DatabaseService.Init();

            return RedirectToPage("./Index");
        }

        private bool DeviceExists(int id)
        {
            return _context.Devices.Any(e => e.Id == id);
        }
    }
}
