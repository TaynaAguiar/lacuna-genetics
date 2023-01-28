using lacuna_genetics.Entity;
using lacuna_genetics.Enum;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace lacuna_genetics.Service
{
    public class LacunaService
    {
        private readonly ILogger<LacunaService> _logger;
        private readonly Client _client;
        public LacunaService(ILogger<LacunaService> logger, Client client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task ExecuteAsync(LacunaConfig config)
        {
            while (true)
            {
                string token = await _client.AuthenticationAsync(config);

                JobResponse job = await _client.RequestJobAsync(token);

                _logger.LogInformation($"Starting Job {job.Job!.Type}");

                ResultResponse result = new();

                if (job.Job!.Type == TypeJobEnum.EncodeStrand.ToString())
                {
                    string strandEncoded = EncodeStrandOperation(job.Job.Strand!);
                    result = await _client.ChecksOperationAsync(job.Job.Id, token, job.Job!.Type, false, strandEncoded);
                }
                else if (job.Job!.Type == TypeJobEnum.DecodeStrand.ToString())
                {
                    string strand = DecodeStrandOperation(job.Job.StrandEncoded!);
                    result = await _client.ChecksOperationAsync(job.Job.Id, token, job.Job!.Type, false, strand);

                }
                else
                {
                    string strand = DecodeStrandOperation(job.Job.StrandEncoded!);
                    string gene = DecodeStrandOperation(job.Job.GeneEncoded!);
                    bool isActivated = CheckGeneOperation(strand, gene);
                    result = await _client.ChecksOperationAsync(job.Job.Id, token, job.Job!.Type, isActivated);
                }

                if (result.Code == "Success")
                    _logger.LogInformation("Job Done Successfully");
                else
                    _logger.LogError($"Job Ended With Error: {result.Message}");


                await Task.Delay(TimeSpan.FromMinutes(2));

            }
        }

        private static string EncodeStrandOperation(string strand)
        {
            string convert = string.Empty;

            for (int i = 0; i < strand.Length; i++)
            {
                switch (strand[i])
                {
                    case 'A':
                        convert += "00";
                        break;
                    case 'C':
                        convert += "01";
                        break;
                    case 'T':
                        convert += "11";
                        break;
                    case 'G':
                        convert += "10";
                        break;
                }
            }

            byte[] bytes = Enumerable.Range(0, convert.Length / 8).Select(pos => Convert.ToByte(convert.Substring(pos * 8, 8), 2)).ToArray();
            string result = Convert.ToBase64String(bytes);

            return result;
        }

        private static string DecodeStrandOperation(string strandEncoded)
        {
            string strand = "";

            string convert = BigEndianReverse(strandEncoded);

            for (int i = 0; i < convert.Length; i += 2)
            {
                if (convert[i] == '0' && convert[1 + i] == '0')
                    strand += "A";
                else if (convert[i] == '0' && convert[1 + i] == '1')
                    strand += "C";
                else if (convert[i] == '1' && convert[1 + i] == '0')
                    strand += "G";
                else
                    strand += "T";
            }

            return strand;
        }

        private static string BigEndianReverse(string strandEncoded)
        {
            byte[] strandBytes = Convert.FromBase64String(strandEncoded);

            BitArray bitArr = new(strandBytes);

            string bitStr = ToBitString(bitArr);
            string splitString = string.Join(string.Empty, bitStr.Select((x, i) => i > 0 && i % 8 == 0 ? string.Format(" {0}", x) : x.ToString()));
            string[] stringBit = splitString.Split(' ');
            string convert = "";

            foreach (var t in stringBit)
                convert += Reverse(t);

            return convert;
        }

        private static string ToBitString(BitArray bits)
        {
            var sb = new StringBuilder();

            for (int i = 0; i < bits.Count; i++)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }

        private static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        private bool CheckGeneOperation(string strand, string gene)
        {
            if (strand[..3] != "CAT")
                strand = StrandReverse(strand);

            for (int i = 0; i < (gene.Length) / 2; i++)
            {
                string chechGene = gene.Substring(0 + i, (gene.Length / 2) + 1);
                if (strand.Contains(chechGene))
                    return true;
            }

            return false;
        }

        private static string StrandReverse(string strand)
        {
            return strand.Replace('A', '*').Replace('T', 'A').Replace('*', 'T')
                         .Replace('C', '*').Replace('G', 'C').Replace('*', 'G');
        }
    }
}
