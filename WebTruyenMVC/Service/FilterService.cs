﻿using MongoDB.Driver;
using WebTruyenMVC.Entity;

namespace WebTruyenMVC.Service
{
    public class FilterService
    {
        public static FilterDefinition<T> BuildFilter<T>(string q, Filter filter) where T : class
        {
            var builder = Builders<T>.Filter;
            var filterDefinition = builder.Empty;

            // Lọc theo Title hoặc AuthorName nếu q có giá trị (áp dụng cho StoryEntity)
            if (!string.IsNullOrEmpty(q))
            {
                if (typeof(T).Name == nameof(WebTruyenMVC.Entity.StoryEntity))
                {
                    var titleFilter = builder.Regex("Title", new MongoDB.Bson.BsonRegularExpression(q, "i"));
                    var authorFilter = builder.Regex("AuthorName", new MongoDB.Bson.BsonRegularExpression(q, "i"));
                    filterDefinition &= builder.Or(titleFilter, authorFilter);
                }
                else
                {
                    filterDefinition &= builder.Regex("Title", new MongoDB.Bson.BsonRegularExpression(q, "i"));
                }
            }

            // Lọc theo CategoryId nếu có (chỉ áp dụng cho StoryEntity)
            if (typeof(T).Name == nameof(WebTruyenMVC.Entity.StoryEntity) && filter != null && !string.IsNullOrEmpty(filter.CategoryId))
            {
                filterDefinition &= builder.Eq("CategoryId", filter.CategoryId);
            }

            return filterDefinition;
        }

        public static FilterDefinition<AuthorEntity> BuildAuthorFilter(string q, Filter filter)
        {
            var builder = Builders<AuthorEntity>.Filter;
            var filterDefinition = builder.Empty;

            // Lọc theo Title nếu q có giá trị
            if (!string.IsNullOrEmpty(q))
            {
                filterDefinition &= builder.Regex("Title", new MongoDB.Bson.BsonRegularExpression(q, "i"));
            }

            return filterDefinition;
        }

        public static FilterDefinition<CategoryEntity> BuildCategoryFilter(string q, Filter filter)
        {
            var builder = Builders<CategoryEntity>.Filter;
            var filterDefinition = builder.Empty;

            // Lọc theo Title nếu q có giá trị
            if (!string.IsNullOrEmpty(q))
            {
                filterDefinition &= builder.Regex("Title", new MongoDB.Bson.BsonRegularExpression(q, "i"));
            }

            return filterDefinition;
        }
    }
}
