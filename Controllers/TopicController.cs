using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHeThongBanHang.Data;
using QuanLyHeThongBanHang.Models;
using QuanLyHeThongBanHang.Models.DTOs;
using System.Text.RegularExpressions;
using System.Text;

using QuanLyHeThongBanHang.Filters;

namespace QuanLyHeThongBanHang.Controllers
{
    [Authorize]
    [RequirePermission("topics_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TopicController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Topic
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicDto>>> GetTopics()
        {
            var topics = await _context.Topics
                .Include(t => t.Children)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            // Return flat list but with children populated for easy UI tree building
            var result = topics.Select(t => MapTopicToDto(t)).ToList();
            return result;
        }

        // GET: api/Topic/5
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<TopicDto>> GetTopic(int id)
        {
            var topic = await _context.Topics
                .Include(t => t.Children)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (topic == null) return NotFound();

            return MapTopicToDto(topic);
        }

        // POST: api/Topic
        [HttpPost]
        public async Task<ActionResult<TopicDto>> PostTopic(CreateTopicDto createDto)
        {
            string slug = string.IsNullOrEmpty(createDto.Slug) 
                ? GenerateSlug(createDto.Name) 
                : createDto.Slug;

            // Ensure unique slug
            if (await _context.Topics.AnyAsync(t => t.Slug == slug))
            {
                slug += "-" + new Random().Next(100, 999);
            }

            var topic = new Topic
            {
                Name = createDto.Name,
                Slug = slug,
                Description = createDto.Description,
                ParentId = createDto.ParentId,
                SortOrder = createDto.SortOrder,
                Status = createDto.Status
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTopic), new { id = topic.Id }, MapTopicToDto(topic));
        }

        // PUT: api/Topic/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTopic(int id, CreateTopicDto updateDto)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null) return NotFound();

            topic.Name = updateDto.Name;
            topic.Description = updateDto.Description;
            topic.ParentId = updateDto.ParentId;
            topic.SortOrder = updateDto.SortOrder;
            topic.Status = updateDto.Status;
            
            if (!string.IsNullOrEmpty(updateDto.Slug))
            {
                topic.Slug = updateDto.Slug;
            }

            topic.NgayCapNhat = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TopicExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Topic/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            var topic = await _context.Topics.FindAsync(id);
            if (topic == null) return NotFound();

            topic.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TopicExists(int id)
        {
            return _context.Topics.Any(e => e.Id == id);
        }

        private static TopicDto MapTopicToDto(Topic t)
        {
            return new TopicDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                Description = t.Description,
                ParentId = t.ParentId,
                SortOrder = t.SortOrder,
                Status = t.Status
            };
        }

        private static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}
