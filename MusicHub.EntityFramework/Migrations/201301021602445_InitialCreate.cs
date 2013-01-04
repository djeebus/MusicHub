namespace MusicHub.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Guid(nullable: false),
                        Username = c.String(nullable: false, maxLength: 100),
                        DisplayName = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.UserId);
            
            CreateTable(
                "dbo.Connections",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        SignalRConnectionId = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Libraries",
                c => new
                    {
                        LibraryId = c.Guid(nullable: false),
                        UserId = c.Guid(nullable: false),
                        TypeId = c.Byte(nullable: false),
                        Path = c.String(maxLength: 1024),
                        Username = c.String(maxLength: 1024),
                        Password = c.String(maxLength: 1024),
                        LastSync = c.DateTime(),
                    })
                .PrimaryKey(t => t.LibraryId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Songs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ExternalId = c.String(nullable: false, maxLength: 1024),
                        LibraryId = c.Guid(nullable: false),
                        Title = c.String(),
                        Artist = c.String(),
                        Album = c.String(),
                        LastSeen = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Libraries", t => t.LibraryId, cascadeDelete: true)
                .Index(t => t.LibraryId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Songs", new[] { "LibraryId" });
            DropIndex("dbo.Libraries", new[] { "UserId" });
            DropIndex("dbo.Connections", new[] { "UserId" });
            DropForeignKey("dbo.Songs", "LibraryId", "dbo.Libraries");
            DropForeignKey("dbo.Libraries", "UserId", "dbo.Users");
            DropForeignKey("dbo.Connections", "UserId", "dbo.Users");
            DropTable("dbo.Songs");
            DropTable("dbo.Libraries");
            DropTable("dbo.Connections");
            DropTable("dbo.Users");
        }
    }
}
