using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ordos.DataService.Data;
using Ordos.Core.Models;
using Ordos.DataService.Services;

namespace DRM.Pages_Devices
{
    public class CreateModel : PageModel
    {
        private readonly SystemContext _context;
        //private Scheduler scheduler;

        public CreateModel(SystemContext context)
        {
            _context = context;
            //scheduler = new Scheduler(context);
        }

        public IActionResult OnGet()
        {
        //ViewData["DeviceTypeId"] = new SelectList(_context.DeviceTypes, "Id", "DeviceTypeName");
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