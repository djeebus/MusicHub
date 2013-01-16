namespace MusicHub.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrackErrors : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Libraries", "ErrorMessage", c => c.String());
            AddColumn("dbo.Libraries", "ErrorsSinceLastSong", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Libraries", "ErrorsSinceLastSong");
            DropColumn("dbo.Libraries", "ErrorMessage");
        }
    }
}
