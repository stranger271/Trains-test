using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Text.Json;

namespace TestClient
{
    class Program
    {

        private const string APP_PATH = "http://localhost:52923/";
        private static string token;

        public class TrainInfoFromSQL
        {
            public int RowNum { get; set; }
            public int TrainNumber { get; set; }
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

        [STAThread]
        static void Main(string[] args)
        {
            bool exit = false;
            string login;
            string password;

            while (exit == false) {
                Console.WriteLine("Выберите команду, наберите нужную цифру:");
                Console.WriteLine( );
                Console.WriteLine("1) Регистрация нового пользователя");
                Console.WriteLine("2) Вход с логином и паролем");
                Console.WriteLine("3) Закрыть программу");
                Console.WriteLine();

                switch (Console.ReadLine()) {
                    case "1"://новый пользователь 

                        Console.WriteLine("Введите логин:");
                        login = Console.ReadLine();

                        Console.WriteLine("Введите пароль:");
                        password = Console.ReadLine();

                        Console.WriteLine("Результат: " + Registration(login,password).ToString() );      

                        break;
                    case "2"://вход
                        Console.WriteLine("Введите логин:");
                        login =  Console.ReadLine();

                        Console.WriteLine("Введите пароль:");
                        password =  Console.ReadLine(); 

                        if (SignIn(login, password) == HttpStatusCode.OK)
                        {

                            Console.WriteLine();
                            Console.WriteLine("Ваш токен доступа: \n" + token);
                            Console.WriteLine("------------------");
                            Console.WriteLine();
                            AccessAllowed();
                        }
                        else {
                            Console.WriteLine();
                            Console.WriteLine("Результат: неверный логин или пароль, попробуйте еще раз");
                        }  

                        break;
                    case "3":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Команда не распознана, попробуйте еще раз");
                        break;

                }

                Console.WriteLine("------------------");
                Console.WriteLine();

                //пользователь вошел
                void AccessAllowed() {

                    while (exit == false) {
                        Console.WriteLine("Выберите действие:");
                        Console.WriteLine();
                        Console.WriteLine("1) Загрузить файл XML в базу");
                        Console.WriteLine("2) Получить натуральный лист по номеру поезда");
                        Console.WriteLine("3) Закрыть программу");

                        switch (Console.ReadLine())
                        {
                            case "1"://загружаем файл в базу

                                OpenFileDialog dialog = new OpenFileDialog();
                                dialog.Filter = "XML-файлы | *.xml";

                                if (DialogResult.OK == dialog.ShowDialog())
                                {
                                    Console.WriteLine();

                                    HttpStatusCode status = SendData(dialog.FileName);
                                    if ( status == HttpStatusCode.OK)
                                    {
                                        Console.WriteLine("Данные успешно загружены!");
                                    }
                                    else {
                                        Console.WriteLine("Что-то пошло не так:" + status.ToString() );
                                    }                
                                }

                                break;
                            case "2":// получаем отчет
                                Console.WriteLine();
                                Console.WriteLine("Введите номер поезда и формат (xlsx, json) отчета, например (2236, xlsx)");

                                Console.WriteLine();
                                GetInfo(Console.ReadLine());                                

                                break;
                            case "3":
                                exit = true;
                                break;
                            default:
                                Console.WriteLine("Команда не распознана, попробуйте еще раз");
                                break;

                        }//switch

                        Console.WriteLine("------------------");
                        Console.WriteLine();


                    }//while

                }//AcccessAllowed


            }

            Logout();           
        }

        // регистрация
        static HttpStatusCode Registration(string email, string password)
        {
            var registerModel = new
            {
                Email = email,
                Password = password     
            };
            using (var client = new HttpClient())
            {
                var response = client.PostAsJsonAsync(APP_PATH + "/api/Account/Register", registerModel).Result;
                return response.StatusCode;
            }
        }

        // Входи и получение токена
        static HttpStatusCode SignIn(string userName, string password)
        {
            var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>( "grant_type", "password" ),
                    new KeyValuePair<string, string>( "username", userName ),
                    new KeyValuePair<string, string> ( "Password", password )
                };
            var content = new FormUrlEncodedContent(pairs);

            using (var client = new HttpClient())
            {
                var response = client.PostAsync(APP_PATH + "/Token", content).Result;
                var result = response.Content.ReadAsStringAsync().Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Dictionary<string, string> tokenDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);
                    token = tokenDictionary["access_token"];
                }               

                return response.StatusCode;
            }
        }


        //отправка файла для загрузки в бд
        static HttpStatusCode SendData(string filePath)
        {                                 
            var multipartFormContent = new MultipartFormDataContent();
            var fileStreamContent = new StreamContent(File.OpenRead(filePath));           
            fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
            multipartFormContent.Add(fileStreamContent, name: "file", fileName: "Data.xml");

            using (var client = CreateClient(token))
            {                
                var response =  client.PostAsync(APP_PATH + "/api/UploadDataFile", multipartFormContent).Result;
                return response.StatusCode;
            }
       
        }

        // Вспомогательная функция добавления токена к каждому запросу
        static HttpClient CreateClient(string accessToken = "")
        {
            var client = new HttpClient();
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
            return client;
        }


        // обращаемся по маршруту api/values 
        static async Task GetInfo(string val)
        {

            string format= "";
            
            try
            {
                string[] str = val.Split(',');                
                format = str[1].Trim().ToLower();                 
            }
            catch { 
                Console.WriteLine("Команда не распознана, попробуйте еще раз");
                return;
            }
           


            using (var client = CreateClient(token))
            {               

                StringContent content = new StringContent(val);  
                //отправка через адресную строку, но можно и через добавление в тело запроса. Ответ получаю по второму варианту
                
                var req = new HttpRequestMessage(HttpMethod.Get, APP_PATH + "api/GetTrainInfoByNum/?val=" + val);

                var response = client.SendAsync(req).Result;

                if (response.StatusCode == HttpStatusCode.OK) {

                    if (format == "xlsx") { 
                        SaveFileDialog saveDlg = new SaveFileDialog();
                        saveDlg.FileName = response.Content.Headers.ContentDisposition.FileName;
                        saveDlg.Filter = "Excel-файл | *.xlsx";

                        var stream = await response.Content.ReadAsStreamAsync();                    

                        if (saveDlg.ShowDialog() == DialogResult.OK)
                        {
                            using (FileStream fstream = new FileStream(saveDlg.FileName, FileMode.Create))
                            {
                                MemoryStream ms = (MemoryStream)stream;
                                byte[] buffer = ms.ToArray();
                            
                                fstream.Write(buffer, 0, buffer.Length);
                            }
                            Console.WriteLine("Получен XLSX и сохранен");
                            return ;

                        }
                        Console.WriteLine("Получен XLSX, но Вы отказались его сохранить");
                        return ;
                    
                    }

                   var data = await response.Content.ReadAsStringAsync(); 
                    
                    Console.WriteLine(data);
                    Console.WriteLine("Получен JSON");
                    return ; //JSON
                }

                Console.WriteLine("Не удалось получить отчет: " + response.StatusCode.ToString());
                return;
            }
        }

        //отправка файла для загрузки в бд
        static void Logout()
        {
            using (var client = CreateClient(token))
            {
                var req = new HttpRequestMessage(HttpMethod.Post, APP_PATH + "/api/Logout");
                var response = client.SendAsync(req);         
            }

        }
    }
}
