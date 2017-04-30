using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CardGrid.Models
{
    public enum SortDirection
    {
        asc,
        desc
    }
    public class GridResponse<T>
    {
        public GridResponse()
        {

            total = 12;
            rows = 1;
            page = 1;
            sord = SortDirection.asc;
            sidx = searchParam = string.Empty;
        }
        /// <summary>
        /// Total No of Pages
        /// </summary>
        public int noOfPages
        {
            get
            {
                return (int)Math.Ceiling((float)total / (float)rows); ;
            }

        }
        /// <summary>
        /// Total No of Records Available
        /// </summary>
        public int total { get; set; }
        /// <summary>
        /// No of Items Per Page
        /// </summary>
        public int rows { get; set; }
        /// <summary>
        /// Current Page Index
        /// </summary>
        public int page { get; set; }
        /// <summary>
        /// Sorting Direction 'asc/desc'
        /// </summary>
        public SortDirection sord { get; set; }
        /// <summary>
        /// Sorting Field
        /// </summary>
        public string sidx { get; set; }
        /// <summary>
        /// Data
        /// </summary>
        public List<T> Data { get; set; }
        /// <summary>
        /// Search Text
        /// </summary>
        public string searchParam { get; set; }
    }
}