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
using FAPCLClient.Model;

namespace BookClassRoom.Pages.ClassroomManagement
{
    public class EditModel : PageModel
    {
        private readonly BookClassRoomContext _context;
        private readonly IHubContext<SignalRServer> _hubContext;
        public EditModel(BookClassRoomContext context, IHubContext<SignalRServer> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public Room Room { get; set; } = new Room();

        public SelectList RoomTypeOptions { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Room = await _context.Rooms.Include(r => r.RoomType).FirstOrDefaultAsync(r => r.RoomId == id);

            if (Room == null)
            {
                return NotFound();
            }

            RoomTypeOptions = new SelectList(await _context.RoomTypes.ToListAsync(), "RoomTypeId", "RoomType1");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            

            var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.RoomId == Room.RoomId);

            if (existingRoom != null)
            {
                existingRoom.RoomName = Room.RoomName;
                existingRoom.Capacity = Room.Capacity;
                existingRoom.RoomTypeId = Room.RoomTypeId;
                existingRoom.HasProjector = Room.HasProjector ?? false;
                existingRoom.HasSoundSystem = Room.HasSoundSystem ?? false;
                existingRoom.Status = existingRoom.Status; 
                existingRoom.IsAction = existingRoom.IsAction ?? existingRoom.IsAction;

                await _context.SaveChangesAsync();
            }
            await _hubContext.Clients.All.SendAsync("LoadRoom");
            return RedirectToPage("/ClassroomManagement/Index");
        }

    }
}