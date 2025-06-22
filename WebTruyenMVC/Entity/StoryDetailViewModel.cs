using System.Collections.Generic;

namespace WebTruyenMVC.Entity
{
    public class StoryDetailViewModel
    {
        public StoryEntity Story { get; set; }
        public List<ChapterEntity> Chapters { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long TotalItemCounts { get; set; }
        public int TotalPages => (int)System.Math.Ceiling((double)TotalItemCounts / PageSize);
    }
}