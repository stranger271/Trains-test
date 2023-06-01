using System;

namespace Trains.Models
{
    public class TrainInfo
    {
        public int TrainNumber { get; set; }
        public int TrainIndexCombined { get; set; }
        public string FromStationName { get; set; }
        public string ToStationName { get; set; }
        public string LastStationName { get; set; }
        public DateTime WhenLastOperation { get; set; }
        public string LastOperationName { get; set; }
        public string InvoiceNum { get; set; }
        public int PositionInTrain { get; set; }
        public int CarNumber { get; set; }
        public string FreightEtsngName { get; set; }
        public int FreightTotalWeightKg { get; set; }

    }

    public class TrainInfoFromSQL
    {
        public int RowNum { get; set; }
        public int TrainNum { get; set; }
        public int StructureNum { get; set; }
        public string CurrentStation { get; set; }
        public DateTime TrainLastOperationDate { get; set; }
        public int CarNum { get; set; }
        public string InvoiceNum { get; set; }
        public DateTime CarLastOperationDate { get; set; }
        public string FreightName { get; set; }
        public decimal FreightWeight { get; set; }
        public string LastOperationName { get; set; }


    }

}