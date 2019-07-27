namespace iSpeak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addBranchesId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Branches_Id", c => c.Guid(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Branches_Id");
        }
    }
}
