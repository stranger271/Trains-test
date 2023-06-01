using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 

namespace Trains.Models
{
    public class TrainsContext : IdentityDbContext<AppUser>
    { 

        public TrainsContext( ) : base( "TrainsDB") {}

        public static TrainsContext Create()
        {
            return new TrainsContext();
        }

        public  DbSet<Train> Trains { get; set; }
        public DbSet<Car> Cars { get; set; }

    }


    public class TrainsRepository : IDisposable, IRepository
    {
        private TrainsContext db;

        public TrainsRepository(TrainsContext context) 
        { 
            db = context; 
        }

        //инфа о поезде по его номеру
        //предпочитаю использовать хранимые процедуры вместо Fluent или Linq
        public async Task<List<TrainInfoFromSQL>> GetTrainInfoByNumAsync(int TrainNum)
        {
            return await db.Database.SqlQuery<TrainInfoFromSQL>("exec dbo.GetTrainInfoByNum {0}", TrainNum).ToListAsync();
        }
        //не используется в нотации Linq
        public List<TrainInfoFromSQL> GetTrainInfoByNum(int TrainNum)
        {
            var cars = (from train in db.Trains.Include(t => t.Cars)
                        where train.TrainNum == TrainNum
                        select train).ToList();

            return   new List<TrainInfoFromSQL>();
        }

        // Загрузить данные из файла в базу
        public async Task<bool> InsertTrainsInfoFromFile(List<Train> trains)
        {
            bool status = true;
            try
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    await db.BulkInsertAsync(trains, options => options.IncludeGraph = true);
                    transaction.Commit();
                }
            }
            catch {
                status = false;            
            }

            return status; 
        } 
        
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    //*********************************** Domain Models
    public class Train
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdTrain { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TrainNum { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int StructureNum { get; set; }

        [Required]
        public DateTime LastOperationDate { get; set; }

        [Required]
        [MaxLength(255)]
        public string EndStation { get; set; }

        [Required]
        [MaxLength(255)]
        public string StartStation { get; set; }

        [Required]
        [MaxLength(255)]
        public string CurrentStation { get; set; }

        public virtual List<Car> Cars { get; set; }

        public Train() { Cars = new List<Car>(); }

    }

    public class Car
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCar { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int PositionInTrain { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int CarNum { get; set; }

        [Required]
        [MaxLength(15)]
        public string InvoiceNum { get; set; }

        [Required]
        [MaxLength(255)]
        public string FreightName { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int FreightWeight { get; set; }

        [Required]
        [MaxLength(255)]
        public string LastOperationName { get; set; }

        [Required] 
        public DateTime LastOperationDate { get; set; }
        //FK_Cars_Trains

        public int IdTrain { get; set; }

        [ForeignKey("IdTrain")]
        public virtual Train Train { get; set; }
    }


    //**********************************  USER 


    public class AppUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<AppUser> manager, string authenticationType)
        {
     
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
             
            return userIdentity;
        }

        public AppUser() { }
    }
}