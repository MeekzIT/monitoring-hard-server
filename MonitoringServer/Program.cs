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

struct TCPData
{
    public uint Devicetype;
    public uint Version;
    public uint OunerID;
    public uint RFID1;
    public uint RFID2;
    public uint MoykaID;
    public uint BoxID;
    public uint Language;
    public uint WorkingMode;
    public uint FreeCard;
    public uint CoinNominal;
    public uint BillNominal;
    public uint CashLessNominal;
    public uint CoinCount;
    public uint BillCount;
    public uint CashLessCount;
    public uint CoinCountTotal;
    public uint BillCountTotal;
    public uint CashLessCountTotal;
    public uint RelayOffTime;
    public uint F1Nominal;
    public uint F2Nominal;
    public uint F3Nominal;
    public uint F4Nominal;
    public uint F5Nominal;
    public uint F6Nominal;
    public uint F1ModeName;
    public uint F2ModeName;
    public uint F3ModeName;
    public uint F4ModeName;
    public uint F5ModeName;
    public uint F6ModeName;
    public uint F1Color;
    public uint F2Color;
    public uint F3Color;
    public uint F4Color;
    public uint F5Color;
    public uint F6Color;
    public uint F1Count;
    public uint F2Count;
    public uint F3Count;
    public uint F4Count;
    public uint F5Count;
    public uint F6Count;
    public uint F1CountTotal;
    public uint F2CountTotal;
    public uint F3CountTotal;
    public uint F4CountTotal;
    public uint F5CountTotal;
    public uint F6CountTotal;
    public uint B1Nominal;
    public uint B2Nominal;
    public uint B3Nominal;
    public uint B4Nominal;
    public uint B5Nominal;
    public uint B6Nominal;
    public uint SleepCount;
    public uint RollTime;
    public uint Sleep1ptr;
    public uint Sleep2ptr;
    public uint Sleep3ptr;
    public uint Sleep4ptr;
    public uint Sleep5ptr;
    public uint Sleep6ptr;
    public uint Sleep1Color;
    public uint Sleep2Color;
    public uint Sleep3Color;
    public uint Sleep4Color;
    public uint Sleep5Color;
    public uint Sleep6Color;
    public DateTime MyTime;
}


class Program
{
    static async Task Main(string[] args)
    {
        // Start TCP listener
        _ = Task.Run(() => StartTcpListener());

        // Start HTTP listener
        _ = Task.Run(() => StartHttpListener());

        Console.WriteLine("Press Enter to exit.");
        Console.ReadLine();
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

        if (requestUrl.EndsWith("/ALL") && context.Request.HttpMethod == "GET")
        {
            Console.WriteLine(requestUrl);
            string jsonResponse = SendCommandAll();
            byte[] responseJsonData = Encoding.UTF8.GetBytes(jsonResponse);
            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = responseJsonData.Length;
            context.Response.OutputStream.Write(responseJsonData, 0, responseJsonData.Length);
            context.Response.OutputStream.Close();
        }
    }
    static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];
            uint[] TCPTempArray = new uint[70];
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
                bool StartCommand = false;
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
                    else if (StartCommand)
                    {
/*                        if(message[i] == '/' && message[i + 1] == 'A' && message[i + 2] == 'L' && message[i + 3] == 'L') { }
                        {
                            //string Nmessage = SendCommandAll();

                            string Nmessage = "HTTP/1.1 200 OK"+13+ "Date: Thu, 29 Jul 2021 19:20:01 GMT" +
                                13 + "Content-Type: text/html; charset=utf-8" + 
                                13 + "Content-Length: 2" +
                                13 + "Connection: close" + 
                                13 + "Server: gunicorn/19.9.0" + 
                                13 + "Access-Control-Allow-Origin: *" + 
                                13 + "Access-Control-Allow-Credentials: true" + 13 + 13 +"OK";
                            var response = Encoding.UTF8.GetBytes($"{Nmessage}");
                            await stream.WriteAsync(response, 0, response.Length);
                            //HTTPGetPackageALL();
                            break;
                        }*/
                    }
                    else if (message[i+1]==' ' && message[i] == ':' && message[i - 1] == 'e' && message[i - 2] == 'v' && message[i - 3] == 'a' && message[i - 4] == 'l' && message[i - 5] == 'S') 
                    {
                        i++;
                        StartUncoding = true;
                        StartCommand = false;
                    }
