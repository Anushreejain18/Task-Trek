namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tosmodel2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Students", "ProfileImagePath", c => c.String());
            DropColumn("dbo.Students", "ProfileImage");
            DropColumn("dbo.Students", "LanguagePreference");
            DropColumn("dbo.Students", "NotificationPreference");
            DropColumn("dbo.Students", "ProfileVisibility");
            DropColumn("dbo.Students", "TaskReminders");
            DropColumn("dbo.Students", "Theme");
            DropColumn("dbo.Students", "FontSize");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Students", "FontSize", c => c.Int(nullable: false));
            AddColumn("dbo.Students", "Theme", c => c.String(maxLength: 50));
            AddColumn("dbo.Students", "TaskReminders", c => c.Boolean(nullable: false));
            AddColumn("dbo.Students", "ProfileVisibility", c => c.String(maxLength: 50));
            AddColumn("dbo.Students", "NotificationPreference", c => c.String(maxLength: 50));
            AddColumn("dbo.Students", "LanguagePreference", c => c.String(maxLength: 50));
            AddColumn("dbo.Students", "ProfileImage", c => c.String(maxLength: 255));
            DropColumn("dbo.Students", "ProfileImagePath");
        }
    }
}
