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
/*using System.Data.Sqlite;*/
using System.Data.SQLite;
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

    public static SQLiteConnection sqlConnection = null;
    //string cS = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Hayk\Desktop\MonitoringServer\MonitoringServer\MoikaData.mdf;Integrated Security=True";
    // string cS = @"";
    
    public static SQLiteCommand sqlCommand = null;
    
    public static SQLiteDataReader sqlDataReader = null;
    static async Task Main(string[] args)
    {
        // Start TCP listener
        if (File.Exists("MoikaData.db")) 
        {
            Console.WriteLine("Database is success");
        }
        else
        {
            Console.WriteLine("Database is not success");
            Environment.Exit(0);
        }
        sqlConnection = new SQLiteConnection("Data Source=MoikaData.db; Version = 3;");
        sqlConnection.Open();
        _ = Task.Run(() => StartTcpListener());

        _ = Task.Run(() => StartHttpListener());

        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();
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
        string ipAddress = "127.0.0.1";
        int port = 8080;

        HttpListener httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://{ipAddress}:{port}/");
        httpListener.Start();

        Console.WriteLine("HTTP listener started. Waiting for requests...");

        while (true)
        {
            HttpListenerContext context = httpListener.GetContext();
            ProcessHttpRequest(context);
        }
    }
    static void ProcessHttpRequest(HttpListenerContext context)
    {
        // Handle HTTP request here
        // Example: Get the request URL and send a response
        string requestUrl = context.Request.Url.ToString();
        /*Console.WriteLine($"Received HTTP request: {requestUrl}");*/
        char[] UrlArray=context.Request.Url.PathAndQuery.ToArray();
        string requestPath = context.Request.Url.AbsolutePath;
        //string[] temp = context.Request.QueryString.AllKeys.GetValue();
  /*      for (int i = 0; i < temp.Length; i++)
        {
            Console.WriteLine(temp[i]);
        }*/

        if (context.Request.HttpMethod == "GET") {
            //Console.WriteLine(context.Request.QueryString.AllKeys + "-----");

            if (requestUrl.EndsWith("/api/v1/devices"))
            {
                try
                {
                    Console.WriteLine(requestUrl);
                    string jsonResponse = SendCommandAll();
                    byte[] responseJsonData = Encoding.UTF8.GetBytes(jsonResponse);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
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
            else if (requestUrl.EndsWith("/api/v1/config"))
            {
                try
                {
                    Console.WriteLine(requestUrl);
                    string jsonResponse = SendConfigTable();
                    byte[] responseJsonData = Encoding.UTF8.GetBytes(jsonResponse);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
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
            else if (requestPath==("/api/v1/devices/"))
            {
                string id = context.Request.QueryString["id"];
                try
                {
                    uint SendOwnerID = uint.Parse(id);

                    Console.WriteLine(requestUrl);
                    string jsonResponse = SendOunerData(SendOwnerID);
                    byte[] responseJsonData = Encoding.UTF8.GetBytes(jsonResponse);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
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
            else if (requestPath == ("/api/v1/config/"))
            {
                string id = context.Request.QueryString["id"];
                try
                {
                    uint SendOwnerID = uint.Parse(id);

                    Console.WriteLine(requestUrl);
                    string jsonResponse = SendOunerConfig(SendOwnerID);
                    byte[] responseJsonData = Encoding.UTF8.GetBytes(jsonResponse);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
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
            else
            {
                Console.WriteLine(context.Request.Url.PathAndQuery.ToString());
                context.Response.StatusCode = 404;
                context.Response.OutputStream.Close();
            }
        }
        else if(context.Request.HttpMethod == "POST")
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

                    string SaveConfigParamState = null;
                    SaveConfigParamState = SaveConfigParam(configDevice);

                    if(SaveConfigParamState == "OK")
                    {
                        context.Response.ContentType = "text/plain";
                        byte[] responseData = Encoding.UTF8.GetBytes("Confirmed");
                        context.Response.OutputStream.Write(responseData, 0, responseData.Length);
                        context.Response.StatusCode = (int)HttpStatusCode.OK;
                    }
                    else
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

                    DeletDevice[] deletDevices = JsonConvert.DeserializeObject<DeletDevice[]>(requestBody);

                    Deleting(deletDevices);

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
    static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];
            uint[] TCPTempArray = new uint[149];
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
                int j=0;
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
                        Char tempChar=message[i];

                        TCPTempArray[j] = TCPTempArray[j]*10 + (uint)Char.GetNumericValue(tempChar);
                    }
                    else if (message[i+1]==' ' && message[i] == ':' && message[i - 1] == 'e' && message[i - 2] == 'v' && message[i - 3] == 'a' && message[i - 4] == 'l' && message[i - 5] == 'S') 
                    {
                        i++;
                        StartUncoding = true;
                    }
                }
                Console.WriteLine(message);
                if(StartUncoding)
                {
                    SQLWriteForTCP(TCPTempArray);
                    string SendTCPMassage = CheckConfigDevice(TCPTempArray);
                    Console.WriteLine(SendTCPMassage);
                    if(SendTCPMassage != "NO1" && SendTCPMassage != "NO2" && SendTCPMassage != "ERROR")
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
            sqlCommand = new SQLiteCommand($"SELECT OwnerID FROM Devices WHERE OwnerID={DataArray[2]};", sqlConnection);
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
                sqlCommand = new SQLiteCommand($"UPDATE [Devices] SET " +
                    $"DeviceType=" +
                    $"@DeviceType, " +
                    $"Version=" +
                    $"@Version, " +
                    $"RFID1=" +
                    $"@RFID1, " +
                    $"RFID2=" +
                    $"@RFID2, " +
                    $"MoykaID=" +
                    $"@MoykaID, " +
                    $"BoxID=" +
                    $"@BoxID, " +
                    $"Language=" +
                    $"@Language, " +
                    $"WorkingMode=" +
                    $"@WorkingMode, " +
                    $"FreeCard=" +
                    $"@FreeCard, " +
                    $"CoinNominal=" +
                    $"@CoinNominal, " +
                    $"BillNominal=" +
                    $"@BillNominal, " +
                    $"CashLessNominal=" +
                    $"@CashLessNominal, " +
                    $"CoinCount=" +
                    $"@CoinCount, " +
                    $"BillCount=" +
                    $"@BillCount, " +
                    $"CashLessCount=" +
                    $"@CashLessCount, " +
                    $"CoinCountTotal=" +
                    $"@CoinCountTotal, " +
                    $"BillCountTotal=" +
                    $"@BillCountTotal, " +
                    $"CashLessCountTotal=" +
                    $"@CashLessCountTotal, " +
                    $"RelayOffTime=" +
                    $"@RelayOffTime, " +
                    $"F1Nominal=" +
                    $"@F1Nominal, " +
                    $"F2Nominal=" +
                    $"@F2Nominal, " +
                    $"F3Nominal=" +
                    $"@F3Nominal, " +
                    $"F4Nominal=" +
                    $"@F4Nominal, " +
                    $"F5Nominal=" +
                    $"@F5Nominal, " +
                    $"F6Nominal=" +
                    $"@F6Nominal, " +
                    $"F1ModeName=" +
                    $"@F1ModeName, " +
                    $"F2ModeName=" +
                    $"@F2ModeName, " +
                    $"F3ModeName=" +
                    $"@F3ModeName, " +
                    $"F4ModeName=" +
                    $"@F4ModeName, " +
                    $"F5ModeName=" +
                    $"@F5ModeName, " +
                    $"F6ModeName=" +
                    $"@F6ModeName, " +
                    $"F1Color=" +
                    $"@F1Color, " +
                    $"F2Color=" +
                    $"@F2Color, " +
                    $"F3Color=" +
                    $"@F3Color, " +
                    $"F4Color=" +
                    $"@F4Color, " +
                    $"F5Color=" +
                    $"@F5Color, " +
                    $"F6Color=" +
                    $"@F6Color, " +
                    $"F1Count=" +
                    $"@F1Count, " +
                    $"F2Count=" +
                    $"@F2Count, " +
                    $"F3Count=" +
                    $"@F3Count, " +
                    $"F4Count=" +
                    $"@F4Count, " +
                    $"F5Count=" +
                    $"@F5Count, " +
                    $"F6Count=" +
                    $"@F6Count, " +
                    $"F1CountTotal=" +
                    $"@F1CountTotal, " +
                    $"F2CountTotal=" +
                    $"@F2CountTotal, " +
                    $"F3CountTotal=" +
                    $"@F3CountTotal, " +
                    $"F4CountTotal=" +
                    $"@F4CountTotal, " +
                    $"F5CountTotal=" +
                    $"@F5CountTotal, " +
                    $"F6CountTotal=" +
                    $"@F6CountTotal, " +
                    $"B1Nominal=" +
                    $"@B1Nominal, " +
                    $"B2Nominal=" +
                    $"@B2Nominal, " +
                    $"B3Nominal=" +
                    $"@B3Nominal, " +
                    $"B4Nominal=" +
                    $"@B4Nominal, " +
                    $"B5Nominal=" +
                    $"@B5Nominal, " +
                    $"B6Nominal=" +
                    $"@B6Nominal, " +
                    $"SleepCount=" +
                    $"@SleepCount, " +
                    $"RollTime=" +
                    $"@RollTime, " +
                    $"Sleep1ptr=" +
                    $"@Sleep1ptr, " +
                    $"Sleep2ptr=" +
                    $"@Sleep2ptr, " +
                    $"Sleep3ptr=" +
                    $"@Sleep3ptr, " +
                    $"Sleep4ptr=" +
                    $"@Sleep4ptr, " +
                    $"Sleep5ptr=" +
                    $"@Sleep5ptr, " +
                    $"Sleep6ptr=" +
                    $"@Sleep6ptr, " +
                    $"Sleep1Color=" +
                    $"@Sleep1Color, " +
                    $"Sleep2Color=" +
                    $"@Sleep2Color, " +
                    $"Sleep3Color=" +
                    $"@Sleep3Color, " +
                    $"Sleep4Color=" +
                    $"@Sleep4Color, " +
                    $"Sleep5Color=" +
                    $"@Sleep5Color, " +
                    $"Sleep6Color=" +
                    $"@Sleep6Color, " +
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
                    $"@P78 " +
                    $"WHERE OwnerID={DataArray[2]}",
                    sqlConnection);
                
                sqlCommand.Parameters.AddWithValue("OwnerID", (int)DataArray[2]);
                sqlCommand.Parameters.AddWithValue("DeviceType", (int)DataArray[0]);
                sqlCommand.Parameters.AddWithValue("Version", (int)DataArray[1]);
                sqlCommand.Parameters.AddWithValue("RFID1", (int)DataArray[3]);
                sqlCommand.Parameters.AddWithValue("RFID2", (int)DataArray[4]);
                sqlCommand.Parameters.AddWithValue("MoykaID", (int)DataArray[5]);
                sqlCommand.Parameters.AddWithValue("BoxID", (int)DataArray[6]);
                sqlCommand.Parameters.AddWithValue("Language", (int)DataArray[7]);
                sqlCommand.Parameters.AddWithValue("WorkingMode", (int)DataArray[8]);
                sqlCommand.Parameters.AddWithValue("FreeCard", (int)DataArray[9]);
                sqlCommand.Parameters.AddWithValue("CoinNominal", (int)DataArray[10]);
                sqlCommand.Parameters.AddWithValue("BillNominal", (int)DataArray[11]);
                sqlCommand.Parameters.AddWithValue("CashLessNominal", (int)DataArray[12]);
                sqlCommand.Parameters.AddWithValue("CoinCount", (int)DataArray[13]);
                sqlCommand.Parameters.AddWithValue("BillCount", (int)DataArray[14]);
                sqlCommand.Parameters.AddWithValue("CashLessCount", (int)DataArray[15]);
                sqlCommand.Parameters.AddWithValue("CoinCountTotal", (int)DataArray[16]);
                sqlCommand.Parameters.AddWithValue("BillCountTotal", (int)DataArray[17]);
                sqlCommand.Parameters.AddWithValue("CashLessCountTotal", (int)DataArray[18]);
                sqlCommand.Parameters.AddWithValue("RelayOffTime", (int)DataArray[19]);
                sqlCommand.Parameters.AddWithValue("F1Nominal", (int)DataArray[20]);
                sqlCommand.Parameters.AddWithValue("F2Nominal", (int)DataArray[21]);
                sqlCommand.Parameters.AddWithValue("F3Nominal", (int)DataArray[22]);
                sqlCommand.Parameters.AddWithValue("F4Nominal", (int)DataArray[23]);
                sqlCommand.Parameters.AddWithValue("F5Nominal", (int)DataArray[24]);
                sqlCommand.Parameters.AddWithValue("F6Nominal", (int)DataArray[25]);
                sqlCommand.Parameters.AddWithValue("F1ModeName", (int)DataArray[26]);
                sqlCommand.Parameters.AddWithValue("F2ModeName", (int)DataArray[27]);
                sqlCommand.Parameters.AddWithValue("F3ModeName", (int)DataArray[28]);
                sqlCommand.Parameters.AddWithValue("F4ModeName", (int)DataArray[29]);
                sqlCommand.Parameters.AddWithValue("F5ModeName", (int)DataArray[30]);
                sqlCommand.Parameters.AddWithValue("F6ModeName", (int)DataArray[31]);
                sqlCommand.Parameters.AddWithValue("F1Color", (int)DataArray[32]);
                sqlCommand.Parameters.AddWithValue("F2Color", (int)DataArray[33]);
                sqlCommand.Parameters.AddWithValue("F3Color", (int)DataArray[34]);
                sqlCommand.Parameters.AddWithValue("F4Color", (int)DataArray[35]);
                sqlCommand.Parameters.AddWithValue("F5Color", (int)DataArray[36]);
                sqlCommand.Parameters.AddWithValue("F6Color", (int)DataArray[37]);
                sqlCommand.Parameters.AddWithValue("F1Count", (int)DataArray[38]);
                sqlCommand.Parameters.AddWithValue("F2Count", (int)DataArray[39]);
                sqlCommand.Parameters.AddWithValue("F3Count", (int)DataArray[40]);
                sqlCommand.Parameters.AddWithValue("F4Count", (int)DataArray[41]);
                sqlCommand.Parameters.AddWithValue("F5Count", (int)DataArray[42]);
                sqlCommand.Parameters.AddWithValue("F6Count", (int)DataArray[43]);
                sqlCommand.Parameters.AddWithValue("F1CountTotal", (int)DataArray[44]);
                sqlCommand.Parameters.AddWithValue("F2CountTotal", (int)DataArray[45]);
                sqlCommand.Parameters.AddWithValue("F3CountTotal", (int)DataArray[46]);
                sqlCommand.Parameters.AddWithValue("F4CountTotal", (int)DataArray[47]);
                sqlCommand.Parameters.AddWithValue("F5CountTotal", (int)DataArray[48]);
                sqlCommand.Parameters.AddWithValue("F6CountTotal", (int)DataArray[49]);
                sqlCommand.Parameters.AddWithValue("B1Nominal", (int)DataArray[50]);
                sqlCommand.Parameters.AddWithValue("B2Nominal", (int)DataArray[51]);
                sqlCommand.Parameters.AddWithValue("B3Nominal", (int)DataArray[52]);
                sqlCommand.Parameters.AddWithValue("B4Nominal", (int)DataArray[53]);
                sqlCommand.Parameters.AddWithValue("B5Nominal", (int)DataArray[54]);
                sqlCommand.Parameters.AddWithValue("B6Nominal", (int)DataArray[55]);
                sqlCommand.Parameters.AddWithValue("SleepCount", (int)DataArray[56]);
                sqlCommand.Parameters.AddWithValue("RollTime", (int)DataArray[57]);
                sqlCommand.Parameters.AddWithValue("Sleep1ptr", (int)DataArray[58]);
                sqlCommand.Parameters.AddWithValue("Sleep2ptr", (int)DataArray[59]);
                sqlCommand.Parameters.AddWithValue("Sleep3ptr", (int)DataArray[60]);
                sqlCommand.Parameters.AddWithValue("Sleep4ptr", (int)DataArray[61]);
                sqlCommand.Parameters.AddWithValue("Sleep5ptr", (int)DataArray[62]);
                sqlCommand.Parameters.AddWithValue("Sleep6ptr", (int)DataArray[63]);
                sqlCommand.Parameters.AddWithValue("Sleep1Color", (int)DataArray[64]);
                sqlCommand.Parameters.AddWithValue("Sleep2Color", (int)DataArray[65]);
                sqlCommand.Parameters.AddWithValue("Sleep3Color", (int)DataArray[66]);
                sqlCommand.Parameters.AddWithValue("Sleep4Color", (int)DataArray[67]);
                sqlCommand.Parameters.AddWithValue("Sleep5Color", (int)DataArray[68]);
                sqlCommand.Parameters.AddWithValue("Sleep6Color", (int)DataArray[69]);
                string sqlFormattedDate = localDate.ToString("yyyy-MM-dd  HH:mm:ss");
                sqlCommand.Parameters.AddWithValue("DataTime", sqlFormattedDate);
                sqlCommand.Parameters.AddWithValue("P0", (int)DataArray[70]);
                sqlCommand.Parameters.AddWithValue("P1", (int)DataArray[71]);
                sqlCommand.Parameters.AddWithValue("P2", (int)DataArray[72]);
                sqlCommand.Parameters.AddWithValue("P3", (int)DataArray[73]);
                sqlCommand.Parameters.AddWithValue("P4", (int)DataArray[74]);
                sqlCommand.Parameters.AddWithValue("P5", (int)DataArray[75]);
                sqlCommand.Parameters.AddWithValue("P6", (int)DataArray[76]);
                sqlCommand.Parameters.AddWithValue("P7", (int)DataArray[77]);
                sqlCommand.Parameters.AddWithValue("P8", (int)DataArray[78]);
                sqlCommand.Parameters.AddWithValue("P9", (int)DataArray[79]);
                sqlCommand.Parameters.AddWithValue("P10", (int)DataArray[80]);
                sqlCommand.Parameters.AddWithValue("P11", (int)DataArray[81]);
                sqlCommand.Parameters.AddWithValue("P12", (int)DataArray[82]);
                sqlCommand.Parameters.AddWithValue("P13", (int)DataArray[83]);
                sqlCommand.Parameters.AddWithValue("P14", (int)DataArray[84]);
                sqlCommand.Parameters.AddWithValue("P15", (int)DataArray[85]);
                sqlCommand.Parameters.AddWithValue("P16", (int)DataArray[86]);
                sqlCommand.Parameters.AddWithValue("P17", (int)DataArray[87]);
                sqlCommand.Parameters.AddWithValue("P18", (int)DataArray[88]);
                sqlCommand.Parameters.AddWithValue("P19", (int)DataArray[89]);
                sqlCommand.Parameters.AddWithValue("P20", (int)DataArray[90]);
                sqlCommand.Parameters.AddWithValue("P21", (int)DataArray[91]);
                sqlCommand.Parameters.AddWithValue("P22", (int)DataArray[92]);
                sqlCommand.Parameters.AddWithValue("P23", (int)DataArray[93]);
                sqlCommand.Parameters.AddWithValue("P24", (int)DataArray[94]);
                sqlCommand.Parameters.AddWithValue("P25", (int)DataArray[95]);
                sqlCommand.Parameters.AddWithValue("P26", (int)DataArray[96]);
                sqlCommand.Parameters.AddWithValue("P27", (int)DataArray[97]);
                sqlCommand.Parameters.AddWithValue("P28", (int)DataArray[98]);
                sqlCommand.Parameters.AddWithValue("P29", (int)DataArray[99]);
                sqlCommand.Parameters.AddWithValue("P30", (int)DataArray[100]);
                sqlCommand.Parameters.AddWithValue("P31", (int)DataArray[101]);
                sqlCommand.Parameters.AddWithValue("P32", (int)DataArray[102]);
                sqlCommand.Parameters.AddWithValue("P33", (int)DataArray[103]);
                sqlCommand.Parameters.AddWithValue("P34", (int)DataArray[104]);
                sqlCommand.Parameters.AddWithValue("P35", (int)DataArray[105]);
                sqlCommand.Parameters.AddWithValue("P36", (int)DataArray[106]);
                sqlCommand.Parameters.AddWithValue("P37", (int)DataArray[107]);
                sqlCommand.Parameters.AddWithValue("P38", (int)DataArray[108]);
                sqlCommand.Parameters.AddWithValue("P39", (int)DataArray[109]);
                sqlCommand.Parameters.AddWithValue("P40", (int)DataArray[110]);
                sqlCommand.Parameters.AddWithValue("P41", (int)DataArray[111]);
                sqlCommand.Parameters.AddWithValue("P42", (int)DataArray[112]);
                sqlCommand.Parameters.AddWithValue("P43", (int)DataArray[113]);
                sqlCommand.Parameters.AddWithValue("P44", (int)DataArray[114]);
                sqlCommand.Parameters.AddWithValue("P45", (int)DataArray[115]);
                sqlCommand.Parameters.AddWithValue("P46", (int)DataArray[116]);
                sqlCommand.Parameters.AddWithValue("P47", (int)DataArray[117]);
                sqlCommand.Parameters.AddWithValue("P48", (int)DataArray[118]);
                sqlCommand.Parameters.AddWithValue("P49", (int)DataArray[119]);
                sqlCommand.Parameters.AddWithValue("P50", (int)DataArray[120]);
                sqlCommand.Parameters.AddWithValue("P51", (int)DataArray[121]);
                sqlCommand.Parameters.AddWithValue("P52", (int)DataArray[122]);
                sqlCommand.Parameters.AddWithValue("P53", (int)DataArray[123]);
                sqlCommand.Parameters.AddWithValue("P54", (int)DataArray[124]);
                sqlCommand.Parameters.AddWithValue("P55", (int)DataArray[125]);
                sqlCommand.Parameters.AddWithValue("P56", (int)DataArray[126]);
                sqlCommand.Parameters.AddWithValue("P57", (int)DataArray[127]);
                sqlCommand.Parameters.AddWithValue("P58", (int)DataArray[128]);
                sqlCommand.Parameters.AddWithValue("P59", (int)DataArray[129]);
                sqlCommand.Parameters.AddWithValue("P60", (int)DataArray[130]);
                sqlCommand.Parameters.AddWithValue("P61", (int)DataArray[131]);
                sqlCommand.Parameters.AddWithValue("P62", (int)DataArray[132]);
                sqlCommand.Parameters.AddWithValue("P63", (int)DataArray[133]);
                sqlCommand.Parameters.AddWithValue("P64", (int)DataArray[134]);
                sqlCommand.Parameters.AddWithValue("P65", (int)DataArray[135]);
                sqlCommand.Parameters.AddWithValue("P66", (int)DataArray[136]);
                sqlCommand.Parameters.AddWithValue("P67", (int)DataArray[137]);
                sqlCommand.Parameters.AddWithValue("P68", (int)DataArray[138]);
                sqlCommand.Parameters.AddWithValue("P69", (int)DataArray[139]);
                sqlCommand.Parameters.AddWithValue("P70", (int)DataArray[140]);
                sqlCommand.Parameters.AddWithValue("P71", (int)DataArray[141]);
                sqlCommand.Parameters.AddWithValue("P72", (int)DataArray[142]);
                sqlCommand.Parameters.AddWithValue("P73", (int)DataArray[143]);
                sqlCommand.Parameters.AddWithValue("P74", (int)DataArray[144]);
                sqlCommand.Parameters.AddWithValue("P75", (int)DataArray[145]);
                sqlCommand.Parameters.AddWithValue("P76", (int)DataArray[146]);
                sqlCommand.Parameters.AddWithValue("P77", (int)DataArray[147]);
                sqlCommand.Parameters.AddWithValue("P78", (int)DataArray[148]);
                sqlCommand.ExecuteNonQuery();
            }
            else
            {
                Console.WriteLine("NEW DEVICE");
                sqlCommand.Parameters.Clear();
                sqlCommand = new SQLiteCommand(
                    $"INSERT INTO [Devices] (" +
                    $"OwnerID, " +
                    $"DeviceType, " +
                    $"Version, " +
                    $"RFID1, " +
                    $"RFID2, " +
                    $"MoykaID, " +
                    $"BoxID, " +
                    $"Language, " +
                    $"WorkingMode, " +
                    $"FreeCard, " +
                    $"CoinNominal, " +
                    $"BillNominal, " +
                    $"CashLessNominal, " +
                    $"CoinCount, " +
                    $"BillCount, " +
                    $"CashLessCount, " +
                    $"CoinCountTotal, " +
                    $"BillCountTotal," +
                    $"CashLessCountTotal, " +
                    $"RelayOffTime, " +
                    $"F1Nominal, " +
                    $"F2Nominal, " +
                    $"F3Nominal, " +
                    $"F4Nominal, " +
                    $"F5Nominal, " +
                    $"F6Nominal, " +
                    $"F1ModeName, " +
                    $"F2ModeName, " +
                    $"F3ModeName, " +
                    $"F4ModeName, " +
                    $"F5ModeName, " +
                    $"F6ModeName, " +
                    $"F1Color, " +
                    $"F2Color, " +
                    $"F3Color, " +
                    $"F4Color, " +
                    $"F5Color, " +
                    $"F6Color, " +
                    $"F1Count, " +
                    $"F2Count, " +
                    $"F3Count, " +
                    $"F4Count, " +
                    $"F5Count, " +
                    $"F6Count, " +
                    $"F1CountTotal, " +
                    $"F2CountTotal, " +
                    $"F3CountTotal, " +
                    $"F4CountTotal," +
                    $"F5CountTotal, " +
                    $"F6CountTotal, " +
                    $"B1Nominal, " +
                    $"B2Nominal, " +
                    $"B3Nominal, " +
                    $"B4Nominal, " +
                    $"B5Nominal, " +
                    $"B6Nominal, " +
                    $"SleepCount, " +
                    $"RollTime, " +
                    $"Sleep1ptr, " +
                    $"Sleep2ptr, " +
                    $"Sleep3ptr, " +
                    $"Sleep4ptr, " +
                    $"Sleep5ptr, " +
                    $"Sleep6ptr, " +
                    $"Sleep1Color, " +
                    $"Sleep2Color, " +
                    $"Sleep3Color, " +
                    $"Sleep4Color, " +
                    $"Sleep5Color, " +
                    $"Sleep6Color," +
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
                    $"P78" +
                    $") VALUES (" +
                    $"@OwnerID, " +
                    $"@DeviceType, " +
                    $"@Version, " +
                    $"@RFID1, " +
                    $"@RFID2, " +
                    $"@MoykaID, " +
                    $"@BoxID, " +
                    $"@Language, " +
                    $"@WorkingMode, " +
                    $"@FreeCard, " +
                    $"@CoinNominal, " +
                    $"@BillNominal, " +
                    $"@CashLessNominal, " +
                    $"@CoinCount, " +
                    $"@BillCount, " +
                    $"@CashLessCount, " +
                    $"@CoinCountTotal, " +
                    $"@BillCountTotal," +
                    $"@CashLessCountTotal, " +
                    $"@RelayOffTime, " +
                    $"@F1Nominal, " +
                    $"@F2Nominal, " +
                    $"@F3Nominal, " +
                    $"@F4Nominal, " +
                    $"@F5Nominal, " +
                    $"@F6Nominal, " +
                    $"@F1ModeName, " +
                    $"@F2ModeName, " +
                    $"@F3ModeName, " +
                    $"@F4ModeName, " +
                    $"@F5ModeName, " +
                    $"@F6ModeName, " +
                    $"@F1Color, " +
                    $"@F2Color, " +
                    $"@F3Color, " +
                    $"@F4Color, " +
                    $"@F5Color, " +
                    $"@F6Color, " +
                    $"@F1Count, " +
                    $"@F2Count, " +
                    $"@F3Count, " +
                    $"@F4Count, " +
                    $"@F5Count, " +
                    $"@F6Count, " +
                    $"@F1CountTotal, " +
                    $"@F2CountTotal, " +
                    $"@F3CountTotal, " +
                    $"@F4CountTotal," +
                    $"@F5CountTotal, " +
                    $"@F6CountTotal, " +
                    $"@B1Nominal, " +
                    $"@B2Nominal, " +
                    $"@B3Nominal, " +
                    $"@B4Nominal, " +
                    $"@B5Nominal, " +
                    $"@B6Nominal, " +
                    $"@SleepCount, " +
                    $"@RollTime, " +
                    $"@Sleep1ptr, " +
                    $"@Sleep2ptr, " +
                    $"@Sleep3ptr, " +
                    $"@Sleep4ptr, " +
                    $"@Sleep5ptr, " +
                    $"@Sleep6ptr, " +
                    $"@Sleep1Color, " +
                    $"@Sleep2Color, " +
                    $"@Sleep3Color, " +
                    $"@Sleep4Color, " +
                    $"@Sleep5Color, " +
                    $"@Sleep6Color," +
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
                    $"@P78" +
                    $") " ,
                    sqlConnection);

                sqlCommand.Parameters.AddWithValue("OwnerID", (int)DataArray[2]);
                sqlCommand.Parameters.AddWithValue("DeviceType", (int)DataArray[0]);
                sqlCommand.Parameters.AddWithValue("Version", (int)DataArray[1]);
                sqlCommand.Parameters.AddWithValue("RFID1", (int)DataArray[3]);
                sqlCommand.Parameters.AddWithValue("RFID2", (int)DataArray[4]);
                sqlCommand.Parameters.AddWithValue("MoykaID", (int)DataArray[5]);
                sqlCommand.Parameters.AddWithValue("BoxID", (int)DataArray[6]);
                sqlCommand.Parameters.AddWithValue("Language", (int)DataArray[7]);
                sqlCommand.Parameters.AddWithValue("WorkingMode", (int)DataArray[8]);
                sqlCommand.Parameters.AddWithValue("FreeCard", (int)DataArray[9]);
                sqlCommand.Parameters.AddWithValue("CoinNominal", (int)DataArray[10]);
                sqlCommand.Parameters.AddWithValue("BillNominal", (int)DataArray[11]);
                sqlCommand.Parameters.AddWithValue("CashLessNominal", (int)DataArray[12]);
                sqlCommand.Parameters.AddWithValue("CoinCount", (int)DataArray[13]);
                sqlCommand.Parameters.AddWithValue("BillCount", (int)DataArray[14]);
                sqlCommand.Parameters.AddWithValue("CashLessCount", (int)DataArray[15]);
                sqlCommand.Parameters.AddWithValue("CoinCountTotal", (int)DataArray[16]);
                sqlCommand.Parameters.AddWithValue("BillCountTotal", (int)DataArray[17]);
                sqlCommand.Parameters.AddWithValue("CashLessCountTotal", (int)DataArray[18]);
                sqlCommand.Parameters.AddWithValue("RelayOffTime", (int)DataArray[19]);
                sqlCommand.Parameters.AddWithValue("F1Nominal", (int)DataArray[20]);
                sqlCommand.Parameters.AddWithValue("F2Nominal", (int)DataArray[21]);
                sqlCommand.Parameters.AddWithValue("F3Nominal", (int)DataArray[22]);
                sqlCommand.Parameters.AddWithValue("F4Nominal", (int)DataArray[23]);
                sqlCommand.Parameters.AddWithValue("F5Nominal", (int)DataArray[24]);
                sqlCommand.Parameters.AddWithValue("F6Nominal", (int)DataArray[25]);
                sqlCommand.Parameters.AddWithValue("F1ModeName", (int)DataArray[26]);
                sqlCommand.Parameters.AddWithValue("F2ModeName", (int)DataArray[27]);
                sqlCommand.Parameters.AddWithValue("F3ModeName", (int)DataArray[28]);
                sqlCommand.Parameters.AddWithValue("F4ModeName", (int)DataArray[29]);
                sqlCommand.Parameters.AddWithValue("F5ModeName", (int)DataArray[30]);
                sqlCommand.Parameters.AddWithValue("F6ModeName", (int)DataArray[31]);
                sqlCommand.Parameters.AddWithValue("F1Color", (int)DataArray[32]);
                sqlCommand.Parameters.AddWithValue("F2Color", (int)DataArray[33]);
                sqlCommand.Parameters.AddWithValue("F3Color", (int)DataArray[34]);
                sqlCommand.Parameters.AddWithValue("F4Color", (int)DataArray[35]);
                sqlCommand.Parameters.AddWithValue("F5Color", (int)DataArray[36]);
                sqlCommand.Parameters.AddWithValue("F6Color", (int)DataArray[37]);
                sqlCommand.Parameters.AddWithValue("F1Count", (int)DataArray[38]);
                sqlCommand.Parameters.AddWithValue("F2Count", (int)DataArray[39]);
                sqlCommand.Parameters.AddWithValue("F3Count", (int)DataArray[40]);
                sqlCommand.Parameters.AddWithValue("F4Count", (int)DataArray[41]);
                sqlCommand.Parameters.AddWithValue("F5Count", (int)DataArray[42]);
                sqlCommand.Parameters.AddWithValue("F6Count", (int)DataArray[43]);
                sqlCommand.Parameters.AddWithValue("F1CountTotal", (int)DataArray[44]);
                sqlCommand.Parameters.AddWithValue("F2CountTotal", (int)DataArray[45]);
                sqlCommand.Parameters.AddWithValue("F3CountTotal", (int)DataArray[46]);
                sqlCommand.Parameters.AddWithValue("F4CountTotal", (int)DataArray[47]);
                sqlCommand.Parameters.AddWithValue("F5CountTotal", (int)DataArray[48]);
                sqlCommand.Parameters.AddWithValue("F6CountTotal", (int)DataArray[49]);
                sqlCommand.Parameters.AddWithValue("B1Nominal", (int)DataArray[50]);
                sqlCommand.Parameters.AddWithValue("B2Nominal", (int)DataArray[51]);
                sqlCommand.Parameters.AddWithValue("B3Nominal", (int)DataArray[52]);
                sqlCommand.Parameters.AddWithValue("B4Nominal", (int)DataArray[53]);
                sqlCommand.Parameters.AddWithValue("B5Nominal", (int)DataArray[54]);
                sqlCommand.Parameters.AddWithValue("B6Nominal", (int)DataArray[55]);
                sqlCommand.Parameters.AddWithValue("SleepCount", (int)DataArray[56]);
                sqlCommand.Parameters.AddWithValue("RollTime", (int)DataArray[57]);
                sqlCommand.Parameters.AddWithValue("Sleep1ptr", (int)DataArray[58]);
                sqlCommand.Parameters.AddWithValue("Sleep2ptr", (int)DataArray[59]);
                sqlCommand.Parameters.AddWithValue("Sleep3ptr", (int)DataArray[60]);
                sqlCommand.Parameters.AddWithValue("Sleep4ptr", (int)DataArray[61]);
                sqlCommand.Parameters.AddWithValue("Sleep5ptr", (int)DataArray[62]);
                sqlCommand.Parameters.AddWithValue("Sleep6ptr", (int)DataArray[63]);
                sqlCommand.Parameters.AddWithValue("Sleep1Color", (int)DataArray[64]);
                sqlCommand.Parameters.AddWithValue("Sleep2Color", (int)DataArray[65]);
                sqlCommand.Parameters.AddWithValue("Sleep3Color", (int)DataArray[66]);
                sqlCommand.Parameters.AddWithValue("Sleep4Color", (int)DataArray[67]);
                sqlCommand.Parameters.AddWithValue("Sleep5Color", (int)DataArray[68]);
                sqlCommand.Parameters.AddWithValue("Sleep6Color", (int)DataArray[69]);
                string sqlFormattedDate = localDate.ToString("yyyy-MM-dd  HH:mm:ss");
                sqlCommand.Parameters.AddWithValue("DataTime", sqlFormattedDate);
                sqlCommand.Parameters.AddWithValue("P0", (int)DataArray[70]);
                sqlCommand.Parameters.AddWithValue("P1", (int)DataArray[71]);
                sqlCommand.Parameters.AddWithValue("P2", (int)DataArray[72]);
                sqlCommand.Parameters.AddWithValue("P3", (int)DataArray[73]);
                sqlCommand.Parameters.AddWithValue("P4", (int)DataArray[74]);
                sqlCommand.Parameters.AddWithValue("P5", (int)DataArray[75]);
                sqlCommand.Parameters.AddWithValue("P6", (int)DataArray[76]);
                sqlCommand.Parameters.AddWithValue("P7", (int)DataArray[77]);
                sqlCommand.Parameters.AddWithValue("P8", (int)DataArray[78]);
                sqlCommand.Parameters.AddWithValue("P9", (int)DataArray[79]);
                sqlCommand.Parameters.AddWithValue("P10", (int)DataArray[80]);
                sqlCommand.Parameters.AddWithValue("P11", (int)DataArray[81]);
                sqlCommand.Parameters.AddWithValue("P12", (int)DataArray[82]);
                sqlCommand.Parameters.AddWithValue("P13", (int)DataArray[83]);
                sqlCommand.Parameters.AddWithValue("P14", (int)DataArray[84]);
                sqlCommand.Parameters.AddWithValue("P15", (int)DataArray[85]);
                sqlCommand.Parameters.AddWithValue("P16", (int)DataArray[86]);
                sqlCommand.Parameters.AddWithValue("P17", (int)DataArray[87]);
                sqlCommand.Parameters.AddWithValue("P18", (int)DataArray[88]);
                sqlCommand.Parameters.AddWithValue("P19", (int)DataArray[89]);
                sqlCommand.Parameters.AddWithValue("P20", (int)DataArray[90]);
                sqlCommand.Parameters.AddWithValue("P21", (int)DataArray[91]);
                sqlCommand.Parameters.AddWithValue("P22", (int)DataArray[92]);
                sqlCommand.Parameters.AddWithValue("P23", (int)DataArray[93]);
                sqlCommand.Parameters.AddWithValue("P24", (int)DataArray[94]);
                sqlCommand.Parameters.AddWithValue("P25", (int)DataArray[95]);
                sqlCommand.Parameters.AddWithValue("P26", (int)DataArray[96]);
                sqlCommand.Parameters.AddWithValue("P27", (int)DataArray[97]);
                sqlCommand.Parameters.AddWithValue("P28", (int)DataArray[98]);
                sqlCommand.Parameters.AddWithValue("P29", (int)DataArray[99]);
                sqlCommand.Parameters.AddWithValue("P30", (int)DataArray[100]);
                sqlCommand.Parameters.AddWithValue("P31", (int)DataArray[101]);
                sqlCommand.Parameters.AddWithValue("P32", (int)DataArray[102]);
                sqlCommand.Parameters.AddWithValue("P33", (int)DataArray[103]);
                sqlCommand.Parameters.AddWithValue("P34", (int)DataArray[104]);
                sqlCommand.Parameters.AddWithValue("P35", (int)DataArray[105]);
                sqlCommand.Parameters.AddWithValue("P36", (int)DataArray[106]);
                sqlCommand.Parameters.AddWithValue("P37", (int)DataArray[107]);
                sqlCommand.Parameters.AddWithValue("P38", (int)DataArray[108]);
                sqlCommand.Parameters.AddWithValue("P39", (int)DataArray[109]);
                sqlCommand.Parameters.AddWithValue("P40", (int)DataArray[110]);
                sqlCommand.Parameters.AddWithValue("P41", (int)DataArray[111]);
                sqlCommand.Parameters.AddWithValue("P42", (int)DataArray[112]);
                sqlCommand.Parameters.AddWithValue("P43", (int)DataArray[113]);
                sqlCommand.Parameters.AddWithValue("P44", (int)DataArray[114]);
                sqlCommand.Parameters.AddWithValue("P45", (int)DataArray[115]);
                sqlCommand.Parameters.AddWithValue("P46", (int)DataArray[116]);
                sqlCommand.Parameters.AddWithValue("P47", (int)DataArray[117]);
                sqlCommand.Parameters.AddWithValue("P48", (int)DataArray[118]);
                sqlCommand.Parameters.AddWithValue("P49", (int)DataArray[119]);
                sqlCommand.Parameters.AddWithValue("P50", (int)DataArray[120]);
                sqlCommand.Parameters.AddWithValue("P51", (int)DataArray[121]);
                sqlCommand.Parameters.AddWithValue("P52", (int)DataArray[122]);
                sqlCommand.Parameters.AddWithValue("P53", (int)DataArray[123]);
                sqlCommand.Parameters.AddWithValue("P54", (int)DataArray[124]);
                sqlCommand.Parameters.AddWithValue("P55", (int)DataArray[125]);
                sqlCommand.Parameters.AddWithValue("P56", (int)DataArray[126]);
                sqlCommand.Parameters.AddWithValue("P57", (int)DataArray[127]);
                sqlCommand.Parameters.AddWithValue("P58", (int)DataArray[128]);
                sqlCommand.Parameters.AddWithValue("P59", (int)DataArray[129]);
                sqlCommand.Parameters.AddWithValue("P60", (int)DataArray[130]);
                sqlCommand.Parameters.AddWithValue("P61", (int)DataArray[131]);
                sqlCommand.Parameters.AddWithValue("P62", (int)DataArray[132]);
                sqlCommand.Parameters.AddWithValue("P63", (int)DataArray[133]);
                sqlCommand.Parameters.AddWithValue("P64", (int)DataArray[134]);
                sqlCommand.Parameters.AddWithValue("P65", (int)DataArray[135]);
                sqlCommand.Parameters.AddWithValue("P66", (int)DataArray[136]);
                sqlCommand.Parameters.AddWithValue("P67", (int)DataArray[137]);
                sqlCommand.Parameters.AddWithValue("P68", (int)DataArray[138]);
                sqlCommand.Parameters.AddWithValue("P69", (int)DataArray[139]);
                sqlCommand.Parameters.AddWithValue("P70", (int)DataArray[140]);
                sqlCommand.Parameters.AddWithValue("P71", (int)DataArray[141]);
                sqlCommand.Parameters.AddWithValue("P72", (int)DataArray[142]);
                sqlCommand.Parameters.AddWithValue("P73", (int)DataArray[143]);
                sqlCommand.Parameters.AddWithValue("P74", (int)DataArray[144]);
                sqlCommand.Parameters.AddWithValue("P75", (int)DataArray[145]);
                sqlCommand.Parameters.AddWithValue("P76", (int)DataArray[146]);
                sqlCommand.Parameters.AddWithValue("P77", (int)DataArray[147]);
                sqlCommand.Parameters.AddWithValue("P78", (int)DataArray[148]);
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
        //sqlConnection.Close();
    }
    static string SendCommandAll() 
    {
        string SendMassages = null;
        Console.WriteLine("ALL");
        sqlDataReader = null;
        try
        {
            sqlCommand = new SQLiteCommand($"SELECT * FROM Devices", sqlConnection);
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
    static string SendConfigTable()
    {
        string SendMassages = null;
        Console.WriteLine("Config");
        sqlDataReader = null;
        try
        {
            sqlCommand = new SQLiteCommand($"SELECT * FROM Config", sqlConnection);
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
    static string SendOunerData(uint MyOwnerID)
    {
        string SendMassages = null;
        Console.WriteLine("Owner ID");
        sqlDataReader = null;
        try
        {
            sqlCommand = new SQLiteCommand($"SELECT * FROM Devices WHERE OwnerID={MyOwnerID}", sqlConnection);
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
    static string SaveConfigParam(ConfigDevice config)
    {
        string returnState = null;
        Console.WriteLine("Save Config");
        sqlDataReader = null;
        try
        {
            if (config.ParamNO > 2)
            {
                sqlCommand = new SQLiteCommand($"SELECT OwnerID FROM Config WHERE OwnerID={config.OwnerID} AND ParamNO={config.ParamNO};", sqlConnection);
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
                    sqlCommand = new SQLiteCommand($"UPDATE [Config] SET " +
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
                    sqlCommand = new SQLiteCommand(
                        $"INSERT INTO [Config] (" +
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
    static string CheckConfigDevice(uint[] DataArray)
    {
        string SendMassage = null;
        sqlDataReader = null;
        try
        {
            Console.WriteLine(DataArray[2]);
            sqlCommand = new SQLiteCommand($"SELECT * FROM Config WHERE OwnerID={DataArray[2]}", sqlConnection);
            DataTable dataTable = new DataTable();
            sqlDataReader = sqlCommand.ExecuteReader();
            dataTable.Load(sqlDataReader);
            if (dataTable != null)
            {
                string TempJSON = JsonConvert.SerializeObject(dataTable);
                //Console.WriteLine(TempJSON);
                ConfigDevice[] configDevices = JsonConvert.DeserializeObject<ConfigDevice[]>(TempJSON);
                bool noChang = true;
                for (int i = 0; i < configDevices.Length; i++)
                {
                    //Console.WriteLine(configDevices[i].ToString());
                    if(DataArray[configDevices[i].ParamNO] != configDevices[i].NewData)
                    {
                        noChang = false;
                        SendMassage = SendMassage + "P" + configDevices[i].ParamNO.ToString() + "-" + configDevices[i].NewData + ",";
                    }
                    else if (DataArray[configDevices[i].ParamNO] == configDevices[i].NewData)
                    {
                        sqlCommand = new SQLiteCommand($"DELETE FROM Config WHERE OwnerID={DataArray[2]} AND ParamNO={configDevices[i].ParamNO};", sqlConnection);
                        sqlCommand.ExecuteNonQuery();
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
    static string SendOunerConfig(uint MyOwnerID)
    {
        string SendMassages = null;
        Console.WriteLine("Owner ID");
        sqlDataReader = null;
        try
        {
            sqlCommand = new SQLiteCommand($"SELECT * FROM Config WHERE OwnerID={MyOwnerID}", sqlConnection);
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
                sqlCommand = new SQLiteCommand($"DELETE FROM Devices WHERE OwnerID={deletDevices[i].OwnerID};", sqlConnection);
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
}