namespace LoginApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initialcreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        Message = c.String(nullable: false, maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UserRole = c.String(nullable: false, maxLength: 50),
                        Username = c.String(maxLength: 100),
                        IsRead = c.Boolean(nullable: false),
                        TaskId = c.Int(),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.Tasks", t => t.TaskId)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.Tasks",
                c => new
                    {
                        TaskId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TaskId);
            
            CreateTable(
                "dbo.TaskRecords",
                c => new
                    {
                        TaskRecordId = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(maxLength: 500),
                        StudentName = c.String(nullable: false, maxLength: 50),
                        Class = c.String(nullable: false, maxLength: 20),
                        RollNumber = c.String(nullable: false, maxLength: 20),
                        StartDate = c.DateTime(nullable: false),
                        EndDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 20),
                        Reply = c.String(maxLength: 500),
                        TaskId = c.Int(nullable: false),
                        FilePath = c.String(maxLength: 300),
                    })
                .PrimaryKey(t => t.TaskRecordId)
                .ForeignKey("dbo.Tasks", t => t.TaskId, cascadeDelete: true)
                .Index(t => t.TaskId);
            
            CreateTable(
                "dbo.Students",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        RollNo = c.String(nullable: false, maxLength: 20),
                        Class = c.String(nullable: false, maxLength: 50),
                        Username = c.String(nullable: false, maxLength: 50),
                        Password = c.String(nullable: false, maxLength: 255),
                        Email = c.String(nullable: false),
                        Phone = c.String(nullable: false),
                        DateOfBirth = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "TaskId", "dbo.Tasks");
            DropForeignKey("dbo.TaskRecords", "TaskId", "dbo.Tasks");
            DropIndex("dbo.TaskRecords", new[] { "TaskId" });
            DropIndex("dbo.Notifications", new[] { "TaskId" });
            DropTable("dbo.Students");
            DropTable("dbo.TaskRecords");
            DropTable("dbo.Tasks");
            DropTable("dbo.Notifications");
        }
    }
}
