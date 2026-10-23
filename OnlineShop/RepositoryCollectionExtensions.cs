using DataAccess.Repositories;

namespace OnlineShop
{
    public static class RepositoryCollectionExtensions 
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            return services;
        }
    }
}
