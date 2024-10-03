using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Timers;
using System.ServiceProcess;
using Newtonsoft.Json;
using System.IO; // Para manipulação de arquivos e diretórios


namespace FileAPIService
{
    public partial class FileAPIService : ServiceBase
    {
        private Timer _timer;
        private readonly HttpClient _httpClient;

        public FileAPIService()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        protected override void OnStart(string[] args)
        {
            _timer = new Timer();
            _timer.Interval = 60000; // 1 minuto
            _timer.Elapsed += OnElapsedTime;
            _timer.Start();
        }

        private async void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            var fileRequest = new FileRequest
            {
                FileName = "teste.txt",
                Content = "Conteúdo do arquivo",
                DirectoryPath = @"C:\Jabil"
            };

            var json = JsonConvert.SerializeObject(fileRequest);
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("http://localhost:5000/api/file/createfile", httpContent);
                if (response.IsSuccessStatusCode)
                {
                    string origem = Path.Combine(fileRequest.DirectoryPath, fileRequest.FileName);
                    string destino = @"C:\rfIDEAS\teste.txt"; // Altere para o diretório desejado

                    // Verifica se o arquivo foi criado
                    if (File.Exists(origem))
                    {
                        // Move o arquivo para o diretório de destino
                        File.Move(origem, destino);

                        // Opcional: registrar no log que o arquivo foi movido
                        EventLog.WriteEntry("FileApiService", $"Arquivo movido de {origem} para {destino}", EventLogEntryType.Information);
                    }
                    else
                    {
                        EventLog.WriteEntry("FileApiService", $"Arquivo {origem} não encontrado", EventLogEntryType.Warning);
                    }
                }
                else
                {
                    EventLog.WriteEntry("FileApiService", "Erro ao criar arquivo via API: " + response.StatusCode, EventLogEntryType.Error);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("FileApiService", ex.ToString(), EventLogEntryType.Error);
            }
        }


        protected override void OnStop()
        {
            _timer?.Stop();
        }
    }

    // Classe para requisição da API
    public class FileRequest
    {
        public string FileName { get; set; }
        public string Content { get; set; }
        public string DirectoryPath { get; set; }
    }
}