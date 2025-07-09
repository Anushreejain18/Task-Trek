namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class toresource : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Resources", "Class", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Resources", "Class");
        }
    }
}
