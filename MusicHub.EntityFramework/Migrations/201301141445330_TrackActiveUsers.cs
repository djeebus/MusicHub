namespace MusicHub.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TrackActiveUsers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "IsAvailable", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "IsAvailable");
        }
    }
}
