namespace iSpeak.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPromoInterest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PromotionEvents_Id", c => c.Guid());
            AddColumn("dbo.AspNetUsers", "Interest", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Interest");
            DropColumn("dbo.AspNetUsers", "PromotionEvents_Id");
        }
    }
}
