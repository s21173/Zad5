using System;
using System.ComponentModel.DataAnnotations;

namespace Zad5
{
    public class Warehouse
    {
        public int IdProduct { get; set; }

        [Required(ErrorMessage = "Pole IdWareHouse jest wymagane")]
        public int IdWarehouse { get; set; }

        [Required(ErrorMessage = "Pole Amount jest wymagane")]
        public int Amount { get; set; }

        [Required(ErrorMessage = "Pole CreatedAt jest wymagane")]
        public DateTime CreatedAt {get; set;}

        public int IdOrder { get; set; }
    }
}
