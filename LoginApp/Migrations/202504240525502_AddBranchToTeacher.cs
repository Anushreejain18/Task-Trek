namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBranchToTeacher : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teachers", "Branch", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Teachers", "Name", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Teachers", "Subject", c => c.String(nullable: false, maxLength: 100));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Teachers", "Subject", c => c.String(nullable: false));
            AlterColumn("dbo.Teachers", "Name", c => c.String(nullable: false));
            DropColumn("dbo.Teachers", "Branch");
        }
    }
}
