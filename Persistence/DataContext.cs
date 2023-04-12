using Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
  public class DataContext : IdentityDbContext<AppUser>
  {
    public DataContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Activity> Activities { get; set; }
    public DbSet<ActivityAttendee> ActivityAttendees { get; set; }
    public DbSet<Photo> Photos { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<UserFollowing> UserFollowings { get; set; }

    // Manually create join table
    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);

      // Combine two keys together to form primary key for table
      builder.Entity<ActivityAttendee>(x => x.HasKey(aa => new { aa.AppUserId, aa.ActivityId }));

      // Create many - many relationship (Activity - AppUser)
      builder.Entity<ActivityAttendee>()
        .HasOne(u => u.AppUser)
        .WithMany(a => a.Activities)
        .HasForeignKey(aa => aa.AppUserId);

      builder.Entity<ActivityAttendee>()
        .HasOne(u => u.Activity)
        .WithMany(a => a.Attendees)
        .HasForeignKey(aa => aa.ActivityId);

      // When we delete activity, associated comments with activity will be cascaded
      builder.Entity<Comment>()
        .HasOne(a => a.Activity)
        .WithMany(c => c.Comments)
        .OnDelete(DeleteBehavior.Cascade);

      // Create many - many relationship for Followings (AppUser - AppUser)
      builder.Entity<UserFollowing>(b => 
      {
        b.HasKey(k => new {k.ObserverId, k.TargetId});

        b.HasOne(o => o.Observer)
          .WithMany(f => f.Followings)
          .HasForeignKey(o => o.ObserverId)
          .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(o => o.Target)
          .WithMany(f => f.Followers)
          .HasForeignKey(o => o.TargetId)
          .OnDelete(DeleteBehavior.Cascade);
      });
    }
  }
}