﻿using Ganss.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NhapHangV2.Entities.ExcelMapper
{
    public class CatalogueMapper
    {
        /// <summary>
        /// Mã
        /// </summary>
        [Column(1)]
        public string Code { get; set; }

        /// <summary>
        /// Tên
        /// </summary>
        [Column(2)]
        public string Name { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        [Column(3)]
        public string Description { get; set; }

        /// <summary>
        /// Kết quả trả về
        /// </summary>
        [Column(4)]
        public string ResultMessage { get; set; }
    }
}
