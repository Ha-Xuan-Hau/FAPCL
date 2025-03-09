using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BookClassRoom.Hubs;
using Microsoft.AspNetCore.SignalR;
using FAPCL.Model;

namespace BookClassRoom.Pages.ClassroomManagement
{
    public class CreateModel : PageModel
    {
        private readonly BookClassRoomContext _context;
        private readonly IHubContext<SignalRServer> _hubContext;
        public CreateModel(BookClassRoomContext context, IHubContext<SignalRServer> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Room Room { get; set; } = new Room();

        public SelectList RoomTypeOptions { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            RoomTypeOptions = new SelectList(await _context.RoomTypes.ToListAsync(), "RoomTypeId", "RoomType1");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            if (Room.Capacity <= 0)
            {
                ModelState.AddModelError("Room.Capacity", "Capacity must be greater than 0.");
                RoomTypeOptions = new SelectList(await _context.RoomTypes.ToListAsync(), "RoomTypeId", "RoomType1");
                return Page();
            }

            var newRoom = new Room
            {
                RoomName = Room.RoomName,
                Capacity = Room.Capacity,
                RoomTypeId = Room.RoomTypeId,
                HasProjector = Room.HasProjector ?? false, 
                HasSoundSystem = Room.HasSoundSystem ?? false, 
                Status = "Available", 
                IsAction = true 
            };

            _context.Rooms.Add(newRoom);

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("LoadRoom");
            return RedirectToPage("/ClassroomManagement/Index");
        }

    }
}