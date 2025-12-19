using efscaffold.Entities;
using Microsoft.EntityFrameworkCore;

namespace efscaffold;

public partial class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Board> Boards { get; set; }

    public virtual DbSet<Game> Games { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<Winningboard> Winningboards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.AdminId).HasName("admin_pkey");

            entity.ToTable("admin", "deadpigeons");

            entity.HasIndex(e => e.Email, "admin_email_key").IsUnique();

            entity.Property(e => e.AdminId)
                .ValueGeneratedNever()
                .HasColumnName("admin_id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
        });

        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(e => e.BoardId).HasName("board_pkey");

            entity.ToTable("board", "deadpigeons");

            entity.Property(e => e.BoardId)
                .ValueGeneratedNever()
                .HasColumnName("board_id");
            entity.Property(e => e.ChosenNumbers).HasColumnName("chosen_numbers");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.IsRepeating)
                .HasDefaultValue(false)
                .HasColumnName("is_repeating");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.RepeatUntilGameId).HasColumnName("repeat_until_game_id");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("now()")
                .HasColumnName("timestamp");

            entity.HasOne(d => d.Game).WithMany(p => p.BoardGames)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_board_game");

            entity.HasOne(d => d.Player).WithMany(p => p.Boards)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_board_player");

            entity.HasOne(d => d.RepeatUntilGame).WithMany(p => p.BoardRepeatUntilGames)
                .HasForeignKey(d => d.RepeatUntilGameId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_board_repeat_game");
        });

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.GameId).HasName("game_pkey");

            entity.ToTable("game", "deadpigeons");

            entity.Property(e => e.GameId)
                .ValueGeneratedNever()
                .HasColumnName("game_id");
            entity.Property(e => e.DrawDate).HasColumnName("draw_date");
            entity.Property(e => e.ExpirationDate).HasColumnName("expiration_date");
            entity.Property(e => e.WinningNumbers).HasColumnName("winning_numbers");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId).HasName("player_pkey");

            entity.ToTable("player", "deadpigeons");

            entity.HasIndex(e => e.Email, "player_email_key").IsUnique();

            entity.Property(e => e.PlayerId)
                .ValueGeneratedNever()
                .HasColumnName("player_id");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber).HasColumnName("phone_number");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("transaction_pkey");

            entity.ToTable("transaction", "deadpigeons");

            entity.HasIndex(e => e.MobilepayReqId, "transaction_mobilepay_req_id_key").IsUnique();

            entity.Property(e => e.TransactionId)
                .ValueGeneratedNever()
                .HasColumnName("transaction_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.MobilepayReqId).HasColumnName("mobilepay_req_id");
            entity.Property(e => e.PlayerId).HasColumnName("player_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("now()")
                .HasColumnName("timestamp");

            entity.HasOne(d => d.Player).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_transaction_player");
        });

        modelBuilder.Entity<Winningboard>(entity =>
        {
            entity.HasKey(e => e.WinningboardId).HasName("winningboard_pkey");

            entity.ToTable("winningboard", "deadpigeons");

            entity.Property(e => e.WinningboardId)
                .ValueGeneratedNever()
                .HasColumnName("winningboard_id");
            entity.Property(e => e.BoardId).HasColumnName("board_id");
            entity.Property(e => e.GameId).HasColumnName("game_id");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("now()")
                .HasColumnName("timestamp");
            entity.Property(e => e.WinningNumbersMatched).HasColumnName("winning_numbers_matched");

            entity.HasOne(d => d.Board).WithMany(p => p.Winningboards)
                .HasForeignKey(d => d.BoardId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_wb_board");

            entity.HasOne(d => d.Game).WithMany(p => p.Winningboards)
                .HasForeignKey(d => d.GameId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_wb_game");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
