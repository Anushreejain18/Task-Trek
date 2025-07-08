namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddResourcesTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Resources",
                c => new
                    {
                        ResourceId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Message = c.String(),
                        FilePath = c.String(),
                        UploadDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ResourceId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Resources");
        }
    }
}
