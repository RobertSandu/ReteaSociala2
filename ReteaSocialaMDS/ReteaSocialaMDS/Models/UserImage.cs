﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ReteaSocialaMDS.Models
{
    public class UserImage
    {
        [Key]
        public int Id { get; set; }


        public string OwnerId { get; set; }


        public string Title { get; set; }


        public string Description { get; set; }


        [Required]
        [DataType(DataType.ImageUrl)]
        public string ImageUrl { get; set; }


        private DateTime? uploadDate;

        public DateTime UploadDate
        {
            get { return uploadDate ?? DateTime.Now; }
            set { uploadDate = value; }
        }

        [ForeignKey("OwnerId")]
        public virtual ApplicationUser Owner { get; set; }
    }
}