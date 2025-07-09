namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResourcesTable1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Resources", "StudentComment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Resources", "StudentComment");
        }
    }
}
