namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResourcesTable2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Resources", "StudentId", c => c.Int());
            AlterColumn("dbo.Resources", "Title", c => c.String());
            CreateIndex("dbo.Resources", "StudentId");
            AddForeignKey("dbo.Resources", "StudentId", "dbo.Students", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Resources", "StudentId", "dbo.Students");
            DropIndex("dbo.Resources", new[] { "StudentId" });
            AlterColumn("dbo.Resources", "Title", c => c.String(nullable: false));
            DropColumn("dbo.Resources", "StudentId");
        }
    }
}
