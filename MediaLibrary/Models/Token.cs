using System;
using System.Collections.Generic;

#nullable disable

namespace MediaLibrary.Models
{
    public partial class Token
    {
        public int TokenId { get; set; }
        public string TokenString { get; set; }
        public int Life { get; set; }
        public string Target { get; set; }
        public int AccountId { get; set; }
    }
}
