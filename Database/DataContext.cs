using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> dbContextOptions) : base(dbContextOptions)
        {
        }
    }
}
