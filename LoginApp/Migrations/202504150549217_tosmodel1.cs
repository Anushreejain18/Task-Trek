namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tosmodel1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Students", "ProfileImage", c => c.String(maxLength: 255));
            AddColumn("dbo.Students", "LanguagePreference", c => c.String(maxLength: 50));
            AddColumn("dbo.Students", "NotificationPreference", c => c.String(maxLength: 50));
            AddColumn("dbo.Students", "ProfileVisibility", c => c.String(maxLength: 50));
            AddColumn("dbo.Students", "TaskReminders", c => c.Boolean(nullable: false));
            AddColumn("dbo.Students", "Theme", c => c.String(maxLength: 50));
            AddColumn("dbo.Students", "FontSize", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Students", "FontSize");
            DropColumn("dbo.Students", "Theme");
            DropColumn("dbo.Students", "TaskReminders");
            DropColumn("dbo.Students", "ProfileVisibility");
            DropColumn("dbo.Students", "NotificationPreference");
            DropColumn("dbo.Students", "LanguagePreference");
            DropColumn("dbo.Students", "ProfileImage");
        }
    }
}
