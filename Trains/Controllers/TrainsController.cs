
using ClosedXML.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using Trains.Models;


namespace Trains.Controllers
{
    [Authorize]

    public class TrainsController : ApiController
    {
        private IRepository _repo;
      
        public TrainsController( )
        {            
        }
       
        public TrainsController(IRepository repo)
        {
            _repo = repo;
        }           

        [Route("api/GetTrainInfoByNum")] 
        public async Task<HttpResponseMessage> GetTrainInfoByNum(string val )
        {
            int num;
            string format = "";
            List<TrainInfoFromSQL> result = new List<TrainInfoFromSQL>();

            try {
                string[] str = val.Split(',');
                num = Int32.Parse(str[0].Trim());
                format = str[1].Trim().ToLower();
                result = await _repo.GetTrainInfoByNumAsync(num);
            }
            catch { return new HttpResponseMessage(HttpStatusCode.BadRequest); }

            //немного не стандартное применение switch без break
            switch (format) {
                case "xlsx":

                    MemoryStream ms = ToExcel(result);

                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StreamContent(ms);
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.FileName = "Excel.xlsx";
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                    response.Content.Headers.ContentLength = ms.Length;

                    return response ;            
                default://json по дефолту

                    response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(JsonConvert.SerializeObject(result));
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");                    

                    return response;                              
            }      

        }

