using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ordos.DataService.Data;
using Ordos.Core.Models;
using Ordos.DataService;

namespace DRM.Pages_Devices
{
    public class CreateModel : PageModel
    {
        private readonly SystemContext _context;

        public CreateModel(SystemContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Device Device { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Devices.Add(Device);
            await _context.SaveChangesAsync();

            DatabaseService.Init();

            return RedirectToPage("./Index");
        }
    }
}