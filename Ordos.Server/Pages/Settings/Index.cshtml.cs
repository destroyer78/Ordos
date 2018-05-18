using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ordos.DataService.Data;
using Ordos.DataService.Services;

namespace DRM.Pages_Settings
{
    public class IndexModel : PageModel
    {
        private readonly SystemContext _context;

        public IndexModel(SystemContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string CompanyName { get; set; }

        public void OnGetAsync()
        {
            CompanyName = _context.ConfigurationValues.FirstOrDefault(x => x.Id.Contains("CompanyName")).Value;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.ConfigurationValues.FirstOrDefault(x => x.Id.Contains("CompanyName")).Value = CompanyName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            DatabaseService.LoadApplicationSettings();

            return RedirectToPage("../Index");
        }
    }
}
