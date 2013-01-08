namespace MusicHub.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSongPlayCounts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Libraries", "LastPlayed", c => c.DateTime());
            AddColumn("dbo.Libraries", "PlayCount", c => c.Long(nullable: false));
            AddColumn("dbo.Songs", "LastPlayed", c => c.DateTime());
            AddColumn("dbo.Songs", "PlayCount", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Songs", "PlayCount");
            DropColumn("dbo.Songs", "LastPlayed");
            DropColumn("dbo.Libraries", "PlayCount");
            DropColumn("dbo.Libraries", "LastPlayed");
        }
    }
}
