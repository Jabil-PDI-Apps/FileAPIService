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
            // Defina o diretório onde os arquivos estão localizados
            string diretorioOrigem = DiretorioAtual.ObterDiretorioAnoMesAtual(@"K:\DATA");

            try
            {
                // Obtém a lista de todos os arquivos no diretório
                string[] arquivos = Directory.GetFiles(diretorioOrigem);

                foreach (string caminhoArquivo in arquivos)
                {
                    // Extraia o nome do arquivo e o conteúdo
                    string nomeArquivo = Path.GetFileName(caminhoArquivo);
                    string conteudoArquivo = File.ReadAllText(caminhoArquivo);

                    // Crie o FileRequest para cada arquivo
                    var fileRequest = new FileRequest
                    {
                        FileName = nomeArquivo,
                        Content = conteudoArquivo,
                        DirectoryPath = @"\\\\BRMANM0PRS02\\maquinas\\SPI"
                    };

                    var json = JsonConvert.SerializeObject(fileRequest);
                    var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                    // Envia o request para a API
                    var response = await _httpClient.PostAsync("http://localhost:5000/api/file/createfile", httpContent);

                    if (response.IsSuccessStatusCode)
                    {
                        EventLog.WriteEntry("FileApiService", $"Arquivo {nomeArquivo} enviado com sucesso.", EventLogEntryType.Information);
                    }
                    else
                    {
                        EventLog.WriteEntry("FileApiService", $"Falha ao enviar arquivo {nomeArquivo}: {response.StatusCode}", EventLogEntryType.Error);
                    }
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

    public class DiretorioAtual
    {
        public static string ObterDiretorioAnoMesAtual(string caminhoBase)
        {
            // Obtém o ano e mês atuais
            string anoAtual = DateTime.Now.Year.ToString();
            string mesAtual = DateTime.Now.ToString("MM"); // Retorna o mês no formato "MM"

            // Monta o caminho completo (ano/mes)
            string caminhoAno = Path.Combine(caminhoBase, anoAtual);
            string caminhoMes = Path.Combine(caminhoAno, mesAtual);

            // Verifica se o diretório existe
            if (Directory.Exists(caminhoMes))
            {
                return caminhoMes; // Retorna o diretório encontrado
            }
            else
            {
                throw new DirectoryNotFoundException($"O diretório para {anoAtual}/{mesAtual} não foi encontrado.");
            }
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