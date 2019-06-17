﻿using System;

namespace xiQCameraUI.Models
{
    public class Image
    {
        /// <summary>
        /// A name for the image, not the file name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description for the image.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Full path such as c:\path\to\image.tiff
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The image file name such as image.png
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The file name extension: bmp, gif, jpg, png, tiff, etc...
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// The image height
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The image width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The file size of the image.
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// The file create time.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
