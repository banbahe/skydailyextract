using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyDailyExtract.Models
{
    public class FileModel
    {
        // https://api.etadirect.com/rest/ofscCore/v1/folders/dailyExtract/folders/2017-12-24/files/

        public string nameFile { get; set; }
        public string bytes { get; set; }
        public string mediaType { get; set; }
        public string hrefFile { get; set; }

    }
}
