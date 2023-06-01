using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trains.Models
{
    public interface IRepository
    {
       Task<List<TrainInfoFromSQL>> GetTrainInfoByNumAsync(int TrainNum);
       Task<bool> InsertTrainsInfoFromFile(List<Train> trains);
       void Dispose();
    }

}