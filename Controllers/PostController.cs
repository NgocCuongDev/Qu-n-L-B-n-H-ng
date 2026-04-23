using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyHeThongBanHang.Data;
using QuanLyHeThongBanHang.Models;
using QuanLyHeThongBanHang.Models.DTOs;
using System.Text.RegularExpressions;

using QuanLyHeThongBanHang.Filters;

namespace QuanLyHeThongBanHang.Controllers
{
    [Authorize]
    [RequirePermission("posts_manage")]
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PostController(AppDbContext context)
        {
            _context = context;
        }

        // =====================
        // PUBLIC ENDPOINTS
        // =====================

        [AllowAnonymous]
        [HttpGet("all-posts")]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetAllPublicPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Topic)
                .Where(p => p.Status == 1)
                .OrderByDescending(p => p.NgayTao)
                .ToListAsync();

            return posts.Select(p => MapPostToDto(p)).ToList();
        }

        [AllowAnonymous]
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<PostDto>> GetPostBySlug(string slug)
        {
            var post = await _context.Posts
                .Include(p => p.Topic)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == 1);

            if (post == null) return NotFound();

            return MapPostToDto(post);
        }

        // =====================
        // ADMIN ENDPOINTS
        // =====================


        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostDto>>> GetPosts()
        {
            var posts = await _context.Posts
                .Include(p => p.Topic)
                .OrderByDescending(p => p.NgayTao)
                .ToListAsync();

            return posts.Select(p => MapPostToDto(p)).ToList();
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<PostDto>> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Topic)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            return MapPostToDto(post);
        }


        [HttpPost]
        public async Task<ActionResult<PostDto>> PostPost(CreatePostDto createDto)
        {
            // ... (rest of method)
            string slug = string.IsNullOrEmpty(createDto.Slug) 
                ? GenerateSlug(createDto.Title) 
                : createDto.Slug;

            // Ensure unique slug
            if (await _context.Posts.AnyAsync(p => p.Slug == slug))
            {
                slug += "-" + new Random().Next(100, 999);
            }

            var post = new Post
            {
                Title = createDto.Title,
                Slug = slug,
                Description = createDto.Description,
                Content = createDto.Content,
                Image = createDto.Image,
                TopicId = createDto.TopicId,
                Status = createDto.Status,
                IsFeatured = createDto.IsFeatured
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, MapPostToDto(post));
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> PutPost(int id, CreatePostDto updateDto)
        {
            // ... (method body)
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            post.Title = updateDto.Title;
            post.Description = updateDto.Description;
            post.Content = updateDto.Content;
            post.Image = updateDto.Image;
            post.TopicId = updateDto.TopicId;
            post.Status = updateDto.Status;
            post.IsFeatured = updateDto.IsFeatured;
            
            if (!string.IsNullOrEmpty(updateDto.Slug))
            {
                post.Slug = updateDto.Slug;
            }

            post.NgayCapNhat = DateTime.Now;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PostExists(id)) return NotFound();
                throw;
            }

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();

            post.IsDeleted = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }

        private static PostDto MapPostToDto(Post p)
        {
            return new PostDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Description = p.Description,
                Content = p.Content,
                Image = p.Image,
                TopicId = p.TopicId,
                TopicName = p.Topic?.Name,
                Status = p.Status,
                IsFeatured = p.IsFeatured,
                NgayTao = p.NgayTao,
                NgayCapNhat = p.NgayCapNhat
            };
        }

        private static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 60 ? str.Length : 60).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
    }
}
