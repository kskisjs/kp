using BelarusQuiz.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace BelarusQuiz.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
}