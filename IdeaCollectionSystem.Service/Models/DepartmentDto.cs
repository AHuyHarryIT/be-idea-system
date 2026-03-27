using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdeaCollectionSystem.Service.Models
{
    // Dùng để trả dữ liệu ra cho Client (GET)
    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // Dùng để nhận dữ liệu khi tạo mới (POST)
    public class DepartmentCreateDto
    {
        // Có thể thêm [Required] nếu bạn dùng DataAnnotations để validate
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // Dùng để nhận dữ liệu khi cập nhật (PUT)
    public class DepartmentUpdateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
