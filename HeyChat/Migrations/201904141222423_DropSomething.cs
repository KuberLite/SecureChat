namespace HeyChat.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropSomething : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Conversations", "SenderId", "dbo.Users");
            DropIndex("dbo.Conversations", new[] { "SenderId" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.Conversations", "SenderId");
            AddForeignKey("dbo.Conversations", "SenderId", "dbo.Users", "Id", cascadeDelete: true);
        }
    }
}
