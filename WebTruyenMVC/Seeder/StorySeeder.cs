﻿using WebTruyenMVC.Entity;
using MongoDB.Driver;

namespace WebTruyenMVC.Seeder
{
    public class StorySeeder
    {
        public static void Seed(MongoContext context)
        {
            var stories = context.GetCollection<StoryEntity>("Stories");

            if (stories.CountDocuments(Builders<StoryEntity>.Filter.Empty) == 0)
            {
                var sampleStories = new List<StoryEntity>
                {
                    new StoryEntity
                    {
                        Title = "One Piece",
                        Description = "Hành trình tìm kho báu huyền thoại của Luffy.",
                        CoverImage = "img/cover1.jpg",
                        Status = "Đang tiến hành",
                        Rating = 9.5,
                        Views = 12345,
                        Created = DateTime.UtcNow
                    },
                    new StoryEntity
                    {
                        Title = "Naruto",
                        Description = "Câu chuyện về ninja Uzumaki Naruto.",
                        CoverImage = "img/cover2.jpg",
                        Status = "Hoàn thành",
                        Rating = 9.2,
                        Views = 20000,
                        Created = DateTime.UtcNow
                    },
                    new StoryEntity
                    {
                        Title = "Bleach",
                        Description = "Ichigo trở thành thần chết thay thế.",
                        CoverImage = "img/cover3.jpg",
                        Status = "Hoàn thành",
                        Rating = 8.9,
                        Views = 15000,
                        Created = DateTime.UtcNow
                    },
                    new StoryEntity
                    {
                        Title = "Demon Slayer",
                        Description = "Cuộc chiến của Tanjiro với quỷ dữ.",
                        CoverImage = "img/cover4.jpg",
                        Status = "Hoàn thành",
                        Rating = 9.0,
                        Views = 17000,
                        Created = DateTime.UtcNow
                    },
                    new StoryEntity
                    {
                        Title = "Jujutsu Kaisen",
                        Description = "Chiến đấu chống lại lời nguyền.",
                        CoverImage = "img/cover5.jpg",
                        Status = "Đang tiến hành",
                        Rating = 8.8,
                        Views = 14000,
                        Created = DateTime.UtcNow
                    }
                };

                stories.InsertMany(sampleStories);
                Console.WriteLine("✅ Đã seed 5 truyện mẫu.");
            }
        }
    }
}
