using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyDailyExtract.Models
{
    public class OFSCModel
    {
        public string name { get; set; }
        public string href { get; set; }
        public List<FileModel> FilesModel { get; set; }
    }
}