        private MemoryStream ToExcel(List<TrainInfoFromSQL> data) {

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Лист1");

            //шапка
            worksheet.Range("A1:G1").Row(1).Merge();  

            worksheet.Cell("A1").Value = "НАТУРНЫЙ ЛИСТ ПОЕЗДА";
            worksheet.Cell("A1").Style.Font.Bold = true;
            worksheet.Cell("A1").Style.Font.FontSize = 18;
            worksheet.Cell("A1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
     

            worksheet.Cell("A3").Value = "Поезд №:";
            worksheet.Cell("A3").Style.Font.Bold = true;
            worksheet.Cell("A4").Value = "Состав №:";
            worksheet.Cell("A4").Style.Font.Bold = true;
            worksheet.Cell("D3").Value = "Станция";
            worksheet.Cell("D3").Style.Font.Bold = true;
            worksheet.Cell("D4").Value = "Дата";
            worksheet.Cell("D4").Style.Font.Bold = true;

            worksheet.Cell("A6").Value = "№";
            worksheet.Cell("B6").Value = "№ вагона";
            worksheet.Cell("C6").Value = "Накладная";
            worksheet.Cell("D6").Value = "Дата отправления";
            worksheet.Cell("E6").Value = "Груз";
            worksheet.Cell("F6").Value = "Вес по документам (т)";
            worksheet.Cell("G6").Value = "Последняя операция";
            worksheet.Range("A6:G6").Style.Font.Bold = true;
            worksheet.Range("A6:G6").Style.Alignment.SetVertical (XLAlignmentVerticalValues.Center);


            worksheet.Column(1).Width = 4;
            worksheet.Column(2).Width = 10;
            worksheet.Column(3).Width = 12;
            worksheet.Column(4).Width = 11;
            worksheet.Column(5).Width = 15;
            worksheet.Column(6).Width = 10;
            worksheet.Column(7).Width = 52;
            worksheet.Cell("F6").Style.Alignment.SetTextRotation(90);

            worksheet.Row(6).Height = 87;

            int linesCount = data.Count();

            //границы
            worksheet.Range("A6:G" + (linesCount + 6).ToString() ).Style
                .Border.SetTopBorder(XLBorderStyleValues.Medium)
                .Border.SetRightBorder(XLBorderStyleValues.Medium)
                .Border.SetBottomBorder(XLBorderStyleValues.Medium)
                .Border.SetLeftBorder(XLBorderStyleValues.Medium)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
 

            //данные шапки

            worksheet.Cell("C3").Value = data[0].TrainNum;
            worksheet.Cell("C3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell("C4").Value = data[0].StructureNum;
            worksheet.Cell("C4").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell("E3").Value = data[0].CurrentStation;
            worksheet.Cell("E3").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
            worksheet.Cell("E4").Value = data[0].TrainLastOperationDate.ToString("d");
            worksheet.Cell("E4").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);

            //строки

            for (int i = 0, line = 7, itog = 0; i < linesCount; i++) {

                if (data[i].RowNum != 0)
                {
                    worksheet.Cell("A" + line).Value = data[i].RowNum;
                    worksheet.Cell("B" + line).Value = data[i].CarNum;
                    worksheet.Cell("C" + line).Value = data[i].InvoiceNum;
                    worksheet.Cell("D" + line).Value = data[i].CarLastOperationDate.ToString("d");
                    worksheet.Cell("E" + line).Value = data[i].FreightName;
                    worksheet.Cell("F" + line).Value = data[i].FreightWeight;
                    worksheet.Cell("G" + line).Value = data[i].LastOperationName;
                }
                else {
                    if (i < linesCount - 1)
                    {
                        if (itog == 0) { itog = line; }
                        worksheet.Cell("B" + line).Value = data[i].CarNum;
                        worksheet.Cell("B" + line).Style.Font.Bold = true;
                        worksheet.Cell("E" + line).Value = data[i].FreightName;
                        worksheet.Cell("E" + line).Style.Font.Bold = true;
                        worksheet.Cell("F" + line).Value = data[i].FreightWeight;
                        worksheet.Cell("F" + line).Style.Font.Bold = true;
                    }
                    else {

                        worksheet.Range("A" + line + ":B" + line).Row(1).Merge();
                        worksheet.Cell("A" + line).Value ="Всего: " + data[i].CarNum.ToString();
                        worksheet.Cell("A" + line).Style.Font.Bold = true;
                        worksheet.Cell("E" + line).Value = line - itog;
                        worksheet.Cell("E" + line).Style.Font.Bold = true;
                        worksheet.Cell("F" + line).Value = data[i].FreightWeight;
                        worksheet.Cell("F" + line).Style.Font.Bold = true;
                    }
                   
                }

                line++ ;
            }

            MemoryStream stream = new  MemoryStream();            
            workbook.SaveAs(stream);
            stream.Position = 0;      

            return stream;              
        } 


        //Загрузка файла в базу
        [Route("api/UploadDataFile")]
        public async Task<IHttpActionResult>  UploadDataFile()
        {            

            if (!Request.Content.IsMimeMultipartContent())
            {
                return StatusCode(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new  MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            string content = "";
            using (StreamReader reader = new StreamReader(await provider.Contents[0].ReadAsStreamAsync(), Encoding.UTF8))
            {
                content = reader.ReadToEnd();
            } 

            XmlDocument xDoc = new XmlDocument(); 
             
            xDoc.LoadXml(content);

            XmlElement root = xDoc.DocumentElement;
            if (root != null)
            {
                List<Train> Trains = new List<Train>();

                foreach (XmlElement row in root)
                {
                    var item = new TrainInfo();     

                    foreach (XmlNode child in row.ChildNodes)
                    {
                        
                        switch (child.Name)
                        {

                            case "TrainNumber":
                                item.TrainNumber = Int32.Parse(child.InnerText);
                                break;

                            case "TrainIndexCombined":
                                item.TrainIndexCombined = SetTrainIndexCombined(child.InnerText);
                                break;

                            case "FromStationName":
                                item.FromStationName = child.InnerText;
                                break;

                            case "ToStationName":
                                item.ToStationName = child.InnerText;
                                break;

                            case "LastStationName":
                                item.LastStationName = child.InnerText;
                                break;

                            case "WhenLastOperation":
                                item.WhenLastOperation = DateTime.Parse(child.InnerText);
                                break;

                            case "LastOperationName":
                                item.LastOperationName = child.InnerText;
                                break;

                            case "InvoiceNum":
                                item.InvoiceNum = child.InnerText;
                                break;

                            case "PositionInTrain":
                                item.PositionInTrain = Int32.Parse(child.InnerText);
                                break;

                            case "CarNumber":
                                item.CarNumber = Int32.Parse(child.InnerText);
                                break;

                            case "FreightEtsngName":
                                item.FreightEtsngName = child.InnerText;
                                break;

                            case "FreightTotalWeightKg":
                                item.FreightTotalWeightKg = Int32.Parse(child.InnerText);
                                break;
                        }//switch
                    }//row

                    Train train = Trains.Find(t => t.TrainNum == item.TrainNumber);
                    if (train == null) {
                        train = new Train() { 
                            TrainNum = item.TrainNumber,
                            StructureNum = item.TrainIndexCombined,
                            StartStation = item.FromStationName,
                            EndStation = item.ToStationName,
                            CurrentStation = item.LastStationName,
                            LastOperationDate = item.WhenLastOperation
                        };                        
                  
                        Trains.Add(train);
                    }
                    // это условие нужно потому что кто то косячно выгрузил файл Data !!!
                    Car car = train.Cars.Find(c => c.CarNum == item.CarNumber);
                    if (car == null) {
                        train.Cars.Add(new Car()
                        {
                            CarNum = item.CarNumber,
                            PositionInTrain = item.PositionInTrain,
                            LastOperationName = item.LastOperationName,
                            LastOperationDate = item.WhenLastOperation,
                            FreightName = item.FreightEtsngName,
                            FreightWeight = item.FreightTotalWeightKg,
                            InvoiceNum = item.InvoiceNum
                        });
                    }

                }//root

                if ( await _repo.InsertTrainsInfoFromFile(Trains) == false ) { return StatusCode(HttpStatusCode.Forbidden); }
  

            }//if root
 
            return StatusCode(HttpStatusCode.OK);       
        }


        //**************************************  
        #region Вспомогательные функции и утилиты

        private int SetTrainIndexCombined(string val)
        {
            string[] str = val.Split('-');
            return Int32.Parse(str[1]);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_repo != null) {
                    _repo.Dispose();
                    _repo = null;
                }
               
            }

            base.Dispose(disposing);
        }
 

    }
        #endregion
}
