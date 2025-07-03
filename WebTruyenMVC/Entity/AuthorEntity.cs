using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace WebTruyenMVC.Entity
{
    public class AuthorEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        [Required(ErrorMessage = "Tên tác giả là bắt buộc")]
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public List<string> Stories { get; set; } = new List<string>();
    }

    public class AuthorWithStoryCountViewModel
    {
        public AuthorEntity Author { get; set; }
        public int StoryCount { get; set; }
    }
}
