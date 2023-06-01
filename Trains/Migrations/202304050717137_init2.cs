namespace Trains.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Cars",
                c => new
                    {
                        IdCar = c.Int(nullable: false, identity: true),
                        PositionInTrain = c.Int(nullable: false),
                        CarNum = c.Int(nullable: false),
                        InvoiceNum = c.String(nullable: false, maxLength: 15),
                        FreightName = c.String(nullable: false, maxLength: 255),
                        FreightWeight = c.Int(nullable: false),
                        LastOperationName = c.String(nullable: false, maxLength: 255),
                        LastOperationDate = c.DateTime(nullable: false),
                        IdTrain = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.IdCar)
                .ForeignKey("dbo.Trains", t => t.IdTrain, cascadeDelete: true)
                .Index(t => t.IdTrain);
            
            CreateTable(
                "dbo.Trains",
                c => new
                    {
                        IdTrain = c.Int(nullable: false, identity: true),
                        TrainNum = c.Int(nullable: false),
                        StructureNum = c.Int(nullable: false),
                        LastOperationDate = c.DateTime(nullable: false),
                        EndStation = c.String(nullable: false, maxLength: 255),
                        StartStation = c.String(nullable: false, maxLength: 255),
                        CurrentStation = c.String(nullable: false, maxLength: 255),
                    })
                .PrimaryKey(t => t.IdTrain);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Cars", "IdTrain", "dbo.Trains");
            DropIndex("dbo.Cars", new[] { "IdTrain" });
            DropTable("dbo.Trains");
            DropTable("dbo.Cars");
        }
    }
}
