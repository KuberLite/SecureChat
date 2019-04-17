namespace HeyChat.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddSecretKeyColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Conversations", "SecretKey", c => c.String());
            DropColumn("dbo.Users", "SecretKey");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "SecretKey", c => c.String());
            DropColumn("dbo.Conversations", "SecretKey");
        }
    }
}
