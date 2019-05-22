namespace iSpeak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class customUserProperty : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Firstname", c => c.String(nullable: false));
            AddColumn("dbo.AspNetUsers", "Middlename", c => c.String());
            AddColumn("dbo.AspNetUsers", "Lastname", c => c.String());
            AddColumn("dbo.AspNetUsers", "Address", c => c.String());
            AddColumn("dbo.AspNetUsers", "Phone1", c => c.String());
            AddColumn("dbo.AspNetUsers", "Phone2", c => c.String());
            AddColumn("dbo.AspNetUsers", "Birthday", c => c.DateTime(nullable: false));
            AddColumn("dbo.AspNetUsers", "Notes", c => c.String());
            AddColumn("dbo.AspNetUsers", "Active", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Active");
            DropColumn("dbo.AspNetUsers", "Notes");
            DropColumn("dbo.AspNetUsers", "Birthday");
            DropColumn("dbo.AspNetUsers", "Phone2");
            DropColumn("dbo.AspNetUsers", "Phone1");
            DropColumn("dbo.AspNetUsers", "Address");
            DropColumn("dbo.AspNetUsers", "Lastname");
            DropColumn("dbo.AspNetUsers", "Middlename");
            DropColumn("dbo.AspNetUsers", "Firstname");
        }
    }
}
