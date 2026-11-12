using Application.Extentions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace DataAccess.Filter
{
    public static class FilterExtensions
    {
        private static readonly ConcurrentDictionary<string, PropertyInfo> PropertyCashe = new();
        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, Dictionary<string, string>? filters)
        {
            if (filters == null || !filters.Any()) return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            Expression? aggregateExpression = null;

            foreach(var filter in filters)
            {
                var property = typeof(T).GetProperty(filter.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                
                if (property==null) continue;

                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                Expression comparison;

                //if(property.PropertyType == typeof(string))
                //{
                //    var method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                //    var value = Expression.Constant(filter.Value, typeof(string));
                //    comparison = Expression.Call(propertyAccess, method!, value);
                //}
                if (property.PropertyType == typeof(string))
                {
                    var toLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
                    var contains = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

                    var lowerProperty = Expression.Call(propertyAccess, toLower);
                    var value = Expression.Constant(filter.Value.ToLower(), typeof(string));

                    comparison = Expression.Call(lowerProperty, contains, value);
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(property.PropertyType);
                    var convertedValue = converter.ConvertFromString(filter.Value); 
                    var constant = Expression.Constant(convertedValue, property.PropertyType);
                    comparison = Expression.Equal(propertyAccess, constant);
                }

                aggregateExpression = aggregateExpression == null 
                    ? comparison 
                    : Expression.AndAlso(aggregateExpression, comparison);
            }

            if(aggregateExpression == null) return query;

            var lambda = Expression.Lambda<Func<T, bool>>(aggregateExpression, parameter);

            return query.Where(lambda);
        }

        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortBy, bool isDescending = false)
        {
            if (string.IsNullOrWhiteSpace(sortBy)) return query;

            var type = typeof(T);
            var Caschekey = $"{typeof(T).FullName}_{sortBy}";

            if(!PropertyCashe.TryGetValue(Caschekey, out var property))
            {
                property = type.GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (property != null) 
                PropertyCashe[Caschekey] = property;
            }   

            if(property == null) return query;

            var parameter = Expression.Parameter(type, "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            string methodName = isDescending ? "OrderByDescending" : "OrderBy";

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { type, property.PropertyType },
                query.Expression,
                Expression.Quote(orderByExpression));

            return query.Provider.CreateQuery<T>(resultExpression);
        }

        public static async Task<PagedResult<T>> ToPagedListAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 50) pageSize = 50;

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
                return new PagedResult<T> { Items = new List<T>(), TotalCount = 0, PageNumber = pageNumber, PageSize = pageSize };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
