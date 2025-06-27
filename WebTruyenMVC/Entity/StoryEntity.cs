using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebTruyenMVC.Entity
{
    public class StoryEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string CoverImage { get; set; } = string.Empty;

        // Tác giả
        [BsonRepresentation(BsonType.ObjectId)]
        public string AuthorId { get; set; } = string.Empty;


        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public double Rating { get; set; }

        public int Views { get; set; } = 0;

        public DateTime Created { get; set; } = DateTime.UtcNow;

        // Thông tin liên kết (không lưu trong DB)
        [BsonIgnore]
        public AuthorEntity? AuthorInfo { get; set; }

        [BsonIgnore]
        public List<ChapterEntity>? Chapters { get; set; }
    }
}
