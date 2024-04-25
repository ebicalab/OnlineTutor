using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace NamedPipeClient {
    class Program {
        static void Main(string[] args) {

            var charGPTService = new ChatGPTService("Ты учитель по MS SQL");

            PipeClient pipeClient = new PipeClient();

            pipeClient.OnPipeCommandReceived += (pipeClient_, pipeCommand) => {
                Console.WriteLine($"Получено сообщение: " + pipeCommand.toString() + "\n");
            };

            pipeClient.OnPipeCommandReceived += (pipeClient_, pipeCommand) => {

                Console.WriteLine("system message: " + pipeCommand.SystemRoleMessage);

                if (!String.IsNullOrEmpty(pipeCommand.SystemRoleMessage)) {
                    charGPTService.AddSystemMessage(pipeCommand.SystemRoleMessage);
                }

                charGPTService.PrintMessages();

                //charGPTService.AddSystemMessage("Отвечай как бэтмен!");

                string answer = charGPTService.getChatGPTResponseTextAsync(pipeCommand.Command).Result;
                if (answer != null) {
                    pipeClient.SendMessage(answer);
                }
            };

            pipeClient.Start();

            while (true) {
                var message = Console.ReadLine();
                if (message != null) {
                    pipeClient.SendMessage(message);
                }
            }

            //while (true) {

            //    Console.WriteLine("\r\nDo you want pass system message? (y/n)");

            //    var sysMessageAnswer = Console.ReadLine();

            //    if (sysMessageAnswer?.ToLower() == "y" || sysMessageAnswer?.ToLower() == "yes") {

            //        Console.WriteLine("Please, enter the system message: ");

            //        var systemMessage = Console.ReadLine();

            //        if (!String.IsNullOrEmpty(systemMessage)) {
            //            charGPTService.AddSystemMessage(systemMessage);
            //        }
            //    }

            //    var question = Console.ReadLine();

            //    string answer = charGPTService.getChatGPTResponseTextAsync(question).Result;

            //    Console.WriteLine("\r\nAnswer: " + answer);
            //}
        }
    }

    [Serializable]
    public struct PipeCommand {
        [JsonPropertyName("command")]
        public string Command { get; set; }

        [JsonPropertyName("system")]
        public string SystemRoleMessage { get; set; }

        public string toString() {
            return "{ " + this.Command + ", " + this.SystemRoleMessage + " }"; 
        }
    }

    public class PipeClient {
        public static PipeClient Instance { get; private set; }

        public const string ReadPipeName = "VCR_W";
        public const string WritePipeName = "VCR_R";

        public event EventHandler<PipeCommand> OnPipeCommandReceived;

        private Thread readThread;
        private Thread writeThread;
        private Thread readFromQueueThread;
        private StreamString streamReadString;
        private StreamString streamWriteString;
        private Queue<string> readQueue;
        private Queue<string> writeQueue;
        private object readLock;
        private object writeLock;



        public PipeClient() {
            Instance = this;
            readQueue = new Queue<string>();
            writeQueue = new Queue<string>();
            readLock = new object();
            writeLock = new object();
        }

        public void Start() {
            Console.WriteLine("Ожидаем подключения клиента...");
            readThread = new Thread(ClientReadThread);
            readThread.Start();
            writeThread = new Thread(ClientWriteThread);
            writeThread.Start();

            readFromQueueThread = new Thread(ReadFromPipeQueue);
            readFromQueueThread.Start();
        }

        public void ReadFromPipeQueue() {
            while (true) {
                lock (readLock) {
                    if (readQueue.Count > 0) {
                        string message = readQueue.Dequeue();
                        PipeCommand pipeCommand = JsonSerializer.Deserialize<PipeCommand>(message);
                        OnPipeCommandReceived?.Invoke(this, pipeCommand);
                    }
                }
            }
        }

        private void ClientReadThread() {
            using (NamedPipeClientStream pipeReadClient = new NamedPipeClientStream(".", ReadPipeName, PipeDirection.In)) {

                // Пытаемся сконнектиться
                while (!pipeReadClient.IsConnected) {
                    Console.WriteLine("Коннектимся к серверу...");
                    try {
                        pipeReadClient.Connect();
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Ошибка подключения к серверу: {ex}");
                    }
                    Thread.Sleep(100);
                }

                Console.WriteLine("Клиент(чтение) подключен!!!");

                try {
                    streamReadString = new StreamString(pipeReadClient);

                    while (true) {
                        string message = streamReadString.ReadString();

                        lock (readLock) {
                            readQueue.Enqueue(message);
                        }

                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Ошибка: " + ex);
                }

                Console.WriteLine("Канал на чтение закрыт!");
            }
        }

        private void ClientWriteThread() {
            using (NamedPipeClientStream pipeWriteClient = new NamedPipeClientStream(".", WritePipeName, PipeDirection.Out)) {

                // Пытаемся сконнектиться
                while (!pipeWriteClient.IsConnected) {
                    Console.WriteLine("Коннектимся к серверу...");
                    try {
                        pipeWriteClient.Connect();
                    }
                    catch (Exception ex) {
                        Console.WriteLine($"Ошибка подключения к серверу: {ex}");
                    }
                    Thread.Sleep(100);
                }

                Console.WriteLine("Клиент(запись) подключен!!!");

                try {
                    streamWriteString = new StreamString(pipeWriteClient);

                    //SendMessage("Hello from the Client!");

                    while (true) {
                        string messageFromQueue = null;

                        // На всякий случай добавляем лок при считывании сообщения из очереди
                        lock (writeLock) {
                            if (writeQueue.Count > 0) {
                                messageFromQueue = writeQueue.Dequeue();
                            }
                        }

                        if (messageFromQueue != null) {
                            Console.WriteLine("Отправляем сообщение: " + messageFromQueue);
                            streamWriteString.WriteString(messageFromQueue);
                        }

                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine("Ошибка: " + ex);
                }

                Console.WriteLine("Канал на запись закрыт!");
            }
        }

        public void SendMessage(string message) {
            SendMessage(new PipeCommand { Command = message });
        }

        public void SendMessage(PipeCommand pipeCommand) {
            lock (writeLock) {
                writeQueue.Enqueue(JsonSerializer.Serialize(pipeCommand));
            }
        }
    }
}

public class StreamString {
    private Stream ioStream;
    private UTF8Encoding streamEncoding;

    public StreamString(Stream ioStream) {
        this.ioStream = ioStream;
        streamEncoding = new UTF8Encoding();
    }

    public string ReadString() {
        int len = 0;

        len = ioStream.ReadByte() * 256;
        len += ioStream.ReadByte();
        byte[] inBuffer = new byte[len];
        ioStream.Read(inBuffer, 0, len);

        return streamEncoding.GetString(inBuffer);
    }

    public int WriteString(string outString) {
        byte[] outBuffer = streamEncoding.GetBytes(outString);
        int len = outBuffer.Length;
        if (len > UInt16.MaxValue) {
            len = (int)UInt16.MaxValue;
        }
        ioStream.WriteByte((byte)(len / 256));
        ioStream.WriteByte((byte)(len & 255));
        ioStream.Write(outBuffer, 0, len);
        ioStream.Flush();

        return outBuffer.Length + 2;
    }
}


class ChatGPTService {

    private static string apiKey = Base64Encoder.ToBase64Decode("c2stcXd3OFRXM29YdDdvMTRLWWhWNEVUM0JsYmtGSnJobTVZVU1MZ09ocVRUbkFlcm4z");

    private static string completionEndPoint = "https://api.openai.com/v1/chat/completions";

    private static string chatGPTModelID = "gpt-3.5-turbo";

    private static List<Message> messages;

    private static HttpClient httpClient;

    public ChatGPTService(string initialSystemContent) {

        messages = new List<Message>();
        messages.Add(new Message() { 
            Role = "system",
            Content = initialSystemContent 
        });

        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }

    public void AddSystemMessage(String message) {

        var newMessage = new Message() {
            Role = "system",
            Content = message
        };

        messages.Add(newMessage);
    }


    public void PrintMessages() {
        messages.ForEach(Console.WriteLine);
    }

    public async Task<string> getChatGPTResponseTextAsync(string question) {
        
        if (question.Length <= 0) return null;

        var newMessage = new Message() {
            Role = "user",
            Content = question //+ ". коротко"
        };

        messages.Add(newMessage);

        var requestData = new Request() {
            ModelId = chatGPTModelID,
            Messages = messages
        };

        using var response = await httpClient.PostAsJsonAsync(completionEndPoint, requestData);

        if (!response.IsSuccessStatusCode) {
            Console.WriteLine($"Response error: {(int) response.StatusCode} {response.StatusCode}");
            return null;
        }

        ResponseData? responseData = await response.Content.ReadFromJsonAsync<ResponseData>();

        var choices = responseData?.Choices ?? new List<Choice>();

        if (choices.Count == 0) {
            Console.WriteLine("No choices were returned by the API");
            return null;
        }

        var choice = choices[0];
        var responseMessage = choice.Message;

        messages.Add(responseMessage);

        return responseMessage.Content.Trim();
    }
}


class Message {

    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";

    public override string ToString() {
        return this.Role + " : " + this.Content;
    }
}
class Request {

    [JsonPropertyName("model")]
    public string ModelId { get; set; } = "";

    [JsonPropertyName("messages")]
    public List<Message> Messages { get; set; } = new();
}

class ResponseData {

    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("object")]
    public string Object { get; set; } = "";

    [JsonPropertyName("created")]
    public ulong Created { get; set; }

    [JsonPropertyName("choices")]
    public List<Choice> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public Usage Usage { get; set; } = new();
}

class Choice {

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("message")]
    public Message Message { get; set; } = new();

    [JsonPropertyName("finish_reason")]
    public string FinishReason { get; set; } = "";
}

class Usage {

    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

class Base64Encoder {

    static public string ToBase64Encode(string text) {

        if (String.IsNullOrEmpty(text)) {
            return text;
        }

        var textBytes = Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(textBytes);
    }

    static public string ToBase64Decode(string base64EncodedText) {

        if (String.IsNullOrEmpty(base64EncodedText)) {
            return base64EncodedText;
        }

        var enTextBytes = Convert.FromBase64String(base64EncodedText);
        return Encoding.UTF8.GetString(enTextBytes);
    }
}