#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8600
#pragma warning disable CS8602
#pragma warning disable CS8603
using Microsoft.Extensions.Logging;
using lacuna_genetics.Entity;
using Newtonsoft.Json;
using System.Text;
using lacuna_genetics.Enum;

namespace lacuna_genetics
{
    public class Client : HttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Client> _logger;

        public Client(HttpClient httpClient, ILogger<Client> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> AuthenticationAsync(LacunaConfig config)
        {
            _logger.LogInformation("Starting Authentication");

            try
            {
                LoginRequest login = new();
                login.Username = config.GeneLacunaUser;
                login.Password = config.GeneLacunaPassword;

                StringContent stringContent = new(JsonConvert.SerializeObject(login), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();

                var result = await _httpClient.PostAsync("users/login", stringContent);
                result.EnsureSuccessStatusCode();

                string jsonResult = await result.Content.ReadAsStringAsync();

                TokenResponse token = JsonConvert.DeserializeObject<TokenResponse>(jsonResult);

                _logger.LogInformation("Authentication Completed Successfully!");
                return token.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Processing Authentication");
                throw new Exception(ex.Message);
            }

        }

        public async Task<JobResponse> RequestJobAsync(string token)
        {
            _logger.LogInformation("Starting Request a Job");

            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer { token }");

                var result = await _httpClient.GetAsync("dna/jobs");
                result.EnsureSuccessStatusCode();

                string jsonResult = await result?.Content.ReadAsStringAsync();
                JobResponse jobResponse = JsonConvert.DeserializeObject<JobResponse>(jsonResult);

                _logger.LogInformation($"Job Resquested: {jobResponse.Job.Type}");

                return jobResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Processing Request a Job");
                throw new Exception(ex.Message);
            }

        }

        public async Task<ResultResponse> ChecksOperationAsync(string id, string token, string type, bool checkActivated, string check="")
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

                HttpResponseMessage result = await ResultCheck(id, check, type, checkActivated);

                result.EnsureSuccessStatusCode();

                string jsonResult = await result?.Content?.ReadAsStringAsync();
                ResultResponse resultResponse = JsonConvert.DeserializeObject<ResultResponse>(jsonResult);

                return resultResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error Processing Request");
                throw new Exception(ex.Message);
            }
        }

        private async Task<HttpResponseMessage> ResultCheck(string id, string check, string type, bool checkActivated)
        {
            if(type == TypeJobEnum.EncodeStrand.ToString())
            {
                dynamic strandEncoded = new { StrandEncoded = check };

                StringContent stringContent = new(JsonConvert.SerializeObject(strandEncoded), Encoding.UTF8, "application/json");

                return await _httpClient.PostAsync($"dna/jobs/{id}/encode", stringContent);

            }else if(type == TypeJobEnum.DecodeStrand.ToString())
            {
                dynamic strand = new { strand = check };

                StringContent stringContent = new(JsonConvert.SerializeObject(strand), Encoding.UTF8, "application/json");

                return await _httpClient.PostAsync($"dna/jobs/{id}/decode", stringContent);
            }
            else
            {
                dynamic isActivaded = new { isActivaded = checkActivated };

                StringContent stringContent = new(JsonConvert.SerializeObject(isActivaded), Encoding.UTF8, "application/json");

                return await _httpClient.PostAsync($"dna/jobs/{id}/gene", stringContent);
            }

            return await ResultCheck(id, check, type, true);

        }




    }
}

