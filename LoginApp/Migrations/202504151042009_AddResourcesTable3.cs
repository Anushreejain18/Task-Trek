namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResourcesTable3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Resources", "UploadedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Resources", "UploadedBy");
        }
    }
}
