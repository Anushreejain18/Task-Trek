namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class todt : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.Teachers");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Teachers",
                c => new
                    {
                        TeacherId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Subject = c.String(nullable: false, maxLength: 100),
                        Username = c.String(nullable: false, maxLength: 50),
                        Password = c.String(nullable: false, maxLength: 255),
                        Branch = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.TeacherId);
            
        }
    }
}
