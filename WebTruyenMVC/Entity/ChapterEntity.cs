using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebTruyenMVC.Entity
{
    public class ChapterEntity
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        [BsonRepresentation(BsonType.ObjectId)]
        public string StoryId { get; set; } = string.Empty;

        public int ChapterNumber { get; set; } = 0;

        public DateTime Created { get; set; } = DateTime.UtcNow;

        [BsonIgnore]
        public StoryEntity? StoryInfo { get; set; }
    }
}
