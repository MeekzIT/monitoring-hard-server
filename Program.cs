// See https://aka.ms/new-console-template for more information
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
/*using System.Data.Sqlite;*/
//using System.Data.SQLite;
using Npgsql;
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

    //public static SQLiteConnection sqlConnection = null;
    public static NpgsqlConnection? sqlConnection = null;
    //string cS = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Hayk\Desktop\MonitoringServer\MonitoringServer\MoikaData.mdf;Integrated Security=True";
    // string cS = @"";

    /*public static SQLiteCommand sqlCommand = null;*/
    public static NpgsqlCommand? sqlCommand = null;

    /*public static SQLiteDataReader sqlDataReader = null;*/
    public static NpgsqlDataReader? sqlDataReader = null;
    static void Main(string[] args)
    {

        using IHost host = Host.CreateApplicationBuilder(args).Build();

        // Ask the service provider for the configuration abstraction.
        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

        // Get values from the config given their key and their target type.
        /*        int keyOneValue = config.GetValue<int>("KeyOne");
                bool keyTwoValue = config.GetValue<bool>("KeyTwo");
                string? keyThreeNestedValue = config.GetValue<string>("KeyThree:Message");

                // Write the values to the console.
                Console.WriteLine($"KeyOne = {keyOneValue}");
                Console.WriteLine($"KeyTwo = {keyTwoValue}");
                Console.WriteLine($"KeyThree:Message = {keyThreeNestedValue}");*/

        // Start TCP listener
        /*        if (File.Exists("MoikaData.db"))
                {
                    Console.WriteLine("Database is success");
                }
                else
                {
                    Console.WriteLine("Database is not success");
                    Environment.Exit(0);
                }*/
        //sqlConnection = new SQLiteConnection("Data Source=MoikaData.db; Version = 3;");
        string? connString = config.GetValue<string>("ConnectionStrings:PgDbConnection");
        Console.WriteLine(connString);
        sqlConnection = new NpgsqlConnection(connString);
        sqlConnection.Open();

        ConfigDataBasue();

        _ = Task.Run(() => StartTcpListener());

        _ = Task.Run(() => StartHttpListener());

        Console.WriteLine("Press Enter to exit.");
        while(true)
        {
            string? tempComm = Console.ReadLine();
            if (tempComm =="OFF")
            {
                break;
            }
        }
        sqlConnection.Close();
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
    static void ProcessHttpRequest(HttpListenerContext context)
    {
        // Handle HTTP request here
        // Example: Get the request URL and send a response

        string? requestUrl = null;
        char[]? UrlArray = null;
        string? requestPath = null;
        if (context.Request.Url != null)
        {
            requestUrl = context.Request.Url.ToString();
            UrlArray = context.Request.Url.PathAndQuery.ToArray();
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
                if (requestUrl.EndsWith("/"))
                {
                    try
                    {
                        string defoultData = $"<!doctype html>" +
                                            $"<html>" +
                                            $"<head>" +
                                            $"<title> This is the title of the webpage! </title>" +
                                            $"</head>" +
                                            $"<body>" +
                                            $"<p> This is an example paragraph.Anything in the<strong> body</strong> tag will appear on the page, just like this <strong> p </strong> tag and its contents.</p>" +
                                            $"</body>" +
                                            $"</html> ";
                        Console.WriteLine(requestUrl);
                        string? jsonResponse = SendCommandAll();
                        byte[] responseJsonData = Encoding.UTF8.GetBytes(defoultData);

                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "text/html";
                        context.Response.ContentLength64 = responseJsonData.Length;
                        context.Response.OutputStream.Write(responseJsonData, 0, responseJsonData.Length);
                    }
                    catch (Exception ex)
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
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                if (requestUrl.EndsWith("/api/v1/devices"))
                {
                    try
                    {
                        Console.WriteLine(requestUrl);
                        string? jsonResponse = SendCommandAll();
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
                    catch (Exception ex)
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
                    finally
                    {
                        context.Response.OutputStream.Close();
                    }
                }
                else if (requestUrl.EndsWith("/api/v1/config"))
                {
                    try
                    {
                        Console.WriteLine(requestUrl);
                        string? jsonResponse = SendConfigTable();
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
                    catch (Exception ex)
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

                        Console.WriteLine(requestUrl);
                        string? jsonResponse = SendOunerData(SendOwnerID);
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
                    catch (Exception ex)
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

                        Console.WriteLine(requestUrl);
                        string? jsonResponse = SendOunerConfig(SendOwnerID);
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
                    catch (Exception ex)
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

                        Console.WriteLine(requestUrl);
                        string? jsonResponse = SendAllOuners(SendOwnerID);
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
                    catch (Exception ex)
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
                            SaveConfigParamState = SaveConfigParam(configDevice);
                        }
                        if (SaveConfigParamState == "OK")
                        {
                            context.Response.ContentType = "text/plain";
                            byte[] responseData = Encoding.UTF8.GetBytes("Confirmed");
                            context.Response.OutputStream.Write(responseData, 0, responseData.Length);
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                        }
                        else if (SaveConfigParamState != null)
                        {
                            context.Response.ContentType = "text/plain";
                            byte[] responseData = Encoding.UTF8.GetBytes(SaveConfigParamState);
                            context.Response.OutputStream.Write(responseData, 0, responseData.Length);
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                        }

                    }
                    catch (Exception ex)
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
                            Deleting(deletDevices);
                        }

                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    catch (Exception ex)
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
            var stream = client.GetStream();
            var buffer = new byte[4096];
            uint[] TCPTempArray = new uint[150];
            Array.Clear(buffer, 0, buffer.Length);
            Array.Clear(TCPTempArray, 0, TCPTempArray.Length);

            while (true)
            {
                // Read data from the client
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                //var bytesRead = stream.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    // Client disconnected
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                //Console.WriteLine($"Received message from client: {message}");


                bool StartUncoding = false;
                int j = 0;
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
                    else if (message[i + 1] == ' ' && message[i] == ':' && message[i - 1] == 'e' && message[i - 2] == 'v' && message[i - 3] == 'a' && message[i - 4] == 'l' && message[i - 5] == 'S')
                    {
                        i++;
                        StartUncoding = true;
                    }
                }
                Console.WriteLine(message);
                if (StartUncoding)
                {
                    SQLWriteForTCP(TCPTempArray);
                    string? SendTCPMassage = CheckConfigDevice(TCPTempArray);
                    Console.WriteLine(SendTCPMassage);
                    if (SendTCPMassage != "NO1" && SendTCPMassage != "NO2" && SendTCPMassage != "ERROR" && SendTCPMassage != null)
                    {
                        byte[] sendClientData = Encoding.ASCII.GetBytes(SendTCPMassage);
                        stream.Write(sendClientData, 0, sendClientData.Length);
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

    static void SQLWriteForTCP(uint[] DataArray)
    {
        DateTime localDate = new DateTime();
        localDate = DateTime.UtcNow;
        Console.WriteLine(localDate.ToString());
        sqlDataReader = null;
        try
        {
            /*sqlCommand = new SQLiteCommand($"SELECT P2 FROM Devices WHERE P2={DataArray[2]};", sqlConnection);*/
            sqlCommand = new NpgsqlCommand($"SELECT P2 FROM Devices WHERE P2={DataArray[2]};", sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();
            bool tempState = false;
            while (sqlDataReader.Read())
            {
                //sqlDataReader.NextResult();
                int tempSql = sqlDataReader.GetInt32(0);
                // Console.WriteLine(tempSql);
                tempState = true;
            }
            sqlDataReader.Close();
            if (tempState)
            {
                Console.WriteLine("UPDATE DEVICE");
                //sqlCommand = new SQLiteCommand($"UPDATE [Devices] SET " +
                sqlCommand = new NpgsqlCommand($"UPDATE Devices SET " +
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
                    sqlConnection);

                string sqlFormattedDate = localDate.ToString("yyyy-MM-dd  HH:mm:ss");
                sqlCommand.Parameters.AddWithValue("DataTime", sqlFormattedDate);
                sqlCommand.Parameters.AddWithValue("P0", (int)DataArray[0]);
                sqlCommand.Parameters.AddWithValue("P1", (int)DataArray[1]);
                sqlCommand.Parameters.AddWithValue("P2", (int)DataArray[2]);
                sqlCommand.Parameters.AddWithValue("P3", (int)DataArray[3]);
                sqlCommand.Parameters.AddWithValue("P4", (int)DataArray[4]);
                sqlCommand.Parameters.AddWithValue("P5", (int)DataArray[5]);
                sqlCommand.Parameters.AddWithValue("P6", (int)DataArray[6]);
                sqlCommand.Parameters.AddWithValue("P7", (int)DataArray[7]);
                sqlCommand.Parameters.AddWithValue("P8", (int)DataArray[8]);
                sqlCommand.Parameters.AddWithValue("P9", (int)DataArray[9]);
                sqlCommand.Parameters.AddWithValue("P10", (int)DataArray[10]);
                sqlCommand.Parameters.AddWithValue("P11", (int)DataArray[11]);
                sqlCommand.Parameters.AddWithValue("P12", (int)DataArray[12]);
                sqlCommand.Parameters.AddWithValue("P13", (int)DataArray[13]);
                sqlCommand.Parameters.AddWithValue("P14", (int)DataArray[14]);
                sqlCommand.Parameters.AddWithValue("P15", (int)DataArray[15]);
                sqlCommand.Parameters.AddWithValue("P16", (int)DataArray[16]);
                sqlCommand.Parameters.AddWithValue("P17", (int)DataArray[17]);
                sqlCommand.Parameters.AddWithValue("P18", (int)DataArray[18]);
                sqlCommand.Parameters.AddWithValue("P19", (int)DataArray[19]);
                sqlCommand.Parameters.AddWithValue("P20", (int)DataArray[20]);
                sqlCommand.Parameters.AddWithValue("P21", (int)DataArray[21]);
                sqlCommand.Parameters.AddWithValue("P22", (int)DataArray[22]);
                sqlCommand.Parameters.AddWithValue("P23", (int)DataArray[23]);
                sqlCommand.Parameters.AddWithValue("P24", (int)DataArray[24]);
                sqlCommand.Parameters.AddWithValue("P25", (int)DataArray[25]);
                sqlCommand.Parameters.AddWithValue("P26", (int)DataArray[26]);
                sqlCommand.Parameters.AddWithValue("P27", (int)DataArray[27]);
                sqlCommand.Parameters.AddWithValue("P28", (int)DataArray[28]);
                sqlCommand.Parameters.AddWithValue("P29", (int)DataArray[29]);
                sqlCommand.Parameters.AddWithValue("P30", (int)DataArray[30]);
                sqlCommand.Parameters.AddWithValue("P31", (int)DataArray[31]);
                sqlCommand.Parameters.AddWithValue("P32", (int)DataArray[32]);
                sqlCommand.Parameters.AddWithValue("P33", (int)DataArray[33]);
                sqlCommand.Parameters.AddWithValue("P34", (int)DataArray[34]);
                sqlCommand.Parameters.AddWithValue("P35", (int)DataArray[35]);
                sqlCommand.Parameters.AddWithValue("P36", (int)DataArray[36]);
                sqlCommand.Parameters.AddWithValue("P37", (int)DataArray[37]);
                sqlCommand.Parameters.AddWithValue("P38", (int)DataArray[38]);
                sqlCommand.Parameters.AddWithValue("P39", (int)DataArray[39]);
                sqlCommand.Parameters.AddWithValue("P40", (int)DataArray[40]);
                sqlCommand.Parameters.AddWithValue("P41", (int)DataArray[41]);
                sqlCommand.Parameters.AddWithValue("P42", (int)DataArray[42]);
                sqlCommand.Parameters.AddWithValue("P43", (int)DataArray[43]);
                sqlCommand.Parameters.AddWithValue("P44", (int)DataArray[44]);
                sqlCommand.Parameters.AddWithValue("P45", (int)DataArray[45]);
                sqlCommand.Parameters.AddWithValue("P46", (int)DataArray[46]);
                sqlCommand.Parameters.AddWithValue("P47", (int)DataArray[47]);
                sqlCommand.Parameters.AddWithValue("P48", (int)DataArray[48]);
                sqlCommand.Parameters.AddWithValue("P49", (int)DataArray[49]);
                sqlCommand.Parameters.AddWithValue("P50", (int)DataArray[50]);
                sqlCommand.Parameters.AddWithValue("P51", (int)DataArray[51]);
                sqlCommand.Parameters.AddWithValue("P52", (int)DataArray[52]);
                sqlCommand.Parameters.AddWithValue("P53", (int)DataArray[53]);
                sqlCommand.Parameters.AddWithValue("P54", (int)DataArray[54]);
                sqlCommand.Parameters.AddWithValue("P55", (int)DataArray[55]);
                sqlCommand.Parameters.AddWithValue("P56", (int)DataArray[56]);
                sqlCommand.Parameters.AddWithValue("P57", (int)DataArray[57]);
                sqlCommand.Parameters.AddWithValue("P58", (int)DataArray[58]);
                sqlCommand.Parameters.AddWithValue("P59", (int)DataArray[59]);
                sqlCommand.Parameters.AddWithValue("P60", (int)DataArray[60]);
                sqlCommand.Parameters.AddWithValue("P61", (int)DataArray[61]);
                sqlCommand.Parameters.AddWithValue("P62", (int)DataArray[62]);
                sqlCommand.Parameters.AddWithValue("P63", (int)DataArray[63]);
                sqlCommand.Parameters.AddWithValue("P64", (int)DataArray[64]);
                sqlCommand.Parameters.AddWithValue("P65", (int)DataArray[65]);
                sqlCommand.Parameters.AddWithValue("P66", (int)DataArray[66]);
                sqlCommand.Parameters.AddWithValue("P67", (int)DataArray[67]);
                sqlCommand.Parameters.AddWithValue("P68", (int)DataArray[68]);
                sqlCommand.Parameters.AddWithValue("P69", (int)DataArray[69]);
                sqlCommand.Parameters.AddWithValue("P70", (int)DataArray[70]);
                sqlCommand.Parameters.AddWithValue("P71", (int)DataArray[71]);
                sqlCommand.Parameters.AddWithValue("P72", (int)DataArray[72]);
                sqlCommand.Parameters.AddWithValue("P73", (int)DataArray[73]);
                sqlCommand.Parameters.AddWithValue("P74", (int)DataArray[74]);
                sqlCommand.Parameters.AddWithValue("P75", (int)DataArray[75]);
                sqlCommand.Parameters.AddWithValue("P76", (int)DataArray[76]);
                sqlCommand.Parameters.AddWithValue("P77", (int)DataArray[77]);
                sqlCommand.Parameters.AddWithValue("P78", (int)DataArray[78]);
                sqlCommand.Parameters.AddWithValue("P79", (int)DataArray[79]);
                sqlCommand.Parameters.AddWithValue("P80", (int)DataArray[80]);
                sqlCommand.Parameters.AddWithValue("P81", (int)DataArray[81]);
                sqlCommand.Parameters.AddWithValue("P82", (int)DataArray[82]);
                sqlCommand.Parameters.AddWithValue("P83", (int)DataArray[83]);
                sqlCommand.Parameters.AddWithValue("P84", (int)DataArray[84]);
                sqlCommand.Parameters.AddWithValue("P85", (int)DataArray[85]);
                sqlCommand.Parameters.AddWithValue("P86", (int)DataArray[86]);
                sqlCommand.Parameters.AddWithValue("P87", (int)DataArray[87]);
                sqlCommand.Parameters.AddWithValue("P88", (int)DataArray[88]);
                sqlCommand.Parameters.AddWithValue("P89", (int)DataArray[89]);
                sqlCommand.Parameters.AddWithValue("P90", (int)DataArray[90]);
                sqlCommand.Parameters.AddWithValue("P91", (int)DataArray[91]);
                sqlCommand.Parameters.AddWithValue("P92", (int)DataArray[92]);
                sqlCommand.Parameters.AddWithValue("P93", (int)DataArray[93]);
                sqlCommand.Parameters.AddWithValue("P94", (int)DataArray[94]);
                sqlCommand.Parameters.AddWithValue("P95", (int)DataArray[95]);
                sqlCommand.Parameters.AddWithValue("P96", (int)DataArray[96]);
                sqlCommand.Parameters.AddWithValue("P97", (int)DataArray[97]);
                sqlCommand.Parameters.AddWithValue("P98", (int)DataArray[98]);
                sqlCommand.Parameters.AddWithValue("P99", (int)DataArray[99]);
                sqlCommand.Parameters.AddWithValue("P100", (int)DataArray[100]);
                sqlCommand.Parameters.AddWithValue("P101", (int)DataArray[101]);
                sqlCommand.Parameters.AddWithValue("P102", (int)DataArray[102]);
                sqlCommand.Parameters.AddWithValue("P103", (int)DataArray[103]);
                sqlCommand.Parameters.AddWithValue("P104", (int)DataArray[104]);
                sqlCommand.Parameters.AddWithValue("P105", (int)DataArray[105]);
                sqlCommand.Parameters.AddWithValue("P106", (int)DataArray[106]);
                sqlCommand.Parameters.AddWithValue("P107", (int)DataArray[107]);
                sqlCommand.Parameters.AddWithValue("P108", (int)DataArray[108]);
                sqlCommand.Parameters.AddWithValue("P109", (int)DataArray[109]);
                sqlCommand.Parameters.AddWithValue("P110", (int)DataArray[110]);
                sqlCommand.Parameters.AddWithValue("P111", (int)DataArray[111]);
                sqlCommand.Parameters.AddWithValue("P112", (int)DataArray[112]);
                sqlCommand.Parameters.AddWithValue("P113", (int)DataArray[113]);
                sqlCommand.Parameters.AddWithValue("P114", (int)DataArray[114]);
                sqlCommand.Parameters.AddWithValue("P115", (int)DataArray[115]);
                sqlCommand.Parameters.AddWithValue("P116", (int)DataArray[116]);
                sqlCommand.Parameters.AddWithValue("P117", (int)DataArray[117]);
                sqlCommand.Parameters.AddWithValue("P118", (int)DataArray[118]);
                sqlCommand.Parameters.AddWithValue("P119", (int)DataArray[119]);
                sqlCommand.Parameters.AddWithValue("P120", (int)DataArray[120]);
                sqlCommand.Parameters.AddWithValue("P121", (int)DataArray[121]);
                sqlCommand.Parameters.AddWithValue("P122", (int)DataArray[122]);
                sqlCommand.Parameters.AddWithValue("P123", (int)DataArray[123]);
                sqlCommand.Parameters.AddWithValue("P124", (int)DataArray[124]);
                sqlCommand.Parameters.AddWithValue("P125", (int)DataArray[125]);
                sqlCommand.Parameters.AddWithValue("P126", (int)DataArray[126]);
                sqlCommand.Parameters.AddWithValue("P127", (int)DataArray[127]);
                sqlCommand.Parameters.AddWithValue("P128", (int)DataArray[128]);
                sqlCommand.Parameters.AddWithValue("P129", (int)DataArray[129]);
                sqlCommand.Parameters.AddWithValue("P130", (int)DataArray[130]);
                sqlCommand.Parameters.AddWithValue("P131", (int)DataArray[131]);
                sqlCommand.Parameters.AddWithValue("P132", (int)DataArray[132]);
                sqlCommand.Parameters.AddWithValue("P133", (int)DataArray[133]);
                sqlCommand.Parameters.AddWithValue("P134", (int)DataArray[134]);
                sqlCommand.Parameters.AddWithValue("P135", (int)DataArray[135]);
                sqlCommand.Parameters.AddWithValue("P136", (int)DataArray[136]);
                sqlCommand.Parameters.AddWithValue("P137", (int)DataArray[137]);
                sqlCommand.Parameters.AddWithValue("P138", (int)DataArray[138]);
                sqlCommand.Parameters.AddWithValue("P139", (int)DataArray[139]);
                sqlCommand.Parameters.AddWithValue("P140", (int)DataArray[140]);
                sqlCommand.Parameters.AddWithValue("P141", (int)DataArray[141]);
                sqlCommand.Parameters.AddWithValue("P142", (int)DataArray[142]);
                sqlCommand.Parameters.AddWithValue("P143", (int)DataArray[143]);
                sqlCommand.Parameters.AddWithValue("P144", (int)DataArray[144]);
                sqlCommand.Parameters.AddWithValue("P145", (int)DataArray[145]);
                sqlCommand.Parameters.AddWithValue("P146", (int)DataArray[146]);
                sqlCommand.Parameters.AddWithValue("P147", (int)DataArray[147]);
                sqlCommand.Parameters.AddWithValue("P148", (int)DataArray[148]);
                sqlCommand.Parameters.AddWithValue("P149", (int)DataArray[149]);
                sqlCommand.ExecuteNonQuery();
            }
            else
            {
                Console.WriteLine("NEW DEVICE");
                sqlCommand.Parameters.Clear();
                /*sqlCommand = new SQLiteCommand(*/
                sqlCommand = new NpgsqlCommand(
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
                    sqlConnection);

                string sqlFormattedDate = localDate.ToString("yyyy-MM-dd  HH:mm:ss");
                sqlCommand.Parameters.AddWithValue("DataTime", sqlFormattedDate);
                sqlCommand.Parameters.AddWithValue("P0", (int)DataArray[0]);
                sqlCommand.Parameters.AddWithValue("P1", (int)DataArray[1]);
                sqlCommand.Parameters.AddWithValue("P2", (int)DataArray[2]);
                sqlCommand.Parameters.AddWithValue("P3", (int)DataArray[3]);
                sqlCommand.Parameters.AddWithValue("P4", (int)DataArray[4]);
                sqlCommand.Parameters.AddWithValue("P5", (int)DataArray[5]);
                sqlCommand.Parameters.AddWithValue("P6", (int)DataArray[6]);
                sqlCommand.Parameters.AddWithValue("P7", (int)DataArray[7]);
                sqlCommand.Parameters.AddWithValue("P8", (int)DataArray[8]);
                sqlCommand.Parameters.AddWithValue("P9", (int)DataArray[9]);
                sqlCommand.Parameters.AddWithValue("P10", (int)DataArray[10]);
                sqlCommand.Parameters.AddWithValue("P11", (int)DataArray[11]);
                sqlCommand.Parameters.AddWithValue("P12", (int)DataArray[12]);
                sqlCommand.Parameters.AddWithValue("P13", (int)DataArray[13]);
                sqlCommand.Parameters.AddWithValue("P14", (int)DataArray[14]);
                sqlCommand.Parameters.AddWithValue("P15", (int)DataArray[15]);
                sqlCommand.Parameters.AddWithValue("P16", (int)DataArray[16]);
                sqlCommand.Parameters.AddWithValue("P17", (int)DataArray[17]);
                sqlCommand.Parameters.AddWithValue("P18", (int)DataArray[18]);
                sqlCommand.Parameters.AddWithValue("P19", (int)DataArray[19]);
                sqlCommand.Parameters.AddWithValue("P20", (int)DataArray[20]);
                sqlCommand.Parameters.AddWithValue("P21", (int)DataArray[21]);
                sqlCommand.Parameters.AddWithValue("P22", (int)DataArray[22]);
                sqlCommand.Parameters.AddWithValue("P23", (int)DataArray[23]);
                sqlCommand.Parameters.AddWithValue("P24", (int)DataArray[24]);
                sqlCommand.Parameters.AddWithValue("P25", (int)DataArray[25]);
                sqlCommand.Parameters.AddWithValue("P26", (int)DataArray[26]);
                sqlCommand.Parameters.AddWithValue("P27", (int)DataArray[27]);
                sqlCommand.Parameters.AddWithValue("P28", (int)DataArray[28]);
                sqlCommand.Parameters.AddWithValue("P29", (int)DataArray[29]);
                sqlCommand.Parameters.AddWithValue("P30", (int)DataArray[30]);
                sqlCommand.Parameters.AddWithValue("P31", (int)DataArray[31]);
                sqlCommand.Parameters.AddWithValue("P32", (int)DataArray[32]);
                sqlCommand.Parameters.AddWithValue("P33", (int)DataArray[33]);
                sqlCommand.Parameters.AddWithValue("P34", (int)DataArray[34]);
                sqlCommand.Parameters.AddWithValue("P35", (int)DataArray[35]);
                sqlCommand.Parameters.AddWithValue("P36", (int)DataArray[36]);
                sqlCommand.Parameters.AddWithValue("P37", (int)DataArray[37]);
                sqlCommand.Parameters.AddWithValue("P38", (int)DataArray[38]);
                sqlCommand.Parameters.AddWithValue("P39", (int)DataArray[39]);
                sqlCommand.Parameters.AddWithValue("P40", (int)DataArray[40]);
                sqlCommand.Parameters.AddWithValue("P41", (int)DataArray[41]);
                sqlCommand.Parameters.AddWithValue("P42", (int)DataArray[42]);
                sqlCommand.Parameters.AddWithValue("P43", (int)DataArray[43]);
                sqlCommand.Parameters.AddWithValue("P44", (int)DataArray[44]);
                sqlCommand.Parameters.AddWithValue("P45", (int)DataArray[45]);
                sqlCommand.Parameters.AddWithValue("P46", (int)DataArray[46]);
                sqlCommand.Parameters.AddWithValue("P47", (int)DataArray[47]);
                sqlCommand.Parameters.AddWithValue("P48", (int)DataArray[48]);
                sqlCommand.Parameters.AddWithValue("P49", (int)DataArray[49]);
                sqlCommand.Parameters.AddWithValue("P50", (int)DataArray[50]);
                sqlCommand.Parameters.AddWithValue("P51", (int)DataArray[51]);
                sqlCommand.Parameters.AddWithValue("P52", (int)DataArray[52]);
                sqlCommand.Parameters.AddWithValue("P53", (int)DataArray[53]);
                sqlCommand.Parameters.AddWithValue("P54", (int)DataArray[54]);
                sqlCommand.Parameters.AddWithValue("P55", (int)DataArray[55]);
                sqlCommand.Parameters.AddWithValue("P56", (int)DataArray[56]);
                sqlCommand.Parameters.AddWithValue("P57", (int)DataArray[57]);
                sqlCommand.Parameters.AddWithValue("P58", (int)DataArray[58]);
                sqlCommand.Parameters.AddWithValue("P59", (int)DataArray[59]);
                sqlCommand.Parameters.AddWithValue("P60", (int)DataArray[60]);
                sqlCommand.Parameters.AddWithValue("P61", (int)DataArray[61]);
                sqlCommand.Parameters.AddWithValue("P62", (int)DataArray[62]);
                sqlCommand.Parameters.AddWithValue("P63", (int)DataArray[63]);
                sqlCommand.Parameters.AddWithValue("P64", (int)DataArray[64]);
                sqlCommand.Parameters.AddWithValue("P65", (int)DataArray[65]);
                sqlCommand.Parameters.AddWithValue("P66", (int)DataArray[66]);
                sqlCommand.Parameters.AddWithValue("P67", (int)DataArray[67]);
                sqlCommand.Parameters.AddWithValue("P68", (int)DataArray[68]);
                sqlCommand.Parameters.AddWithValue("P69", (int)DataArray[69]);
                sqlCommand.Parameters.AddWithValue("P70", (int)DataArray[70]);
                sqlCommand.Parameters.AddWithValue("P71", (int)DataArray[71]);
                sqlCommand.Parameters.AddWithValue("P72", (int)DataArray[72]);
                sqlCommand.Parameters.AddWithValue("P73", (int)DataArray[73]);
                sqlCommand.Parameters.AddWithValue("P74", (int)DataArray[74]);
                sqlCommand.Parameters.AddWithValue("P75", (int)DataArray[75]);
                sqlCommand.Parameters.AddWithValue("P76", (int)DataArray[76]);
                sqlCommand.Parameters.AddWithValue("P77", (int)DataArray[77]);
                sqlCommand.Parameters.AddWithValue("P78", (int)DataArray[78]);
                sqlCommand.Parameters.AddWithValue("P79", (int)DataArray[79]);
                sqlCommand.Parameters.AddWithValue("P80", (int)DataArray[80]);
                sqlCommand.Parameters.AddWithValue("P81", (int)DataArray[81]);
                sqlCommand.Parameters.AddWithValue("P82", (int)DataArray[82]);
                sqlCommand.Parameters.AddWithValue("P83", (int)DataArray[83]);
                sqlCommand.Parameters.AddWithValue("P84", (int)DataArray[84]);
                sqlCommand.Parameters.AddWithValue("P85", (int)DataArray[85]);
                sqlCommand.Parameters.AddWithValue("P86", (int)DataArray[86]);
                sqlCommand.Parameters.AddWithValue("P87", (int)DataArray[87]);
                sqlCommand.Parameters.AddWithValue("P88", (int)DataArray[88]);
                sqlCommand.Parameters.AddWithValue("P89", (int)DataArray[89]);
                sqlCommand.Parameters.AddWithValue("P90", (int)DataArray[90]);
                sqlCommand.Parameters.AddWithValue("P91", (int)DataArray[91]);
                sqlCommand.Parameters.AddWithValue("P92", (int)DataArray[92]);
                sqlCommand.Parameters.AddWithValue("P93", (int)DataArray[93]);
                sqlCommand.Parameters.AddWithValue("P94", (int)DataArray[94]);
                sqlCommand.Parameters.AddWithValue("P95", (int)DataArray[95]);
                sqlCommand.Parameters.AddWithValue("P96", (int)DataArray[96]);
                sqlCommand.Parameters.AddWithValue("P97", (int)DataArray[97]);
                sqlCommand.Parameters.AddWithValue("P98", (int)DataArray[98]);
                sqlCommand.Parameters.AddWithValue("P99", (int)DataArray[99]);
                sqlCommand.Parameters.AddWithValue("P100", (int)DataArray[100]);
                sqlCommand.Parameters.AddWithValue("P101", (int)DataArray[101]);
                sqlCommand.Parameters.AddWithValue("P102", (int)DataArray[102]);
                sqlCommand.Parameters.AddWithValue("P103", (int)DataArray[103]);
                sqlCommand.Parameters.AddWithValue("P104", (int)DataArray[104]);
                sqlCommand.Parameters.AddWithValue("P105", (int)DataArray[105]);
                sqlCommand.Parameters.AddWithValue("P106", (int)DataArray[106]);
                sqlCommand.Parameters.AddWithValue("P107", (int)DataArray[107]);
                sqlCommand.Parameters.AddWithValue("P108", (int)DataArray[108]);
                sqlCommand.Parameters.AddWithValue("P109", (int)DataArray[109]);
                sqlCommand.Parameters.AddWithValue("P110", (int)DataArray[110]);
                sqlCommand.Parameters.AddWithValue("P111", (int)DataArray[111]);
                sqlCommand.Parameters.AddWithValue("P112", (int)DataArray[112]);
                sqlCommand.Parameters.AddWithValue("P113", (int)DataArray[113]);
                sqlCommand.Parameters.AddWithValue("P114", (int)DataArray[114]);
                sqlCommand.Parameters.AddWithValue("P115", (int)DataArray[115]);
                sqlCommand.Parameters.AddWithValue("P116", (int)DataArray[116]);
                sqlCommand.Parameters.AddWithValue("P117", (int)DataArray[117]);
                sqlCommand.Parameters.AddWithValue("P118", (int)DataArray[118]);
                sqlCommand.Parameters.AddWithValue("P119", (int)DataArray[119]);
                sqlCommand.Parameters.AddWithValue("P120", (int)DataArray[120]);
                sqlCommand.Parameters.AddWithValue("P121", (int)DataArray[121]);
                sqlCommand.Parameters.AddWithValue("P122", (int)DataArray[122]);
                sqlCommand.Parameters.AddWithValue("P123", (int)DataArray[123]);
                sqlCommand.Parameters.AddWithValue("P124", (int)DataArray[124]);
                sqlCommand.Parameters.AddWithValue("P125", (int)DataArray[125]);
                sqlCommand.Parameters.AddWithValue("P126", (int)DataArray[126]);
                sqlCommand.Parameters.AddWithValue("P127", (int)DataArray[127]);
                sqlCommand.Parameters.AddWithValue("P128", (int)DataArray[128]);
                sqlCommand.Parameters.AddWithValue("P129", (int)DataArray[129]);
                sqlCommand.Parameters.AddWithValue("P130", (int)DataArray[130]);
                sqlCommand.Parameters.AddWithValue("P131", (int)DataArray[131]);
                sqlCommand.Parameters.AddWithValue("P132", (int)DataArray[132]);
                sqlCommand.Parameters.AddWithValue("P133", (int)DataArray[133]);
                sqlCommand.Parameters.AddWithValue("P134", (int)DataArray[134]);
                sqlCommand.Parameters.AddWithValue("P135", (int)DataArray[135]);
                sqlCommand.Parameters.AddWithValue("P136", (int)DataArray[136]);
                sqlCommand.Parameters.AddWithValue("P137", (int)DataArray[137]);
                sqlCommand.Parameters.AddWithValue("P138", (int)DataArray[138]);
                sqlCommand.Parameters.AddWithValue("P139", (int)DataArray[139]);
                sqlCommand.Parameters.AddWithValue("P140", (int)DataArray[140]);
                sqlCommand.Parameters.AddWithValue("P141", (int)DataArray[141]);
                sqlCommand.Parameters.AddWithValue("P142", (int)DataArray[142]);
                sqlCommand.Parameters.AddWithValue("P143", (int)DataArray[143]);
                sqlCommand.Parameters.AddWithValue("P144", (int)DataArray[144]);
                sqlCommand.Parameters.AddWithValue("P145", (int)DataArray[145]);
                sqlCommand.Parameters.AddWithValue("P146", (int)DataArray[146]);
                sqlCommand.Parameters.AddWithValue("P147", (int)DataArray[147]);
                sqlCommand.Parameters.AddWithValue("P148", (int)DataArray[148]);
                sqlCommand.Parameters.AddWithValue("P149", (int)DataArray[149]);
                sqlCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            //Array.Clear(DataArray,0, DataArray.Length);
           if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
    }
    static string? SendCommandAll()
    {
        string? SendMassages = null;
        Console.WriteLine("ALL");
        sqlDataReader = null;
        try
        {
            //sqlCommand = new SQLiteCommand($"SELECT * FROM Devices", sqlConnection);
            sqlCommand = new NpgsqlCommand($"SELECT * FROM Devices", sqlConnection);
            DataTable dataTable = new DataTable();
            sqlDataReader = sqlCommand.ExecuteReader();
            dataTable.Load(sqlDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
        //sqlConnection.Close();
        return SendMassages;
    }
    static string? SendConfigTable()
    {
        string? SendMassages = null;
        Console.WriteLine("Config");
        sqlDataReader = null;
        try
        {
            /*sqlCommand = new SQLiteCommand($"SELECT * FROM Config", sqlConnection);*/
            sqlCommand = new NpgsqlCommand($"SELECT * FROM Config", sqlConnection);
            DataTable dataTable = new DataTable();
            sqlDataReader = sqlCommand.ExecuteReader();
            dataTable.Load(sqlDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
        //sqlConnection.Close();
        return SendMassages;
    }
    static string? SendOunerData(uint MyOwnerID)
    {
        string? SendMassages = null;
        Console.WriteLine("Owner ID");
        sqlDataReader = null;
        try
        {
            /*       sqlCommand = new SQLiteCommand($"SELECT * FROM Devices WHERE P2={MyOwnerID}", sqlConnection);*/
            sqlCommand = new NpgsqlCommand($"SELECT * FROM Devices WHERE P2={MyOwnerID}", sqlConnection);
            DataTable dataTable = new DataTable();
            sqlDataReader = sqlCommand.ExecuteReader();
            dataTable.Load(sqlDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
        //sqlConnection.Close();
        //Console.WriteLine(SendMassages);
        return SendMassages;
    }
    static string? SaveConfigParam(ConfigDevice config)
    {
        string? returnState = null;
        Console.WriteLine("Save Config");
        sqlDataReader = null;
        try
        {
            if (config.ParamNO > 2)
            {
                /*sqlCommand = new SQLiteCommand($"SELECT OwnerID FROM Config WHERE OwnerID={config.OwnerID} AND ParamNO={config.ParamNO};", sqlConnection);*/
                sqlCommand = new NpgsqlCommand($"SELECT OwnerID FROM Config WHERE OwnerID={config.OwnerID} AND ParamNO={config.ParamNO};", sqlConnection);
                sqlDataReader = sqlCommand.ExecuteReader();
                bool tempState = false;
                while (sqlDataReader.Read())
                {
                    int tempSql = sqlDataReader.GetInt32(0);
                    tempState = true;
                }
                sqlDataReader.Close();
                if (tempState)
                {
                    Console.WriteLine("UPDATE Config paradeters");
                    /*sqlCommand = new SQLiteCommand($"UPDATE [Config] SET " +*/
                    sqlCommand = new NpgsqlCommand($"UPDATE Config SET " +
                        $"NewData=" +
                        $"@NewData " +
                        $"WHERE OwnerID={config.OwnerID} AND ParamNO={config.ParamNO};",
                        sqlConnection);

                    sqlCommand.Parameters.AddWithValue("NewData", config.NewData);
                    sqlCommand.ExecuteNonQuery();
                }
                else
                {
                    Console.WriteLine("NEW Config");
                    sqlCommand.Parameters.Clear();
                    /* sqlCommand = new SQLiteCommand(*/
                    sqlCommand = new NpgsqlCommand(
                        $"INSERT INTO Config (" +
                        $"OwnerID," +
                        $"ParamNO," +
                        $"NewData) " +
                        $"VALUES (" +
                        $"@OwnerID," +
                        $"@ParamNO," +
                        $"@NewData) ",
                        sqlConnection);

                    sqlCommand.Parameters.AddWithValue("OwnerID", config.OwnerID);
                    sqlCommand.Parameters.AddWithValue("ParamNO", config.ParamNO);
                    sqlCommand.Parameters.AddWithValue("NewData", config.NewData);
                    sqlCommand.ExecuteNonQuery();
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
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
        return returnState;
    }
    static string? CheckConfigDevice(uint[] DataArray)
    {
        string? SendMassage = null;
        sqlDataReader = null;
        try
        {
            Console.WriteLine(DataArray[2]);
            /*sqlCommand = new SQLiteCommand($"SELECT * FROM Config WHERE OwnerID={DataArray[2]}", sqlConnection);*/
            sqlCommand = new NpgsqlCommand($"SELECT * FROM Config WHERE OwnerID={DataArray[2]}", sqlConnection);
            DataTable dataTable = new DataTable();
            sqlDataReader = sqlCommand.ExecuteReader();
            dataTable.Load(sqlDataReader);
            if (dataTable != null)
            {
                string TempJSON = JsonConvert.SerializeObject(dataTable);
                //Console.WriteLine(TempJSON);
                ConfigDevice[]? configDevices = JsonConvert.DeserializeObject<ConfigDevice[]>(TempJSON);
                bool noChang = true;
                if(configDevices!=null)
                {
                    for (int i = 0; i < configDevices.Length; i++)
                    {
                        //Console.WriteLine(configDevices[i].ToString());
                        if (DataArray[configDevices[i].ParamNO] != configDevices[i].NewData)
                        {
                            noChang = false;
                            SendMassage = SendMassage + "P" + configDevices[i].ParamNO.ToString() + "-" + configDevices[i].NewData + ",";
                        }
                        else if (DataArray[configDevices[i].ParamNO] == configDevices[i].NewData)
                        {
                            /* sqlCommand = new SQLiteCommand($"DELETE FROM Config WHERE OwnerID={DataArray[2]} AND ParamNO={configDevices[i].ParamNO};", sqlConnection);*/
                            sqlCommand = new NpgsqlCommand($"DELETE FROM Config WHERE OwnerID={DataArray[2]} AND ParamNO={configDevices[i].ParamNO};", sqlConnection);
                            sqlCommand.ExecuteNonQuery();
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
        }
        finally
        {
            Array.Clear(DataArray, 0, DataArray.Length);
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
        return SendMassage;
    }
    static string? SendOunerConfig(uint MyOwnerID)
    {
        string? SendMassages = null;
        Console.WriteLine("Owner ID");
        sqlDataReader = null;
        try
        {
            /* sqlCommand = new SQLiteCommand($"SELECT * FROM Config WHERE OwnerID={MyOwnerID}", sqlConnection);*/
            sqlCommand = new NpgsqlCommand($"SELECT * FROM Config WHERE OwnerID={MyOwnerID}", sqlConnection);
            DataTable dataTable = new DataTable();
            sqlDataReader = sqlCommand.ExecuteReader();
            dataTable.Load(sqlDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
        //sqlConnection.Close();
        //Console.WriteLine(SendMassages);
        return SendMassages;
    }
    static void Deleting(DeletDevice[] deletDevices)
    {
        try
        {
            for (int i = 0; i < deletDevices.Length; i++)
            {
                /*sqlCommand = new SQLiteCommand($"DELETE FROM Devices WHERE P2={deletDevices[i].OwnerID};", sqlConnection);*/
                sqlCommand = new NpgsqlCommand($"DELETE FROM Devices WHERE P2={deletDevices[i].OwnerID};", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
    }
    static string? SendAllOuners(uint MyOwnerID)
    {
        string? SendMassages = null;
        Console.WriteLine("All Owner ID");
        sqlDataReader = null;
        try
        {
            /*sqlCommand = new SQLiteCommand($"SELECT * FROM Devices WHERE P2>={MyOwnerID*1000} AND P2<={(MyOwnerID+1) * 1000}", sqlConnection);*/
            sqlCommand = new NpgsqlCommand($"SELECT * FROM Devices WHERE P2>={MyOwnerID * 1000} AND P2<={(MyOwnerID + 1) * 1000}", sqlConnection);
            DataTable dataTable = new DataTable();
            sqlDataReader = sqlCommand.ExecuteReader();
            dataTable.Load(sqlDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
        //sqlConnection.Close();
        //Console.WriteLine(SendMassages);
        return SendMassages;
    }
    static void ConfigDataBasue()
    {
        sqlDataReader = null;

        ////exists Devices
        try
        {
            /*var tempState;*/
            sqlCommand = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name  = 'devices') AS table_existence;", sqlConnection);
            var tempState = sqlCommand.ExecuteScalar();
            //Console.WriteLine(tempState);
            bool tempBool = Convert.ToBoolean(tempState);
            if (tempBool)
                Console.WriteLine("YES");
            else
            {
                Console.WriteLine("NO");
                try
                {
                    sqlCommand = new NpgsqlCommand($"CREATE TABLE Devices(" +
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
                        $");", sqlConnection);
                    sqlCommand.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        try
        {
            sqlCommand = new NpgsqlCommand($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name  = 'config') AS table_existence;", sqlConnection);
            var tempState = sqlCommand.ExecuteScalar();
            //Console.WriteLine(tempState);
            bool tempBool = Convert.ToBoolean(tempState);
            if (tempBool)
                Console.WriteLine("YES");
            else
            {
                Console.WriteLine("NO");
                try
                {
                    sqlCommand = new NpgsqlCommand($"CREATE TABLE Config(" +
                        $"OwnerID INT," +
                        $"ParamNO INT," +
                        $"NewData INT" +
                        $");", sqlConnection);
                    sqlCommand.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        finally
        {
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }


        ///////Devices dell
/*        try
        {
 *//*           sqlCommand = new NpgsqlCommand($"SHOW TABLE Devices;", sqlConnection);
            sqlDataReader = sqlCommand.ExecuteReader();*//*

            sqlCommand = new NpgsqlCommand($"DROP TABLE Devices;", sqlConnection);
            sqlCommand.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        ///////Config dell
        try
        {
            sqlCommand = new NpgsqlCommand($"DROP TABLE Config;", sqlConnection);
            sqlCommand.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        ///////Devices add 
        try
        {
            sqlCommand = new NpgsqlCommand($"CREATE TABLE Devices(" +
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
                $");", sqlConnection);
            sqlCommand.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        ///////Config add
        try
        {
            sqlCommand = new NpgsqlCommand($"CREATE TABLE Config(" +
                $"OwnerID INT," +
                $"ParamNO INT," +
                $"NewData INT" +
                $");", sqlConnection);
            sqlCommand.ExecuteNonQuery();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
            Console.WriteLine("DB is ready");
        }
        */
    }
}