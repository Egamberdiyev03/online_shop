using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Common
{
    public static class Languages
    {
        public const string Uz = "uz";
        public const string Ru = "ru";
        public const string En = "en";
        public const string Default = Uz;
        public static readonly string[] Supported = { Uz, Ru, En };
    }
}
