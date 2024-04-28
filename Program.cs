// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
/*using System.Data.Sqlite;*/
//using System.Data.SQLite;
using Npgsql;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
//using System.Data.SqlClient;

// git add .
// git commit --amend
// ctrl + x
// git push origin +main



class Program
{
    public class ConfigDevice
    {
        public int OwnerID { get; set; }
        public int ParamNO { get; set; }
        public int NewData { get; set; }
    }

    public class DeletDevice
    {
        public int OwnerID { get; set; }
    }

    public class SetMoney
    {
        public int OwnerID { get; set; }
        public int Money { get; set; }  
        public int Reserv { get; set; }
    }

    //public static SQLiteConnection sqlConnection = null;
    //public static NpgsqlConnection? sqlConnection = null;
    //string cS = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Hayk\Desktop\MonitoringServer\MonitoringServer\MoikaData.mdf;Integrated Security=True";
    // string cS = @"";

    /*public static SQLiteCommand sqlCommand = null;*/
    //public static NpgsqlCommand? sqlCommand = null;

    /*public static SQLiteDataReader sqlDataReader = null;*/
    //public static NpgsqlDataReader? sqlDataReader = null;

    public static string? connString=null;

    static void Main(string[] args)
    {

        using IHost host = Host.CreateApplicationBuilder(args).Build();

        // Ask the service provider for the configuration abstraction.
        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

       
        connString = config.GetValue<string>("ConnectionStrings:PgDbConnection");
        Console.WriteLine(connString);
        /*sqlConnection = new NpgsqlConnection(connString);
        sqlConnection.Open();*/

        _ = ConfigDataBasue();

        _ = Task.Run(() => StartTcpListener());

        _ = Task.Run(() => StartHttpListener());
        
        Console.WriteLine("Press Enter to exit.");
        while (true)
        {
            string? tempComm = Console.ReadLine();
            if (tempComm == "OFF")
            {
                break;
            }
        }
        //sqlConnection.Close();
    }
    static async Task StartTcpListener()
    {
        var listener = new TcpListener(IPAddress.Any, 1234);
        listener.Start();
        Console.WriteLine("Server started. Waiting for clients....");
        while (true)
        {
            var client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected.");

            //Console.WriteLine(localDate.ToString());

            // Start a new task to handle the client connection
            _ = HandleClientAsync(client);
        }
    }

