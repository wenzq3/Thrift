using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.ClientWin
{
    public static class HttpClientHelper
    {
        private static HttpClient _client;

        public static HttpClient GetClient()
        {
            try
            {
                if (_client == null)
                {
                    _client = new HttpClient();
                    _client.DefaultRequestHeaders.Connection.Add("keep-alive");
                    return _client;
                }
                return _client;
            }
            catch (Exception)
            {
                return new HttpClient();
                //throw;
            }
        }
    }
}
