namespace QuanLyHeThongBanHang.Models.DTOs
{
    // === TOPIC DTOs ===
    public class TopicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public int Status { get; set; }
        public List<TopicDto> Children { get; set; } = new List<TopicDto>();
    }

    public class CreateTopicDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public int Status { get; set; } = 1;
    }

    // === POST DTOs ===
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Image { get; set; }
        public int TopicId { get; set; }
        public string? TopicName { get; set; }
        public int Status { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }

    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Image { get; set; }
        public int TopicId { get; set; }
        public int Status { get; set; } = 1;
        public bool IsFeatured { get; set; } = false;
    }

    // === MENU DTOs ===
    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string? Type { get; set; }
        public int? ParentId { get; set; }
        public string? Position { get; set; }
        public int SortOrder { get; set; }
        public int Status { get; set; }
        public List<MenuDto> Children { get; set; } = new List<MenuDto>();
    }

    public class CreateMenuDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Link { get; set; }
        public string? Type { get; set; }
        public int? ParentId { get; set; }
        public string? Position { get; set; }
        public int SortOrder { get; set; }
        public int Status { get; set; } = 1;
    }
}