    static void StartHttpListener()
    {
        // Set HTTP listener IP and port
        /*string ipAddress = "127.0.0.1";*/
        //string ipAddress = ;
        int port = 2000;

        HttpListener httpListener = new HttpListener();
        //httpListener.Prefixes.Add($"http://{ipAddress}:{port}/");
        /* httpListener.Prefixes.Add($"http://+:{port}/");*/

        httpListener.Prefixes.Add($"http://*:{port}/");
        /*  httpListener.Prefixes.Add($"
      //  https ://hard-server-0e43d0480fed.herokuapp.com");*/

        httpListener.Start();
        //httpListener.Prefixes.Add($"http://192.168.1.106:8181/");
        // DisplayPrefixesAndState(httpListener);

        Console.WriteLine("HTTP listener started. Waiting for requests...");

        while (true)
        {
            HttpListenerContext context = httpListener.GetContext();
            ProcessHttpRequest(context);
        }
    }
    public static void DisplayPrefixesAndState(HttpListener listener)
    {
        // List the prefixes to which the server listens.
        HttpListenerPrefixCollection prefixes = listener.Prefixes;
        if (prefixes.Count == 0)
        {
            Console.WriteLine("There are no prefixes.");
        }
        foreach (string prefix in prefixes)
        {
            Console.WriteLine(prefix);
        }
        // Show the listening state.
        if (listener.IsListening)
        {
            Console.WriteLine("The server is listening.");
        }
    }
    static async void ProcessHttpRequest(HttpListenerContext context)
    {
        // Handle HTTP request here
        // Example: Get the request URL and send a response

        string? requestUrl = null;
        //char[]? UrlArray = null;
        string? requestPath = null;
        if (context.Request.Url != null)
        {
            requestUrl = context.Request.Url.ToString();
            //UrlArray = context.Request.Url.PathAndQuery.ToArray();
            requestPath = context.Request.Url.AbsolutePath;
        }
        /*Console.WriteLine($"Received HTTP request: {requestUrl}");*/

        //string[] temp = context.Request.QueryString.AllKeys.GetValue();
        /*      for (int i = 0; i < temp.Length; i++)
              {
                  Console.WriteLine(temp[i]);
              }*/


        if (requestUrl != null && context.Request.Url != null)
        {
            if (context.Request.HttpMethod == "GET")
            {
                //Console.WriteLine(context.Request.QueryString.AllKeys + "-----");
                if (requestUrl.EndsWith("/api/v1/devices"))
                {
                    try
                    {
                        //Console.WriteLine(requestUrl);
                        string? jsonResponse = await SendCommandAll();
                        URL_JsonResponse200(context, jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else if (requestUrl.EndsWith("/api/v1/config"))
                {
                    try
                    {
                        //Console.WriteLine(requestUrl);
                        string? jsonResponse = await SendConfigTable();
                        URL_JsonResponse200(context, jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else if (requestPath == ("/api/v1/devices/"))
                {
                    string? id = context.Request.QueryString["id"];
                    try
                    {
                        uint SendOwnerID = 0;
                        if (id != null)
                        {
                            SendOwnerID = uint.Parse(id);
                        }

                        //Console.WriteLine(requestUrl);
                        string? jsonResponse = await SendOunerData(SendOwnerID);
                        URL_JsonResponse200(context, jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else if (requestPath == ("/api/v1/config/"))
                {
                    string? id = context.Request.QueryString["id"];
                    try
                    {
                        uint SendOwnerID = 0;
                        if (id != null)
                        {
                            SendOwnerID = uint.Parse(id);
                        }

                        string? jsonResponse = null;
                        //Console.WriteLine(requestUrl);
                        if(SendOwnerID>9999999 && SendOwnerID < 100000000)
                        {
                            jsonResponse = await SendOunerConfig(SendOwnerID);
                        }
                        URL_JsonResponse200(context, jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);  
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else if (requestPath == ("/api/v1/Owner/"))
                {
                    string? id = context.Request.QueryString["id"];
                    try
                    {
                        uint SendOwnerID = 0;
                        if (id != null)
                        {
                            SendOwnerID = uint.Parse(id);
                        }

                        //Console.WriteLine(requestUrl);
                        string? jsonResponse = await SendAllOuners(SendOwnerID);
                        URL_JsonResponse200(context, jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else if (requestUrl.EndsWith("/api/v1/money"))
                {
                    try
                    {
                        //Console.WriteLine(requestUrl);
                        string? jsonResponse = await SendCommandAllMoney();
                        URL_JsonResponse200(context, jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else if (requestPath == ("/api/v1/money/"))
                {
                    string? id = context.Request.QueryString["id"];
                    try
                    {
                        uint SendOwnerID = 0;
                        if (id != null)
                        {
                            SendOwnerID = uint.Parse(id);
                        }

                        //Console.WriteLine(requestUrl);
                        string? jsonResponse = await SendDeviceMoney(SendOwnerID);
                        URL_JsonResponse200(context, jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);   
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else if (requestUrl.EndsWith("/api/v1/reserv"))
                {
                    try
                    {
                        //Console.WriteLine(requestUrl);
                        string? jsonResponse = await SendCommandAllReserv();
                        URL_JsonResponse200(context, jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else if (requestPath == ("/api/v1/reserv/"))
                {
                    string? id = context.Request.QueryString["id"];
                    try
                    {
                        uint SendOwnerID = 0;
                        if (id != null)
                        {
                            SendOwnerID = uint.Parse(id);
                        }

                        //Console.WriteLine(requestUrl);
                        string? jsonResponse = await SendDeviceReserv(SendOwnerID);
                        URL_JsonResponse200(context, jsonResponse);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);
                    }
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else
                {
                    Console.WriteLine(context.Request.Url.PathAndQuery.ToString());
                    context.Response.StatusCode = 404;
                    context.Response.OutputStream.Close();
                }
            }
            else if (context.Request.HttpMethod == "POST")
            {
                if (requestUrl.EndsWith("/api/v1/devie/edit"))
                {
                    try
                    {
                        // Read the request body
                        string requestBody;
                        using (StreamReader reader = new StreamReader(context.Request.InputStream))
                        {
                            requestBody = reader.ReadToEnd();
                        }

                        var configDevice = JsonConvert.DeserializeObject<ConfigDevice>(requestBody);

                        string? SaveConfigParamState = null;
                        if (configDevice != null)
                        {
                            SaveConfigParamState = await SaveConfigParam(configDevice);
                        }

                        URL_PostResponse200(context, SaveConfigParamState);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex); 
                    }
                    finally
                    {
                        context.Response.Close();
                    }
                }
                else if (requestUrl.EndsWith("/api/v1/device/destroy"))
                {
                    try
                    {
                        // Read the request body
                        string requestBody;
                        using (StreamReader reader = new StreamReader(context.Request.InputStream))
                        {
                            requestBody = reader.ReadToEnd();
                        }

                        DeletDevice[]? deletDevices = JsonConvert.DeserializeObject<DeletDevice[]>(requestBody);
                        if (deletDevices != null)
                        {
                            await Deleting(deletDevices);
                        }

                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);
                    }
                    finally
                    {
                        context.Response.Close();
                    }
                }
                else if (requestUrl.EndsWith("/api/v1/device/reserv"))
                {
                    try
                    {
                        // Read the request body
                        string requestBody;
                        using (StreamReader reader = new StreamReader(context.Request.InputStream))
                        {
                            requestBody = reader.ReadToEnd();
                        }

                        var SetDeviceMoney = JsonConvert.DeserializeObject<SetMoney>(requestBody);

                        string? SaveReservParamState = null;
                        if (SetDeviceMoney != null)
                        {
                            SaveReservParamState = await SaveReservParam(SetDeviceMoney);
                        }

                        URL_PostResponse200(context, SaveReservParamState);
                    }
                    catch (Exception ex)
                    {
                        URL_GET_ErrorCatch(context: context, ex: ex);
                    }
                    finally
                    {
                        context.Response.Close();
                    }
                }
                else
                {
                    Console.WriteLine(context.Request.Url.PathAndQuery.ToString());
                    context.Response.StatusCode = 404;
                    context.Response.OutputStream.Close();
                }
            }
            else
            {
                Console.WriteLine(context.Request.Url.PathAndQuery.ToString());
                context.Response.StatusCode = 404;
                context.Response.OutputStream.Close();
            }
        }
    }
    static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            Console.Clear();

            var stream = client.GetStream();
            var buffer = new byte[4096];
            uint[] TCPTempArray = new uint[150];

            uint[] ActualMoney = new uint[2];

            Array.Clear(buffer, 0, buffer.Length);

            //Array.Clear(buffer, 0, buffer.Length);
            //Array.Clear(TCPTempArray, 0, TCPTempArray.Length);
            //Array.Clear(ActualMoney, 0, ActualMoney.Length);
            //Console.WriteLine(ActualMoney[0]);
            //Console.WriteLine(ActualMoney[1]);


            while (true)
            {
                // Read data from the client
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                //var bytesRead = stream.Read(buffer, 0, buffer.Length);
                
                Array.Clear(TCPTempArray, 0, TCPTempArray.Length);
                Array.Clear(ActualMoney, 0, ActualMoney.Length);
                //Console.WriteLine(ActualMoney[0]);
                //Console.WriteLine(ActualMoney[1]);

                if (bytesRead == 0)
                {
                    // Client disconnected
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                //Console.WriteLine($"Received message from client: {message}");


                bool StartUncoding = false;
                bool StartUncodingMoney = false;
                bool DeletReserv = false;

                int j = 0;
                int k = 0;
                for (int i = 0; i < message.Length; i++)
                {
                    if (message[i] == 13)
                    {
                        break;
                    }
                    if (StartUncoding == true)
                    {
                        if (message[i] == ',')
                        {
                            j++;
                            continue;
                        }
                        Char tempChar = message[i];

                        TCPTempArray[j] = TCPTempArray[j] * 10 + (uint)Char.GetNumericValue(tempChar);
                    }
                    else if (StartUncodingMoney == true)
                    {
                        if (message[i] == ',')
                        {
                            k++;
                            continue;
                        }
                        Char tempChar = message[i];

                        ActualMoney[k] = ActualMoney[k] * 10 + (uint)Char.GetNumericValue(tempChar);
                    }
                    else if (message[i + 1] == ' ' && message[i] == ':' && message[i - 1] == 'e' && message[i - 2] == 'v' && message[i - 3] == 'a' && message[i - 4] == 'l' && message[i - 5] == 'S')
                    {
                        i++;
                        StartUncoding = true;
                    }
                    else if (message[i + 1] == ' ' && message[i] == ':' && message[i - 1] == 'y' && message[i - 2] == 'e' && message[i - 3] == 'n' && message[i - 4] == 'o' && message[i - 5] == 'M')
                    {
                        i++;
                        StartUncodingMoney = true;
                    }
                    else if (message[i + 1] == ' ' && message[i] == ':' && message[i - 1] == 'r' && message[i - 2] == 'e' && message[i - 3] == 's' && message[i - 4] == 'e' && message[i - 5] == 'R')
                    {
                        i++;
                        StartUncodingMoney = true;
                        DeletReserv = true;
                    }
                }
                //Console.WriteLine(message);

                if (StartUncoding && TCPTempArray[2] > 0 && TCPTempArray[2] < 100000000) 
                {
                    _ = SQLWriteForTCP(TCPTempArray);
                    string? SendTCPMassage = await CheckConfigDevice(TCPTempArray, j);
                    Console.WriteLine(SendTCPMassage);
                    if (SendTCPMassage != "NO1" && SendTCPMassage != "NO2" && SendTCPMassage != "ERROR" && SendTCPMassage != null)
                    {
                        if(stream.CanWrite==true)
                        {
                            byte[] sendClientData = Encoding.ASCII.GetBytes(SendTCPMassage);
                            stream.Write(sendClientData, 0, sendClientData.Length);
                        }
                        else
                        {
                            Console.WriteLine("Monitoring data: Stream is not open");
                        }
                    }
                    else
                    {
                        if (stream.CanRead == true)
                        {
                            Console.WriteLine("TCP is OK");
                        }
                        else
                        {
                            Console.WriteLine("TCP Connection Error");
                        }
                    }
                }
                if (StartUncodingMoney && ActualMoney[0] > 0 && ActualMoney[0] < 100000000) 
                {
                    if (DeletReserv == true)
                    {
                        _ = DeletingReserv(ActualMoney[0]);
                    }
                    else
                    {
                        string? MoneyState = await SaveActualMoney(ActualMoney);
                        string? SendTCPReservMasage = await CheckReservDevice(ActualMoney);


                        //Console.WriteLine(MoneyState);
                        Console.WriteLine(SendTCPReservMasage);
                        if (SendTCPReservMasage != "NO1" && SendTCPReservMasage != "NO2" && SendTCPReservMasage != "ERROR" && SendTCPReservMasage != null)
                        {
                            if(stream.CanWrite==true)
                            {
                                byte[] sendClientData = Encoding.ASCII.GetBytes(SendTCPReservMasage);
                                stream.Write(sendClientData, 0, sendClientData.Length);
                            }
                            else
                            {
                                Console.WriteLine("Money data: Stream is not open");
                            }
                        }
                        else
                        {
                            if (stream.CanRead == true)
                            {
                                Console.WriteLine("TCP is OK 2");
                            }
                            else
                            {
                                Console.WriteLine("TCP Connection Error 2");
                            }
                        }
                    }
                }
            }

            // Clean up resources
            stream.Close();
            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client: {ex.Message}");
        }
    }

    static async Task SQLWriteForTCP(uint[] DataArray)
    {
        DateTime localDate = new();
        localDate = DateTime.UtcNow;
        Console.WriteLine(localDate.ToString());
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        //sqlDataReader = null;
        try
        {
            /*sqlCommand = new SQLiteCommand($"SELECT P2 FROM Devices WHERE P2={DataArray[2]};", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT P2 FROM Devices WHERE P2={DataArray[2]};", TempSQLConnection);
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            bool tempState = false;
            while (TempSQLDataReader.Read())
            {
                //sqlDataReader.NextResult();
                int tempSql = TempSQLDataReader.GetInt32(0);
                // Console.WriteLine(tempSql);
                tempState = true;
            }
            TempSQLDataReader.Close();
            if (tempState)
            {
                Console.WriteLine("UPDATE DEVICE");
                TempSQLCommand.Parameters.Clear();
                //sqlCommand = new SQLiteCommand($"UPDATE [Devices] SET " +
                TempSQLCommand = new NpgsqlCommand($"UPDATE Devices SET " +
                    $"DataTime=" +
                    $"@DataTime, " +
                    $"P0=" +
                    $"@P0, " +
                    $"P1=" +
                    $"@P1, " +
                    $"P2=" +
                    $"@P2, " +
                    $"P3=" +
                    $"@P3, " +
                    $"P4=" +
                    $"@P4, " +
                    $"P5=" +
                    $"@P5, " +
                    $"P6=" +
                    $"@P6, " +
                    $"P7=" +
                    $"@P7, " +
                    $"P8=" +
                    $"@P8, " +
                    $"P9=" +
                    $"@P9, " +
                    $"P10=" +
                    $"@P10, " +
                    $"P11=" +
                    $"@P11, " +
                    $"P12=" +
                    $"@P12, " +
                    $"P13=" +
                    $"@P13, " +
                    $"P14=" +
                    $"@P14, " +
                    $"P15=" +
                    $"@P15, " +
                    $"P16=" +
                    $"@P16, " +
                    $"P17=" +
                    $"@P17, " +
                    $"P18=" +
                    $"@P18, " +
                    $"P19=" +
                    $"@P19, " +
                    $"P20=" +
                    $"@P20, " +
                    $"P21=" +
                    $"@P21, " +
                    $"P22=" +
                    $"@P22, " +
                    $"P23=" +
                    $"@P23, " +
                    $"P24=" +
                    $"@P24, " +
                    $"P25=" +
                    $"@P25, " +
                    $"P26=" +
                    $"@P26, " +
                    $"P27=" +
                    $"@P27, " +
                    $"P28=" +
                    $"@P28, " +
                    $"P29=" +
                    $"@P29, " +
                    $"P30=" +
                    $"@P30, " +
                    $"P31=" +
                    $"@P31, " +
                    $"P32=" +
                    $"@P32, " +
                    $"P33=" +
                    $"@P33, " +
                    $"P34=" +
                    $"@P34, " +
                    $"P35=" +
                    $"@P35, " +
                    $"P36=" +
                    $"@P36, " +
                    $"P37=" +
                    $"@P37, " +
                    $"P38=" +
                    $"@P38, " +
                    $"P39=" +
                    $"@P39, " +
                    $"P40=" +
                    $"@P40, " +
                    $"P41=" +
                    $"@P41, " +
                    $"P42=" +
                    $"@P42, " +
                    $"P43=" +
                    $"@P43, " +
                    $"P44=" +
                    $"@P44, " +
                    $"P45=" +
                    $"@P45, " +
                    $"P46=" +
                    $"@P46, " +
                    $"P47=" +
                    $"@P47, " +
                    $"P48=" +
                    $"@P48, " +
                    $"P49=" +
                    $"@P49, " +
                    $"P50=" +
                    $"@P50, " +
                    $"P51=" +
                    $"@P51, " +
                    $"P52=" +
                    $"@P52, " +
                    $"P53=" +
                    $"@P53, " +
                    $"P54=" +
                    $"@P54, " +
                    $"P55=" +
                    $"@P55, " +
                    $"P56=" +
                    $"@P56, " +
                    $"P57=" +
                    $"@P57, " +
                    $"P58=" +
                    $"@P58, " +
                    $"P59=" +
                    $"@P59, " +
                    $"P60=" +
                    $"@P60, " +
                    $"P61=" +
                    $"@P61, " +
                    $"P62=" +
                    $"@P62, " +
                    $"P63=" +
                    $"@P63, " +
                    $"P64=" +
                    $"@P64, " +
                    $"P65=" +
                    $"@P65, " +
                    $"P66=" +
                    $"@P66, " +
                    $"P67=" +
                    $"@P67, " +
                    $"P68=" +
                    $"@P68, " +
                    $"P69=" +
                    $"@P69, " +
                    $"P70=" +
                    $"@P70, " +
                    $"P71=" +
                    $"@P71, " +
                    $"P72=" +
                    $"@P72, " +
                    $"P73=" +
                    $"@P73, " +
                    $"P74=" +
                    $"@P74, " +
                    $"P75=" +
                    $"@P75, " +
                    $"P76=" +
                    $"@P76, " +
                    $"P77=" +
                    $"@P77, " +
                    $"P78=" +
                    $"@P78, " +
                    $"P79=" +
                    $"@P79, " +
                    $"P80=" +
                    $"@P80, " +
                    $"P81=" +
                    $"@P81, " +
                    $"P82=" +
                    $"@P82, " +
                    $"P83=" +
                    $"@P83, " +
                    $"P84=" +
                    $"@P84, " +
                    $"P85=" +
                    $"@P85, " +
                    $"P86=" +
                    $"@P86, " +
                    $"P87=" +
                    $"@P87, " +
                    $"P88=" +
                    $"@P88, " +
                    $"P89=" +
                    $"@P89, " +
                    $"P90=" +
                    $"@P90, " +
                    $"P91=" +
                    $"@P91, " +
                    $"P92=" +
                    $"@P92, " +
                    $"P93=" +
                    $"@P93, " +
                    $"P94=" +
                    $"@P94, " +
                    $"P95=" +
                    $"@P95, " +
                    $"P96=" +
                    $"@P96, " +
                    $"P97=" +
                    $"@P97, " +
                    $"P98=" +
                    $"@P98, " +
                    $"P99=" +
                    $"@P99, " +
                    $"P100=" +
                    $"@P100, " +
                    $"P101=" +
                    $"@P101, " +
                    $"P102=" +
                    $"@P102, " +
                    $"P103=" +
                    $"@P103, " +
                    $"P104=" +
                    $"@P104, " +
                    $"P105=" +
                    $"@P105, " +
                    $"P106=" +
                    $"@P106, " +
                    $"P107=" +
                    $"@P107, " +
                    $"P108=" +
                    $"@P108, " +
                    $"P109=" +
                    $"@P109, " +
                    $"P110=" +
                    $"@P110, " +
                    $"P111=" +
                    $"@P111, " +
                    $"P112=" +
                    $"@P112, " +
                    $"P113=" +
                    $"@P113, " +
                    $"P114=" +
                    $"@P114, " +
                    $"P115=" +
                    $"@P115, " +
                    $"P116=" +
                    $"@P116, " +
                    $"P117=" +
                    $"@P117, " +
                    $"P118=" +
                    $"@P118, " +
                    $"P119=" +
                    $"@P119, " +
                    $"P120=" +
                    $"@P120, " +
                    $"P121=" +
                    $"@P121, " +
                    $"P122=" +
                    $"@P122, " +
                    $"P123=" +
                    $"@P123, " +
                    $"P124=" +
                    $"@P124, " +
                    $"P125=" +
                    $"@P125, " +
                    $"P126=" +
                    $"@P126, " +
                    $"P127=" +
                    $"@P127, " +
                    $"P128=" +
                    $"@P128, " +
                    $"P129=" +
                    $"@P129, " +
                    $"P130=" +
                    $"@P130, " +
                    $"P131=" +
                    $"@P131, " +
                    $"P132=" +
                    $"@P132, " +
                    $"P133=" +
                    $"@P133, " +
                    $"P134=" +
                    $"@P134, " +
                    $"P135=" +
                    $"@P135, " +
                    $"P136=" +
                    $"@P136, " +
                    $"P137=" +
                    $"@P137, " +
                    $"P138=" +
                    $"@P138, " +
                    $"P139=" +
                    $"@P139, " +
                    $"P140=" +
                    $"@P140, " +
                    $"P141=" +
                    $"@P141, " +
                    $"P142=" +
                    $"@P142, " +
                    $"P143=" +
                    $"@P143, " +
                    $"P144=" +
                    $"@P144, " +
                    $"P145=" +
                    $"@P145, " +
                    $"P146=" +
                    $"@P146, " +
                    $"P147=" +
                    $"@P147, " +
                    $"P148=" +
                    $"@P148, " +
                    $"P149=" +
                    $"@P149 " +
                    $"WHERE P2={DataArray[2]}",
                    TempSQLConnection);

                string sqlFormattedDate = localDate.ToString("yyyy-MM-dd  HH:mm:ss");
                TempSQLCommand.Parameters.AddWithValue("DataTime", sqlFormattedDate);
                TempSQLCommand.Parameters.AddWithValue("P0", (int)DataArray[0]);
                TempSQLCommand.Parameters.AddWithValue("P1", (int)DataArray[1]);
                TempSQLCommand.Parameters.AddWithValue("P2", (int)DataArray[2]);
                TempSQLCommand.Parameters.AddWithValue("P3", (int)DataArray[3]);
                TempSQLCommand.Parameters.AddWithValue("P4", (int)DataArray[4]);
                TempSQLCommand.Parameters.AddWithValue("P5", (int)DataArray[5]);
                TempSQLCommand.Parameters.AddWithValue("P6", (int)DataArray[6]);
                TempSQLCommand.Parameters.AddWithValue("P7", (int)DataArray[7]);
                TempSQLCommand.Parameters.AddWithValue("P8", (int)DataArray[8]);
                TempSQLCommand.Parameters.AddWithValue("P9", (int)DataArray[9]);
                TempSQLCommand.Parameters.AddWithValue("P10", (int)DataArray[10]);
                TempSQLCommand.Parameters.AddWithValue("P11", (int)DataArray[11]);
                TempSQLCommand.Parameters.AddWithValue("P12", (int)DataArray[12]);
                TempSQLCommand.Parameters.AddWithValue("P13", (int)DataArray[13]);
                TempSQLCommand.Parameters.AddWithValue("P14", (int)DataArray[14]);
                TempSQLCommand.Parameters.AddWithValue("P15", (int)DataArray[15]);
                TempSQLCommand.Parameters.AddWithValue("P16", (int)DataArray[16]);
                TempSQLCommand.Parameters.AddWithValue("P17", (int)DataArray[17]);
                TempSQLCommand.Parameters.AddWithValue("P18", (int)DataArray[18]);
                TempSQLCommand.Parameters.AddWithValue("P19", (int)DataArray[19]);
                TempSQLCommand.Parameters.AddWithValue("P20", (int)DataArray[20]);
                TempSQLCommand.Parameters.AddWithValue("P21", (int)DataArray[21]);
                TempSQLCommand.Parameters.AddWithValue("P22", (int)DataArray[22]);
                TempSQLCommand.Parameters.AddWithValue("P23", (int)DataArray[23]);
                TempSQLCommand.Parameters.AddWithValue("P24", (int)DataArray[24]);
                TempSQLCommand.Parameters.AddWithValue("P25", (int)DataArray[25]);
                TempSQLCommand.Parameters.AddWithValue("P26", (int)DataArray[26]);
                TempSQLCommand.Parameters.AddWithValue("P27", (int)DataArray[27]);
                TempSQLCommand.Parameters.AddWithValue("P28", (int)DataArray[28]);
                TempSQLCommand.Parameters.AddWithValue("P29", (int)DataArray[29]);
                TempSQLCommand.Parameters.AddWithValue("P30", (int)DataArray[30]);
                TempSQLCommand.Parameters.AddWithValue("P31", (int)DataArray[31]);
                TempSQLCommand.Parameters.AddWithValue("P32", (int)DataArray[32]);
                TempSQLCommand.Parameters.AddWithValue("P33", (int)DataArray[33]);
                TempSQLCommand.Parameters.AddWithValue("P34", (int)DataArray[34]);
                TempSQLCommand.Parameters.AddWithValue("P35", (int)DataArray[35]);
                TempSQLCommand.Parameters.AddWithValue("P36", (int)DataArray[36]);
                TempSQLCommand.Parameters.AddWithValue("P37", (int)DataArray[37]);
                TempSQLCommand.Parameters.AddWithValue("P38", (int)DataArray[38]);
                TempSQLCommand.Parameters.AddWithValue("P39", (int)DataArray[39]);
                TempSQLCommand.Parameters.AddWithValue("P40", (int)DataArray[40]);
                TempSQLCommand.Parameters.AddWithValue("P41", (int)DataArray[41]);
                TempSQLCommand.Parameters.AddWithValue("P42", (int)DataArray[42]);
                TempSQLCommand.Parameters.AddWithValue("P43", (int)DataArray[43]);
                TempSQLCommand.Parameters.AddWithValue("P44", (int)DataArray[44]);
                TempSQLCommand.Parameters.AddWithValue("P45", (int)DataArray[45]);
                TempSQLCommand.Parameters.AddWithValue("P46", (int)DataArray[46]);
                TempSQLCommand.Parameters.AddWithValue("P47", (int)DataArray[47]);
                TempSQLCommand.Parameters.AddWithValue("P48", (int)DataArray[48]);
                TempSQLCommand.Parameters.AddWithValue("P49", (int)DataArray[49]);
                TempSQLCommand.Parameters.AddWithValue("P50", (int)DataArray[50]);
                TempSQLCommand.Parameters.AddWithValue("P51", (int)DataArray[51]);
                TempSQLCommand.Parameters.AddWithValue("P52", (int)DataArray[52]);
                TempSQLCommand.Parameters.AddWithValue("P53", (int)DataArray[53]);
                TempSQLCommand.Parameters.AddWithValue("P54", (int)DataArray[54]);
                TempSQLCommand.Parameters.AddWithValue("P55", (int)DataArray[55]);
                TempSQLCommand.Parameters.AddWithValue("P56", (int)DataArray[56]);
                TempSQLCommand.Parameters.AddWithValue("P57", (int)DataArray[57]);
                TempSQLCommand.Parameters.AddWithValue("P58", (int)DataArray[58]);
                TempSQLCommand.Parameters.AddWithValue("P59", (int)DataArray[59]);
                TempSQLCommand.Parameters.AddWithValue("P60", (int)DataArray[60]);
                TempSQLCommand.Parameters.AddWithValue("P61", (int)DataArray[61]);
                TempSQLCommand.Parameters.AddWithValue("P62", (int)DataArray[62]);
                TempSQLCommand.Parameters.AddWithValue("P63", (int)DataArray[63]);
                TempSQLCommand.Parameters.AddWithValue("P64", (int)DataArray[64]);
                TempSQLCommand.Parameters.AddWithValue("P65", (int)DataArray[65]);
                TempSQLCommand.Parameters.AddWithValue("P66", (int)DataArray[66]);
                TempSQLCommand.Parameters.AddWithValue("P67", (int)DataArray[67]);
                TempSQLCommand.Parameters.AddWithValue("P68", (int)DataArray[68]);
                TempSQLCommand.Parameters.AddWithValue("P69", (int)DataArray[69]);
                TempSQLCommand.Parameters.AddWithValue("P70", (int)DataArray[70]);
                TempSQLCommand.Parameters.AddWithValue("P71", (int)DataArray[71]);
                TempSQLCommand.Parameters.AddWithValue("P72", (int)DataArray[72]);
                TempSQLCommand.Parameters.AddWithValue("P73", (int)DataArray[73]);
                TempSQLCommand.Parameters.AddWithValue("P74", (int)DataArray[74]);
                TempSQLCommand.Parameters.AddWithValue("P75", (int)DataArray[75]);
                TempSQLCommand.Parameters.AddWithValue("P76", (int)DataArray[76]);
                TempSQLCommand.Parameters.AddWithValue("P77", (int)DataArray[77]);
                TempSQLCommand.Parameters.AddWithValue("P78", (int)DataArray[78]);
                TempSQLCommand.Parameters.AddWithValue("P79", (int)DataArray[79]);
                TempSQLCommand.Parameters.AddWithValue("P80", (int)DataArray[80]);
                TempSQLCommand.Parameters.AddWithValue("P81", (int)DataArray[81]);
                TempSQLCommand.Parameters.AddWithValue("P82", (int)DataArray[82]);
                TempSQLCommand.Parameters.AddWithValue("P83", (int)DataArray[83]);
                TempSQLCommand.Parameters.AddWithValue("P84", (int)DataArray[84]);
                TempSQLCommand.Parameters.AddWithValue("P85", (int)DataArray[85]);
                TempSQLCommand.Parameters.AddWithValue("P86", (int)DataArray[86]);
                TempSQLCommand.Parameters.AddWithValue("P87", (int)DataArray[87]);
                TempSQLCommand.Parameters.AddWithValue("P88", (int)DataArray[88]);
                TempSQLCommand.Parameters.AddWithValue("P89", (int)DataArray[89]);
                TempSQLCommand.Parameters.AddWithValue("P90", (int)DataArray[90]);
                TempSQLCommand.Parameters.AddWithValue("P91", (int)DataArray[91]);
                TempSQLCommand.Parameters.AddWithValue("P92", (int)DataArray[92]);
                TempSQLCommand.Parameters.AddWithValue("P93", (int)DataArray[93]);
                TempSQLCommand.Parameters.AddWithValue("P94", (int)DataArray[94]);
                TempSQLCommand.Parameters.AddWithValue("P95", (int)DataArray[95]);
                TempSQLCommand.Parameters.AddWithValue("P96", (int)DataArray[96]);
                TempSQLCommand.Parameters.AddWithValue("P97", (int)DataArray[97]);
                TempSQLCommand.Parameters.AddWithValue("P98", (int)DataArray[98]);
                TempSQLCommand.Parameters.AddWithValue("P99", (int)DataArray[99]);
                TempSQLCommand.Parameters.AddWithValue("P100", (int)DataArray[100]);
                TempSQLCommand.Parameters.AddWithValue("P101", (int)DataArray[101]);
                TempSQLCommand.Parameters.AddWithValue("P102", (int)DataArray[102]);
                TempSQLCommand.Parameters.AddWithValue("P103", (int)DataArray[103]);
                TempSQLCommand.Parameters.AddWithValue("P104", (int)DataArray[104]);
                TempSQLCommand.Parameters.AddWithValue("P105", (int)DataArray[105]);
                TempSQLCommand.Parameters.AddWithValue("P106", (int)DataArray[106]);
                TempSQLCommand.Parameters.AddWithValue("P107", (int)DataArray[107]);
                TempSQLCommand.Parameters.AddWithValue("P108", (int)DataArray[108]);
                TempSQLCommand.Parameters.AddWithValue("P109", (int)DataArray[109]);
                TempSQLCommand.Parameters.AddWithValue("P110", (int)DataArray[110]);
                TempSQLCommand.Parameters.AddWithValue("P111", (int)DataArray[111]);
                TempSQLCommand.Parameters.AddWithValue("P112", (int)DataArray[112]);
                TempSQLCommand.Parameters.AddWithValue("P113", (int)DataArray[113]);
                TempSQLCommand.Parameters.AddWithValue("P114", (int)DataArray[114]);
                TempSQLCommand.Parameters.AddWithValue("P115", (int)DataArray[115]);
                TempSQLCommand.Parameters.AddWithValue("P116", (int)DataArray[116]);
                TempSQLCommand.Parameters.AddWithValue("P117", (int)DataArray[117]);
                TempSQLCommand.Parameters.AddWithValue("P118", (int)DataArray[118]);
                TempSQLCommand.Parameters.AddWithValue("P119", (int)DataArray[119]);
                TempSQLCommand.Parameters.AddWithValue("P120", (int)DataArray[120]);
                TempSQLCommand.Parameters.AddWithValue("P121", (int)DataArray[121]);
                TempSQLCommand.Parameters.AddWithValue("P122", (int)DataArray[122]);
                TempSQLCommand.Parameters.AddWithValue("P123", (int)DataArray[123]);
                TempSQLCommand.Parameters.AddWithValue("P124", (int)DataArray[124]);
                TempSQLCommand.Parameters.AddWithValue("P125", (int)DataArray[125]);
                TempSQLCommand.Parameters.AddWithValue("P126", (int)DataArray[126]);
                TempSQLCommand.Parameters.AddWithValue("P127", (int)DataArray[127]);
                TempSQLCommand.Parameters.AddWithValue("P128", (int)DataArray[128]);
                TempSQLCommand.Parameters.AddWithValue("P129", (int)DataArray[129]);
                TempSQLCommand.Parameters.AddWithValue("P130", (int)DataArray[130]);
                TempSQLCommand.Parameters.AddWithValue("P131", (int)DataArray[131]);
                TempSQLCommand.Parameters.AddWithValue("P132", (int)DataArray[132]);
                TempSQLCommand.Parameters.AddWithValue("P133", (int)DataArray[133]);
                TempSQLCommand.Parameters.AddWithValue("P134", (int)DataArray[134]);
                TempSQLCommand.Parameters.AddWithValue("P135", (int)DataArray[135]);
                TempSQLCommand.Parameters.AddWithValue("P136", (int)DataArray[136]);
                TempSQLCommand.Parameters.AddWithValue("P137", (int)DataArray[137]);
                TempSQLCommand.Parameters.AddWithValue("P138", (int)DataArray[138]);
                TempSQLCommand.Parameters.AddWithValue("P139", (int)DataArray[139]);
                TempSQLCommand.Parameters.AddWithValue("P140", (int)DataArray[140]);
                TempSQLCommand.Parameters.AddWithValue("P141", (int)DataArray[141]);
                TempSQLCommand.Parameters.AddWithValue("P142", (int)DataArray[142]);
                TempSQLCommand.Parameters.AddWithValue("P143", (int)DataArray[143]);
                TempSQLCommand.Parameters.AddWithValue("P144", (int)DataArray[144]);
                TempSQLCommand.Parameters.AddWithValue("P145", (int)DataArray[145]);
                TempSQLCommand.Parameters.AddWithValue("P146", (int)DataArray[146]);
                TempSQLCommand.Parameters.AddWithValue("P147", (int)DataArray[147]);
                TempSQLCommand.Parameters.AddWithValue("P148", (int)DataArray[148]);
                TempSQLCommand.Parameters.AddWithValue("P149", (int)DataArray[149]);
                TempSQLCommand.ExecuteNonQuery();
            }
            else
            {
                Console.WriteLine("NEW DEVICE");
                TempSQLCommand.Parameters.Clear();
                /*sqlCommand = new SQLiteCommand(*/
                TempSQLCommand = new NpgsqlCommand(
                   $"INSERT INTO Devices (" +
                    $"DataTime," +
                    $"P0," +
                    $"P1," +
                    $"P2," +
                    $"P3," +
                    $"P4," +
                    $"P5," +
                    $"P6," +
                    $"P7," +
                    $"P8," +
                    $"P9," +
                    $"P10," +
                    $"P11," +
                    $"P12," +
                    $"P13," +
                    $"P14," +
                    $"P15," +
                    $"P16," +
                    $"P17," +
                    $"P18," +
                    $"P19," +
                    $"P20," +
                    $"P21," +
                    $"P22," +
                    $"P23," +
                    $"P24," +
                    $"P25," +
                    $"P26," +
                    $"P27," +
                    $"P28," +
                    $"P29," +
                    $"P30," +
                    $"P31," +
                    $"P32," +
                    $"P33," +
                    $"P34," +
                    $"P35," +
                    $"P36," +
                    $"P37," +
                    $"P38," +
                    $"P39," +
                    $"P40," +
                    $"P41," +
                    $"P42," +
                    $"P43," +
                    $"P44," +
                    $"P45," +
                    $"P46," +
                    $"P47," +
                    $"P48," +
                    $"P49," +
                    $"P50," +
                    $"P51," +
                    $"P52," +
                    $"P53," +
                    $"P54," +
                    $"P55," +
                    $"P56," +
                    $"P57," +
                    $"P58," +
                    $"P59," +
                    $"P60," +
                    $"P61," +
                    $"P62," +
                    $"P63," +
                    $"P64," +
                    $"P65," +
                    $"P66," +
                    $"P67," +
                    $"P68," +
                    $"P69," +
                    $"P70," +
                    $"P71," +
                    $"P72," +
                    $"P73," +
                    $"P74," +
                    $"P75," +
                    $"P76," +
                    $"P77," +
                    $"P78," +
                    $"P79," +
                    $"P80," +
                    $"P81," +
                    $"P82," +
                    $"P83," +
                    $"P84," +
                    $"P85," +
                    $"P86," +
                    $"P87," +
                    $"P88," +
                    $"P89," +
                    $"P90," +
                    $"P91," +
                    $"P92," +
                    $"P93," +
                    $"P94," +
                    $"P95," +
                    $"P96," +
                    $"P97," +
                    $"P98," +
                    $"P99," +
                    $"P100," +
                    $"P101," +
                    $"P102," +
                    $"P103," +
                    $"P104," +
                    $"P105," +
                    $"P106," +
                    $"P107," +
                    $"P108," +
                    $"P109," +
                    $"P110," +
                    $"P111," +
                    $"P112," +
                    $"P113," +
                    $"P114," +
                    $"P115," +
                    $"P116," +
                    $"P117," +
                    $"P118," +
                    $"P119," +
                    $"P120," +
                    $"P121," +
                    $"P122," +
                    $"P123," +
                    $"P124," +
                    $"P125," +
                    $"P126," +
                    $"P127," +
                    $"P128," +
                    $"P129," +
                    $"P130," +
                    $"P131," +
                    $"P132," +
                    $"P133," +
                    $"P134," +
                    $"P135," +
                    $"P136," +
                    $"P137," +
                    $"P138," +
                    $"P139," +
                    $"P140," +
                    $"P141," +
                    $"P142," +
                    $"P143," +
                    $"P144," +
                    $"P145," +
                    $"P146," +
                    $"P147," +
                    $"P148," +
                    $"P149 " +
                    $") VALUES (" +
                    $"@DataTime," +
                    $"@P0," +
                    $"@P1," +
                    $"@P2," +
                    $"@P3," +
                    $"@P4," +
                    $"@P5," +
                    $"@P6," +
                    $"@P7," +
                    $"@P8," +
                    $"@P9," +
                    $"@P10," +
                    $"@P11," +
                    $"@P12," +
                    $"@P13," +
                    $"@P14," +
                    $"@P15," +
                    $"@P16," +
                    $"@P17," +
                    $"@P18," +
                    $"@P19," +
                    $"@P20," +
                    $"@P21," +
                    $"@P22," +
                    $"@P23," +
                    $"@P24," +
                    $"@P25," +
                    $"@P26," +
                    $"@P27," +
                    $"@P28," +
                    $"@P29," +
                    $"@P30," +
                    $"@P31," +
                    $"@P32," +
                    $"@P33," +
                    $"@P34," +
                    $"@P35," +
                    $"@P36," +
                    $"@P37," +
                    $"@P38," +
                    $"@P39," +
                    $"@P40," +
                    $"@P41," +
                    $"@P42," +
                    $"@P43," +
                    $"@P44," +
                    $"@P45," +
                    $"@P46," +
                    $"@P47," +
                    $"@P48," +
                    $"@P49," +
                    $"@P50," +
                    $"@P51," +
                    $"@P52," +
                    $"@P53," +
                    $"@P54," +
                    $"@P55," +
                    $"@P56," +
                    $"@P57," +
                    $"@P58," +
                    $"@P59," +
                    $"@P60," +
                    $"@P61," +
                    $"@P62," +
                    $"@P63," +
                    $"@P64," +
                    $"@P65," +
                    $"@P66," +
                    $"@P67," +
                    $"@P68," +
                    $"@P69," +
                    $"@P70," +
                    $"@P71," +
                    $"@P72," +
                    $"@P73," +
                    $"@P74," +
                    $"@P75," +
                    $"@P76," +
                    $"@P77," +
                    $"@P78," +
                    $"@P79," +
                    $"@P80," +
                    $"@P81," +
                    $"@P82," +
                    $"@P83," +
                    $"@P84," +
                    $"@P85," +
                    $"@P86," +
                    $"@P87," +
                    $"@P88," +
                    $"@P89," +
                    $"@P90," +
                    $"@P91," +
                    $"@P92," +
                    $"@P93," +
                    $"@P94," +
                    $"@P95," +
                    $"@P96," +
                    $"@P97," +
                    $"@P98," +
                    $"@P99," +
                    $"@P100," +
                    $"@P101," +
                    $"@P102," +
                    $"@P103," +
                    $"@P104," +
                    $"@P105," +
                    $"@P106," +
                    $"@P107," +
                    $"@P108," +
                    $"@P109," +
                    $"@P110," +
                    $"@P111," +
                    $"@P112," +
                    $"@P113," +
                    $"@P114," +
                    $"@P115," +
                    $"@P116," +
                    $"@P117," +
                    $"@P118," +
                    $"@P119," +
                    $"@P120," +
                    $"@P121," +
                    $"@P122," +
                    $"@P123," +
                    $"@P124," +
                    $"@P125," +
                    $"@P126," +
                    $"@P127," +
                    $"@P128," +
                    $"@P129," +
                    $"@P130," +
                    $"@P131," +
                    $"@P132," +
                    $"@P133," +
                    $"@P134," +
                    $"@P135," +
                    $"@P136," +
                    $"@P137," +
                    $"@P138," +
                    $"@P139," +
                    $"@P140," +
                    $"@P141," +
                    $"@P142," +
                    $"@P143," +
                    $"@P144," +
                    $"@P145," +
                    $"@P146," +
                    $"@P147," +
                    $"@P148," +
                    $"@P149 " +
                    $") ",
                    TempSQLConnection);

                string sqlFormattedDate = localDate.ToString("yyyy-MM-dd  HH:mm:ss");
                TempSQLCommand.Parameters.AddWithValue("DataTime", sqlFormattedDate);
                TempSQLCommand.Parameters.AddWithValue("P0", (int)DataArray[0]);
                TempSQLCommand.Parameters.AddWithValue("P1", (int)DataArray[1]);
                TempSQLCommand.Parameters.AddWithValue("P2", (int)DataArray[2]);
                TempSQLCommand.Parameters.AddWithValue("P3", (int)DataArray[3]);
                TempSQLCommand.Parameters.AddWithValue("P4", (int)DataArray[4]);
                TempSQLCommand.Parameters.AddWithValue("P5", (int)DataArray[5]);
                TempSQLCommand.Parameters.AddWithValue("P6", (int)DataArray[6]);
                TempSQLCommand.Parameters.AddWithValue("P7", (int)DataArray[7]);
                TempSQLCommand.Parameters.AddWithValue("P8", (int)DataArray[8]);
                TempSQLCommand.Parameters.AddWithValue("P9", (int)DataArray[9]);
                TempSQLCommand.Parameters.AddWithValue("P10", (int)DataArray[10]);
                TempSQLCommand.Parameters.AddWithValue("P11", (int)DataArray[11]);
                TempSQLCommand.Parameters.AddWithValue("P12", (int)DataArray[12]);
                TempSQLCommand.Parameters.AddWithValue("P13", (int)DataArray[13]);
                TempSQLCommand.Parameters.AddWithValue("P14", (int)DataArray[14]);
                TempSQLCommand.Parameters.AddWithValue("P15", (int)DataArray[15]);
                TempSQLCommand.Parameters.AddWithValue("P16", (int)DataArray[16]);
                TempSQLCommand.Parameters.AddWithValue("P17", (int)DataArray[17]);
                TempSQLCommand.Parameters.AddWithValue("P18", (int)DataArray[18]);
                TempSQLCommand.Parameters.AddWithValue("P19", (int)DataArray[19]);
                TempSQLCommand.Parameters.AddWithValue("P20", (int)DataArray[20]);
                TempSQLCommand.Parameters.AddWithValue("P21", (int)DataArray[21]);
                TempSQLCommand.Parameters.AddWithValue("P22", (int)DataArray[22]);
                TempSQLCommand.Parameters.AddWithValue("P23", (int)DataArray[23]);
                TempSQLCommand.Parameters.AddWithValue("P24", (int)DataArray[24]);
                TempSQLCommand.Parameters.AddWithValue("P25", (int)DataArray[25]);
                TempSQLCommand.Parameters.AddWithValue("P26", (int)DataArray[26]);
                TempSQLCommand.Parameters.AddWithValue("P27", (int)DataArray[27]);
                TempSQLCommand.Parameters.AddWithValue("P28", (int)DataArray[28]);
                TempSQLCommand.Parameters.AddWithValue("P29", (int)DataArray[29]);
                TempSQLCommand.Parameters.AddWithValue("P30", (int)DataArray[30]);
                TempSQLCommand.Parameters.AddWithValue("P31", (int)DataArray[31]);
                TempSQLCommand.Parameters.AddWithValue("P32", (int)DataArray[32]);
                TempSQLCommand.Parameters.AddWithValue("P33", (int)DataArray[33]);
                TempSQLCommand.Parameters.AddWithValue("P34", (int)DataArray[34]);
                TempSQLCommand.Parameters.AddWithValue("P35", (int)DataArray[35]);
                TempSQLCommand.Parameters.AddWithValue("P36", (int)DataArray[36]);
                TempSQLCommand.Parameters.AddWithValue("P37", (int)DataArray[37]);
                TempSQLCommand.Parameters.AddWithValue("P38", (int)DataArray[38]);
                TempSQLCommand.Parameters.AddWithValue("P39", (int)DataArray[39]);
                TempSQLCommand.Parameters.AddWithValue("P40", (int)DataArray[40]);
                TempSQLCommand.Parameters.AddWithValue("P41", (int)DataArray[41]);
                TempSQLCommand.Parameters.AddWithValue("P42", (int)DataArray[42]);
                TempSQLCommand.Parameters.AddWithValue("P43", (int)DataArray[43]);
                TempSQLCommand.Parameters.AddWithValue("P44", (int)DataArray[44]);
                TempSQLCommand.Parameters.AddWithValue("P45", (int)DataArray[45]);
                TempSQLCommand.Parameters.AddWithValue("P46", (int)DataArray[46]);
                TempSQLCommand.Parameters.AddWithValue("P47", (int)DataArray[47]);
                TempSQLCommand.Parameters.AddWithValue("P48", (int)DataArray[48]);
                TempSQLCommand.Parameters.AddWithValue("P49", (int)DataArray[49]);
                TempSQLCommand.Parameters.AddWithValue("P50", (int)DataArray[50]);
                TempSQLCommand.Parameters.AddWithValue("P51", (int)DataArray[51]);
                TempSQLCommand.Parameters.AddWithValue("P52", (int)DataArray[52]);
                TempSQLCommand.Parameters.AddWithValue("P53", (int)DataArray[53]);
                TempSQLCommand.Parameters.AddWithValue("P54", (int)DataArray[54]);
                TempSQLCommand.Parameters.AddWithValue("P55", (int)DataArray[55]);
                TempSQLCommand.Parameters.AddWithValue("P56", (int)DataArray[56]);
                TempSQLCommand.Parameters.AddWithValue("P57", (int)DataArray[57]);
                TempSQLCommand.Parameters.AddWithValue("P58", (int)DataArray[58]);
                TempSQLCommand.Parameters.AddWithValue("P59", (int)DataArray[59]);
                TempSQLCommand.Parameters.AddWithValue("P60", (int)DataArray[60]);
                TempSQLCommand.Parameters.AddWithValue("P61", (int)DataArray[61]);
                TempSQLCommand.Parameters.AddWithValue("P62", (int)DataArray[62]);
                TempSQLCommand.Parameters.AddWithValue("P63", (int)DataArray[63]);
                TempSQLCommand.Parameters.AddWithValue("P64", (int)DataArray[64]);
                TempSQLCommand.Parameters.AddWithValue("P65", (int)DataArray[65]);
                TempSQLCommand.Parameters.AddWithValue("P66", (int)DataArray[66]);
                TempSQLCommand.Parameters.AddWithValue("P67", (int)DataArray[67]);
                TempSQLCommand.Parameters.AddWithValue("P68", (int)DataArray[68]);
                TempSQLCommand.Parameters.AddWithValue("P69", (int)DataArray[69]);
                TempSQLCommand.Parameters.AddWithValue("P70", (int)DataArray[70]);
                TempSQLCommand.Parameters.AddWithValue("P71", (int)DataArray[71]);
                TempSQLCommand.Parameters.AddWithValue("P72", (int)DataArray[72]);
                TempSQLCommand.Parameters.AddWithValue("P73", (int)DataArray[73]);
                TempSQLCommand.Parameters.AddWithValue("P74", (int)DataArray[74]);
                TempSQLCommand.Parameters.AddWithValue("P75", (int)DataArray[75]);
                TempSQLCommand.Parameters.AddWithValue("P76", (int)DataArray[76]);
                TempSQLCommand.Parameters.AddWithValue("P77", (int)DataArray[77]);
                TempSQLCommand.Parameters.AddWithValue("P78", (int)DataArray[78]);
                TempSQLCommand.Parameters.AddWithValue("P79", (int)DataArray[79]);
                TempSQLCommand.Parameters.AddWithValue("P80", (int)DataArray[80]);
                TempSQLCommand.Parameters.AddWithValue("P81", (int)DataArray[81]);
                TempSQLCommand.Parameters.AddWithValue("P82", (int)DataArray[82]);
                TempSQLCommand.Parameters.AddWithValue("P83", (int)DataArray[83]);
                TempSQLCommand.Parameters.AddWithValue("P84", (int)DataArray[84]);
                TempSQLCommand.Parameters.AddWithValue("P85", (int)DataArray[85]);
                TempSQLCommand.Parameters.AddWithValue("P86", (int)DataArray[86]);
                TempSQLCommand.Parameters.AddWithValue("P87", (int)DataArray[87]);
                TempSQLCommand.Parameters.AddWithValue("P88", (int)DataArray[88]);
                TempSQLCommand.Parameters.AddWithValue("P89", (int)DataArray[89]);
                TempSQLCommand.Parameters.AddWithValue("P90", (int)DataArray[90]);
                TempSQLCommand.Parameters.AddWithValue("P91", (int)DataArray[91]);
                TempSQLCommand.Parameters.AddWithValue("P92", (int)DataArray[92]);
                TempSQLCommand.Parameters.AddWithValue("P93", (int)DataArray[93]);
                TempSQLCommand.Parameters.AddWithValue("P94", (int)DataArray[94]);
                TempSQLCommand.Parameters.AddWithValue("P95", (int)DataArray[95]);
                TempSQLCommand.Parameters.AddWithValue("P96", (int)DataArray[96]);
                TempSQLCommand.Parameters.AddWithValue("P97", (int)DataArray[97]);
                TempSQLCommand.Parameters.AddWithValue("P98", (int)DataArray[98]);
                TempSQLCommand.Parameters.AddWithValue("P99", (int)DataArray[99]);
                TempSQLCommand.Parameters.AddWithValue("P100", (int)DataArray[100]);
                TempSQLCommand.Parameters.AddWithValue("P101", (int)DataArray[101]);
                TempSQLCommand.Parameters.AddWithValue("P102", (int)DataArray[102]);
                TempSQLCommand.Parameters.AddWithValue("P103", (int)DataArray[103]);
                TempSQLCommand.Parameters.AddWithValue("P104", (int)DataArray[104]);
                TempSQLCommand.Parameters.AddWithValue("P105", (int)DataArray[105]);
                TempSQLCommand.Parameters.AddWithValue("P106", (int)DataArray[106]);
                TempSQLCommand.Parameters.AddWithValue("P107", (int)DataArray[107]);
                TempSQLCommand.Parameters.AddWithValue("P108", (int)DataArray[108]);
                TempSQLCommand.Parameters.AddWithValue("P109", (int)DataArray[109]);
                TempSQLCommand.Parameters.AddWithValue("P110", (int)DataArray[110]);
                TempSQLCommand.Parameters.AddWithValue("P111", (int)DataArray[111]);
                TempSQLCommand.Parameters.AddWithValue("P112", (int)DataArray[112]);
                TempSQLCommand.Parameters.AddWithValue("P113", (int)DataArray[113]);
                TempSQLCommand.Parameters.AddWithValue("P114", (int)DataArray[114]);
                TempSQLCommand.Parameters.AddWithValue("P115", (int)DataArray[115]);
                TempSQLCommand.Parameters.AddWithValue("P116", (int)DataArray[116]);
                TempSQLCommand.Parameters.AddWithValue("P117", (int)DataArray[117]);
                TempSQLCommand.Parameters.AddWithValue("P118", (int)DataArray[118]);
                TempSQLCommand.Parameters.AddWithValue("P119", (int)DataArray[119]);
                TempSQLCommand.Parameters.AddWithValue("P120", (int)DataArray[120]);
                TempSQLCommand.Parameters.AddWithValue("P121", (int)DataArray[121]);
                TempSQLCommand.Parameters.AddWithValue("P122", (int)DataArray[122]);
                TempSQLCommand.Parameters.AddWithValue("P123", (int)DataArray[123]);
                TempSQLCommand.Parameters.AddWithValue("P124", (int)DataArray[124]);
                TempSQLCommand.Parameters.AddWithValue("P125", (int)DataArray[125]);
                TempSQLCommand.Parameters.AddWithValue("P126", (int)DataArray[126]);
                TempSQLCommand.Parameters.AddWithValue("P127", (int)DataArray[127]);
                TempSQLCommand.Parameters.AddWithValue("P128", (int)DataArray[128]);
                TempSQLCommand.Parameters.AddWithValue("P129", (int)DataArray[129]);
                TempSQLCommand.Parameters.AddWithValue("P130", (int)DataArray[130]);
                TempSQLCommand.Parameters.AddWithValue("P131", (int)DataArray[131]);
                TempSQLCommand.Parameters.AddWithValue("P132", (int)DataArray[132]);
                TempSQLCommand.Parameters.AddWithValue("P133", (int)DataArray[133]);
                TempSQLCommand.Parameters.AddWithValue("P134", (int)DataArray[134]);
                TempSQLCommand.Parameters.AddWithValue("P135", (int)DataArray[135]);
                TempSQLCommand.Parameters.AddWithValue("P136", (int)DataArray[136]);
                TempSQLCommand.Parameters.AddWithValue("P137", (int)DataArray[137]);
                TempSQLCommand.Parameters.AddWithValue("P138", (int)DataArray[138]);
                TempSQLCommand.Parameters.AddWithValue("P139", (int)DataArray[139]);
                TempSQLCommand.Parameters.AddWithValue("P140", (int)DataArray[140]);
                TempSQLCommand.Parameters.AddWithValue("P141", (int)DataArray[141]);
                TempSQLCommand.Parameters.AddWithValue("P142", (int)DataArray[142]);
                TempSQLCommand.Parameters.AddWithValue("P143", (int)DataArray[143]);
                TempSQLCommand.Parameters.AddWithValue("P144", (int)DataArray[144]);
                TempSQLCommand.Parameters.AddWithValue("P145", (int)DataArray[145]);
                TempSQLCommand.Parameters.AddWithValue("P146", (int)DataArray[146]);
                TempSQLCommand.Parameters.AddWithValue("P147", (int)DataArray[147]);
                TempSQLCommand.Parameters.AddWithValue("P148", (int)DataArray[148]);
                TempSQLCommand.Parameters.AddWithValue("P149", (int)DataArray[149]);
                TempSQLCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            //DataBasueReConnect();
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
    }
    static async Task<string?> SendCommandAll()
    {
        string? SendMassages = null;
        Console.WriteLine("ALL");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            //sqlCommand = new SQLiteCommand($"SELECT * FROM Devices", sqlConnection);
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Devices", TempSQLConnection);
            DataTable dataTable = new();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        //sqlConnection.Close();
        return SendMassages;
    }
    static async Task<string?> SendConfigTable()
    {
        string? SendMassages = null;
        Console.WriteLine("Config");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            /*sqlCommand = new SQLiteCommand($"SELECT * FROM Config", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Config", TempSQLConnection);
            DataTable dataTable = new();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        //sqlConnection.Close();
        return SendMassages;
    }
    static async Task<string?> SendOunerData(uint MyOwnerID)
    {
        string? SendMassages = null;
        Console.WriteLine("Owner ID");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            /*       sqlCommand = new SQLiteCommand($"SELECT * FROM Devices WHERE P2={MyOwnerID}", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Devices WHERE P2={MyOwnerID}", TempSQLConnection);
            DataTable dataTable = new();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        //sqlConnection.Close();
        //Console.WriteLine(SendMassages);
        return SendMassages;
    }
    static async Task<string?> SaveConfigParam(ConfigDevice config)
    {
        string? returnState = null;
        Console.WriteLine("Save Config");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            if (config.ParamNO > 2 && config.OwnerID>9999999 && config.OwnerID<100000000)
            {
                /*sqlCommand = new SQLiteCommand($"SELECT OwnerID FROM Config WHERE OwnerID={config.OwnerID} AND ParamNO={config.ParamNO};", sqlConnection);*/
                var TempSQLCommand = new NpgsqlCommand($"SELECT OwnerID FROM Config WHERE OwnerID={config.OwnerID} AND ParamNO={config.ParamNO};", TempSQLConnection);
                TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
                bool tempState = false;
                while (TempSQLDataReader.Read())
                {
                    int tempSql = TempSQLDataReader.GetInt32(0);
                    tempState = true;
                }
                TempSQLDataReader.Close();
                if (tempState)
                {
                    Console.WriteLine("UPDATE Config paradeters");
                    TempSQLCommand.Parameters.Clear();
                    /*sqlCommand = new SQLiteCommand($"UPDATE [Config] SET " +*/
                    TempSQLCommand = new NpgsqlCommand($"UPDATE Config SET " +
                        $"NewData=" +
                        $"@NewData " +
                        $"WHERE OwnerID={config.OwnerID} AND ParamNO={config.ParamNO};",
                        TempSQLConnection);

                    TempSQLCommand.Parameters.AddWithValue("NewData", config.NewData);
                    TempSQLCommand.ExecuteNonQuery();
                }
                else
                {
                    Console.WriteLine("NEW Config");
                    TempSQLCommand.Parameters.Clear();
                    /* sqlCommand = new SQLiteCommand(*/
                    TempSQLCommand = new NpgsqlCommand(
                        $"INSERT INTO Config (" +
                        $"OwnerID," +
                        $"ParamNO," +
                        $"NewData) " +
                        $"VALUES (" +
                        $"@OwnerID," +
                        $"@ParamNO," +
                        $"@NewData) ",
                        TempSQLConnection);

                    TempSQLCommand.Parameters.AddWithValue("OwnerID", config.OwnerID);
                    TempSQLCommand.Parameters.AddWithValue("ParamNO", config.ParamNO);
                    TempSQLCommand.Parameters.AddWithValue("NewData", config.NewData);
                    TempSQLCommand.ExecuteNonQuery();
                }
                returnState = "OK";
            }
            else
            {
                returnState = "Error ParametrNO";
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            returnState = ex.Message;
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        return returnState;
    }
    static async Task<string?> CheckConfigDevice(uint[] DataArray, int LastParam)
    {
        string? SendMassage = null;
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            Console.WriteLine(DataArray[2]);
            /*sqlCommand = new SQLiteCommand($"SELECT * FROM Config WHERE OwnerID={DataArray[2]}", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Config WHERE OwnerID={DataArray[2]}", TempSQLConnection);
            DataTable dataTable = new();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            if (dataTable != null)
            {
                string TempJSON = JsonConvert.SerializeObject(dataTable);
                //Console.WriteLine(TempJSON);
                ConfigDevice[]? configDevices = JsonConvert.DeserializeObject<ConfigDevice[]>(TempJSON);
                bool noChang = true;
                if (configDevices != null)
                {
                    for (int i = 0; i < configDevices.Length; i++)
                    {
                        //Console.WriteLine(configDevices[i].ToString());
                        if (DataArray[configDevices[i].ParamNO] != configDevices[i].NewData && configDevices[i].ParamNO <= LastParam - 1)
                        {
                            noChang = false;
                            SendMassage = SendMassage + "P" + configDevices[i].ParamNO.ToString() + "-" + configDevices[i].NewData + ",";
                        }
                        else if (DataArray[configDevices[i].ParamNO] == configDevices[i].NewData || configDevices[i].ParamNO > LastParam - 1)
                        {
                            TempSQLCommand = null;
                            /* sqlCommand = new SQLiteCommand($"DELETE FROM Config WHERE OwnerID={DataArray[2]} AND ParamNO={configDevices[i].ParamNO};", sqlConnection);*/
                            TempSQLCommand = new NpgsqlCommand($"DELETE FROM Config WHERE OwnerID={DataArray[2]} AND ParamNO={configDevices[i].ParamNO};", TempSQLConnection);
                            TempSQLCommand.ExecuteNonQuery();
                        }
                    }
                }
                if (noChang)
                {
                    SendMassage = "NO2";
                }
                else
                {
                    SendMassage = "Config: " + SendMassage + "#";
                }
            }
            else
            {
                SendMassage = "NO1";
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            SendMassage = "ERROR";
            //DataBasueReConnect();
        }
        finally
        {
            Array.Clear(DataArray, 0, DataArray.Length);
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        return SendMassage;
    }
    static async Task<string?> SendOunerConfig(uint MyOwnerID)
    {
        string? SendMassages = null;
        Console.WriteLine("Owner ID");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            /* sqlCommand = new SQLiteCommand($"SELECT * FROM Config WHERE OwnerID={MyOwnerID}", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Config WHERE OwnerID={MyOwnerID}", TempSQLConnection);
            DataTable dataTable = new();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        //sqlConnection.Close();
        //Console.WriteLine(SendMassages);
        return SendMassages;
    }
    static async Task Deleting(DeletDevice[] deletDevices)
    {
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        try
        {
            for (int i = 0; i < deletDevices.Length; i++)
            {
                /*sqlCommand = new SQLiteCommand($"DELETE FROM Devices WHERE P2={deletDevices[i].OwnerID};", sqlConnection);*/
                var TempSQLCommand = new NpgsqlCommand($"DELETE FROM Devices WHERE P2={deletDevices[i].OwnerID};", TempSQLConnection);
                TempSQLCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
    }
    static async Task<string?> SendAllOuners(uint MyOwnerID)
    {
        string? SendMassages = null;
        Console.WriteLine("All Owner ID");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            /*sqlCommand = new SQLiteCommand($"SELECT * FROM Devices WHERE P2>={MyOwnerID*1000} AND P2<={(MyOwnerID+1) * 1000}", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Devices WHERE P2>={MyOwnerID * 1000} AND P2<={(MyOwnerID + 1) * 1000}", TempSQLConnection);
            DataTable dataTable = new DataTable();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            SendMassages = ex.Message;
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        //sqlConnection.Close();
        //Console.WriteLine(SendMassages);
        return SendMassages;
    }

    static async Task<string?> SaveActualMoney(uint[] DataArray)
    {
        string? StateReturn=null;
        Console.WriteLine("Save Money");
        DateTime localDate = new();
        localDate = DateTime.UtcNow;
        Console.WriteLine(localDate.ToString());
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            var TempSQLCommand = new NpgsqlCommand($"SELECT OwnerID FROM Money WHERE OwnerID={DataArray[0]};", TempSQLConnection);
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            bool tempState = false;
            while (TempSQLDataReader.Read())
            {
                //sqlDataReader.NextResult();
                int tempSql = TempSQLDataReader.GetInt32(0);
                // Console.WriteLine(tempSql);
                tempState = true;
            }
            TempSQLDataReader.Close();
            if (tempState)
            {
                Console.WriteLine("Update DM");
                TempSQLCommand.Parameters.Clear();
                TempSQLCommand = new NpgsqlCommand($"UPDATE Money SET " +
                    $"DataTime=" +
                    $"@DataTime, " +
                    $"OwnerID=" +
                    $"@OwnerID, " +
                    $"Money=" +
                    $"@Money " +
                    $"WHERE OwnerID={DataArray[0]}",
                    TempSQLConnection);

                string sqlFormattedDate = localDate.ToString("yyyy-MM-dd  HH:mm:ss");
                TempSQLCommand.Parameters.AddWithValue("DataTime", sqlFormattedDate);
                TempSQLCommand.Parameters.AddWithValue("OwnerID", (int)DataArray[0]);
                TempSQLCommand.Parameters.AddWithValue("Money", (int)DataArray[1]);
                //Console.WriteLine(DataArray[1]);
                TempSQLCommand.ExecuteNonQuery();
            }
            else
            {
                Console.WriteLine("New DM");
                TempSQLCommand.Parameters.Clear();
                /*TempSQLCommand = new SQLiteCommand(*/
                TempSQLCommand = new NpgsqlCommand(
                   $"INSERT INTO Money (" +
                    $"DataTime," +
                    $"OwnerID," +
                    $"Money" +
                    $") VALUES (" +
                    $"@DataTime," +
                    $"@OwnerID," +
                    $"@Money" +
                    $") ",
                    TempSQLConnection);

                string sqlFormattedDate = localDate.ToString("yyyy-MM-dd  HH:mm:ss");
                TempSQLCommand.Parameters.AddWithValue("DataTime", sqlFormattedDate);
                TempSQLCommand.Parameters.AddWithValue("OwnerID", (int)DataArray[0]);
                TempSQLCommand.Parameters.AddWithValue("Money", (int)DataArray[1]);
                TempSQLCommand.ExecuteNonQuery();
            }
            StateReturn = "OK";
        }
        catch(Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            StateReturn = "E1";
            //DataBasueReConnect();
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        return StateReturn;
    }

    static async Task<string?> SendCommandAllMoney()
    {
        string? SendMassages = null;
        Console.WriteLine("ALL DM");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            //sqlCommand = new SQLiteCommand($"SELECT * FROM Devices", sqlConnection);
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Money", TempSQLConnection);
            DataTable dataTable = new();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            SendMassages = ex.Message;
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        //sqlConnection.Close();
        return SendMassages;
    }
    static async Task<string?> SendDeviceMoney(uint MyOwnerID)
    {
        string? SendMassages = null;
        Console.WriteLine("Owner DM");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            /* sqlCommand = new SQLiteCommand($"SELECT * FROM Config WHERE OwnerID={MyOwnerID}", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Money WHERE OwnerID={MyOwnerID}", TempSQLConnection);
            DataTable dataTable = new DataTable();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            SendMassages = ex.Message;
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        //sqlConnection.Close();
        //Console.WriteLine(SendMassages);
        return SendMassages;
    }
    static async Task<string?> SaveReservParam(SetMoney MyReserv)
    {
        string? returnState = null;
        Console.WriteLine("Save Reserv");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            /*sqlCommand = new SQLiteCommand($"SELECT OwnerID FROM Config WHERE OwnerID={config.OwnerID} AND ParamNO={config.ParamNO};", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT OwnerID FROM Reserv WHERE OwnerID={MyReserv.OwnerID};", TempSQLConnection);
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            bool tempState = false;
            while (TempSQLDataReader.Read())
            {
                int tempSql = TempSQLDataReader.GetInt32(0);
                tempState = true;
            }
            TempSQLDataReader.Close();
            if (tempState)
            {
                Console.WriteLine("Update Reserv paradeters");
                TempSQLCommand.Parameters.Clear();
                /*TempSQLCommand = new SQLiteCommand($"UPDATE [Config] SET " +*/
                TempSQLCommand = new NpgsqlCommand($"UPDATE Reserv SET " +
                    $"Money=" +
                    $"@Money, " +
                    $"Reserv=" +
                    $"@Reserv " +
                    $"WHERE OwnerID={MyReserv.OwnerID};",
                    TempSQLConnection);

                TempSQLCommand.Parameters.AddWithValue("Money", MyReserv.Money);
                TempSQLCommand.Parameters.AddWithValue("Reserv", MyReserv.Reserv);
                TempSQLCommand.ExecuteNonQuery();
            }
            else
            {
                Console.WriteLine("New Reserv");
                TempSQLCommand.Parameters.Clear();
                /* TempSQLCommand = new SQLiteCommand(*/
                TempSQLCommand = new NpgsqlCommand(
                    $"INSERT INTO Reserv (" +
                    $"OwnerID," +
                    $"Money," +
                    $"Reserv) " +
                    $"VALUES (" +
                    $"@OwnerID," +
                    $"@Money," +
                    $"@Reserv) ",
                    TempSQLConnection);

                TempSQLCommand.Parameters.AddWithValue("OwnerID", MyReserv.OwnerID);
                TempSQLCommand.Parameters.AddWithValue("Money", MyReserv.Money);
                TempSQLCommand.Parameters.AddWithValue("Reserv", MyReserv.Reserv);
                TempSQLCommand.ExecuteNonQuery();
            }
            returnState = "OK";
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            returnState = ex.Message;
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        return returnState;
    }
    static async Task<string?> SendCommandAllReserv()
    {
        string? SendMassages = null;
        Console.WriteLine("ALL DM");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            //sqlCommand = new SQLiteCommand($"SELECT * FROM Devices", sqlConnection);
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Reserv", TempSQLConnection);
            DataTable dataTable = new DataTable();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            SendMassages = ex.Message;
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        //sqlConnection.Close();
        return SendMassages;
    }
    static async Task<string?> SendDeviceReserv(uint MyOwnerID)
    {
        string? SendMassages = null;
        Console.WriteLine("Owner DM");
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            /* sqlCommand = new SQLiteCommand($"SELECT * FROM Config WHERE OwnerID={MyOwnerID}", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Reserv WHERE OwnerID={MyOwnerID}", TempSQLConnection);
            DataTable dataTable = new DataTable();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        //sqlConnection.Close();
        //Console.WriteLine(SendMassages);
        return SendMassages;
    }

    static async Task<string?> CheckReservDevice(uint[] DataArray)
    {
        string? SendMassage = null;
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        NpgsqlDataReader? TempSQLDataReader = null;
        try
        {
            //Console.WriteLine(DataArray[0]);
            /*sqlCommand = new SQLiteCommand($"SELECT * FROM Config WHERE OwnerID={DataArray[2]}", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"SELECT * FROM Reserv WHERE OwnerID={DataArray[0]}", TempSQLConnection);
            //Console.WriteLine("SQL OK");
            DataTable dataTable = new DataTable();
            TempSQLDataReader = await TempSQLCommand.ExecuteReaderAsync();
            dataTable.Load(TempSQLDataReader);
            if (dataTable != null)
            {
                string TempJSON = JsonConvert.SerializeObject(dataTable);
                //Console.WriteLine(TempJSON);
                SetMoney[]? reservDevices = JsonConvert.DeserializeObject<SetMoney[]>(TempJSON);
                bool noChang = true;
/*                Console.WriteLine(reservDevices[0].OwnerID);
                Console.WriteLine(reservDevices[0].Money);
                Console.WriteLine(reservDevices[0].Reserv);*/
                if (reservDevices != null && reservDevices.Length != 0)
                {
                    noChang = false;
                    SendMassage = "M-" + reservDevices[0].Money + "," + "R-" + reservDevices[0].Reserv + ",";
                }

                if (noChang)
                {
                    SendMassage = "NO2";
                }
                else
                {
                    SendMassage = "Reserv: " + SendMassage + "#";
                }
            }
            else
            {
                SendMassage = "NO1";
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            SendMassage = "ERROR";
            //DataBasueReConnect();
        }
        finally
        {
            Array.Clear(DataArray, 0, DataArray.Length);
            if (TempSQLDataReader != null && !TempSQLDataReader.IsClosed)
            {
                TempSQLDataReader.Close();
            }
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
        return SendMassage;
    }

    static async Task DeletingReserv(uint deletDevices)
    {
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        try
        {
            /*sqlCommand = new SQLiteCommand($"DELETE FROM Devices WHERE P2={deletDevices[i].OwnerID};", sqlConnection);*/
            var TempSQLCommand = new NpgsqlCommand($"DELETE FROM Reserv WHERE OwnerID={deletDevices};", TempSQLConnection);
            TempSQLCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
    }


    static async Task ConfigDataBasue()
    {
        //sqlDataReader = null;

        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();

        //////////////////Device/////////////////////
        try
        {
            var TempSQLCommand = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name  = 'devices') AS table_existence;", TempSQLConnection);
            var tempState = TempSQLCommand.ExecuteScalar();

            bool tempBool = Convert.ToBoolean(tempState);
            if (tempBool)
            {
                Console.WriteLine("YES Device");
                _ = CheckTable_DeleteNull("Devices", "P2");
            }
            else
            {
                Console.WriteLine("NO Device");
                _ = CreatDeviceTable();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        //////////////////END Device/////////////////////

        //////////////////Config/////////////////////
        try
        {
            var TempSQLCommand = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name  = 'config') AS table_existence;", TempSQLConnection);
            var tempState = TempSQLCommand.ExecuteScalar();
            //Console.WriteLine(tempState);
            bool tempBool = Convert.ToBoolean(tempState);
            if (tempBool)
            {
                Console.WriteLine("YES Config");
                _ = CheckTable_DeleteNull("Config", "OwnerID");
            }
            else
            {
                Console.WriteLine("NO Config");
                _ = CreatConfigTable();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        //////////////////END Config/////////////////////

        //////////////////Money/////////////////////
        try
        {
            var TempSQLCommand = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name  = 'money') AS table_existence;", TempSQLConnection);
            var tempState = TempSQLCommand.ExecuteScalar();
            //Console.WriteLine(tempState);
            bool tempBool = Convert.ToBoolean(tempState);
            if (tempBool)
            {
                Console.WriteLine("YES Money");
                _ = CheckTable_DeleteNull("Money", "OwnerID");
            }
            else
            {
                Console.WriteLine("NO Money");
                _ = CreatMoneyTable();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        //////////////////END Money/////////////////////

        //////////////////Set Money/////////////////////
        try
        {
            var TempSQLCommand = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name  = 'reserv') AS table_existence;", TempSQLConnection);
            var tempState = TempSQLCommand.ExecuteScalar();
            //Console.WriteLine(tempState);
            bool tempBool = Convert.ToBoolean(tempState);
            if (tempBool)
            {
                Console.WriteLine("YES Reserv");
                _ = CheckTable_DeleteNull("Reserv", "OwnerID");
            }
            else
            {
                Console.WriteLine("NO Reserv");
                _ = CreatReservTable();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        //////////////////END Set Money/////////////////////

        //////////////Close SQL Connection//////////////////
        if (TempSQLConnection.State != ConnectionState.Closed)
        {
            TempSQLConnection.Close();
        }
        ///////////////END Close SQL Connection//////////////////
    }

    static void URL_GET_ErrorCatch(HttpListenerContext context, Exception ex)
    {
        string errorMessage = ex.Message;
        var errorObject = new
        {
            error = errorMessage
        };
        string errorResponse = JsonConvert.SerializeObject(errorObject);
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        byte[] errorData = Encoding.UTF8.GetBytes(errorResponse);
        context.Response.OutputStream.Write(errorData, 0, errorData.Length);
    }
    static void URL_JsonResponse200(HttpListenerContext context ,string? jsonResponse)
    {
        if (jsonResponse != null)
        {
            byte[] responseJsonData = Encoding.UTF8.GetBytes(jsonResponse);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = responseJsonData.Length;
            context.Response.OutputStream.Write(responseJsonData, 0, responseJsonData.Length);
        }
        else
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
        }
    }
    static void URL_PostResponse200(HttpListenerContext context, string? FuncStatus)
    {
        if (FuncStatus == "OK")
        {
            context.Response.ContentType = "text/plain";
            byte[] responseData = Encoding.UTF8.GetBytes("Confirmed");
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.OutputStream.Write(responseData, 0, responseData.Length);
        }
        else if (FuncStatus != null)
        {
            context.Response.ContentType = "text/plain";
            byte[] responseData = Encoding.UTF8.GetBytes(FuncStatus);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.OutputStream.Write(responseData, 0, responseData.Length);
        }
        else
        {
            context.Response.ContentType = "text/plain";
            byte[] responseData = Encoding.UTF8.GetBytes("Json file is missing");
            context.Response.StatusCode = 400;
            context.Response.OutputStream.Write(responseData, 0, responseData.Length);
        }
    }
    static async Task CreatDeviceTable()
    {
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        try
        {
            var TempSQLCommand = new NpgsqlCommand($"CREATE TABLE Devices(" +
                $"DataTime CHAR(25)," +
                $"P0 INT," +
                $"P1 INT," +
                $"P2 INT NOT NULL PRIMARY KEY," +
                $"P3 INT," +
                $"P4 INT," +
                $"P5 INT," +
                $"P6 INT," +
                $"P7 INT," +
                $"P8 INT," +
                $"P9 INT," +
                $"P10 INT," +
                $"P11 INT," +
                $"P12 INT," +
                $"P13 INT," +
                $"P14 INT," +
                $"P15 INT," +
                $"P16 INT," +
                $"P17 INT," +
                $"P18 INT," +
                $"P19 INT," +
                $"P20 INT," +
                $"P21 INT," +
                $"P22 INT," +
                $"P23 INT," +
                $"P24 INT," +
                $"P25 INT," +
                $"P26 INT," +
                $"P27 INT," +
                $"P28 INT," +
                $"P29 INT," +
                $"P30 INT," +
                $"P31 INT," +
                $"P32 INT," +
                $"P33 INT," +
                $"P34 INT," +
                $"P35 INT," +
                $"P36 INT," +
                $"P37 INT," +
                $"P38 INT," +
                $"P39 INT," +
                $"P40 INT," +
                $"P41 INT," +
                $"P42 INT," +
                $"P43 INT," +
                $"P44 INT," +
                $"P45 INT," +
                $"P46 INT," +
                $"P47 INT," +
                $"P48 INT," +
                $"P49 INT," +
                $"P50 INT," +
                $"P51 INT," +
                $"P52 INT," +
                $"P53 INT," +
                $"P54 INT," +
                $"P55 INT," +
                $"P56 INT," +
                $"P57 INT," +
                $"P58 INT," +
                $"P59 INT," +
                $"P60 INT," +
                $"P61 INT," +
                $"P62 INT," +
                $"P63 INT," +
                $"P64 INT," +
                $"P65 INT," +
                $"P66 INT," +
                $"P67 INT," +
                $"P68 INT," +
                $"P69 INT," +
                $"P70 INT," +
                $"P71 INT," +
                $"P72 INT," +
                $"P73 INT," +
                $"P74 INT," +
                $"P75 INT," +
                $"P76 INT," +
                $"P77 INT," +
                $"P78 INT," +
                $"P79 INT," +
                $"P80 INT," +
                $"P81 INT," +
                $"P82 INT," +
                $"P83 INT," +
                $"P84 INT," +
                $"P85 INT," +
                $"P86 INT," +
                $"P87 INT," +
                $"P88 INT," +
                $"P89 INT," +
                $"P90 INT," +
                $"P91 INT," +
                $"P92 INT," +
                $"P93 INT," +
                $"P94 INT," +
                $"P95 INT," +
                $"P96 INT," +
                $"P97 INT," +
                $"P98 INT," +
                $"P99 INT," +
                $"P100 INT," +
                $"P101 INT," +
                $"P102 INT," +
                $"P103 INT," +
                $"P104 INT," +
                $"P105 INT," +
                $"P106 INT," +
                $"P107 INT," +
                $"P108 INT," +
                $"P109 INT," +
                $"P110 INT," +
                $"P111 INT," +
                $"P112 INT," +
                $"P113 INT," +
                $"P114 INT," +
                $"P115 INT," +
                $"P116 INT," +
                $"P117 INT," +
                $"P118 INT," +
                $"P119 INT," +
                $"P120 INT," +
                $"P121 INT," +
                $"P122 INT," +
                $"P123 INT," +
                $"P124 INT," +
                $"P125 INT," +
                $"P126 INT," +
                $"P127 INT," +
                $"P128 INT," +
                $"P129 INT," +
                $"P130 INT," +
                $"P131 INT," +
                $"P132 INT," +
                $"P133 INT," +
                $"P134 INT," +
                $"P135 INT," +
                $"P136 INT," +
                $"P137 INT," +
                $"P138 INT," +
                $"P139 INT," +
                $"P140 INT," +
                $"P141 INT," +
                $"P142 INT," +
                $"P143 INT," +
                $"P144 INT," +
                $"P145 INT," +
                $"P146 INT," +
                $"P147 INT," +
                $"P148 INT," +
                $"P149 INT" +
                $");", TempSQLConnection);
            TempSQLCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
    }
    static async Task CreatConfigTable()
    {
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        try
        {
            var TempSQLCommand = new NpgsqlCommand($"CREATE TABLE Config(" +
                $"OwnerID INT," +
                $"ParamNO INT," +
                $"NewData INT" +
                $");", TempSQLConnection);
            TempSQLCommand.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
    }
    static async Task CreatMoneyTable()
    {
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        try
        {
            var TempSQLCommand = new NpgsqlCommand($"CREATE TABLE Money(" +
                $"DataTime CHAR(25)," +
                $"OwnerID INT," +
                $"Money INT" +
                $");", TempSQLConnection);
            TempSQLCommand.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
    }
    static async Task CreatReservTable()
    {
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        try
        {
            var TempSQLCommand = new NpgsqlCommand($"CREATE TABLE Reserv(" +
                $"OwnerID INT NOT NULL PRIMARY KEY," +
                $"Money INT," +
                $"Reserv INT" +
                $");", TempSQLConnection);
            TempSQLCommand.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
    }
    static async Task CheckTable_DeleteNull(string TableName, string NameOfID)
    {
        using var TempSQLConnection = new NpgsqlConnection(connString);
        await TempSQLConnection.OpenAsync();
        try
        {
            /*sqlCommand = new SQLiteCommand($"DELETE FROM Devices WHERE P2={deletDevices[i].OwnerID};", sqlConnection);*/
            uint MinimumOwnerID = 0;
            var TempSQLCommand = new NpgsqlCommand($"DELETE FROM {TableName} WHERE {NameOfID}<={MinimumOwnerID};", TempSQLConnection);
            TempSQLCommand.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (TempSQLConnection.State != ConnectionState.Closed)
            {
                TempSQLConnection.Close();
            }
        }
    }
    /*static void DataBasueReConnect()
    {
        if (sqlConnection != null)
        {
            if (sqlConnection.State != ConnectionState.Open)
            {
                sqlConnection = new NpgsqlConnection(connString);
                sqlConnection.Open();
            }
            else
            {
                sqlConnection.Close();
            }
        }
        else
        {
            sqlConnection = new NpgsqlConnection(connString);
            if (sqlConnection.State != ConnectionState.Open)
            {
                sqlConnection = new NpgsqlConnection(connString);
                sqlConnection.Open();
            }
            else
            {
                sqlConnection.Close();
            }
        }
    }*/
}