/*                    else if (message[i + 1] == ' ' && message[i] == 'T' && message[i - 1] == 'E' && message[i - 2] == 'G')
                    {
                        i++;
                        StartCommand = true;
                        StartUncoding = false;
                    }*/
                }
                /*for (int i = 0; i < TCPTempArray.Length; i++)
                {
                    Console.WriteLine(TCPTempArray[i]);
                }*/
                // Echo the message back to the client
                //var response = Encoding.UTF8.GetBytes($"Echo: {message}");
                //await stream.WriteAsync(response, 0, response.Length);
                Console.WriteLine(message);
                if(StartUncoding)
                {
                    SQLWriteForTCP(TCPTempArray);
                }
            }
            /////////SQL//////////

            ///////END SQL////////

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
        SQLiteConnection sqlConnection = null;
        //string cS = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Hayk\Desktop\MonitoringServer\MonitoringServer\MoikaData.mdf;Integrated Security=True";
        // string cS = @"";
        sqlConnection = new SQLiteConnection("Data Source=MoikaData.db; Version = 3;");
        SQLiteCommand sqlCommand = null;
        sqlConnection.Open();
        SQLiteDataReader sqlDataReader = null;
        try
        {
            /*var sqlCommandTest = new SQLiteCommand($"SELECT * FROM sqlite_master WHERE type='table';", sqlConnection);
            var sqlreaderTest = sqlCommandTest.ExecuteReader();
            Console.WriteLine(sqlreaderTest.ToString());*/
            sqlCommand = new SQLiteCommand($"SELECT OunerID FROM Devices WHERE OunerID={DataArray[2]};", sqlConnection);
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
                    $"@DataTime " +
                    $"WHERE OunerID={DataArray[2]}",
                    sqlConnection);
                
                sqlCommand.Parameters.AddWithValue("OunerID", (int)DataArray[2]);
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

                sqlCommand.ExecuteNonQuery();

                Console.Write(DataArray[0] + ",");
                Console.Write(DataArray[1] + ",");
                Console.Write(DataArray[2] + ",");
                Console.Write(DataArray[3] + ",");
                Console.Write(DataArray[4] + ",");
                Console.Write(DataArray[5] + ",");
                Console.Write(DataArray[6] + ",");
                Console.Write(DataArray[7] + ",");
                Console.Write(DataArray[8] + ",");
                Console.Write(DataArray[9] + ",");
                Console.Write(DataArray[10] + ",");
                Console.Write(DataArray[11] + ",");
                Console.Write(DataArray[12] + ",");
                Console.Write(DataArray[13] + ",");
                Console.Write(DataArray[14] + ",");
                Console.Write(DataArray[15] + ",");
                Console.Write(DataArray[16] + ",");
                Console.Write(DataArray[17] + ",");
                Console.Write(DataArray[18] + ",");
                Console.Write(DataArray[19] + ",");
                Console.Write(DataArray[20] + ",");
                Console.Write(DataArray[21] + ",");
                Console.Write(DataArray[22] + ",");
                Console.Write(DataArray[23] + ",");
                Console.Write(DataArray[24] + ",");
                Console.Write(DataArray[25] + ",");
                Console.Write(DataArray[26] + ",");
                Console.Write(DataArray[27] + ",");
                Console.Write(DataArray[28] + ",");
                Console.Write(DataArray[29] + ",");
                Console.Write(DataArray[30] + ",");
                Console.Write(DataArray[31] + ",");
                Console.Write(DataArray[32] + ",");
                Console.Write(DataArray[33] + ",");
                Console.Write(DataArray[34] + ",");
                Console.Write(DataArray[35] + ",");
                Console.Write(DataArray[36] + ",");
                Console.Write(DataArray[37] + ",");
                Console.Write(DataArray[38] + ",");
                Console.Write(DataArray[39] + ",");
                Console.Write(DataArray[40] + ",");
                Console.Write(DataArray[41] + ",");
                Console.Write(DataArray[42] + ",");
                Console.Write(DataArray[43] + ",");
                Console.Write(DataArray[44] + ",");
                Console.Write(DataArray[45] + ",");
                Console.Write(DataArray[46] + ",");
                Console.Write(DataArray[47] + ",");
                Console.Write(DataArray[48] + ",");
                Console.Write(DataArray[49] + ",");
                Console.Write(DataArray[50] + ",");
                Console.Write(DataArray[51] + ",");
                Console.Write(DataArray[52] + ",");
                Console.Write(DataArray[53] + ",");
                Console.Write(DataArray[54] + ",");
                Console.Write(DataArray[55] + ",");
                Console.Write(DataArray[56] + ",");
                Console.Write(DataArray[57] + ",");
                Console.Write(DataArray[58] + ",");
                Console.Write(DataArray[59] + ",");
                Console.Write(DataArray[60] + ",");
                Console.Write(DataArray[61] + ",");
                Console.Write(DataArray[62] + ",");
                Console.Write(DataArray[63] + ",");
                Console.Write(DataArray[64] + ",");
                Console.Write(DataArray[65] + ",");
                Console.Write(DataArray[66] + ",");
                Console.Write(DataArray[67] + ",");
                Console.Write(DataArray[68] + ",");
                Console.Write(DataArray[69] + ",");
                Console.WriteLine();
                Console.WriteLine();
            }
            else
            {
                Console.WriteLine("NEW DEVICE");
                sqlCommand.Parameters.Clear();
                sqlCommand = new SQLiteCommand(
                    $"INSERT INTO [Devices] (" +
                    $"OunerID, " +
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
                    $"DataTime) " +
                    $"VALUES (" +
                    $"@OunerID, " +
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
                    $"@DataTime) " ,
                    sqlConnection);

                sqlCommand.Parameters.AddWithValue("OunerID", (int)DataArray[2]);
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
                sqlCommand.ExecuteNonQuery();

                Console.Write(DataArray[0] + ",");
                Console.Write(DataArray[1] + ",");
                Console.Write(DataArray[2] + ",");
                Console.Write(DataArray[3] + ",");
                Console.Write(DataArray[4] + ",");
                Console.Write(DataArray[5] + ",");
                Console.Write(DataArray[6] + ",");
                Console.Write(DataArray[7] + ",");
                Console.Write(DataArray[8] + ",");
                Console.Write(DataArray[9] + ",");
                Console.Write(DataArray[10] + ",");
                Console.Write(DataArray[11] + ",");
                Console.Write(DataArray[12] + ",");
                Console.Write(DataArray[13] + ",");
                Console.Write(DataArray[14] + ",");
                Console.Write(DataArray[15] + ",");
                Console.Write(DataArray[16] + ",");
                Console.Write(DataArray[17] + ",");
                Console.Write(DataArray[18] + ",");
                Console.Write(DataArray[19] + ",");
                Console.Write(DataArray[20] + ",");
                Console.Write(DataArray[21] + ",");
                Console.Write(DataArray[22] + ",");
                Console.Write(DataArray[23] + ",");
                Console.Write(DataArray[24] + ",");
                Console.Write(DataArray[25] + ",");
                Console.Write(DataArray[26] + ",");
                Console.Write(DataArray[27] + ",");
                Console.Write(DataArray[28] + ",");
                Console.Write(DataArray[29] + ",");
                Console.Write(DataArray[30] + ",");
                Console.Write(DataArray[31] + ",");
                Console.Write(DataArray[32] + ",");
                Console.Write(DataArray[33] + ",");
                Console.Write(DataArray[34] + ",");
                Console.Write(DataArray[35] + ",");
                Console.Write(DataArray[36] + ",");
                Console.Write(DataArray[37] + ",");
                Console.Write(DataArray[38] + ",");
                Console.Write(DataArray[39] + ",");
                Console.Write(DataArray[40] + ",");
                Console.Write(DataArray[41] + ",");
                Console.Write(DataArray[42] + ",");
                Console.Write(DataArray[43] + ",");
                Console.Write(DataArray[44] + ",");
                Console.Write(DataArray[45] + ",");
                Console.Write(DataArray[46] + ",");
                Console.Write(DataArray[47] + ",");
                Console.Write(DataArray[48] + ",");
                Console.Write(DataArray[49] + ",");
                Console.Write(DataArray[50] + ",");
                Console.Write(DataArray[51] + ",");
                Console.Write(DataArray[52] + ",");
                Console.Write(DataArray[53] + ",");
                Console.Write(DataArray[54] + ",");
                Console.Write(DataArray[55] + ",");
                Console.Write(DataArray[56] + ",");
                Console.Write(DataArray[57] + ",");
                Console.Write(DataArray[58] + ",");
                Console.Write(DataArray[59] + ",");
                Console.Write(DataArray[60] + ",");
                Console.Write(DataArray[61] + ",");
                Console.Write(DataArray[62] + ",");
                Console.Write(DataArray[63] + ",");
                Console.Write(DataArray[64] + ",");
                Console.Write(DataArray[65] + ",");
                Console.Write(DataArray[66] + ",");
                Console.Write(DataArray[67] + ",");
                Console.Write(DataArray[68] + ",");
                Console.Write(DataArray[69] + ",");
                Console.WriteLine();
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        finally
        {
            Array.Clear(DataArray,0, DataArray.Length);
            if (sqlDataReader != null && !sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }
        }
        sqlConnection.Close();
    }
    static string SendCommandAll() 
    {
        string SendMassages = null;
        Console.WriteLine("ALL");
        DateTime localDate = new DateTime();
        localDate = DateTime.UtcNow;
        Console.WriteLine(localDate.ToString());
        SQLiteConnection sqlConnection = null;
        //string cS = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Hayk\Desktop\MonitoringServer\MonitoringServer\MoikaData.mdf;Integrated Security=True";
        //sqlConnection = new SQLiteConnection("Data Source=MoikaData.db;");
        sqlConnection = new SQLiteConnection("Data Source=MoikaData.db; Version = 3;");
        //sqlConnection = new SqliteConnection(cS);
        SQLiteCommand sqlCommand = null;
        sqlConnection.Open();
        SQLiteDataReader sqlDataReader = null;
        try
        {
            sqlCommand = new SQLiteCommand($"SELECT * FROM Devices", sqlConnection);
            DataTable dataTable = new DataTable();
            sqlDataReader = sqlCommand.ExecuteReader();
            dataTable.Load(sqlDataReader);
            SendMassages = JsonConvert.SerializeObject(dataTable);
            /*var items = new Dictionary<object, Dictionary<string, object>>();
            while (sqlDataReader.Read())
            {
                var item = new Dictionary<string, object>(sqlDataReader.FieldCount - 1);
                for (var i = 1; i < sqlDataReader.FieldCount; i++)
                {
                    item[sqlDataReader.GetName(i)] = sqlDataReader.GetValue(i);
                }
                items[sqlDataReader.GetValue(0)] = item;
            }
            string json = JsonConvert.SerializeObject(items, Formatting.Indented);


            Console.WriteLine(json);
            SendMassages = json;*/
            /*var fileName = @"C:\Users\Hayk\Desktop\MonitoringServer\MonitoringServer\FileJson.json";
            File.WriteAllTextAsync(fileName, json);*/


            /*            while (sqlDataReader.Read())
                        {
                            SendMassages=sqlDataReader.GetString(0);
                        }
                        Console.WriteLine(SendMassages);*/
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
        sqlConnection.Close();
        return SendMassages;
    }
}