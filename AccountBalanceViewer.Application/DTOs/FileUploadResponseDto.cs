﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountBalanceViewer.Application.DTOs
{
    public class FileUploadResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public int AffectedRows { get; set; }
        public string Status { get; set; } = "Falied";
    }
}
