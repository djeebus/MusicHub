namespace MusicHub.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddAffinityTracking : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SongAffinities",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        SongId = c.Guid(nullable: false),
                        IsLove = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: false)
                .ForeignKey("dbo.Songs", t => t.SongId, cascadeDelete: false)
                .Index(t => t.UserId)
                .Index(t => t.SongId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.SongAffinities", new[] { "SongId" });
            DropIndex("dbo.SongAffinities", new[] { "UserId" });
            DropForeignKey("dbo.SongAffinities", "SongId", "dbo.Songs");
            DropForeignKey("dbo.SongAffinities", "UserId", "dbo.Users");
            DropTable("dbo.SongAffinities");
        }
    }
}
