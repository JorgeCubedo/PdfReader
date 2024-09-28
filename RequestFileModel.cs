using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionAppTest
{
    public class RequestFileModel
    {
        [JsonProperty("file")]
        public string File { get; set; }
    }
}
